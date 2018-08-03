﻿#region USING_DIRECTIVES
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services.Database.Bank;
using TheGodfather.Services.Database.Shop;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using TheGodfather.Services.Database;
#endregion

namespace TheGodfather.Modules.Misc
{
    [Group("shop"), Module(ModuleType.Miscellaneous)]
    [Description("Shop for items using WM credits from your bank account. If invoked without subcommand, lists all available items for purchase.")]
    [Aliases("store")]
    [UsageExamples("!shop")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    [NotBlocked]
    public class ShopModule : TheGodfatherModule
    {

        public ShopModule(DBService db) : base(db: db) { }


        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => ListAsync(ctx);


        #region COMMAND_SHOP_ADD
        [Command("add"), Priority(1)]
        [Module(ModuleType.Miscellaneous)]
        [Description("Add a new item to guild purchasable items list.")]
        [Aliases("+", "a")]
        [UsageExamples("!shop add Barbie 500",
                       "!shop add \"New Barbie\" 500",
                       "!shop add 500 Newest Barbie")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task AddAsync(CommandContext ctx,
                                  [Description("Item price.")] long price,
                                  [RemainingText, Description("Item name.")] string name)
        {
            if (name?.Length >= 60)
                throw new InvalidCommandUsageException("Item name cannot exceed 60 characters");

            if (price <= 0 || price > 100000000000)
                throw new InvalidCommandUsageException("Item price must be positive and cannot exceed 100 billion credits.");

            await Database.AddPurchasableItemAsync(ctx.Guild.Id, name, price)
                .ConfigureAwait(false);
            await InformAsync(ctx, $"Item {Formatter.Bold(name)} ({Formatter.Bold(price.ToString())} credits) successfully added to this guild's shop.")
                .ConfigureAwait(false);
        }

        [Command("add"), Priority(0)]
        public Task AddAsync(CommandContext ctx,
                            [Description("Item name.")] string name,
                            [Description("Item price.")] long price)
            => AddAsync(ctx, price, name);
        #endregion

        #region COMMAND_SHOP_BUY
        [Command("buy"), Module(ModuleType.Miscellaneous)]
        [Description("Purchase an item from this guild's shop.")]
        [Aliases("purchase", "shutupandtakemymoney", "b", "p")]
        [UsageExamples("!shop buy 3")]
        [UsesInteractivity]
        public async Task BuyAsync(CommandContext ctx,
                                  [Description("Item ID.")] int id)
        {
            var item = await Database.GetPurchasableItemAsync(ctx.Guild.Id, id)
                .ConfigureAwait(false);
            if (item == null)
                throw new CommandFailedException("Item with such ID does not exist in this guild's shop!");

            if (await Database.UserHasPurchasedItemAsync(ctx.User.Id, item.Id))
                throw new CommandFailedException("You have already purchased this item!");

            if (!await ctx.WaitForBoolReplyAsync($"Are you sure you want to buy a {Formatter.Bold(item.Name)} for {Formatter.Bold(item.Price.ToString())} credits?").ConfigureAwait(false))
                return;

            if (!await Database.DecreaseBankAccountBalanceAsync(ctx.User.Id, ctx.Guild.Id, item.Price))
                throw new CommandFailedException("You do not have enough money to purchase that item!");

            await Database.AddPurchaseAsync(ctx.User.Id, item.Id)
                .ConfigureAwait(false);
            await InformAsync(ctx, $"{ctx.User.Mention} bought a {Formatter.Bold(item.Name)} for {Formatter.Bold(item.Price.ToString())} credits!", ":moneybag:")
                .ConfigureAwait(false);
        }
        #endregion
        
        #region COMMAND_SHOP_SELL
        [Command("sell"), Module(ModuleType.Miscellaneous)]
        [Description("Sell a purchased item for half the buy price.")]
        [Aliases("return")]
        [UsageExamples("!shop sell 3")]
        [UsesInteractivity]
        public async Task SellAsync(CommandContext ctx,
                                   [Description("Item ID.")] int id)
        {
            var item = await Database.GetPurchasableItemAsync(ctx.Guild.Id, id)
                .ConfigureAwait(false);
            if (item == null)
                throw new CommandFailedException("Item with such ID does not exist in this guild's shop!");

            if (!await Database.UserHasPurchasedItemAsync(ctx.User.Id, item.Id))
                throw new CommandFailedException("You did not purchase this item!");

            long retval = item.Price / 2;
            if (!await ctx.WaitForBoolReplyAsync($"Are you sure you want to sell a {Formatter.Bold(item.Name)} for {Formatter.Bold(retval.ToString())} credits?").ConfigureAwait(false))
                return;

            await Database.IncreaseBankAccountBalanceAsync(ctx.User.Id, ctx.Guild.Id, retval)
                .ConfigureAwait(false);
            await Database.RemovePurchaseAsync(ctx.User.Id, item.Id)
                .ConfigureAwait(false);
            await InformAsync(ctx, $"{ctx.User.Mention} sold a {Formatter.Bold(item.Name)} for {Formatter.Bold(retval.ToString())} credits!", ":moneybag:")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_SHOP_DELETE
        [Command("delete"), Priority(1)]
        [Module(ModuleType.Miscellaneous)]
        [Description("Remove purchasable item from this guild item list. You can remove an item by ID or by name.")]
        [Aliases("-", "remove", "rm", "del")]
        [UsageExamples("!shop delete Barbie",
                       "!shop delete 5",
                       "!shop delete 1 2 3 4 5")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task DeleteAsync(CommandContext ctx,
                                     [Description("ID list of items to remove.")] params int[] ids)
        {
            if (!ids.Any())
                throw new InvalidCommandUsageException("Missing item IDs.");

            await Database.RemovePurchasableItemsAsync(ctx.Guild.Id, ids)
                .ConfigureAwait(false);
            await InformAsync(ctx)
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_SHOP_LIST
        [Command("list"), Module(ModuleType.Miscellaneous)]
        [Description("List all purchasable items for this guild.")]
        [Aliases("ls")]
        [UsageExamples("!shop list")]
        public async Task ListAsync(CommandContext ctx)
        {
            var items = await Database.GetAllPurchasableItemsAsync(ctx.Guild.Id)
                .ConfigureAwait(false);

            if (!items.Any())
                throw new CommandFailedException("No items in shop!");

            await ctx.SendCollectionInPagesAsync(
                $"{ctx.Guild.Name}'s shop:",
                items,
                item => $"{item.Id} | {Formatter.Bold(item.Name)} : {Formatter.Bold(item.Price.ToString())} credits",
                DiscordColor.Azure,
                5
            ).ConfigureAwait(false);
        }
        #endregion
    }
}
