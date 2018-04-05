﻿#region USING_DIRECTIVES
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Services;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using Imgur.API;
using Imgur.API.Enums;
using Imgur.API.Models;
using Imgur.API.Models.Impl;
#endregion

namespace TheGodfather.Modules.Search
{
    [Group("imgur")]
    [Description("Search imgur. Invoking without subcommand retrieves top ranked images from given subreddit.")]
    [Aliases("img", "im", "i")]
    [UsageExample("!imgur aww")]
    [UsageExample("!imgur 10 aww")]
    [UsageExample("!imgur aww 10")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    [ListeningCheck]
    public class ImgurModule : TheGodfatherServiceModule<ImgurService>
    {

        public ImgurModule(ImgurService imgur) : base(imgur) { }


        [GroupCommand, Priority(1)]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [Description("Number of images to print [1-10].")] int amount,
                                           [RemainingText, Description("Subreddit.")] string sub)
        {
            if (string.IsNullOrWhiteSpace(sub))
                throw new InvalidCommandUsageException("Missing search query.");
            if (amount < 1 || amount > 10)
                throw new CommandFailedException("Number of results must be in range [1-10].");

            var res = await _Service.GetItemsFromSubAsync(
                sub,
                amount,
                SubredditGallerySortOrder.Top,
                TimeWindow.Day
            ).ConfigureAwait(false);

            await PrintImagesAsync(ctx.Channel, res, amount)
                .ConfigureAwait(false);
        }

        [GroupCommand, Priority(0)]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [Description("Subreddit.")] string sub,
                                           [Description("Number of images to print [1-10].")] int n = 1)
            => await ExecuteGroupAsync(ctx, n, sub).ConfigureAwait(false);

        #region COMMAND_IMGUR_LATEST
        [Command("latest"), Priority(1)]
        [Description("Return latest images from given subreddit.")]
        [Aliases("l", "new", "newest")]
        [UsageExample("!imgur latest 5 aww")]
        [UsageExample("!imgur latest aww 5")]
        public async Task LatestAsync(CommandContext ctx,
                                     [Description("Number of images to print [1-10].")] int amount,
                                     [RemainingText, Description("Subreddit.")] string sub)
        {
            if (string.IsNullOrWhiteSpace(sub))
                throw new InvalidCommandUsageException("Missing subreddit.");
            if (amount < 1 || amount > 10)
                throw new CommandFailedException("Number of results must be in range [1-10].");

            var res = await _Service.GetItemsFromSubAsync(
                sub, 
                amount, 
                SubredditGallerySortOrder.Time, 
                TimeWindow.Day
            ).ConfigureAwait(false);

            await PrintImagesAsync(ctx.Channel, res, amount)
                .ConfigureAwait(false);
        }

        [Command("latest"), Priority(0)]
        public async Task LatestAsync(CommandContext ctx,
                                     [Description("Subreddit.")] string sub,
                                     [Description("Number of images to print [1-10].")] int n)
            => await LatestAsync(ctx, n, sub).ConfigureAwait(false);
        #endregion

        #region COMMAND_IMGUR_TOP
        [Command("top"), Priority(3)]
        [Description("Return amount of top rated images in the given subreddit for given timespan.")]
        [Aliases("t")]
        [UsageExample("!imgur top day 10 aww")]
        [UsageExample("!imgur top 10 day aww")]
        [UsageExample("!imgur top 5 aww")]
        [UsageExample("!imgur top day aww")]
        public async Task TopAsync(CommandContext ctx,
                                  [Description("Timespan in which to search (day/week/month/year/all).")] TimeWindow timespan,
                                  [Description("Number of images to print [1-10].")] int amount,
                                  [RemainingText, Description("Subreddit.")] string sub)
        {
            if (string.IsNullOrWhiteSpace(sub))
                throw new InvalidCommandUsageException("Missing subreddit.");
            if (amount < 1 || amount > 10)
                throw new CommandFailedException("Number of results must be in range [1-10].");

            var res = await _Service.GetItemsFromSubAsync(
                sub,
                amount,
                SubredditGallerySortOrder.Time,
                timespan
            ).ConfigureAwait(false);

            await PrintImagesAsync(ctx.Channel, res, amount)
                .ConfigureAwait(false);
        }

        [Command("top"), Priority(2)]
        public async Task TopAsync(CommandContext ctx,
                                  [Description("Timespan in which to search (day/week/month/year/all).")] TimeWindow timespan,
                                  [Description("Subreddit.")] string sub,
                                  [Description("Number of images to print [1-10].")] int amount = 1)
            => await TopAsync(ctx, timespan, amount, sub).ConfigureAwait(false);

        [Command("top"), Priority(1)]
        public async Task TopAsync(CommandContext ctx,
                                  [Description("Number of images to print [1-10].")] int amount,
                                  [Description("Timespan in which to search (day/week/month/year/all).")] TimeWindow timespan,
                                  [RemainingText, Description("Subreddit.")] string sub)
            => await TopAsync(ctx, timespan, amount, sub).ConfigureAwait(false);

        [Command("top"), Priority(0)]
        public async Task TopAsync(CommandContext ctx,
                                  [Description("Number of images to print [1-10].")] int amount,
                                  [RemainingText, Description("Subreddit.")] string sub)
            => await TopAsync(ctx, TimeWindow.Day, amount, sub).ConfigureAwait(false);

        #endregion


        #region HELPER_FUNCTIONS
        private async Task PrintImagesAsync(DiscordChannel channel, IEnumerable<IGalleryItem> results, int num)
        {
            if (!results.Any()) {
                await channel.SendMessageAsync("No results...")
                    .ConfigureAwait(false);
                return;
            }

            try {
                foreach (var im in results) {
                    if (im.GetType().Name == "GalleryImage") {
                        var img = ((GalleryImage)im);
                        if (!channel.IsNSFW && img.Nsfw != null && img.Nsfw == true)
                            throw new CommandFailedException("This is not a NSFW channel!");
                        await channel.SendMessageAsync(img.Link)
                            .ConfigureAwait(false);
                    } else if (im.GetType().Name == "GalleryAlbum") {
                        var img = ((GalleryAlbum)im);
                        if (!channel.IsNSFW && img.Nsfw != null && img.Nsfw == true)
                            throw new CommandFailedException("This is not a NSFW channel!");
                        await channel.SendMessageAsync(img.Link)
                            .ConfigureAwait(false);
                    } else
                        throw new CommandFailedException("Imgur API error.");

                    await Task.Delay(1000)
                        .ConfigureAwait(false);
                }
            } catch (ImgurException e) {
                throw new CommandFailedException("Imgur API error.", e);
            }

            if (results.Count() != num) {
                await channel.SendMessageAsync("These are all of the results returned.")
                    .ConfigureAwait(false);
            }
        }
        #endregion
    }
}
