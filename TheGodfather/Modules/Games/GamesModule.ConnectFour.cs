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
        [Group("connect4")]
        [Description("Starts a \"Connect 4\" game. Play a move by writing a number from 1 to 9 corresponding to the column where you wish to insert your piece. You can also specify a time window in which player must submit their move.")]
        [Aliases("connectfour", "chain4", "chainfour", "c4")]
        [UsageExample("!game connect4")]
        [UsageExample("!game connect4 10s")]
        public class ConnectFourModule : TheGodfatherBaseModule
        {

            public ConnectFourModule(SharedData shared, DBService db) : base(shared, db) { }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("Move time (def. 30s).")] TimeSpan? movetime = null)
            {
                if (Game.RunningInChannel(ctx.Channel.Id))
                    throw new CommandFailedException("Another game is already running in the current channel!");

                await ReplyWithEmbedAsync(ctx, $"Who wants to play Connect4 with {ctx.User.Username}?", ":question:")
                    .ConfigureAwait(false);
                var opponent = await InteractivityUtil.WaitForGameOpponentAsync(ctx)
                    .ConfigureAwait(false);
                if (opponent == null)
                    return;

                if (movetime?.TotalSeconds > 120 || movetime?.TotalSeconds < 2)
                    throw new InvalidCommandUsageException("Move time must be in range of [2-120] seconds.");

                var connect4 = new Connect4(ctx.Client.GetInteractivity(), ctx.Channel, ctx.User, opponent, movetime);
                Game.RegisterGameInChannel(connect4, ctx.Channel.Id);
                try {
                    await connect4.RunAsync()
                        .ConfigureAwait(false);

                    if (connect4.Winner != null) {
                        if (connect4.NoReply == false)
                            await ReplyWithEmbedAsync(ctx, $"The winner is: {connect4.Winner.Mention}!", ":trophy:").ConfigureAwait(false);
                        else
                            await ReplyWithEmbedAsync(ctx, $"{connect4.Winner.Mention} won due to no replies from opponent!", ":trophy:").ConfigureAwait(false);
                        
                        await Database.UpdateUserStatsAsync(connect4.Winner.Id, "chain4_won")
                            .ConfigureAwait(false);
                        if (connect4.Winner.Id == ctx.User.Id)
                            await Database.UpdateUserStatsAsync(opponent.Id, "chain4_lost").ConfigureAwait(false);
                        else
                            await Database.UpdateUserStatsAsync(ctx.User.Id, "chain4_lost").ConfigureAwait(false);
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
