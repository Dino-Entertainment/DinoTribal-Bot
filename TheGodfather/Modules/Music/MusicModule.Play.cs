﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using TheGodfather.Common;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Music.Common;

namespace TheGodfather.Modules.Music
{
    public sealed partial class MusicModule
    {
        #region music play
        [Command("play"), Priority(1)]
        [Aliases("p", "+", "+=", "add", "a")]
        public async Task PlayAsync(CommandContext ctx,
                                   [Description("desc-audio-url")] Uri uri)
        {
            LavalinkLoadResult tlr = await this.Service.GetTracksAsync(uri);
            IEnumerable<LavalinkTrack> tracks = tlr.Tracks;
            if (tlr.LoadResultType == LavalinkLoadResultType.LoadFailed || !tracks.Any() || this.Player is null)
                throw new CommandFailedException(ctx, "cmd-err-music-none");

            if (this.Player.IsShuffled)
                tracks = this.Service.Shuffle(tracks);

            int trackCount = tracks.Count();
            foreach (LavalinkTrack track in tracks)
                this.Player.Enqueue(new Song(track, ctx.Member));

            DiscordChannel? chn = ctx.Member.VoiceState?.Channel;
            if (chn is null)
                throw new CommandFailedException(ctx, "cmd-err-music-vc");

            await this.Player.CreatePlayerAsync(chn);
            await this.Player.PlayAsync();

            if (trackCount > 1) {
                await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Headphones, "fmt-music-add-many", trackCount);
            } else {
                LavalinkTrack track = tracks.First();
                await ctx.RespondWithLocalizedEmbedAsync(emb => {
                    emb.WithColor(this.ModuleColor);
                    emb.WithLocalizedTitle("fmt-music-add", Emojis.Headphones);
                    emb.WithDescription(Formatter.Bold(Formatter.Sanitize(track.Title)));
                    emb.AddLocalizedTitleField("str-author", track.Author, inline: true);
                    emb.AddLocalizedTitleField("str-duration", track.Length.ToDurationString(), inline: true);
                    emb.WithUrl(track.Uri);
                });
            }
        }

        [Command("play"), Priority(0)]
        public async Task PlayAsync(CommandContext ctx,
                                   [RemainingText, Description("desc-audio-query")] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new InvalidCommandUsageException(ctx, "cmd-err-query");

            // TODO
        }
        #endregion
    }
}
