﻿#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Threading.Tasks;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Polls.Common;
using TheGodfather.Services.Database;
#endregion

namespace TheGodfather.Modules.Polls
{
    [Group("vote"), Module(ModuleType.Polls), NotBlocked]
    [Description("Commands for voting in running polls. Group call registers a vote in the current poll for the option you entered.")]
    [Aliases("votefor", "vf")]
    [UsageExamples("!vote 1")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public class VotingModule : TheGodfatherModule
    {

        public VotingModule(SharedData shared, DBService db)
            : base(shared, db)
        {
            this.ModuleColor = DiscordColor.Orange;
        }


        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [Description("Option to vote for.")] int option)
        {
            Poll poll = this.Shared.GetPollInChannel(ctx.Channel.Id);
            if (poll == null || !poll.IsRunning || poll is ReactionsPoll)
                throw new CommandFailedException("There are no polls running in this channel.");

            option--;
            if (!poll.IsValidVote(option))
                throw new CommandFailedException($"Invalid poll option. Valid range: [1, {poll.Options.Count}].");

            if (poll.UserVoted(ctx.User.Id))
                throw new CommandFailedException("You have already voted in this poll!");

            poll.VoteFor(ctx.User.Id, option);

            await InformAsync(ctx, $"{ctx.User.Mention} voted for: {Formatter.Bold(poll.OptionWithId(option))} in poll: {Formatter.Italic($"\"{poll.Question}\"")}", important: false);
        }


        #region COMMAND_CANCEL
        [Command("cancel")]
        [Description("Vote for an option in the current running poll.")]
        [Aliases("c", "reset")]
        [UsageExamples("!vote cancel")]
        public Task CancelAsync(CommandContext ctx)
        {
            Poll poll = this.Shared.GetPollInChannel(ctx.Channel.Id);
            if (poll == null || !poll.IsRunning || poll is ReactionsPoll)
                throw new CommandFailedException("There are no polls running in this channel.");
            
            if (!poll.UserVoted(ctx.User.Id))
                throw new CommandFailedException("You have not voted in this poll!");

            if (!poll.CancelVote(ctx.User.Id))
                throw new CommandFailedException("Failed to cancel your vote!");

            return InformAsync(ctx, "Your vote has been cancelled!", important: false);
        }
        #endregion
    }
}
