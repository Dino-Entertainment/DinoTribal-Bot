﻿#region USING_DIRECTIVES
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using System;
using System.Threading.Tasks;
using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Polls.Common;
using TheGodfather.Services.Database;
#endregion

namespace TheGodfather.Modules.Polls
{
    [Module(ModuleType.Polls), NotBlocked, UsesInteractivity]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public class ReactionsPollModule : TheGodfatherModule
    {

        public ReactionsPollModule(SharedData shared, DBService db)
            : base(shared, db)
        {
            this.ModuleColor = DiscordColor.Orange;
        }


        #region COMMAND_REACTIONSPOLL
        [Command("reactionspoll"), Priority(1)]
        [Description("Starts a poll with reactions in the channel.")]
        [Aliases("rpoll", "pollr", "voter")]
        [UsageExamples("!rpoll :smile: :joy:")]
        public async Task ReactionsPollAsync(CommandContext ctx,
                                            [Description("Time for poll to run.")] TimeSpan timeout,
                                            [RemainingText, Description("Question.")] string question)
        {
            if (string.IsNullOrWhiteSpace(question))
                throw new InvalidCommandUsageException("Poll requires a question.");

            if (this.Shared.IsPollRunningInChannel(ctx.Channel.Id))
                throw new CommandFailedException("Another poll is already running in this channel.");

            if (timeout < TimeSpan.FromSeconds(10) || timeout >= TimeSpan.FromDays(1))
                throw new InvalidCommandUsageException("Poll cannot run for less than 10 seconds or more than 1 day(s).");

            var rpoll = new ReactionsPoll(ctx.Client.GetInteractivity(), ctx.Channel, question);
            if (!this.Shared.RegisterPollInChannel(rpoll, ctx.Channel.Id))
                throw new ConcurrentOperationException("Failed to start the poll. Please try again.");

            try {
                await InformAsync(ctx, StaticDiscordEmoji.Question, "And what will be the possible answers? (separate with a semicolon)");
                var options = await ctx.WaitAndParsePollOptionsAsync();
                if (options.Count < 2 || options.Count > 10)
                    throw new CommandFailedException("Poll must have minimum 2 and maximum 10 options!");
                rpoll.Options = options;

                await rpoll.RunAsync(timeout);
            } finally {
                this.Shared.UnregisterPollInChannel(ctx.Channel.Id);
            }
        }

        [Command("reactionspoll"), Priority(0)]
        public Task ReactionsPollAsync(CommandContext ctx,
                                      [RemainingText, Description("Question.")] string question)
            => ReactionsPollAsync(ctx, TimeSpan.FromMinutes(1), question);
        #endregion
    }
}
