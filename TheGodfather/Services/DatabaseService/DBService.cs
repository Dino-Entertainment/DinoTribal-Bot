﻿#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Exceptions;

using DSharpPlus;

using Npgsql;
#endregion

namespace TheGodfather.Services
{
    public partial class DBService
    {
        private string _connectionString { get; }
        private SemaphoreSlim _sem { get; }
        private SemaphoreSlim _tsem { get; }
        private DatabaseConfig _cfg { get; }


        public DBService(DatabaseConfig config)
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
                Database = _cfg.DatabaseName,
                Username = _cfg.Username,
                Password = _cfg.Password,
                Pooling = true
                //SslMode = SslMode.Require,
                //TrustServerCertificate = true
            };
            _connectionString = csb.ConnectionString;
        }


        private async Task<NpgsqlConnection> OpenConnectionAndCreateCommandAsync()
        {
            var con = new NpgsqlConnection(_connectionString);
            await con.OpenAsync();
            return con;
        }


        public async Task InitializeAsync()
        {
            await _sem.WaitAsync();
            try {
                var cmd = await OpenConnectionAndCreateCommandAsync();
                cmd.Dispose();
            } catch (NpgsqlException e) {
                throw new DatabaseOperationException("Database connection failed. Check your login details in the config.json file.", e);
            } finally {
                _sem.Release();
            }
        }

        public async Task CheckIntegrityAsync()
        {
            await Task.Delay(0);
            //throw new DatabaseServiceException("Integrity check failed!");
        }

        public async Task<IReadOnlyList<IReadOnlyDictionary<string, string>>> ExecuteRawQueryAsync(string query)
        {
            var dicts = new List<IReadOnlyDictionary<string, string>>();

            await _sem.WaitAsync();
            try {
                using (var con = await OpenConnectionAndCreateCommandAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = query;

                    using (var reader = await cmd.ExecuteReaderAsync()) {
                        while (await reader.ReadAsync()) {
                            var dict = new Dictionary<string, string>();

                            for (var i = 0; i < reader.FieldCount; i++)
                                dict[reader.GetName(i)] = reader[i] is DBNull ? "<null>" : reader[i].ToString();

                            dicts.Add(new ReadOnlyDictionary<string, string>(dict));
                        }
                    }
                }
            } finally {
                _sem.Release();
            }

            return dicts.AsReadOnly();
        }
    }
}
