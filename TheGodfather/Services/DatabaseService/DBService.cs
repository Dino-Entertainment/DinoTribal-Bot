﻿#region USING_DIRECTIVES
using Npgsql;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using TheGodfather.Common;
using TheGodfather.Exceptions;
#endregion

namespace TheGodfather.Services.Database
{
    // TODO : Remove partial once all the parts are refactored to extensions
    public partial class DBService
    {
        private readonly string connectionString;
        private readonly SemaphoreSlim accessSemaphore;
        private readonly SemaphoreSlim transactionSemaphore;
        private readonly DatabaseConfig cfg;


        public DBService(DatabaseConfig config)
        {
            this.cfg = config ?? DatabaseConfig.Default;
            this.accessSemaphore = new SemaphoreSlim(100, 100);
            this.transactionSemaphore = new SemaphoreSlim(1, 1);

            var csb = new NpgsqlConnectionStringBuilder() {
                Host = this.cfg.Hostname,
                Port = this.cfg.Port,
                Database = this.cfg.DatabaseName,
                Username = this.cfg.Username,
                Password = this.cfg.Password,
                Pooling = true
                //SslMode = SslMode.Require,
                //TrustServerCertificate = true
            };
            this.connectionString = csb.ConnectionString;
        }


        public async Task ExecuteCommandAsync(Func<NpgsqlCommand, Task> action)
        {
            await this.accessSemaphore.WaitAsync();
            try {
                using (var con = await OpenConnectionAsync())
                using (var cmd = con.CreateCommand())
                    await action(cmd);
            } catch (NpgsqlException e) {
                throw new DatabaseOperationException("Database operation failed!", e);
            } finally {
                this.accessSemaphore.Release();
            }
        }

        public Task InitializeAsync()
            => ExecuteCommandAsync(cmd => Task.CompletedTask );

        public async Task CheckIntegrityAsync()
        {
            // TODO
            await Task.Delay(0);
            //throw new DatabaseServiceException("Database integrity check failed!");
        }

        public async Task<IReadOnlyList<IReadOnlyDictionary<string, string>>> ExecuteRawQueryAsync(string query)
        {
            var dicts = new List<IReadOnlyDictionary<string, string>>();

            await ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = query;
                using (var reader = await cmd.ExecuteReaderAsync()) {
                    while (await reader.ReadAsync()) {
                        var dict = new Dictionary<string, string>();

                        for (int i = 0; i < reader.FieldCount; i++)
                            dict[reader.GetName(i)] = reader[i] is DBNull ? "<null>" : reader[i].ToString();

                        dicts.Add(new ReadOnlyDictionary<string, string>(dict));
                    }
                }
            });

            return dicts.AsReadOnly();
        }


        private async Task<NpgsqlConnection> OpenConnectionAsync()
        {
            var con = new NpgsqlConnection(this.connectionString);
            await con.OpenAsync().ConfigureAwait(false);
            return con;
        }
    }
}
