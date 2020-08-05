﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using TheGodfather.Common;

namespace TheGodfather.Extensions
{
    internal static class DiscordChannelExtensions
    {
        public static Task<DiscordMessage> EmbedAsync(this DiscordChannel channel, string message, DiscordEmoji? icon = null, DiscordColor? color = null)
        {
            return channel.SendMessageAsync(embed: new DiscordEmbedBuilder {
                Description = $"{icon ?? ""} {message}",
                Color = color ?? DiscordColor.Green
            });
        }

        public static Task<DiscordMessage> InformFailureAsync(this DiscordChannel channel, string message)
        {
            return channel.SendMessageAsync(embed: new DiscordEmbedBuilder {
                Description = $"{Emojis.X} {message}",
                Color = DiscordColor.IndianRed
            });
        }

        public static async Task<IReadOnlyList<DiscordMessage>> GetMessagesFromAsync(this DiscordChannel channel, DiscordMember member, int limit = 1)
        {
            var messages = new List<DiscordMessage>();

            for (int step = 50; messages.Count < limit && step < 400; step *= 2) {
                ulong? lastId = messages.FirstOrDefault()?.Id;
                IReadOnlyList<DiscordMessage> requested;
                if (lastId is null)
                    requested = await channel.GetMessagesAsync(step);
                else
                    requested = await channel.GetMessagesBeforeAsync(messages.FirstOrDefault().Id, step - messages.Count);
                messages.AddRange(requested.Where(m => m.Author.Id == member.Id).Take(limit));
            }

            return messages.AsReadOnly();
        }
    }
}
