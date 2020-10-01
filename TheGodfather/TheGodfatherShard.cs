﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.VoiceNext;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Extensions.Logging;
using TheGodfather.Database;
using TheGodfather.EventListeners;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Services;
using TheGodfather.Services.Common;

namespace TheGodfather
{
    public sealed class TheGodfatherShard
    {
        public int Id { get; }
        public ServiceProvider Services { get; private set; }
        public BotConfig Config { get; private set; }
        public DbContextBuilder Database { get; private set; }
        public DiscordClient Client { get; private set; }
        public CommandsNextExtension CNext { get; private set; }
        public InteractivityExtension Interactivity { get; private set; }
        public VoiceNextExtension Voice { get; private set; }
        public IReadOnlyDictionary<string, Command> Commands { get; private set; }


        // TODO change argument type to ServiceProvider and pass already built provider from main program
        public TheGodfatherShard(int shardId, IServiceCollection services)
        {
            this.Id = shardId;
            this.Services = services.AddShardServices(this).BuildServiceProvider().Initialize();
            this.Database = this.Services.GetService<DbContextBuilder>();
            this.Config = this.Services.GetService<BotConfigService>().CurrentConfiguration;

            this.Commands = new Dictionary<string, Command>();
            this.Client = this.SetupClient();
            this.CNext = this.SetupCommands();
            this.Interactivity = this.SetupInteractivity();
            this.Voice = this.SetupVoice();

            this.UpdateCommandList();
            Listeners.FindAndRegister(this);
        }

        public async Task DisposeAsync()
        {
            if (this.Client is { }) {
                await this.Client.DisconnectAsync();
                this.Client.Dispose();
            }
        }


        public async Task StartAsync()
        {
            if (this.Client is null)
                throw new InvalidOperationException("Shard needs to be initialized before it could be started.");
            await this.Client.ConnectAsync();
        }

        public void UpdateCommandList()
        {
            this.Commands = this.CNext.GetRegisteredCommands()
                .Where(cmd => cmd.Parent is null)
                .SelectMany(cmd => cmd.Aliases.Select(alias => (alias, cmd)).Concat(new[] { (cmd.Name, cmd) }))
                .ToDictionary(tup => tup.Item1, tup => tup.cmd);
        }


        private DiscordClient SetupClient()
        {
            var cfg = new DiscordConfiguration {
                Token = this.Config.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                LargeThreshold = 250,
                ShardCount = this.Config.ShardCount,
                ShardId = this.Id,
                LoggerFactory = new SerilogLoggerFactory(dispose: true),
                Intents = DiscordIntents.All
                       & ~DiscordIntents.GuildMessageTyping
                       & ~DiscordIntents.DirectMessageTyping
            };

            var client = new DiscordClient(cfg);
            client.Ready += (s, e) => {
                Log.Information("Client ready!");
                return Task.CompletedTask;
            };

            return client;
        }

        private CommandsNextExtension SetupCommands()
        {
            CommandsNextExtension cnext = this.Client.UseCommandsNext(new CommandsNextConfiguration {
                CaseSensitive = false,
                EnableDefaultHelp = false,
                EnableDms = true,
                EnableMentionPrefix = true,
                IgnoreExtraArguments = false,
                PrefixResolver = m => {
                    string p = m.Channel.Guild is null
                        ? this.Config.Prefix
                        : this.Services.GetService<GuildConfigService>().GetGuildPrefix(m.Channel.Guild.Id) ?? this.Config.Prefix;
                    return Task.FromResult(m.GetStringPrefixLength(p));
                },
                Services = this.Services
            });

            var assembly = Assembly.GetExecutingAssembly();
            cnext.RegisterCommands(assembly);
            cnext.RegisterConverters(assembly);

            return cnext;
        }

        private InteractivityExtension SetupInteractivity()
        {
            return this.Client.UseInteractivity(new InteractivityConfiguration {
                PaginationBehaviour = PaginationBehaviour.WrapAround,
                PaginationDeletion = PaginationDeletion.DeleteEmojis,
                PaginationEmojis = new PaginationEmojis(),
                PollBehaviour = PollBehaviour.KeepEmojis,
                Timeout = TimeSpan.FromMinutes(1)
            });
        }

        [Obsolete]
        private VoiceNextExtension SetupVoice()
        {
            return this.Client.UseVoiceNext();
        }
    }
}