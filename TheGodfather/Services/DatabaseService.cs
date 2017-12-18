﻿#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using TheGodfather.Helpers;
using TheGodfather.Exceptions;

using Npgsql;
using NpgsqlTypes;
using System.Collections.ObjectModel;
#endregion

namespace TheGodfather.Services
{
    public class DatabaseService
    {
        private string _connectionString { get; }
        private SemaphoreSlim _sem { get; }
        private SemaphoreSlim _tsem { get; }
        private DatabaseConfig _cfg { get; }


        public DatabaseService(DatabaseConfig config)
        {
            _sem = new SemaphoreSlim(100, 100);
            _tsem = new SemaphoreSlim(1, 1);

            if (config == null)
                _cfg = DatabaseConfig.Default;
            else
                _cfg = config;

            var csb = new NpgsqlConnectionStringBuilder() {
                Host = _cfg.Hostname,
                Port = _cfg.Port,
                Database = _cfg.Database,
                Username = _cfg.Username,
                Password = _cfg.Password,
                Pooling = true
                /*
                SslMode = SslMode.Require,
                TrustServerCertificate = true
                */
            };
            _connectionString = csb.ConnectionString;
        }


        public async Task InitializeAsync()
        {
            await _sem.WaitAsync();

            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);
                }
            } catch (NpgsqlException e) {
                throw new DatabaseServiceException("Database connection failed. Check your login details in the config.json file.", e);
            }

            _sem.Release();
        }

        public async Task<IReadOnlyList<IReadOnlyDictionary<string, string>>> ExecuteRawQueryAsync(string query)
        {
            await _sem.WaitAsync();
            var dicts = new List<IReadOnlyDictionary<string, string>>();

            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = query;

                    using (var rdr = await cmd.ExecuteReaderAsync()) {
                        while (await rdr.ReadAsync()) {
                            var dict = new Dictionary<string, string>();

                            for (var i = 0; i < rdr.FieldCount; i++)
                                dict[rdr.GetName(i)] = rdr[i] is DBNull ? "<null>" : rdr[i].ToString();

                            dicts.Add(new ReadOnlyDictionary<string, string>(dict));
                        }
                    }
                }
            } catch (NpgsqlException e) {
                throw new DatabaseServiceException("", e);
            }

            _sem.Release();
            return new ReadOnlyCollection<IReadOnlyDictionary<string, string>>(dicts);
        }

        // BANK

        public async Task<bool> HasBankAccountAsync(ulong uid)
        {
            int? balance = await GetBalanceForUserAsync(uid).ConfigureAwait(false);
            return balance.HasValue;
        }

        public async Task<int?> GetBalanceForUserAsync(ulong uid)
        {
            await _sem.WaitAsync();

            int? balance = null;

            using (var con = new NpgsqlConnection(_connectionString))
            using (var cmd = con.CreateCommand()) {
                await con.OpenAsync().ConfigureAwait(false);

                cmd.CommandText = $"SELECT balance FROM gf.accounts WHERE uid = {uid} LIMIT 1;";

                var res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                if (res != null && !(res is DBNull))
                    balance = (int)res;
            }

            _sem.Release();
            return balance;
        }

        public async Task OpenAccountForUserAsync(ulong uid)
        {
            await _sem.WaitAsync();

            using (var con = new NpgsqlConnection(_connectionString))
            using (var cmd = con.CreateCommand()) {
                await con.OpenAsync().ConfigureAwait(false);

                cmd.CommandText = $"INSERT INTO gf.accounts VALUES({uid}, 25);";

                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }

            _sem.Release();
        }

        public async Task IncreaseBalanceForUserAsync(ulong uid, int ammount)
        {
            await _sem.WaitAsync();

            using (var con = new NpgsqlConnection(_connectionString))
            using (var cmd = con.CreateCommand()) {
                await con.OpenAsync().ConfigureAwait(false);

                cmd.CommandText = $"UPDATE gf.accounts SET balance = balance + {ammount} WHERE uid = {uid};";

                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }

            _sem.Release();
        }

        public async Task TransferCurrencyAsync(ulong source, ulong target, long amount)
        {
            await _sem.WaitAsync();

            using (var con = new NpgsqlConnection(_connectionString)) {
                await con.OpenAsync().ConfigureAwait(false);

                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = $"SELECT balance FROM gf.accounts WHERE uid = {target};";

                    var res = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                    
                    if (res == null || res is DBNull)
                        await OpenAccountForUserAsync(target);
                }

                await _tsem.WaitAsync().ConfigureAwait(false);
                try {
                    using (var transaction = con.BeginTransaction()) {
                        var cmd1 = con.CreateCommand();
                        cmd1.Transaction = transaction;
                        cmd1.CommandText = $"SELECT balance FROM gf.accounts WHERE uid = {source} OR uid = {target} FOR UPDATE;";

                        await cmd1.ExecuteNonQueryAsync().ConfigureAwait(false);

                        var cmd2 = con.CreateCommand();
                        cmd2.Transaction = transaction;
                        cmd2.CommandText = $"SELECT balance FROM gf.accounts WHERE uid = {source};";

                        var res = await cmd2.ExecuteScalarAsync().ConfigureAwait(false);
                        if (res == null || res is DBNull || (int)res < amount) {
                            await transaction.RollbackAsync().ConfigureAwait(false);
                            throw new CommandFailedException("Source user's currency amount is insufficient for the transfer.");
                        }

                        var cmd3 = con.CreateCommand();
                        cmd3.Transaction = transaction;
                        cmd3.CommandText = $"UPDATE gf.accounts SET balance = balance - {amount} WHERE uid = {source};";

                        await cmd3.ExecuteNonQueryAsync().ConfigureAwait(false);

                        var cmd4 = con.CreateCommand();
                        cmd4.Transaction = transaction;
                        cmd4.CommandText = $"UPDATE gf.accounts SET balance = balance + {amount} WHERE uid = {target};";

                        await cmd4.ExecuteNonQueryAsync().ConfigureAwait(false);

                        await transaction.CommitAsync().ConfigureAwait(false);

                        cmd1.Dispose();
                        cmd2.Dispose();
                        cmd3.Dispose();
                        cmd4.Dispose();
                    }
                } finally {
                    _tsem.Release();
                    _sem.Release();
                }
            }
        }

        public async Task<IReadOnlyList<IReadOnlyDictionary<string, string>>> GetTopAccountsAsync()
        {
            var res = await ExecuteRawQueryAsync("SELECT * FROM gf.accounts ORDER BY balance DESC LIMIT 10")
                .ConfigureAwait(false);
            return res;
        }


        public async Task<IReadOnlyDictionary<ulong, string>> GetGuildPrefixesAsync()
        {
            await _sem.WaitAsync();
            var dict = new Dictionary<ulong, string>();

            using (var con = new NpgsqlConnection(_connectionString))
            using (var cmd = con.CreateCommand()) {
                await con.OpenAsync().ConfigureAwait(false);

                cmd.CommandText = "SELECT * FROM gf.prefixes;";

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                        dict[(ulong)(long)reader["gid"]] = (string)reader["prefix"];
                }
            }

            _sem.Release();
            return new ReadOnlyDictionary<ulong, string>(dict);
        }

        public async Task<IReadOnlyDictionary<string, string>> GetStatsForUserAsync(ulong uid)
        {
            var res = await ExecuteRawQueryAsync($"SELECT * FROM gf.stats WHERE uid = {uid};")
                .ConfigureAwait(false);

            if (res != null && res.Any())
                return res.First();
            else
                return null;
        }

        public async Task<IReadOnlyList<IReadOnlyDictionary<string, string>>> GetOrderedStatsAsync(string orderstr, params string[] selectors)
        {
            var res = await ExecuteRawQueryAsync($@"
                SELECT uid, {string.Join(", ", selectors)} 
                FROM gf.stats
                ORDER BY {orderstr} DESC
                LIMIT 5
            ").ConfigureAwait(false);

            return res;
        }

        public async Task UpdateStatAsync(ulong uid, string col, int add)
        {
            var stats = await GetStatsForUserAsync(uid);

            using (var con = new NpgsqlConnection(_connectionString))
            using (var cmd = con.CreateCommand()) {
                await con.OpenAsync().ConfigureAwait(false);

                if (stats != null && stats.Any())
                    cmd.CommandText = $"UPDATE gf.stats SET {col} = {col} + {add} WHERE uid = {uid};";
                else
                    cmd.CommandText = $"INSERT INTO gf.stats (uid, {col}) VALUES ({uid}, {add});";

                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }
    }
}
