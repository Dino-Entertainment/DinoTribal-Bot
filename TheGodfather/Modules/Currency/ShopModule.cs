﻿#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Database;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Currency.Extensions;
using TheGodfather.Modules.Misc.Common;
using TheGodfather.Modules.Misc.Extensions;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Currency
{
    [Group("shop"), Module(ModuleType.Currency), NotBlocked]
    [Description("Shop for items using WM credits from your bank account. If invoked without subcommand, lists all available items for purchase.")]
    [Aliases("store")]
    [UsageExamples("!shop")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public class ShopModule : TheGodfatherModule
    {

        public ShopModule(SharedData shared, DBService db)
            : base(shared, db)
        {
            this.ModuleColor = DiscordColor.SpringGreen;
        }


        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => this.ListAsync(ctx);


        #region COMMAND_SHOP_ADD
        [Command("add"), Priority(1)]
        [Description("Add a new item to guild purchasable items list.")]
        [Aliases("+", "a", "+=", "<", "<<", "additem")]
        [UsageExamples("!shop add Barbie 500",
                       "!shop add \"New Barbie\" 500",
                       "!shop add 500 Newest Barbie")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task AddAsync(CommandContext ctx,
                                  [Description("Item price.")] long price,
                                  [RemainingText, Description("Item name.")] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException("The name  for the item is missing.");

            if (name.Length >= 60)
                throw new InvalidCommandUsageException("Item name cannot exceed 60 characters");

            if (price <1  || price > 100_000_000_000)
                throw new InvalidCommandUsageException($"Item price must be positive and cannot exceed 100 billion {this.Shared.GetGuildConfig(ctx.Guild.Id).Currency ?? "credits"}.");

            await this.Database.AddPurchasableItemAsync(ctx.Guild.Id, name, price);
            await this.InformAsync(ctx, $"Item {Formatter.Bold(name)} ({Formatter.Bold(price.ToString())} {this.Shared.GetGuildConfig(ctx.Guild.Id).Currency ?? "credits"}) successfully added to this guild's shop.", important: false);
        }

        [Command("add"), Priority(0)]
        public Task AddAsync(CommandContext ctx,
                            [Description("Item name.")] string name,
                            [Description("Item price.")] long price)
            => this.AddAsync(ctx, price, name);
        #endregion

        #region COMMAND_SHOP_BUY
        [Command("buy"), UsesInteractivity]
        [Description("Purchase an item from this guild's shop.")]
        [Aliases("purchase", "shutupandtakemymoney", "b", "p")]
        [UsageExamples("!shop buy 3")]
        public async Task BuyAsync(CommandContext ctx,
                                  [Description("Item ID.")] int id)
        {
            PurchasableItem item = await this.Database.GetPurchasableItemAsync(ctx.Guild.Id, id);
            if (item is null)
                throw new CommandFailedException("Item with such ID does not exist in this guild's shop!");

            if (await this.Database.UserHasPurchasedItemAsync(ctx.User.Id, item.Id))
                throw new CommandFailedException("You have already purchased this item!");

            if (!await ctx.WaitForBoolReplyAsync($"Are you sure you want to buy a {Formatter.Bold(item.Name)} for {Formatter.Bold(item.Price.ToString())} {this.Shared.GetGuildConfig(ctx.Guild.Id).Currency ?? "credits"}?"))
                return;

            using (DatabaseContext db = this.DatabaseBuilder.CreateContext()) {
                if (!await db.TryDecreaseBankAccountAsync(ctx.User.Id, ctx.Guild.Id, item.Price))
                    throw new CommandFailedException("You do not have enough money to purchase that item!");
                await db.SaveChangesAsync();
            }

            await this.Database.AddPurchaseAsync(ctx.User.Id, item.Id);
            await this.InformAsync(ctx, StaticDiscordEmoji.MoneyBag, $"{ctx.User.Mention} bought a {Formatter.Bold(item.Name)} for {Formatter.Bold(item.Price.ToString())} {this.Shared.GetGuildConfig(ctx.Guild.Id).Currency ?? "credits"}!", important: false);
        }
        #endregion
        
        #region COMMAND_SHOP_SELL
        [Command("sell"), UsesInteractivity]
        [Description("Sell a purchased item for half the buy price.")]
        [Aliases("return")]
        [UsageExamples("!shop sell 3")]
        public async Task SellAsync(CommandContext ctx,
                                   [Description("Item ID.")] int id)
        {
            PurchasableItem item = await this.Database.GetPurchasableItemAsync(ctx.Guild.Id, id);
            if (item is null)
                throw new CommandFailedException("Item with such ID does not exist in this guild's shop!");

            if (!await this.Database.UserHasPurchasedItemAsync(ctx.User.Id, item.Id))
                throw new CommandFailedException("You did not purchase this item!");

            long retval = item.Price / 2;
            if (!await ctx.WaitForBoolReplyAsync($"Are you sure you want to sell a {Formatter.Bold(item.Name)} for {Formatter.Bold(retval.ToString())} {this.Shared.GetGuildConfig(ctx.Guild.Id).Currency ?? "credits"}?"))
                return;

            using (DatabaseContext db = this.DatabaseBuilder.CreateContext()) {
                await db.ModifyBankAccountAsync(ctx.User.Id, ctx.Guild.Id, v => v + retval);
                await db.SaveChangesAsync();
            }

            await this.Database.RemovePurchaseAsync(ctx.User.Id, item.Id);
            await this.InformAsync(ctx, StaticDiscordEmoji.MoneyBag, $"{ctx.User.Mention} sold a {Formatter.Bold(item.Name)} for {Formatter.Bold(retval.ToString())} {this.Shared.GetGuildConfig(ctx.Guild.Id).Currency ?? "credits"}!", important: false);
        }
        #endregion

        #region COMMAND_SHOP_DELETE
        [Command("delete"), Priority(1)]
        [Description("Remove purchasable item from this guild item list. You can remove an item by ID or by name.")]
        [Aliases("-", "remove", "rm", "del", "-=", ">", ">>")]
        [UsageExamples("!shop delete Barbie",
                       "!shop delete 5",
                       "!shop delete 1 2 3 4 5")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task DeleteAsync(CommandContext ctx,
                                     [Description("ID list of items to remove.")] params int[] ids)
        {
            if (!ids.Any())
                throw new InvalidCommandUsageException("Missing item IDs.");

            await this.Database.RemovePurchasableItemsAsync(ctx.Guild.Id, ids);
            await this.InformAsync(ctx, $"Removed items with the following IDs: {string.Join(", ", ids)}", important: false);
        }
        #endregion

        #region COMMAND_SHOP_LIST
        [Command("list")]
        [Description("List all purchasable items for this guild.")]
        [Aliases("ls")]
        [UsageExamples("!shop list")]
        public async Task ListAsync(CommandContext ctx)
        {
            IReadOnlyList<PurchasableItem> items = await this.Database.GetAllPurchasableItemsAsync(ctx.Guild.Id);
            if (!items.Any())
                throw new CommandFailedException("No items in shop!");

            await ctx.SendCollectionInPagesAsync(
                $"Items for guild {ctx.Guild.Name}",
                items.OrderBy(item => item.Price),
                item => $"{Formatter.InlineCode($"{item.Id:D4}")} | {Formatter.Bold(item.Name)} : {Formatter.Bold(item.Price.ToString())} {this.Shared.GetGuildConfig(ctx.Guild.Id).Currency ?? "credits"}",
                this.ModuleColor,
                5
            ).ConfigureAwait(false);
        }
        #endregion
    }
}
