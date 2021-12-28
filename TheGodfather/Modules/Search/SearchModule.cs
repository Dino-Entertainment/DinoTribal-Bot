﻿using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Attributes;
using TheGodfather.Common;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Search.Common;
using TheGodfather.Modules.Search.Services;

namespace TheGodfather.Modules.Search
{
    [Module(ModuleType.Searches), NotBlocked]
    [Cooldown(5, 10, CooldownBucketType.Channel)]
    public sealed class SearchModule : TheGodfatherModule
    {
        #region cat
        [Command("cat")]
        [Aliases("kitty", "kitten")]
        public async Task RandomCatAsync(CommandContext ctx)
        {
            string? url = await PetImagesService.GetRandomCatImageAsync();
            if (url is null)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_image);

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Description = Emojis.Animals.All[1],
                ImageUrl = url,
                Color = this.ModuleColor
            });
        }
        #endregion

        #region catfact
        [Command("catfact")]
        [Aliases("kittyfact", "kittenfact")]
        public async Task RandomCatFactAsync(CommandContext ctx)
        {
            string? fact = await ctx.Services.GetRequiredService<CatFactsService>().GetFactAsync();
            if (fact is null)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_res_none);

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Description = $"{Emojis.Animals.All[1]} {fact}",
                Color = this.ModuleColor
            });
        }
        #endregion

        #region dog
        [Command("dog")]
        [Aliases("doge", "puppy", "pup")]
        public async Task RandomDogAsync(CommandContext ctx)
        {
            string? url = await PetImagesService.GetRandomDogImageAsync();
            if (url is null)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_image);

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Description = DiscordEmoji.FromName(ctx.Client, ":dog:"),
                ImageUrl = url,
                Color = this.ModuleColor
            });
        }
        #endregion

        #region ip
        [Command("ip")]
        [Aliases("ipstack", "geolocation", "iplocation", "iptracker", "iptrack", "trackip", "iplocate", "geoip")]
        public async Task IpAsync(CommandContext ctx,
                                 [Description(TranslationKey.desc_ip)] IPAddress ip)
        {
            IpInfo? info = await IpGeolocationService.GetInfoForIpAsync(ip);
            if (info is null || !info.Success)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_geoloc);

            await ctx.RespondWithLocalizedEmbedAsync(emb => {
                emb.WithTitle(info.Ip);
                emb.WithColor(this.ModuleColor);
                emb.AddLocalizedField(TranslationKey.str_location, $"{info.City}, {info.RegionName} {info.RegionCode}, {info.CountryName} {info.CountryCode}");
                emb.AddLocalizedField(TranslationKey.str_location_exact, $"({info.Latitude} , {info.Longitude})", inline: true);
                emb.AddLocalizedField(TranslationKey.str_isp, info.Isp, inline: true);
                emb.AddLocalizedField(TranslationKey.str_org, info.Organization, inline: true);
                emb.AddLocalizedField(TranslationKey.str_as, info.As, inline: true);
                emb.WithLocalizedFooter(TranslationKey.fmt_powered_by("ip-api"), null);
            });
        }
        #endregion

        #region news
        [Command("news")]
        [Aliases("worldnews")]
        public Task NewsRssAsync(CommandContext ctx,
                                [Description(TranslationKey.desc_topic)] string topic = "world")
        {
            IReadOnlyList<SyndicationItem>? res = NewsService.FetchNews(this.Localization.GetGuildCulture(ctx.Guild.Id), topic);
            if (res is null || !res.Any())
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_news);

            return ctx.RespondWithLocalizedEmbedAsync(emb => {
                emb.WithLocalizedTitle(TranslationKey.fmt_news(Emojis.Globe, topic));
                emb.WithColor(this.ModuleColor);
                var sb = new StringBuilder();
                foreach (SyndicationItem r in res)
                    sb.Append(Emojis.SmallBlueDiamond).Append(' ').AppendLine(Formatter.MaskedUrl(r.Title.Text, r.Links.First().Uri));
                emb.WithDescription(sb.ToString());
            });
        }
        #endregion

        #region quoteoftheday
        [Command("quoteoftheday")]
        [Aliases("qotd", "qod", "quote", "q")]
        public async Task QotdAsync(CommandContext ctx,
                                   [Description(TranslationKey.desc_topic)] string? category = null)
        {
            Quote? quote = await QuoteService.GetQuoteOfTheDayAsync(category);
            if (quote is null)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_quote);

            await ctx.RespondWithLocalizedEmbedAsync(emb => {
                if (string.IsNullOrWhiteSpace(category))
                    emb.WithLocalizedTitle(TranslationKey.str_qotd);
                else
                    emb.WithLocalizedTitle(TranslationKey.str_qotd_cat(category));
                emb.WithColor(this.ModuleColor);
                emb.WithLocalizedDescription(TranslationKey.fmt_qotd(quote.Content, quote.Author));
                emb.WithImageUrl(quote.BackgroundImageUrl);
                emb.WithUrl(quote.Permalink);
                emb.WithLocalizedFooter(TranslationKey.fmt_powered_by("theysaidso.com"), null);
            });
        }
        #endregion
    }
}
