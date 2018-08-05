﻿#region USING_DIRECTIVES
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Threading.Tasks;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Services;
using TheGodfather.Services.Common;
using TheGodfather.Services.Database;
#endregion

namespace TheGodfather.Modules.Search
{
    [Group("xkcd"), Module(ModuleType.Searches), NotBlocked]
    [Description("Search xkcd. Group call returns random comic or, if an ID is provided, a comic with given ID.")]
    [Aliases("x")]
    [UsageExamples("!xkcd")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public class XkcdModule : TheGodfatherModule
    {

        public XkcdModule(SharedData shared, DBService db)
            : base(shared, db)
        {
            this.ModuleColor = DiscordColor.Blue;
        }


        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("Comic ID.")] int id)
            => ByIdAsync(ctx, id);

        [GroupCommand, Priority(0)]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => RandomAsync(ctx);


        #region COMMAND_XKCD_ID
        [Command("id")]
        [Description("Retrieves comic with given ID from xkcd.")]
        [UsageExamples("!xkcd id 650")]
        public async Task ByIdAsync(CommandContext ctx,
                                 [Description("Comic ID.")] int? id = null)
        {
            XkcdComic comic = await XkcdService.GetComicAsync(id);
            if (comic == null)
                throw new CommandFailedException("Failed to retrieve comic from xkcd.");

            await ctx.RespondAsync(embed: comic.ToDiscordEmbed());
        }
        #endregion

        #region COMMAND_XKCD_LATEST
        [Command("latest")]
        [Description("Retrieves latest comic from xkcd.")]
        [Aliases("fresh", "newest", "l")]
        [UsageExamples("!xkcd latest")]
        public Task LatestAsync(CommandContext ctx)
            => ByIdAsync(ctx);
        #endregion

        #region COMMAND_XKCD_RANDOM
        [Command("random")]
        [Description("Retrieves a random comic.")]
        [Aliases("rnd", "r", "rand")]
        [UsageExamples("!xkcd random")]
        public async Task RandomAsync(CommandContext ctx)
        {
            XkcdComic comic = await XkcdService.GetRandomComicAsync();
            if (comic == null)
                throw new CommandFailedException("Failed to retrieve comic from xkcd.");

            await ctx.RespondAsync(embed: comic.ToDiscordEmbed());
        }
        #endregion
    }
}
