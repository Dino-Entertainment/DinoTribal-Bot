﻿#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
#endregion

namespace TheGodfather.Modules.Administration
{
    [Group("message"), Module(ModuleType.Administration)]
    [Description("Commands for manipulating messages.")]
    [Aliases("m", "msg", "msgs", "messages")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    [NotBlocked]
    public class MessageModule : TheGodfatherModule
    {

        public MessageModule()
            : base()
        {
            this.ModuleColor = DiscordColor.Azure; 
        }


        #region COMMAND_MESSAGES_ATTACHMENTS
        [Command("attachments")]
        [Description("View all message attachments. If the message is not provided, uses the last sent message before command invocation.")]
        [Aliases("a", "files", "la")]
        [UsageExamples("!message attachments",
                       "!message attachments 408226948855234561")]
        public async Task ListAttachmentsAsync(CommandContext ctx,
                                              [Description("Message ID.")] ulong id = 0)
        {
            DiscordMessage msg;
            if (id != 0) 
                msg = await ctx.Channel.GetMessageAsync(id);
             else 
                msg = (await ctx.Channel.GetMessagesBeforeAsync(ctx.Channel.LastMessageId, 1))?.FirstOrDefault();

            if (msg == null)
                throw new CommandFailedException("Cannot retrieve the message!");

            var emb = new DiscordEmbedBuilder() {
                Title = "Attachments:",
                Color = DiscordColor.Azure
            };
            foreach (var attachment in msg.Attachments) 
                emb.AddField($"{attachment.FileName} ({attachment.FileSize} bytes)", attachment.Url);

            await ctx.RespondAsync(embed: emb.Build())
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_MESSAGES_DELETE
        [Command("delete")]
        [Description("Deletes the specified amount of most-recent messages from the channel.")]
        [Aliases("-", "prune", "del", "d")]
        [UsageExamples("!messages delete 10",
                       "!messages delete 10 Cleaning spam")]
        [RequirePermissions(Permissions.ManageMessages)]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task DeleteMessagesAsync(CommandContext ctx, 
                                             [Description("Amount.")] int amount = 5,
                                             [RemainingText, Description("Reason.")] string reason = null)
        {
            if (amount <= 0 || amount > 100)
                throw new CommandFailedException("Invalid number of messages to delete (must be in range [1, 100].");

            var msgs = await ctx.Channel.GetMessagesAsync(amount)
                .ConfigureAwait(false);
            if (!msgs.Any())
                throw new CommandFailedException("None of the messages in the given range match your description.");

            await ctx.Channel.DeleteMessagesAsync(msgs, ctx.BuildReasonString(reason))
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_MESSAGES_DELETE_FROM
        [Command("deletefrom"), Priority(1)]
        [Module(ModuleType.Administration)]
        [Description("Deletes given amount of most-recent messages from given user.")]
        [Aliases("-user", "-u", "deluser", "du", "dfu", "delfrom")]
        [UsageExamples("!messages deletefrom @Someone 10 Cleaning spam",
                       "!messages deletefrom 10 @Someone Cleaning spam")]
        [RequirePermissions(Permissions.ManageMessages)]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task DeleteMessagesFromUserAsync(CommandContext ctx, 
                                                     [Description("User.")] DiscordUser user,
                                                     [Description("Amount.")] int amount = 5,
                                                     [RemainingText, Description("Reason.")] string reason = null)
        {
            if (amount <= 0 || amount > 100)
                throw new CommandFailedException("Invalid number of messages to delete (must be in range [1, 100].");

            var msgs = await ctx.Channel.GetMessagesAsync(100)
                .ConfigureAwait(false);
            var del = msgs.Where(m => m.Author.Id == user.Id).Take(amount);
            if (!del.Any())
                throw new CommandFailedException("None of the messages in the given range match your description.");

            await ctx.Channel.DeleteMessagesAsync(del, ctx.BuildReasonString(reason))
                .ConfigureAwait(false);
            await ctx.InformSuccessAsync()
                .ConfigureAwait(false);
        }

        [Command("deletefrom"), Priority(0)]
        public Task DeleteMessagesFromUserAsync(CommandContext ctx,
                                               [Description("Amount.")] int amount,
                                               [Description("User.")] DiscordUser user,
                                               [RemainingText, Description("Reason.")] string reason = null)
            => DeleteMessagesFromUserAsync(ctx, user, amount, reason);
        #endregion

        #region COMMAND_MESSAGES_DELETE_REACTIONS
        [Command("deletereactions")]
        [Description("Deletes all reactions from the given message.")]
        [Aliases("-reactions", "-r", "delreactions", "dr")]
        [UsageExamples("!messages deletereactions 408226948855234561")]
        [RequirePermissions(Permissions.ManageMessages)]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task DeleteReactionsAsync(CommandContext ctx,
                                              [Description("ID.")] ulong id = 0,
                                              [RemainingText, Description("Reason.")] string reason = null)
        {
            DiscordMessage msg;
            if (id != 0)
                msg = await ctx.Channel.GetMessageAsync(id)
                    .ConfigureAwait(false);
            else {
                var _ = await ctx.Channel.GetMessagesBeforeAsync(ctx.Channel.LastMessageId, 1)
                    .ConfigureAwait(false);
                msg = _.First();
            }

            await msg.DeleteAllReactionsAsync(ctx.BuildReasonString(reason))
                .ConfigureAwait(false);
            await ctx.InformSuccessAsync()
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_MESSAGES_DELETE_REGEX
        [Command("deleteregex"), Priority(1)]
        [Module(ModuleType.Administration)]
        [Description("Deletes given amount of most-recent messages that match a given regular expression.")]
        [Aliases("-regex", "-rx", "delregex", "drx")]
        [UsageExamples("!messages deletefrom s+p+a+m+ 10 Cleaning spam",
                       "!messages deletefrom 10 s+p+a+m+ Cleaning spam")]
        [RequirePermissions(Permissions.ManageMessages)]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task DeleteMessagesFromRegexAsync(CommandContext ctx,
                                                      [Description("Pattern (Regex).")] string pattern,
                                                      [Description("Amount.")] int amount = 5,
                                                      [RemainingText, Description("Reason.")] string reason = null)
        {
            if (amount <= 0 || amount > 100)
                throw new CommandFailedException("Invalid number of messages to delete (must be in range [1, 100].");

            Regex regex;
            try {
                regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            } catch (ArgumentException e) {
                throw new CommandFailedException("Pattern parsing error.", e);
            }

            var msgs = await ctx.Channel.GetMessagesBeforeAsync(ctx.Channel.LastMessageId, 100)
                .ConfigureAwait(false);
            var del = msgs.Where(m => regex.IsMatch(m.Content)).Take(amount);
            if (!del.Any())
                throw new CommandFailedException("None of the messages in the given range match your description.");

            await ctx.Channel.DeleteMessagesAsync(del, ctx.BuildReasonString(reason))
                .ConfigureAwait(false);
            await ctx.InformSuccessAsync()
                .ConfigureAwait(false);
        }

        [Command("deleteregex"), Priority(0)]
        public Task DeleteMessagesFromRegexAsync(CommandContext ctx,
                                                [Description("Amount.")] int amount,
                                                [Description("Pattern (Regex).")] string pattern,
                                                [RemainingText, Description("Reason.")] string reason = null)
            => DeleteMessagesFromRegexAsync(ctx, pattern, amount, reason);
        #endregion

        #region COMMAND_MESSAGES_LISTPINNED
        [Command("listpinned")]
        [Description("List pinned messages in this channel.")]
        [Aliases("lp", "listpins", "listpin", "pinned")]
        [UsageExamples("!messages listpinned")]
        public async Task ListPinnedMessagesAsync(CommandContext ctx)
        {
            var pinned = await ctx.Channel.GetPinnedMessagesAsync()
                .ConfigureAwait(false);
            
            if (!pinned.Any()) {
                await ctx.InformSuccessAsync("No pinned messages in this channel")
                    .ConfigureAwait(false);
                return;
            }
            
            await ctx.SendCollectionInPagesAsync(
                "Pinned messages:",
                pinned,
                m => $"({Formatter.InlineCode(m.CreationTimestamp.ToString())}) {Formatter.Bold(m.Author.Username)} : {(string.IsNullOrWhiteSpace(m.Content) ? "<embedded message>" : m.Content)}" , 
                DiscordColor.Cyan,
                5
            ).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_MESSAGES_MODIFY
        [Command("modify")]
        [Description("Modify the given message.")]
        [Aliases("edit", "mod", "e", "m")]
        [UsageExamples("!messages modify 408226948855234561 modified text")]
        [RequirePermissions(Permissions.ManageMessages)]
        public async Task ModifyMessageAsync(CommandContext ctx,
                                            [Description("Message ID.")] ulong id,
                                            [RemainingText, Description("New content.")] string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new CommandFailedException("Missing new message content!");

            var msg = await ctx.Channel.GetMessageAsync(id)
                .ConfigureAwait(false);
            await msg.ModifyAsync(content)
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_MESSAGES_PIN
        [Command("pin")]
        [Description("Pins the message given by ID. If the message is not provided, pins the last sent message before command invocation.")]
        [Aliases("p")]
        [UsageExamples("!messages pin",
                       "!messages pin 408226948855234561")]
        [RequirePermissions(Permissions.ManageMessages)]
        public async Task PinMessageAsync(CommandContext ctx,
                                         [Description("ID.")] ulong id = 0)
        {
            try {
                DiscordMessage msg;
                if (id != 0)
                    msg = await ctx.Channel.GetMessageAsync(id)
                        .ConfigureAwait(false);
                else {
                    var _ = await ctx.Channel.GetMessagesBeforeAsync(ctx.Channel.LastMessageId, 1)
                        .ConfigureAwait(false);
                    msg = _.First();
                }

                await msg.PinAsync()
                    .ConfigureAwait(false);
            } catch (BadRequestException e) {
                throw new CommandFailedException("That message cannot be pinned!", e);
            }
        }
        #endregion

        #region COMMAND_MESSAGES_UNPIN
        [Command("unpin")]
        [Description("Unpins the message at given index (starting from 1). If the index is not given, unpins the most recent one.")]
        [Aliases("up")]
        [UsageExamples("!messages unpin",
                       "!messages unpin 10")]
        [RequirePermissions(Permissions.ManageMessages)]
        public async Task UnpinMessageAsync(CommandContext ctx,
                                           [Description("Index (starting from 1).")] int index = 1)
        {
            var pinned = await ctx.Channel.GetPinnedMessagesAsync()
                .ConfigureAwait(false);

            if (index < 1 || index > pinned.Count)
                throw new CommandFailedException($"Invalid index (must be in range [1-{pinned.Count}]!");

            await pinned.ElementAt(index - 1).UnpinAsync()
                .ConfigureAwait(false);
            await ctx.InformSuccessAsync()
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_MESSAGES_UNPINALL
        [Command("unpinall")]
        [Description("Unpins all pinned messages in this channel.")]
        [Aliases("upa")]
        [UsageExamples("!messages unpinall")]
        [RequirePermissions(Permissions.ManageMessages)]
        public async Task UnpinAllMessagesAsync(CommandContext ctx)
        {
            var pinned = await ctx.Channel.GetPinnedMessagesAsync()
                .ConfigureAwait(false);

            int failed = 0;
            foreach (var m in pinned) {
                try {
                    await m.UnpinAsync()
                        .ConfigureAwait(false);
                } catch {
                    failed++;
                }
            }
            await ctx.InformSuccessAsync(failed > 0 ? $"Failed to unpin {failed} messages!" : "All messages successfully unpinned!")
                .ConfigureAwait(false);
        }
        #endregion
    }
}