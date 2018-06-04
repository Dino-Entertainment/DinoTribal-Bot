﻿#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services;
using TheGodfather.Services.Common;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.Chickens
{
    public partial class ChickenModule
    {
        [Group("upgrades"), Module(ModuleType.Chickens)]
        [Description("Upgrade your chicken with items you can buy using your credits from WM bank. Invoking the group lists all upgrades available.")]
        [Aliases("perks", "upgrade", "u")]
        [UsageExample("!chicken upgrade")]
        public class UpgradeModule : TheGodfatherBaseModule
        {

            public UpgradeModule(DBService db) : base(db: db) { }


            [GroupCommand, Priority(1)]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("ID of the upgrade to buy.")] int id)
            {
                var chicken = await Database.GetChickenInfoAsync(ctx.User.Id, ctx.Guild.Id)
                    .ConfigureAwait(false);
                if (chicken == null)
                    throw new CommandFailedException($"You do not own a chicken in this guild! Use command {Formatter.InlineCode("chicken buy")} to buy a chicken (1000 credits).");

                if (chicken.Stats.Upgrades.Any(u => u.Id == id))
                    throw new CommandFailedException("Your chicken already has that upgrade!");

                var upgrade = await Database.GetChickenUpgradeAsync(id)
                    .ConfigureAwait(false);
                if (upgrade == null)
                    throw new CommandFailedException($"An upgrade with ID {Formatter.InlineCode(id.ToString())} does not exist! Use command {Formatter.InlineCode("chicken upgrades")} to view all available upgrades.");

                if (!await ctx.AskYesNoQuestionAsync($"{ctx.User.Mention}, are you sure you want to buy an upgrade for {Formatter.Bold(upgrade.Price.ToString())} credits?"))
                    return;

                if (!await Database.TakeCreditsFromUserAsync(ctx.User.Id, ctx.Guild.Id, upgrade.Price).ConfigureAwait(false))
                    throw new CommandFailedException($"You do not have enought credits to buy that upgrade!");

                await Database.BuyChickenUpgradeAsync(ctx.User.Id, ctx.Guild.Id, upgrade)
                    .ConfigureAwait(false);

                await ctx.RespondWithIconEmbedAsync(StaticDiscordEmoji.Chicken, $"{ctx.User.Mention} bought upgraded his chicken with {Formatter.Bold(upgrade.Name)} (+{upgrade.Modifier}) {upgrade.UpgradesStat.ToStatString()}!")
                    .ConfigureAwait(false);
            }

            [GroupCommand, Priority(0)]
            public Task ExecuteGroupAsync(CommandContext ctx)
                => ListAsync(ctx);


            #region COMMAND_CHICKEN_UPGRADE_LIST
            [Command("list"), Module(ModuleType.Chickens)]
            [Description("List all available upgrades.")]
            [Aliases("ls", "view")]
            [UsageExample("!chicken upgrade list")]
            public async Task ListAsync(CommandContext ctx)
            {
                var upgrades = await Database.GetAllChickenUpgradesAsync()
                    .ConfigureAwait(false);

                await ctx.SendPaginatedCollectionAsync(
                    "Available chicken upgrades",
                    upgrades,
                    upgrade => $"{upgrade.Id} | {upgrade.Name} | {Formatter.Bold(upgrade.Price.ToString())} | +{upgrade.Modifier} {upgrade.UpgradesStat.ToStatString()}",
                    DiscordColor.Orange
                ).ConfigureAwait(false);
            }
            #endregion
        }
    }
}
