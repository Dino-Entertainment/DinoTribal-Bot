﻿using DSharpPlus;
using TheGodfather.Modules.Search.Common;
using TheGodfather.Services.Common;

namespace TheGodfather.Modules.Search.Extensions;

public static class RedditPostExtensions
{
    public static LocalizedEmbedBuilder WithRedditPost(this LocalizedEmbedBuilder emb, RedditPost msg)
    {
        if (msg.IsLocked)
            emb.WithTitle($"[LOCKED] {msg.Title}");
        else if (msg.IsArchived)
            emb.WithTitle($"[ARCHIVED] {msg.Title}");
        else if (msg.IsNsfw)
            emb.WithTitle($"[NSFW] {msg.Title}");
        else if (msg.IsPinned)
            emb.WithTitle($"[PINNED] {msg.Title}");
        else if (msg.IsSpoiler)
            emb.WithTitle($"[SPOILER] {msg.Title}");
        else
            emb.WithTitle(msg.Title);

        emb.WithDescription(Formatter.Strip(msg.MarkdownText), false);

        if (string.Equals(msg.PostType, "image", StringComparison.InvariantCultureIgnoreCase) && Uri.TryCreate(msg.Url, UriKind.Absolute, out Uri? imageUri))
            emb.WithImageUrl(imageUri);
        else if (Uri.TryCreate(msg.ThumbnailUrl, UriKind.Absolute, out Uri? thumbnailUri))
            emb.WithThumbnail(thumbnailUri);
        emb.WithUrl(msg.Url);

        emb.AddLocalizedField(TranslationKey.str_type, msg.PostType, true, false);
        emb.AddLocalizedField(TranslationKey.str_author, msg.Author, true);
        emb.AddLocalizedField(TranslationKey.str_comments, msg.CommentCount, true);
        emb.AddLocalizedField(TranslationKey.str_upvotes, msg.UpvoteCount, true);
        emb.AddLocalizedField(TranslationKey.str_upvote_ratio, msg.UpvoteRatio, true);
        if (msg.AwardCount > 0)
            emb.AddLocalizedField(TranslationKey.str_awards, msg.AwardCount, true);

        return emb;
    }
}