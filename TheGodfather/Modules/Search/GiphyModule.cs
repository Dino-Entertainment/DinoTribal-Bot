﻿#region USING_DIRECTIVES
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.Search
{
    [Group("gif"), Module(ModuleType.Searches)]
    [Description("GIPHY commands. If invoked without a subcommand, searches GIPHY with given query.")]
    [Aliases("giphy")]
    [UsageExamples("!gif wat")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    [NotBlocked]
    public class GiphyModule : TheGodfatherServiceModule<GiphyService>
    {

        public GiphyModule(GiphyService giphy) : base(giphy) { }


        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [RemainingText, Description("Query.")] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new InvalidCommandUsageException("Missing search query.");

            var res = await Service.SearchAsync(query)
                .ConfigureAwait(false);

            if (!res.Any()) {
                await ctx.InformFailureAsync("No results...")
                    .ConfigureAwait(false);
                return;
            }

            await ctx.RespondAsync(res.First().Url)
                .ConfigureAwait(false);
        }


        #region COMMAND_GIPHY_RANDOM
        [Command("random"), Module(ModuleType.Searches)]
        [Description("Return a random GIF.")]
        [Aliases("r", "rand", "rnd")]
        [UsageExamples("!gif random")]
        public async Task RandomAsync(CommandContext ctx)
        {
            var res = await Service.GetRandomGifAsync()
                .ConfigureAwait(false);
            await ctx.RespondAsync(res.Url)
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_GIPHY_TRENDING
        [Command("trending"), Module(ModuleType.Searches)]
        [Description("Return an amount of trending GIFs.")]
        [Aliases("t", "tr", "trend")]
        [UsageExamples("!gif trending",
                       "!gif trending 3")]
        public async Task TrendingAsync(CommandContext ctx,
                                       [Description("Number of results (1-10).")] int amount = 5)
        {
            if (amount < 1 || amount > 10)
                throw new CommandFailedException("Number of results must be in range [1-10].");

            var res = await Service.GetTrendingGifsAsync(amount)
                .ConfigureAwait(false);

            var emb = new DiscordEmbedBuilder() {
                Title = "Trending gifs:",
                Color = DiscordColor.Gold
            };

            foreach (var r in res)
                emb.AddField($"{r.Username} (rating: {r.Rating})", r.EmbedUrl);

            await ctx.RespondAsync(embed: emb.Build())
                .ConfigureAwait(false);
        }
        #endregion
    }
}
