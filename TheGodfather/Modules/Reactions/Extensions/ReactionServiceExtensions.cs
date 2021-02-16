﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using TheGodfather.Database.Models;
using TheGodfather.Extensions;
using TheGodfather.Modules.Reactions.Services;

namespace TheGodfather.Modules.Reactions.Extensions
{
    public static class ReactionServiceExtensions
    {
        public static Task<int> AddEmojiReactionEAsync(this ReactionsService service, ulong gid, DiscordEmoji emoji, IEnumerable<string> triggers, bool regex)
            => service.AddEmojiReactionAsync(gid, emoji.GetDiscordName(), triggers, regex);

        public static Task<int> RemoveEmojiReactionsEAsync(this ReactionsService service, ulong gid, DiscordEmoji emoji)
            => service.RemoveEmojiReactionsAsync(gid, emoji.GetDiscordName());

        public static async Task HandleEmojiReactions(this ReactionsService service, DiscordClient shard, DiscordMessage msg)
        {
            ulong gid = msg.Channel.GuildId;

            EmojiReaction? er = service.FindMatchingEmojiReactions(gid, msg.Content)
                .Shuffle()
                .FirstOrDefault();

            if (er is null)
                return;

            try {
                var emoji = DiscordEmoji.FromName(shard, er.Response);
                await msg.CreateReactionAsync(emoji);
            } catch (ArgumentException) {
                await service.RemoveEmojiReactionsWhereAsync(gid, r => r.HasSameResponseAs(er));
            }
        }

        public static async Task HandleTextReactions(this ReactionsService service, DiscordMessage msg)
        {
            TextReaction? tr = service.FindMatchingTextReaction(msg.Channel.GuildId, msg.Content);
            if (tr is { } && tr.CanSend())
                await msg.Channel.SendMessageAsync(tr.Response.Replace("%user%", msg.Author.Mention));
        }
    }
}
