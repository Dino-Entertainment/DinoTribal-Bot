﻿#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Npgsql;
using NpgsqlTypes;
#endregion

namespace TheGodfather.Services
{
    public partial class DBService
    {
        #region BLOCKED_USERS
        public async Task AddBlockedUserAsync(ulong uid, string reason = null)
        {
            await _sem.WaitAsync();
            try {
                using (var con = await OpenConnectionAndCreateCommandAsync())
                using (var cmd = con.CreateCommand()) {
                    if (string.IsNullOrWhiteSpace(reason)) {
                        cmd.CommandText = "INSERT INTO gf.blocked_users VALUES (@uid, NULL);";
                        cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, uid);
                    } else {
                        cmd.CommandText = "INSERT INTO gf.blocked_users VALUES (@uid, @reason);";
                        cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, uid);
                        cmd.Parameters.AddWithValue("reason", NpgsqlDbType.Varchar, reason);
                    }

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task<IReadOnlyList<(ulong, string)>> GetAllBlockedUsersAsync()
        {
            var blocked = new List<(ulong, string)>();

            await _sem.WaitAsync();
            try {
                using (var con = await OpenConnectionAndCreateCommandAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "SELECT * FROM gf.blocked_users;";

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                            blocked.Add(((ulong)(long)reader["uid"], reader["reason"] is DBNull ? null : (string)reader["reason"]));
                    }
                }
            } finally {
                _sem.Release();
            }

            return blocked.AsReadOnly();
        }

        public async Task RemoveBlockedUserAsync(ulong uid)
        {
            await _sem.WaitAsync();
            try {
                using (var con = await OpenConnectionAndCreateCommandAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "DELETE FROM gf.blocked_users WHERE uid = @uid;";
                    cmd.Parameters.AddWithValue("uid", NpgsqlDbType.Bigint, uid);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }
        #endregion

        #region BLOCKED_CHANNELS
        public async Task AddBlockedChannelAsync(ulong cid, string reason = null)
        {
            await _sem.WaitAsync();
            try {
                using (var con = await OpenConnectionAndCreateCommandAsync())
                using (var cmd = con.CreateCommand()) {
                    if (string.IsNullOrWhiteSpace(reason)) {
                        cmd.CommandText = "INSERT INTO gf.blocked_channels VALUES (@cid, NULL);";
                        cmd.Parameters.AddWithValue("cid", NpgsqlDbType.Bigint, cid);
                    } else {
                        cmd.CommandText = "INSERT INTO gf.blocked_channels VALUES (@cid, @reason);";
                        cmd.Parameters.AddWithValue("cid", NpgsqlDbType.Bigint, cid);
                        cmd.Parameters.AddWithValue("reason", NpgsqlDbType.Varchar, reason);
                    }

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task<IReadOnlyList<(ulong, string)>> GetAllBlockedChannelsAsync()
        {
            var blocked = new List<(ulong, string)>();

            await _sem.WaitAsync();
            try {
                using (var con = await OpenConnectionAndCreateCommandAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "SELECT * FROM gf.blocked_channels;";

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                            blocked.Add(((ulong)(long)reader["cid"], reader["reason"] is DBNull ? null : (string)reader["reason"]));
                    }
                }
            } finally {
                _sem.Release();
            }

            return blocked.AsReadOnly();
        }

        public async Task RemoveBlockedChannelAsync(ulong cid)
        {
            await _sem.WaitAsync();
            try {
                using (var con = await OpenConnectionAndCreateCommandAsync())
                using (var cmd = con.CreateCommand()) {
                    cmd.CommandText = "DELETE FROM gf.blocked_channels WHERE cid = @cid;";
                    cmd.Parameters.AddWithValue("cid", NpgsqlDbType.Bigint, cid);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }
        #endregion
    }
}
