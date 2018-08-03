﻿#region USING_DIRECTIVES
using System;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Games.Common;
using TheGodfather.Services;
using TheGodfather.Services.Common;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using TheGodfather.Services.Database;
#endregion

namespace TheGodfather.Modules.Games
{
    public partial class GamesModule
    {
        [Group("typingrace"), Module(ModuleType.Games)]
        [Description("Start a new typing race!")]
        [Aliases("tr", "trace", "typerace", "typing", "typingr")]
        [UsageExamples("!game typingrace")]
        [NotBlocked]
        public class TypingRaceModule : TheGodfatherModule
        {

            public TypingRaceModule(SharedData shared, DBService db) : base(shared, db) { }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx)
            {
                if (this.Shared.IsEventRunningInChannel(ctx.Channel.Id)) {
                    if (this.Shared.GetEventInChannel(ctx.Channel.Id) is TypingRace)
                        await JoinAsync(ctx).ConfigureAwait(false);
                    else
                        throw new CommandFailedException("Another event is already running in the current channel.");
                    return;
                }

                var game = new TypingRace(ctx.Client.GetInteractivity(), ctx.Channel);
                this.Shared.RegisterEventInChannel(game, ctx.Channel.Id);
                try {
                    await ctx.InformSuccessAsync(StaticDiscordEmoji.Clock1, $"The typing race will start in 30s or when there are 10 participants. Use command {Formatter.InlineCode("game typingrace")} to join the race.")
                        .ConfigureAwait(false);
                    await JoinAsync(ctx)
                        .ConfigureAwait(false);
                    await Task.Delay(TimeSpan.FromSeconds(30))
                        .ConfigureAwait(false);

                    if (game.ParticipantCount > 1) {
                        await ctx.InformSuccessAsync(StaticDiscordEmoji.Clock1, "I will send a random quote in 10s. First one to type it correctly wins. Remember, you can try again, your best result will be remembered.")
                            .ConfigureAwait(false);
                        await Task.Delay(TimeSpan.FromSeconds(10))
                            .ConfigureAwait(false);

                        await game.RunAsync()
                            .ConfigureAwait(false);

                        if (game.Winner != null) {
                            await ctx.InformSuccessAsync(StaticDiscordEmoji.Trophy, $"The winner is {game.Winner?.Mention ?? "<unknown>"}!")
                                .ConfigureAwait(false);
                        }
                    } else {
                        await ctx.InformSuccessAsync(StaticDiscordEmoji.AlarmClock, "Not enough users joined the typing race.")
                            .ConfigureAwait(false);
                    }
                } finally {
                    this.Shared.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }


            #region COMMAND_TYPINGRACE_JOIN
            [Command("join"), Module(ModuleType.Games)]
            [Description("Join an existing typing race game.")]
            [Aliases("+", "compete", "enter", "j")]
            [UsageExamples("!game typingrace join")]
            public async Task JoinAsync(CommandContext ctx)
            {
                if (!(this.Shared.GetEventInChannel(ctx.Channel.Id) is TypingRace game))
                    throw new CommandFailedException("There is no typing race game running in this channel.");

                if (game.Started)
                    throw new CommandFailedException("Race has already started, you can't join it.");

                if (game.ParticipantCount >= 10)
                    throw new CommandFailedException("Race slots are full (max 10 participants), kthxbye.");

                if (!game.AddParticipant(ctx.User))
                    throw new CommandFailedException("You are already participating in the race!");

                await ctx.InformSuccessAsync($"{ctx.User.Mention} joined the typing race.", ":bicyclist:")
                    .ConfigureAwait(false);
            }
            #endregion
        }
    }
}
