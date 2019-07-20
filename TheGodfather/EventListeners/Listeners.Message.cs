﻿#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

using Humanizer;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Common.Collections;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
using TheGodfather.EventListeners.Extensions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Modules.Reactions.Common;
using TheGodfather.Modules.Reactions.Services;
using TheGodfather.Services;
#endregion

namespace TheGodfather.EventListeners
{
    internal static partial class Listeners
    {
        [AsyncEventListener(DiscordEventType.MessagesBulkDeleted)]
        public static async Task BulkDeleteEventHandlerAsync(TheGodfatherShard shard, MessageBulkDeleteEventArgs e)
        {
            if (e.Channel.IsPrivate)
                return;

            DiscordChannel logchn = shard.SharedData.GetLogChannelForGuild(e.Channel.Guild);
            if (logchn is null || e.Channel.IsExempted(shard))
                return;

            DiscordEmbedBuilder emb = FormEmbedBuilder(EventOrigin.Message, $"Bulk message deletion occured ({e.Messages.Count} total)", $"In channel {e.Channel.Mention}");
            await logchn.SendMessageAsync(embed: emb.Build());
        }

        [AsyncEventListener(DiscordEventType.MessageCreated)]
        public static async Task MessageCreateEventHandlerAsync(TheGodfatherShard shard, MessageCreateEventArgs e)
        {
            if (e.Author.IsBot || e.Channel.IsPrivate)
                return;

            if (shard.SharedData.BlockedChannels.Contains(e.Channel.Id) || shard.SharedData.BlockedUsers.Contains(e.Author.Id))
                return;

            if (!e.Channel.PermissionsFor(e.Guild.CurrentMember).HasFlag(Permissions.SendMessages))
                return;

            if (!string.IsNullOrWhiteSpace(e.Message?.Content) && !e.Message.Content.StartsWith(shard.SharedData.GetGuildPrefix(e.Guild.Id))) {
                short rank = shard.Services.GetService<UserRanksService>().IncrementMessageCountForUser(e.Author.Id);
                if (rank != 0) {
                    DatabaseGuildRank rankInfo;
                    using (DatabaseContext db = shard.Database.CreateContext())
                        rankInfo = db.GuildRanks.SingleOrDefault(r => r.GuildId == e.Guild.Id && r.Rank == rank);
                    await e.Channel.EmbedAsync($"GG {e.Author.Mention}! You have advanced to level {Formatter.Bold(rank.ToString())} {(rankInfo is null ? "" : $": {Formatter.Italic(rankInfo.Name)}")} !", StaticDiscordEmoji.Medal);
                }
            }
        }

        [AsyncEventListener(DiscordEventType.MessageCreated)]
        public static async Task MessageCreateProtectionHandlerAsync(TheGodfatherShard shard, MessageCreateEventArgs e)
        {
            if (e.Author.IsBot || e.Channel.IsPrivate || string.IsNullOrWhiteSpace(e.Message?.Content))
                return;

            if (shard.SharedData.BlockedChannels.Contains(e.Channel.Id))
                return;

            CachedGuildConfig gcfg = shard.SharedData.GetGuildConfig(e.Guild.Id);
            if (gcfg.RatelimitSettings.Enabled)
                await shard.CNext.Services.GetService<RatelimitService>().HandleNewMessageAsync(e, gcfg.RatelimitSettings);

            if (gcfg.AntispamSettings.Enabled)
                await shard.CNext.Services.GetService<AntispamService>().HandleNewMessageAsync(e, gcfg.AntispamSettings);
        }

        [AsyncEventListener(DiscordEventType.MessageCreated)]
        public static async Task MessageFilterEventHandlerAsync(TheGodfatherShard shard, MessageCreateEventArgs e)
        {
            if (e.Author.IsBot || e.Channel.IsPrivate || string.IsNullOrWhiteSpace(e.Message?.Content))
                return;

            if (shard.SharedData.BlockedChannels.Contains(e.Channel.Id))
                return;

            CachedGuildConfig gcfg = shard.SharedData.GetGuildConfig(e.Guild.Id);
            if (gcfg.LinkfilterSettings.Enabled) {
                if (await shard.CNext.Services.GetService<LinkfilterService>().HandleNewMessageAsync(e, gcfg.LinkfilterSettings))
                    return;
            }

            if (!shard.Services.GetService<FilteringService>().TextContainsFilter(e.Guild.Id, e.Message.Content))
                return;

            if (!e.Channel.PermissionsFor(e.Guild.CurrentMember).HasFlag(Permissions.ManageMessages))
                return;

            await e.Message.DeleteAsync("_gf: Filter hit");
            await e.Channel.SendMessageAsync($"{e.Author.Mention} said: {FormatterExtensions.Spoiler(Formatter.BlockCode(FormatterExtensions.StripMarkdown(e.Message.Content)))}");
        }

        [AsyncEventListener(DiscordEventType.MessageCreated)]
        public static async Task MessageReactionEventHandlerAsync(TheGodfatherShard shard, MessageCreateEventArgs e)
        {
            if (e.Author.IsBot || e.Channel.IsPrivate || string.IsNullOrWhiteSpace(e.Message?.Content))
                return;

            if (shard.SharedData.BlockedChannels.Contains(e.Channel.Id) || shard.SharedData.BlockedUsers.Contains(e.Author.Id))
                return;

            ReactionsService gdata = shard.Services.GetService<ReactionsService>();

            EmojiReaction triggeredEmojiReaction = gdata.FindMatchingEmojiReactions(e.Guild.Id, e.Message.Content)
                .Shuffle()
                .FirstOrDefault();
            
            if (!(triggeredEmojiReaction is null) && e.Channel.PermissionsFor(e.Guild.CurrentMember).HasFlag(Permissions.AddReactions)) {
                try {
                    var emoji = DiscordEmoji.FromName(shard.Client, triggeredEmojiReaction.Response);
                    await e.Message.CreateReactionAsync(emoji);
                } catch (ArgumentException) {
                    using (DatabaseContext db = shard.Database.CreateContext()) {
                        db.EmojiReactions.RemoveRange(db.EmojiReactions.Where(er => er.GuildId == e.Guild.Id && er.Reaction == triggeredEmojiReaction.Response));
                        await db.SaveChangesAsync();
                    }
                }
            }

            TextReaction triggeredTextReaction = gdata.FindMatchingTextReaction(e.Guild.Id, e.Message.Content);
            if (!(triggeredTextReaction is null) && triggeredTextReaction.CanSend())
                await e.Channel.SendMessageAsync(triggeredTextReaction.Response.Replace("%user%", e.Author.Mention));
        }

        [AsyncEventListener(DiscordEventType.MessageDeleted)]
        public static async Task MessageDeleteEventHandlerAsync(TheGodfatherShard shard, MessageDeleteEventArgs e)
        {
            if (e.Channel.IsPrivate || e.Message is null)
                return;

            DiscordChannel logchn = shard.SharedData.GetLogChannelForGuild(e.Guild);
            if (logchn is null || e.Channel.IsExempted(shard))
                return;

            if (e.Message.Author == e.Client.CurrentUser && shard.Services.GetService<ChannelEventService>().IsEventRunningInChannel(e.Channel.Id))
                return;

            DiscordEmbedBuilder emb = FormEmbedBuilder(EventOrigin.Message, "Message deleted");
            emb.AddField("Location", e.Channel.Mention, inline: true);
            emb.AddField("Author", e.Message.Author?.Mention ?? _unknown, inline: true);

            DiscordAuditLogEntry entry = await e.Guild.GetLatestAuditLogEntryAsync(AuditLogActionType.MessageDelete);
            if (!(entry is null) && entry is DiscordAuditLogMessageEntry mentry) {
                DiscordMember member = await e.Guild.GetMemberAsync(mentry.UserResponsible.Id);
                if (member.IsExempted(shard))
                    return;

                emb.AddField("User responsible", mentry.UserResponsible.Mention, inline: true);
                if (!string.IsNullOrWhiteSpace(mentry.Reason))
                    emb.AddField("Reason", mentry.Reason);
                emb.WithFooter(mentry.CreationTimestamp.ToUtcTimestamp(), mentry.UserResponsible.AvatarUrl);
            }

            if (!string.IsNullOrWhiteSpace(e.Message.Content)) {
                emb.AddField("Content", $"{Formatter.BlockCode(string.IsNullOrWhiteSpace(e.Message.Content) ? "<empty content>" : FormatterExtensions.StripMarkdown(e.Message.Content.Truncate(1000)))}");
                if (shard.Services.GetService<FilteringService>().TextContainsFilter(e.Guild.Id, e.Message.Content))
                    emb.WithDescription(Formatter.Italic("Message contained a filter."));
            }
            if (e.Message.Embeds.Any())
                emb.AddField("Embeds", e.Message.Embeds.Count.ToString(), inline: true);
            if (e.Message.Reactions.Any())
                emb.AddField("Reactions", string.Join(" ", e.Message.Reactions.Select(r => r.Emoji.GetDiscordName())), inline: true);
            if (e.Message.Attachments.Any())
                emb.AddField("Attachments", string.Join("\n", e.Message.Attachments.Select(a => a.FileName)), inline: true);
            if (e.Message.CreationTimestamp != null)
                emb.AddField("Message creation time", e.Message.CreationTimestamp.ToUtcTimestamp(), inline: true);

            await logchn.SendMessageAsync(embed: emb.Build());
        }

        [AsyncEventListener(DiscordEventType.MessageUpdated)]
        public static async Task MessageUpdateEventHandlerAsync(TheGodfatherShard shard, MessageUpdateEventArgs e)
        {
            if (e.Author is null || e.Author.IsBot || e.Channel is null || e.Channel.IsPrivate || e.Message is null)
                return;

            if (shard.SharedData.BlockedChannels.Contains(e.Channel.Id))
                return;

            if (e.Message.Author == e.Client.CurrentUser && shard.Services.GetService<ChannelEventService>().IsEventRunningInChannel(e.Channel.Id))
                return;

            if (!(e.Message.Content is null) && shard.Services.GetService<FilteringService>().TextContainsFilter(e.Guild.Id, e.Message.Content)) {
                try {
                    await e.Message.DeleteAsync("_gf: Filter hit after update");
                    await e.Channel.SendMessageAsync($"{e.Author.Mention} said: {FormatterExtensions.Spoiler(Formatter.BlockCode(FormatterExtensions.StripMarkdown(e.Message.Content)))}");
                } catch {

                }
            }

            DiscordChannel logchn = shard.SharedData.GetLogChannelForGuild(e.Guild);
            if (logchn is null || !e.Message.IsEdited || e.Channel.IsExempted(shard))
                return;

            DiscordMember member = await e.Guild.GetMemberAsync(e.Author.Id);
            if (member.IsExempted(shard))
                return;

            string pcontent = string.IsNullOrWhiteSpace(e.MessageBefore?.Content) ? "" : e.MessageBefore.Content.Truncate(700);
            string acontent = string.IsNullOrWhiteSpace(e.Message?.Content) ? "" : e.Message.Content.Truncate(700);
            string ctime = e.Message.CreationTimestamp == null ? _unknown : e.Message.CreationTimestamp.ToUtcTimestamp();
            string etime = e.Message.EditedTimestamp is null ? _unknown : e.Message.EditedTimestamp.Value.ToUtcTimestamp();
            string bextra = $"Embeds: {e.MessageBefore?.Embeds?.Count ?? 0}, Reactions: {e.MessageBefore?.Reactions?.Count ?? 0}, Attachments: {e.MessageBefore?.Attachments?.Count ?? 0}";
            string aextra = $"Embeds: {e.Message.Embeds.Count}, Reactions: {e.Message.Reactions.Count}, Attachments: {e.Message.Attachments.Count}";

            DiscordEmbedBuilder emb = FormEmbedBuilder(EventOrigin.Message, "Message updated");
            emb.WithDescription(Formatter.MaskedUrl("Jump to message", e.Message.JumpLink));
            emb.AddField("Location", e.Channel.Mention, inline: true);
            emb.AddField("Author", e.Message.Author?.Mention ?? _unknown, inline: true);
            emb.AddField("Before update", $"Created {ctime}\n{bextra}\nContent:{Formatter.BlockCode(FormatterExtensions.StripMarkdown(pcontent))}");
            emb.AddField("After update", $"Edited {etime}\n{aextra}\nContent:{Formatter.BlockCode(FormatterExtensions.StripMarkdown(acontent))}");

            await logchn.SendMessageAsync(embed: emb.Build());
        }
    }
}
