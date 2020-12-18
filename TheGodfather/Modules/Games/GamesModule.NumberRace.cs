﻿#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Common;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Games.Common;
using TheGodfather.Modules.Games.Extensions;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Games
{
    public partial class GamesModule
    {
        [Group("numberrace")]
        [Description("Number racing game commands.")]
        [Aliases("nr", "n", "nunchi", "numbers", "numbersrace")]
        public class NumberRaceModule : TheGodfatherServiceModule<ChannelEventService>
        {

            public NumberRaceModule(ChannelEventService service, DbContextBuilder db)
                : base(service, db)
            {

            }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx)
            {
                if (this.Service.IsEventRunningInChannel(ctx.Channel.Id)) {
                    if (this.Service.GetEventInChannel(ctx.Channel.Id) is NumberRace)
                        await this.JoinRaceAsync(ctx);
                    else
                        throw new CommandFailedException("Another event is already running in the current channel.");
                    return;
                }

                var race = new NumberRace(ctx.Client.GetInteractivity(), ctx.Channel);
                this.Service.RegisterEventInChannel(race, ctx.Channel.Id);
                try {
                    await this.InformAsync(ctx, Emojis.Clock1, $"The race will start in 30s or when there are 10 participants. Use command {Formatter.InlineCode("game numberrace")} to join the race.");
                    await this.JoinRaceAsync(ctx);
                    await Task.Delay(TimeSpan.FromSeconds(30));

                    if (race.ParticipantCount > 1) {
                        await race.RunAsync(ctx.Services.GetRequiredService<LocalizationService>());

                        if (race.IsTimeoutReached) {
                            if (race.Winner is null)
                                await this.InformAsync(ctx, Emojis.AlarmClock, "No replies, aborting the game...");
                            else
                                await this.InformAsync(ctx, Emojis.Trophy, $"{race.Winner.Mention} won due to no replies from other users!");
                        } else {
                            await this.InformAsync(ctx, Emojis.Trophy, $"Winner is: {race.Winner.Mention}! GGWP!");
                        }

                        if (!(race.Winner is null))
                            await this.Database.UpdateStatsAsync(race.Winner.Id, s => s.NumberRacesWon++);
                    } else {
                        await this.InformAsync(ctx, Emojis.AlarmClock, "Not enough users joined the race.");
                    }
                } finally {
                    this.Service.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }


            #region COMMAND_NUMBERRACE_JOIN
            [Command("join")]
            [Description("Join an existing number race game.")]
            [Aliases("+", "compete", "j", "enter")]
            public Task JoinRaceAsync(CommandContext ctx)
            {
                if (!this.Service.IsEventRunningInChannel(ctx.Channel.Id, out NumberRace game))
                    throw new CommandFailedException("There is no number race game running in this channel.");

                if (game.Started)
                    throw new CommandFailedException("Race has already started, you can't join it.");

                if (game.ParticipantCount >= 10)
                    throw new CommandFailedException("Race slots are full (max 10 participants), kthxbye.");

                if (!game.AddParticipant(ctx.User))
                    throw new CommandFailedException("You are already participating in the race!");

                return this.InformAsync(ctx, Emojis.Bicyclist, $"{ctx.User.Mention} joined the race.");
            }
            #endregion

            #region COMMAND_NUMBERRACE_RULES
            [Command("rules")]
            [Description("Explain the number race rules.")]
            [Aliases("help", "h", "ruling", "rule")]
            public Task RulesAsync(CommandContext ctx)
            {
                return this.InformAsync(ctx,
                    Emojis.Information,
                    "I will start by typing a number. Users have to count up by 1 from that number. " +
                    "If someone makes a mistake (types an incorrent number, or repeats the same number) " +
                    "they are out of the game. If nobody posts a number 20s after the last number was posted, " +
                    "then the user that posted that number wins the game. The game ends when only one user remains."
                );
            }
            #endregion

            #region COMMAND_NUMBERRACE_STATS
            [Command("stats")]
            [Description("Print the leaderboard for this game.")]
            [Aliases("top", "leaderboard")]
            public async Task StatsAsync(CommandContext ctx)
            {
                IReadOnlyList<GameStats> topStats = await this.Database.GetTopNumberRaceStatsAsync();
                string top = await GameStatsExtensions.BuildStatsStringAsync(ctx.Client, topStats, s => s.BuildNumberRaceStatsString());
                await this.InformAsync(ctx, Emojis.Trophy, $"Top players in Number Race:\n\n{top}");
            }
            #endregion
        }
    }
}