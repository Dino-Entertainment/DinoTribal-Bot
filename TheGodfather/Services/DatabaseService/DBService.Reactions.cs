﻿#region USING_DIRECTIVES
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using TheGodfather.Modules.Reactions.Common;

using Npgsql;
using NpgsqlTypes;
#endregion

namespace TheGodfather.Services
{
    public partial class DBService
    {
        #region TEXT_REACTION_SERVICES
        public async Task<Dictionary<ulong, List<TextReaction>>> GetAllTextReactionsAsync()
        {
            var treactions = new Dictionary<ulong, List<TextReaction>>();

            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "SELECT * FROM gf.text_reactions;";

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                        while (await reader.ReadAsync().ConfigureAwait(false)) {
                            ulong gid = (ulong)(long)reader["gid"];
                            if (treactions.ContainsKey(gid)) {
                                if (treactions[gid] == null)
                                    treactions[gid] = new List<TextReaction>();
                            } else {
                                treactions.Add(gid, new List<TextReaction>());
                            }

                            string response = (string)reader["response"];
                            var reaction = treactions[gid].FirstOrDefault(tr => tr.Response == response);
                            if (reaction != null)
                                reaction.AddTrigger((string)reader["trigger"], is_regex_trigger: true);
                            else
                                treactions[gid].Add(new TextReaction((string)reader["trigger"], (string)reader["response"], is_regex_trigger: true));
                        }
                    }
                }
            } finally {
                _sem.Release();
            }

            return treactions;
        }

        public async Task AddTextReactionAsync(ulong gid, string trigger, string response, bool is_regex_trigger = false)
        {
            if (!is_regex_trigger)
                trigger = Regex.Escape(trigger);

            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "INSERT INTO gf.text_reactions VALUES (@gid, @trigger, @response);";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);
                    cmd.Parameters.AddWithValue("trigger", NpgsqlDbType.Varchar, trigger);
                    cmd.Parameters.AddWithValue("response", NpgsqlDbType.Varchar, response);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task RemoveTextReactionAsync(ulong gid, string trigger)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "DELETE FROM gf.text_reactions WHERE gid = @gid AND trigger = @trigger;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);
                    cmd.Parameters.AddWithValue("trigger", NpgsqlDbType.Varchar, trigger);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task DeleteAllGuildTextReactionsAsync(ulong gid)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "DELETE FROM gf.text_reactions WHERE gid = @gid;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }
        #endregion

        #region EMOJI_REACTION_SERVICES
        public async Task<Dictionary<ulong, Dictionary<string, List<Regex>>>> GetAllEmojiReactionsAsync()
        {
            var triggers = new Dictionary<ulong, Dictionary<string, List<Regex>>>();

            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "SELECT * FROM gf.emoji_reactions;";

                    using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                        while (await reader.ReadAsync().ConfigureAwait(false)) {
                            ulong gid = (ulong)(long)reader["gid"];

                            if (triggers.ContainsKey(gid)) {
                                if (triggers[gid] == null)
                                    triggers[gid] = new Dictionary<string, List<Regex>>();
                            } else {
                                triggers.Add(gid, new Dictionary<string, List<Regex>>());
                            }

                            string reaction = (string)reader["reaction"];
                            if (triggers[gid].ContainsKey(reaction)) {
                                if (triggers[gid][reaction] == null)
                                    triggers[gid][reaction] = new List<Regex>();
                            } else {
                                triggers[gid].Add(reaction, new List<Regex>());
                            }

                            var regex = new Regex($@"\b({(string)reader["trigger"]})\b", RegexOptions.IgnoreCase);
                            triggers[gid][reaction].Add(regex);
                        }
                    }
                }
            } finally {
                _sem.Release();
            }

            return triggers;
        }

        public async Task AddEmojiReactionAsync(ulong gid, string trigger, string reaction)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "INSERT INTO gf.emoji_reactions VALUES (@gid, @trigger, @reaction);";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);
                    cmd.Parameters.AddWithValue("trigger", NpgsqlDbType.Varchar, trigger);
                    cmd.Parameters.AddWithValue("reaction", NpgsqlDbType.Varchar, reaction);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task RemoveEmojiReactionTriggerAsync(ulong gid, string trigger)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "DELETE FROM gf.emoji_reactions WHERE gid = @gid AND trigger = @trigger;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);
                    cmd.Parameters.AddWithValue("trigger", NpgsqlDbType.Varchar, trigger);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task RemoveAllEmojiReactionTriggersForReactionAsync(ulong gid, string reaction)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "DELETE FROM gf.emoji_reactions WHERE gid = @gid AND reaction = @reaction;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);
                    cmd.Parameters.AddWithValue("reaction", NpgsqlDbType.Varchar, reaction);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }

        public async Task DeleteAllGuildEmojiReactionsAsync(ulong gid)
        {
            await _sem.WaitAsync();
            try {
                using (var con = new NpgsqlConnection(_connectionString))
                using (var cmd = con.CreateCommand()) {
                    await con.OpenAsync().ConfigureAwait(false);

                    cmd.CommandText = "DELETE FROM gf.emoji_reactions WHERE gid = @gid;";
                    cmd.Parameters.AddWithValue("gid", NpgsqlDbType.Bigint, gid);

                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            } finally {
                _sem.Release();
            }
        }
        #endregion
    }
}
