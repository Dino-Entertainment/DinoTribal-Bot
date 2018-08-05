﻿#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.SWAT.Common;
using TheGodfather.Services.Database;
using TheGodfather.Services.Database.Swat;
#endregion

namespace TheGodfather.Modules.SWAT
{
    [Group("swat"), Module(ModuleType.SWAT), NotBlocked]
    [Description("SWAT4 related commands.")]
    [Aliases("s4", "swat4")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public partial class SwatModule : TheGodfatherModule
    {

        public SwatModule(SharedData shared, DBService db) 
            : base(shared, db)
        {
            this.ModuleColor = DiscordColor.Black;
        }


        #region COMMAND_IP
        [Command("ip")]
        [Description("Return IP of the registered server by name.")]
        [Aliases("getip")]
        [UsageExamples("!s4 ip wm")]
        public async Task QueryAsync(CommandContext ctx,
                                    [Description("Registered name.")] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException("Name missing.");

            SwatServer server = await this.Database.GetSwatServerFromDatabaseAsync(name.ToLowerInvariant());
            if (server == null)
                throw new CommandFailedException("Server with such name isn't found in the database.");

            await InformAsync(ctx, $"IP: {Formatter.Bold($"{server.Ip}:{server.JoinPort}")}");
        }
        #endregion

        #region COMMAND_QUERY
        [Command("query")]
        [Description("Return server information.")]
        [Aliases("q", "info", "i")]
        [UsageExamples("!s4 q 109.70.149.158",
                       "!s4 q 109.70.149.158:10480",
                       "!s4 q wm")]
        public async Task QueryAsync(CommandContext ctx,
                                    [Description("Registered name or IP.")] string ip,
                                    [Description("Query port")] int queryport = 10481)
        {
            if (string.IsNullOrWhiteSpace(ip))
                throw new InvalidCommandUsageException("IP missing.");

            if (queryport <= 0 || queryport > 65535)
                throw new InvalidCommandUsageException("Port range invalid (must be in range [1, 65535])!");

            SwatServer server = await this.Database.GetSwatServerAsync(ip, queryport, name: ip.ToLowerInvariant());
            SwatServerInfo info = await SwatServerInfo.QueryIPAsync(server.Ip, server.QueryPort);
            if (info != null)
                await ctx.RespondAsync(embed: info.ToDiscordEmbed(this.ModuleColor));
            else
                await InformFailureAsync(ctx, "No reply from server.");
        }
        #endregion

        #region COMMAND_SETTIMEOUT
        [Command("settimeout")]
        [Description("Set checking timeout.")]
        [UsageExamples("!swat settimeout 500")]
        [RequireOwner]
        public Task SetTimeoutAsync(CommandContext ctx,
                                   [Description("Timeout (in ms).")] int timeout)
        {
            if (timeout < 100 || timeout > 1000)
                throw new InvalidCommandUsageException("Timeout not in valid range [100-1000] ms.");

            SwatServerInfo.CheckTimeout = timeout;
            return InformAsync(ctx, $"Timeout changed to: {Formatter.Bold(timeout.ToString())}", important: false);
        }
        #endregion

        #region COMMAND_SERVERLIST
        [Command("serverlist")]
        [Description("Print the serverlist with current player numbers.")]
        [UsageExamples("!swat serverlist")]
        public async Task ServerlistAsync(CommandContext ctx)
        {
            var em = new DiscordEmbedBuilder() {
                Title = "Servers",
                Color = this.ModuleColor
            };

            IReadOnlyList<SwatServer> servers = await this.Database.GetAllSwatServersAsync();
            if (servers == null || !servers.Any())
                throw new CommandFailedException("No servers found in the database.");

            SwatServerInfo[] infos = await Task.WhenAll(servers.Select(s => SwatServerInfo.QueryIPAsync(s.Ip, s.QueryPort)));
            foreach (SwatServerInfo info in infos.Where(i => i != null).OrderByDescending(i => i.Players))
                em.AddField(info.HostName, $"{Formatter.Bold(info.Players + " / " + info.MaxPlayers)} | {info.Ip}:{info.JoinPort}");

            await ctx.RespondAsync(embed: em.Build());
        }
        #endregion

        #region COMMAND_STARTCHECK
        [Command("startcheck"), UsesInteractivity]
        [Description("Start listening for space on a given server and notifies you when there is space.")]
        [Aliases("checkspace", "spacecheck")]
        [UsageExamples("!s4 startcheck 109.70.149.158",
                       "!s4 startcheck 109.70.149.158:10480",
                       "!swat startcheck wm")]
        public async Task StartCheckAsync(CommandContext ctx,
                                         [Description("Registered name or IP.")] string ip,
                                         [Description("Query port")] int queryport = 10481)
        {
            if (string.IsNullOrWhiteSpace(ip))
                throw new InvalidCommandUsageException("Name/IP missing.");

            if (queryport <= 0 || queryport > 65535)
                throw new InvalidCommandUsageException("Port range invalid (must be in range [1, 65535])!");

            if (this.Shared.SpaceCheckingCTS.ContainsKey(ctx.User.Id))
                throw new CommandFailedException("Already checking space for you!");

            if (this.Shared.SpaceCheckingCTS.Count > 10)
                throw new CommandFailedException("Maximum number of simultanous checks reached (10), please try later!");

            SwatServer server = await this.Database.GetSwatServerAsync(ip, queryport, name: ip.ToLowerInvariant());
            await InformAsync(ctx, $"Starting space listening on {server.Ip}:{server.JoinPort}...", important: false);

            if (!this.Shared.SpaceCheckingCTS.TryAdd(ctx.User.Id, new CancellationTokenSource()))
                throw new ConcurrentOperationException("Failed to register space check task! Please try again.");

            try {
                var t = Task.Run(async () => {
                    while (this.Shared.SpaceCheckingCTS.ContainsKey(ctx.User.Id) && !this.Shared.SpaceCheckingCTS[ctx.User.Id].IsCancellationRequested) {
                        SwatServerInfo info = await SwatServerInfo.QueryIPAsync(server.Ip, server.QueryPort);
                        if (info == null) {
                            if (!await ctx.WaitForBoolReplyAsync("No reply from server. Should I try again?")) {
                                await StopCheckAsync(ctx);
                                throw new OperationCanceledException();
                            }
                        } else if (info.HasSpace) {
                            await InformAsync(ctx, StaticDiscordEmoji.AlarmClock, $"{ctx.User.Mention}, there is space on {Formatter.Bold(info.HostName)}!");
                        }

                        await Task.Delay(TimeSpan.FromSeconds(2));
                    }
                }, 
                this.Shared.SpaceCheckingCTS[ctx.User.Id].Token);
            } catch {
                this.Shared.SpaceCheckingCTS.TryRemove(ctx.User.Id, out _);
            }
        }
        #endregion

        #region COMMAND_STOPCHECK
        [Command("stopcheck")]
        [Description("Stops space checking.")]
        [Aliases("checkstop")]
        [UsageExamples("!swat stopcheck")]
        public Task StopCheckAsync(CommandContext ctx)
        {
            if (!this.Shared.SpaceCheckingCTS.ContainsKey(ctx.User.Id))
                throw new CommandFailedException("You haven't started any space listeners.");

            this.Shared.SpaceCheckingCTS[ctx.User.Id].Cancel();
            this.Shared.SpaceCheckingCTS[ctx.User.Id].Dispose();
            this.Shared.SpaceCheckingCTS.TryRemove(ctx.User.Id, out _);

            return InformAsync(ctx, "Checking stopped.", important: false);
        }
        #endregion
    }
}
