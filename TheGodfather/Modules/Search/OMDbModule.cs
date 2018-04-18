﻿#region USING_DIRECTIVES
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Services;
using TheGodfather.Services.Common;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Search
{
    [Group("imdb"), Module(ModuleType.Searches)]
    [Description("Search Open Movie Database.")]
    [Aliases("movies", "series", "serie", "movie", "film", "cinema", "omdb")]
    [UsageExample("!imdb Airplane")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    [ListeningCheck]
    public class OMDbModule : TheGodfatherServiceModule<OMDbService>
    {

        public OMDbModule(OMDbService omdb) : base(omdb) { }


        [GroupCommand, Priority(0)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [RemainingText, Description("Title.")] string title)
            => SearchByTitleAsync(ctx, title);

        #region COMMAND_IMDB_SEARCH
        [Command("search"), Module(ModuleType.Searches)]
        [Description("Searches IMDb for given query and returns paginated results.")]
        [Aliases("s", "find")]
        [UsageExample("!imdb search Kill Bill")]
        public async Task SearchAsync(CommandContext ctx,
                                     [RemainingText, Description("Search query.")] string query)
        {
            var pages = await _Service.GetPaginatedResultsAsync(query)
                .ConfigureAwait(false);

            if (pages == null)
                throw new CommandFailedException("No results found!");

            await ctx.Client.GetInteractivity().SendPaginatedMessage(ctx.Channel, ctx.User, pages)
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_IMDB_TITLE
        [Command("title"), Module(ModuleType.Searches)]
        [Description("Search by title.")]
        [Aliases("t", "name", "n")]
        [UsageExample("!imdb title Airplane")]
        public Task SearchByTitleAsync(CommandContext ctx,
                                      [RemainingText, Description("Title.")] string title)
            => SearchAndSendResultAsync(ctx, OMDbQueryType.Title, title);
        #endregion

        #region COMMAND_IMDB_ID
        [Command("id"), Module(ModuleType.Searches)]
        [Description("Search by IMDb ID.")]
        [UsageExample("!imdb id tt4158110")]
        public Task SearchByIdAsync(CommandContext ctx,
                                   [Description("ID.")] string id)
            => SearchAndSendResultAsync(ctx, OMDbQueryType.Id, id);
        #endregion


        #region HELPER_FUNCTIONS
        private async Task SearchAndSendResultAsync(CommandContext ctx, OMDbQueryType type, string query)
        {
            var page = await _Service.GetSingleResultAsync(type, query)
                .ConfigureAwait(false);

            if (page == null)
                throw new CommandFailedException("No results found!");

            await ctx.Client.GetInteractivity().SendPaginatedMessage(ctx.Channel, ctx.User, new Page[] { page })
                .ConfigureAwait(false);
        }
        #endregion
    }
}
