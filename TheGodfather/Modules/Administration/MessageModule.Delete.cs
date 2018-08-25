﻿#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Administration
{
    public partial class MessageModule
    {
        [Group("delete")]
        [Description("Deletes messages from the current channel. Group call deletes given amount of most recent messages.")]
        [Aliases("-", "prune", "del", "d")]
        [UsageExamples("!messages delete 10",
                       "!messages delete 10 Cleaning spam")]
        [RequirePermissions(Permissions.ManageMessages), RequireUserPermissions(Permissions.Administrator)]
        public class MessageDeleteModule : TheGodfatherModule
        {

            public MessageDeleteModule(SharedData shared, DBService db)
                : base(shared, db)
            {
                this.ModuleColor = DiscordColor.Azure;
            }


            [GroupCommand]
            public async Task DeleteMessagesAsync(CommandContext ctx,
                                                 [Description("Amount.")] int amount = 5,
                                                 [RemainingText, Description("Reason.")] string reason = null)
            {
                if (amount < 1 || amount > 10000)
                    throw new CommandFailedException("Invalid number of messages to delete (must be in range [1, 10000].");

                if (amount > 100 && !await ctx.WaitForBoolReplyAsync($"Are you sure you want to delete {Formatter.Bold(amount.ToString())} messages from this channel?"))
                    return;

                IReadOnlyList<DiscordMessage> msgs = await ctx.Channel.GetMessagesAsync(amount);
                if (!msgs.Any())
                    throw new CommandFailedException("None of the messages in the given range match your description.");

                await ctx.Channel.DeleteMessagesAsync(msgs, ctx.BuildReasonString(reason));
            }
            

            #region COMMAND_MESSAGES_DELETE_FROM
            [Command("from"), Priority(1)]
            [Description("Deletes given amount of most recent messages from the given member.")]
            [Aliases("f", "frm")]
            [UsageExamples("!messages delete from @Someone 10 Cleaning spam",
                           "!messages delete from 10 @Someone Cleaning spam")]
            [RequirePermissions(Permissions.ManageMessages), RequireUserPermissions(Permissions.ManageMessages)]
            public async Task DeleteMessagesFromUserAsync(CommandContext ctx,
                                                         [Description("User whose messages to delete.")] DiscordMember member,
                                                         [Description("Message range.")] int amount = 5,
                                                         [RemainingText, Description("Reason.")] string reason = null)
            {
                if (amount <= 0 || amount > 10000)
                    throw new CommandFailedException("Invalid number of messages to delete (must be in range [1, 10000].");

                IReadOnlyList<DiscordMessage> msgs = await ctx.Channel.GetMessagesAsync(amount);

                await ctx.Channel.DeleteMessagesAsync(msgs.Where(m => m.Author.Id == member.Id), ctx.BuildReasonString(reason));
            }

            [Command("from"), Priority(0)]
            public Task DeleteMessagesFromUserAsync(CommandContext ctx,
                                                   [Description("Amount.")] int amount,
                                                   [Description("User.")] DiscordMember member,
                                                   [RemainingText, Description("Reason.")] string reason = null)
                => this.DeleteMessagesFromUserAsync(ctx, member, amount, reason);
            #endregion

            #region COMMAND_MESSAGES_DELETE_REACTIONS
            [Command("reactions")]
            [Description("Deletes all reactions from the given message.")]
            [Aliases("react", "re")]
            [UsageExamples("!messages delete reactions 408226948855234561")]
            [RequirePermissions(Permissions.ManageMessages), RequireUserPermissions(Permissions.Administrator)]
            public async Task DeleteReactionsAsync(CommandContext ctx,
                                                  [Description("ID.")] ulong id = 0,
                                                  [RemainingText, Description("Reason.")] string reason = null)
            {
                DiscordMessage msg;
                if (id != 0)
                    msg = await ctx.Channel.GetMessageAsync(id);
                else
                    msg = (await ctx.Channel.GetMessagesBeforeAsync(ctx.Channel.LastMessageId, 1)).FirstOrDefault();

                if (msg == null)
                    throw new CommandFailedException("Cannot find the specified message.");

                await msg.DeleteAllReactionsAsync(ctx.BuildReasonString(reason));
                await this.InformAsync(ctx);
            }
            #endregion

            #region COMMAND_MESSAGES_DELETE_REGEX
            [Command("regex"), Priority(1)]
            [Description("Deletes given amount of most-recent messages that match a given regular expression withing a given message amount.")]
            [Aliases("r", "rgx", "regexp", "reg")]
            [UsageExamples("!messages delete regex s+p+a+m+ 10 Cleaning spam",
                           "!messages delete regex 10 s+p+a+m+ Cleaning spam")]
            [RequirePermissions(Permissions.ManageMessages), RequireUserPermissions(Permissions.Administrator)]
            public async Task DeleteMessagesFromRegexAsync(CommandContext ctx,
                                                          [Description("Pattern (Regex).")] string pattern,
                                                          [Description("Amount.")] int amount = 100,
                                                          [RemainingText, Description("Reason.")] string reason = null)
            {
                if (amount <= 0 || amount > 100)
                    throw new CommandFailedException("Invalid number of messages to delete (must be in range [1, 100].");

                if (!pattern.TryParseRegex(out Regex regex))
                    throw new CommandFailedException("Regex pattern specified is not valid!");

                IReadOnlyList<DiscordMessage> msgs = await ctx.Channel.GetMessagesBeforeAsync(ctx.Channel.LastMessageId, amount);

                await ctx.Channel.DeleteMessagesAsync(msgs.Where(m => !string.IsNullOrWhiteSpace(m.Content) && regex.IsMatch(m.Content)), ctx.BuildReasonString(reason));
            }

            [Command("regex"), Priority(0)]
            public Task DeleteMessagesFromRegexAsync(CommandContext ctx,
                                                    [Description("Amount.")] int amount,
                                                    [Description("Pattern (Regex).")] string pattern,
                                                    [RemainingText, Description("Reason.")] string reason = null)
                => this.DeleteMessagesFromRegexAsync(ctx, pattern, amount, reason);
            #endregion
        }
    }
}
