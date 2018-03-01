﻿#region USING_DIRECTIVES
using System;
using System.Threading.Tasks;

using TheGodfather.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Games.Common;
using TheGodfather.Services;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Games
{
    public partial class GamesModule : TheGodfatherBaseModule
    {
        [Group("othello")]
        [Description("Starts an \"Othello\" game. Play a move by writing a pair of numbers from 1 to 10 corresponding to the row and column where you wish to play. You can also specify a time window in which player must submit their move.")]
        [Aliases("reversi", "oth", "rev")]
        [UsageExample("!game othello")]
        [UsageExample("!game othello 10s")]
        public class OthelloModule : TheGodfatherBaseModule
        {

            public OthelloModule(SharedData shared, DBService db) : base(shared, db) { }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("Move time (def. 30s).")] TimeSpan? movetime = null)
            {
                if (Game.RunningInChannel(ctx.Channel.Id))
                    throw new CommandFailedException("Another game is already running in the current channel!");

                await ReplyWithEmbedAsync(ctx, $"Who wants to play Othello with {ctx.User.Username}?", ":question:")
                    .ConfigureAwait(false);
                var opponent = await InteractivityUtil.WaitForGameOpponentAsync(ctx)
                    .ConfigureAwait(false);
                if (opponent == null)
                    return;

                if (movetime?.TotalSeconds > 120 || movetime?.TotalSeconds < 2)
                    throw new InvalidCommandUsageException("Move time must be in range of [2-120] seconds.");

                var othello = new Othello(ctx.Client.GetInteractivity(), ctx.Channel, ctx.User, opponent, movetime);
                Game.RegisterGameInChannel(othello, ctx.Channel.Id);
                try {
                    await othello.RunAsync()
                        .ConfigureAwait(false);
                    
                    if (othello.Winner != null) {
                        if (othello.NoReply == false)
                            await ReplyWithEmbedAsync(ctx, $"The winner is: {othello.Winner.Mention}!", ":trophy:").ConfigureAwait(false);
                        else
                            await ReplyWithEmbedAsync(ctx, $"{othello.Winner.Mention} won due to no replies from opponent!", ":trophy:").ConfigureAwait(false);

                        await Database.UpdateUserStatsAsync(othello.Winner.Id, "othello_won")
                            .ConfigureAwait(false);
                        if (othello.Winner.Id == ctx.User.Id)
                            await Database.UpdateUserStatsAsync(opponent.Id, "othello_lost").ConfigureAwait(false);
                        else
                            await Database.UpdateUserStatsAsync(ctx.User.Id, "othello_lost").ConfigureAwait(false);
                    } else {
                        await ReplyWithEmbedAsync(ctx, "A draw... Pathetic...", ":video_game:")
                            .ConfigureAwait(false);
                    }
                } finally {
                    Game.UnregisterGameInChannel(ctx.Channel.Id);
                }
            }
        }
    }
}