﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using TheGodfather.Common;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Modules.Administration.Services;

namespace TheGodfatherTests.Modules.Administration.Services
{
    public sealed class GuildConfigServiceTests : ITheGodfatherServiceTest<GuildConfigService>
    {
        public GuildConfigService Service { get; private set; }

        private readonly ImmutableDictionary<ulong, DatabaseGuildConfig> gcfg;


        public GuildConfigServiceTests()
        {
            this.gcfg = new Dictionary<ulong, DatabaseGuildConfig> {
                { MockData.Ids[0],
                  new DatabaseGuildConfig {
                    GuildId = MockData.Ids[0],
                    AntifloodSettings = new AntifloodSettings {
                        Enabled = true,
                        Action = PunishmentActionType.Kick,
                        Cooldown = 5,
                        Sensitivity = 4
                    },
                    Currency = "sheckels",
                    WelcomeChannelId = MockData.Ids[1],
                    LeaveChannelId = MockData.Ids[1],
                    MuteRoleId = MockData.Ids[2],
                    Prefix = ".",
                    SuggestionsEnabled = false,
                    WelcomeMessage = "Welcome!",
                  }
                },
                { MockData.Ids[1], new DatabaseGuildConfig { GuildId = MockData.Ids[1] } },
                { MockData.Ids[2], new DatabaseGuildConfig { GuildId = MockData.Ids[2] } },
                { MockData.Ids[3], new DatabaseGuildConfig { GuildId = MockData.Ids[3] } },
                { MockData.Ids[4], new DatabaseGuildConfig { GuildId = MockData.Ids[4] } },
            }.ToImmutableDictionary();
        }


        [SetUp]
        public void InitializeService()
        {
            this.Service = new GuildConfigService(BotConfig.Default, TestDatabaseProvider.Database, loadData: false);
        }


        [Test]
        public void IsGuildRegisteredTests()
        {
            foreach (ulong id in MockData.Ids)
                Assert.IsFalse(this.Service.IsGuildRegistered(id));
            Assert.IsFalse(this.Service.IsGuildRegistered(1));
            Assert.IsFalse(this.Service.IsGuildRegistered(MockData.Ids[0] + 1));
            Assert.IsFalse(this.Service.IsGuildRegistered(MockData.Ids[0] - 1));

            TestDatabaseProvider.AlterAndVerify(
                alter: db => this.Service.LoadData(),
                verify: db => {
                    foreach (ulong id in MockData.Ids)
                        Assert.IsTrue(this.Service.IsGuildRegistered(id));
                    Assert.IsFalse(this.Service.IsGuildRegistered(1));
                    Assert.IsFalse(this.Service.IsGuildRegistered(MockData.Ids[0] + 1));
                    Assert.IsFalse(this.Service.IsGuildRegistered(MockData.Ids[0] - 1));
                }
            );
        }

        [Test]
        public void GetCachedConfigTests()
        {
            TestDatabaseProvider.SetupAlterAndVerify(
                setup: db => this.SetMockGuildConfig(db),
                alter: db => this.Service.LoadData(),
                verify: db => {
                    Assert.IsNull(this.Service.GetCachedConfig(1));
                    Assert.IsTrue(this.HaveSamePropertyValues(this.gcfg[MockData.Ids[0]].CachedConfig, this.Service.GetCachedConfig(MockData.Ids[0])));
                    Assert.IsTrue(this.HaveSamePropertyValues(this.gcfg[MockData.Ids[1]].CachedConfig, this.Service.GetCachedConfig(MockData.Ids[1])));
                }
            );
        }

        [Test]
        public void GetGuildPrefixTests()
        {
            TestDatabaseProvider.SetupAlterAndVerify(
                setup: db => this.SetMockGuildConfig(db),
                alter: db => this.Service.LoadData(),
                verify: db => {
                    Assert.AreEqual(this.gcfg[MockData.Ids[0]].Prefix, this.Service.GetGuildPrefix(MockData.Ids[0]));
                    Assert.AreEqual(BotConfig.Default.Prefix, this.Service.GetGuildPrefix(MockData.Ids[1]));
                    Assert.AreEqual(BotConfig.Default.Prefix, this.Service.GetGuildPrefix(MockData.Ids[2]));
                    Assert.AreEqual(BotConfig.Default.Prefix, this.Service.GetGuildPrefix(1));
                }
            );
        }

        [Test]
        public async Task GetConfigAsyncTests()
        {
            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.SetMockGuildConfig(db);
                    return Task.CompletedTask;
                },
                alter: db => {
                    this.Service.LoadData();
                    return Task.CompletedTask;
                }, 
                verify: async db => {
                    Assert.IsTrue(this.HaveSamePropertyValues(
                        this.gcfg[MockData.Ids[0]].CachedConfig, 
                        (await this.Service.GetConfigAsync(MockData.Ids[0])).CachedConfig
                    ));
                    Assert.IsTrue(this.HaveSamePropertyValues(
                        this.gcfg[MockData.Ids[1]].CachedConfig,
                        (await this.Service.GetConfigAsync(MockData.Ids[1])).CachedConfig
                    ));
                    Assert.IsNull(await this.Service.GetConfigAsync(1));
                }
            );
        }

        [Test]
        public async Task ModifyConfigAsyncTests()
        {
            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.SetMockGuildConfig(db);
                    return Task.CompletedTask;
                },
                alter: async db => {
                    this.Service.LoadData();
                    await this.Service.ModifyConfigAsync(MockData.Ids[0], gcfg => gcfg.Prefix = "!!");
                },
                verify: async db => {
                    DatabaseGuildConfig gcfg = await db.GuildConfig.FindAsync((long)MockData.Ids[0]);
                    Assert.AreEqual("!!", gcfg.Prefix);
                    Assert.AreEqual("!!", this.Service.GetCachedConfig(MockData.Ids[0]).Prefix);
                    Assert.AreEqual("!!", (await this.Service.GetConfigAsync(MockData.Ids[0])).Prefix);
                }
            );

            await TestDatabaseProvider.AlterAndVerifyAsync(
                alter: async db => {
                    this.Service.LoadData();
                    await this.Service.ModifyConfigAsync(MockData.Ids[1], gcfg => gcfg.AntispamSettings = new AntispamSettings {
                        Action = PunishmentActionType.TemporaryBan,
                        Enabled = true,
                        Sensitivity = 10
                    });
                },
                verify: async db => {
                    DatabaseGuildConfig gcfg = await db.GuildConfig.FindAsync((long)MockData.Ids[1]);
                    Assert.IsTrue(gcfg.AntispamEnabled);
                    Assert.AreEqual(PunishmentActionType.TemporaryBan, gcfg.AntispamAction);
                    Assert.AreEqual(10, gcfg.AntispamSensitivity);
                }
            );

            await TestDatabaseProvider.AlterAndVerifyAsync(
                alter: async db => {
                    this.Service.LoadData();
                    Assert.IsNull(await this.Service.ModifyConfigAsync(1, null));
                },
                verify: db => Task.CompletedTask
            );
        }

        [Test]
        public async Task RegisterGuildAsyncTests()
        {
            await TestDatabaseProvider.AlterAndVerifyAsync(
                alter: async db => {
                    this.Service.LoadData();
                    Assert.IsNull(await db.GuildConfig.FindAsync(1L));
                    await this.Service.RegisterGuildAsync(1);
                },
                verify: async db => {
                    DatabaseGuildConfig gcfg = await db.GuildConfig.FindAsync(1L);
                    Assert.IsTrue(this.HaveSamePropertyValues(CachedGuildConfig.Default, gcfg.CachedConfig));
                }
            );

            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.SetMockGuildConfig(db);
                    return Task.CompletedTask;
                },
                alter: async db => {
                    this.Service.LoadData();
                    await this.Service.RegisterGuildAsync(MockData.Ids[0]);
                },
                verify: async db => {
                    DatabaseGuildConfig gcfg = await db.GuildConfig.FindAsync((long)MockData.Ids[0]);
                    Assert.IsTrue(this.HaveSamePropertyValues(this.gcfg[MockData.Ids[0]].CachedConfig, gcfg.CachedConfig));
                }
            );
        }

        [Test]
        public async Task UnregisterGuildAsyncTests()
        {
            await TestDatabaseProvider.AlterAndVerifyAsync(
                alter: async db => {
                    this.Service.LoadData();
                    await this.Service.UnregisterGuildAsync(MockData.Ids[0]);
                },
                verify: async db => Assert.IsNull(await db.GuildConfig.FindAsync((long)MockData.Ids[0]))
            );

            await TestDatabaseProvider.AlterAndVerifyAsync(
                alter: async db => {
                    this.Service.LoadData();
                    await this.Service.UnregisterGuildAsync(1);
                },
                verify: db => Task.CompletedTask
            );

            await TestDatabaseProvider.SetupAlterAndVerifyAsync(
                setup: db => {
                    this.SetMockGuildConfig(db);
                    return Task.CompletedTask;
                },
                alter: async db => {
                    this.Service.LoadData();
                    await this.Service.UnregisterGuildAsync(MockData.Ids[0]);
                    await this.Service.RegisterGuildAsync(MockData.Ids[0]);
                },
                verify: async db => {
                    DatabaseGuildConfig gcfg = await db.GuildConfig.FindAsync((long)MockData.Ids[0]);
                    Assert.IsTrue(this.HaveSamePropertyValues(CachedGuildConfig.Default, gcfg.CachedConfig));
                }
            );
        }


        private void SetMockGuildConfig(DatabaseContext db)
        {
            foreach (KeyValuePair<ulong, DatabaseGuildConfig> kvp in this.gcfg) {
                db.Attach(kvp.Value);
                db.GuildConfig.Update(kvp.Value);
            }
        }

        private bool HaveSamePropertyValues(CachedGuildConfig first, CachedGuildConfig second)
        {
            if (first.Currency != second.Currency)
                return false;

            if (first.LoggingEnabled != second.LoggingEnabled || first.LogChannelId != second.LogChannelId)
                return false;

            if (first.Prefix != second.Prefix)
                return false;

            if (first.ReactionResponse != second.ReactionResponse || first.SuggestionsEnabled != second.SuggestionsEnabled)
                return false;

            AntispamSettings as1 = first.AntispamSettings;
            AntispamSettings as2 = second.AntispamSettings;
            if (as1.Action != as2.Action || as1.Enabled != as2.Enabled || as1.Sensitivity != as2.Sensitivity)
                return false;

            LinkfilterSettings ls1 = first.LinkfilterSettings;
            LinkfilterSettings ls2 = second.LinkfilterSettings;
            if (ls1.BlockBooterWebsites != ls2.BlockBooterWebsites || ls1.BlockDiscordInvites != ls2.BlockDiscordInvites ||
                ls1.BlockDisturbingWebsites != ls2.BlockDisturbingWebsites || ls1.BlockIpLoggingWebsites != ls2.BlockIpLoggingWebsites ||
                ls1.BlockUrlShorteners != ls2.BlockUrlShorteners || ls1.Enabled != ls2.Enabled)
                return false;

            RatelimitSettings rs1 = first.RatelimitSettings;
            RatelimitSettings rs2 = second.RatelimitSettings;
            if (rs1.Action != rs2.Action || rs1.Enabled != rs2.Enabled || rs1.Sensitivity != rs2.Sensitivity)
                return false;

            return true;
        }
    }
}
