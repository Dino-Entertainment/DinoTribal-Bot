﻿#region USING_DIRECTIVES
using Npgsql;
using System.Collections.Generic;
using System.Threading.Tasks;
using TheGodfather.Modules.Chickens.Common;
#endregion

namespace TheGodfather.Services.Database.Chickens
{
    public static class DBServiceChickenExtensions
    {
        public static async Task AddChickenAsync(this DBService db, ulong uid, ulong gid, string name, ChickenStats stats)
        {
            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "INSERT INTO gf.chickens(uid, gid, name, strength, vitality, max_vitality) VALUES (@uid, @gid, @name, @strength, @vitality, @max_vitality) ON CONFLICT DO NOTHING;";
                cmd.Parameters.Add(new NpgsqlParameter("uid", (long)uid));
                cmd.Parameters.Add(new NpgsqlParameter("gid", (long)gid));
                cmd.Parameters.Add(new NpgsqlParameter("strength", stats.BareStrength));
                cmd.Parameters.Add(new NpgsqlParameter("vitality", stats.BareVitality));
                cmd.Parameters.Add(new NpgsqlParameter("max_vitality", stats.BareMaxVitality));
                if (string.IsNullOrWhiteSpace(name))
                    cmd.Parameters.Add(new NpgsqlParameter<string>("name", null));
                else
                    cmd.Parameters.Add(new NpgsqlParameter("name", name));

                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            });
        }

        public static async Task AddChickenUpgradeAsync(this DBService db, ulong uid, ulong gid, ChickenUpgrade upgrade)
        {
            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "INSERT INTO gf.chicken_active_upgrades VALUES (@uid, @gid, @wid) ON CONFLICT DO NOTHING;";
                cmd.Parameters.Add(new NpgsqlParameter("uid", (long)uid));
                cmd.Parameters.Add(new NpgsqlParameter("gid", (long)gid));
                cmd.Parameters.Add(new NpgsqlParameter("wid", upgrade.Id));

                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            });
        }

        public static async Task FilterChickensByVitalityAsync(this DBService db, ulong gid, int threshold)
        {
            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "DELETE FROM gf.chickens WHERE gid = @gid AND vitality <= @threshold;";
                cmd.Parameters.Add(new NpgsqlParameter("gid", (long)gid));

                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            });
        }

        public static async Task<IReadOnlyList<ChickenUpgrade>> GetAllChickenUpgradesAsync(this DBService db)
        {
            var upgrades = new List<ChickenUpgrade>();

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "SELECT wid, modifier, name, price, upgrades_stat FROM gf.chicken_upgrades;";

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                    while (await reader.ReadAsync().ConfigureAwait(false)) {
                        upgrades.Add(new ChickenUpgrade() {
                            Id = (int)reader["wid"],
                            Modifier = (int)reader["modifier"],
                            Name = (string)reader["name"],
                            Price = (long)reader["price"],
                            UpgradesStat = (ChickenStatUpgrade)(short)reader["upgrades_stat"]
                        });
                    }
                }
            });

            return upgrades.AsReadOnly();
        }

        public static async Task<Chicken> GetChickenAsync(this DBService db, ulong uid, ulong gid)
        {
            Chicken chicken = null;

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "SELECT * FROM gf.chickens WHERE uid = @uid AND gid = @gid LIMIT 1;";
                cmd.Parameters.Add(new NpgsqlParameter("uid", (long)uid));
                cmd.Parameters.Add(new NpgsqlParameter("gid", (long)gid));

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                    if (await reader.ReadAsync().ConfigureAwait(false)) {
                        chicken = new Chicken() {
                            Name = (string)reader["name"],
                            OwnerId = (ulong)(long)reader["uid"],
                            Stats = new ChickenStats() {
                                BareMaxVitality = (int)reader["max_vitality"],
                                BareStrength = (int)reader["strength"],
                                BareVitality = (int)reader["vitality"],
                            }
                        };
                    }
                }
            });

            if (chicken != null) {
                IReadOnlyList<ChickenUpgrade> upgrades = await db.GetUpgradesForChickenAsync(uid, gid);
                chicken.Stats.Upgrades = upgrades;
            }

            return chicken;
        }

        public static async Task<ChickenUpgrade> GetChickenUpgradeAsync(this DBService db, int wid)
        {
            ChickenUpgrade upgrade = null;

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "SELECT name, modifier, price, upgrades_stat FROM gf.chicken_upgrades WHERE wid = @wid LIMIT 1;";
                cmd.Parameters.Add(new NpgsqlParameter("wid", wid));

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                    if (await reader.ReadAsync().ConfigureAwait(false)) {
                        upgrade = new ChickenUpgrade() {
                            Id = wid,
                            Modifier = (int)reader["modifier"],
                            Name = (string)reader["name"],
                            Price = (long)reader["price"],
                            UpgradesStat = (ChickenStatUpgrade)(short)reader["upgrades_stat"]
                        };
                    }
                }
            });

            return upgrade;
        }

        public static async Task<IReadOnlyList<Chicken>> GetStrongestChickensAsync(this DBService db, ulong gid = 0)
        {
            var chickens = new List<Chicken>();

            await db.ExecuteCommandAsync(async (cmd) => {
                if (gid != 0) {
                    cmd.CommandText = "SELECT * FROM gf.chickens WHERE gid = @gid ORDER BY strength DESC;";
                    cmd.Parameters.Add(new NpgsqlParameter("gid", (long)gid));
                } else {
                    cmd.CommandText = "SELECT * FROM gf.chickens ORDER BY strength DESC;";
                }

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                    while (await reader.ReadAsync().ConfigureAwait(false)) {
                        chickens.Add(new Chicken() {
                            Name = (string)reader["name"],
                            OwnerId = (ulong)(long)reader["uid"],
                            Stats = new ChickenStats() {
                                BareMaxVitality = (int)reader["max_vitality"],
                                BareStrength = (int)reader["strength"],
                                BareVitality = (int)reader["vitality"]
                            }
                        });
                    }
                }
            });

            return chickens.AsReadOnly();
        }

        public static async Task<IReadOnlyList<ChickenUpgrade>> GetUpgradesForChickenAsync(this DBService db, ulong uid, ulong gid)
        {
            var upgrades = new List<ChickenUpgrade>();

            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "SELECT gf.chicken_upgrades.wid, modifier, name, price, upgrades_stat FROM gf.chicken_active_upgrades JOIN gf.chicken_upgrades ON gid = @gid AND uid = @uid AND gf.chicken_active_upgrades.wid = gf.chicken_upgrades.wid;";
                cmd.Parameters.Add(new NpgsqlParameter("uid", (long)uid));
                cmd.Parameters.Add(new NpgsqlParameter("gid", (long)gid));

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false)) {
                    while (await reader.ReadAsync().ConfigureAwait(false)) {
                        upgrades.Add(new ChickenUpgrade() {
                            Id = (int)reader["wid"],
                            Modifier = (int)reader["modifier"],
                            Name = (string)reader["name"],
                            Price = (long)reader["price"],
                            UpgradesStat = (ChickenStatUpgrade)(short)reader["upgrades_stat"]
                        });
                    }
                }
            });

            return upgrades.AsReadOnly();
        }

        public static async Task ModifyChickenAsync(this DBService db, Chicken chicken, ulong gid)
        {
            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "UPDATE gf.chickens SET (name, strength, vitality, max_vitality) = (@name, @strength, @vitality, @max_vitality) WHERE uid = @uid AND gid = @gid;";
                cmd.Parameters.Add(new NpgsqlParameter("uid", (long)chicken.OwnerId));
                cmd.Parameters.Add(new NpgsqlParameter("gid", (long)gid));
                cmd.Parameters.Add(new NpgsqlParameter("name", chicken.Name));
                cmd.Parameters.Add(new NpgsqlParameter("strength", chicken.Stats.BareStrength));
                cmd.Parameters.Add(new NpgsqlParameter("vitality", chicken.Stats.BareVitality));
                cmd.Parameters.Add(new NpgsqlParameter("max_vitality", chicken.Stats.BareMaxVitality));

                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            });
        }

        public static async Task RemoveChickenAsync(this DBService db, ulong uid, ulong gid)
        {
            await db.ExecuteCommandAsync(async (cmd) => {
                cmd.CommandText = "DELETE FROM gf.chickens WHERE uid = @uid AND gid = @gid;";
                cmd.Parameters.Add(new NpgsqlParameter("uid", (long)uid));
                cmd.Parameters.Add(new NpgsqlParameter("gid", (long)gid));

                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            });
        }
    }
}
