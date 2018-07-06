﻿#region USING_DIRECTIVES
using System;
using System.Text;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using Humanizer;
#endregion

namespace TheGodfather.Modules.Currency
{
    [Group("bank"), Module(ModuleType.Currency)]
    [Description("Bank account manipulation. If invoked alone, prints out your bank balance. Accounts periodically get a bonus.")]
    [Aliases("$", "$$", "$$$")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    [UsageExample("!bank")]
    [NotBlocked]
    public class BankModule : TheGodfatherBaseModule
    {

        public BankModule(DBService db) : base(db: db) { }


        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("User.")] DiscordUser user = null)
            => GetBalanceAsync(ctx, user);


        #region COMMAND_BANK_BALANCE
        [Command("balance"), Module(ModuleType.Currency)]
        [Description("View account balance for given user. If the user is not given, checks sender's balance.")]
        [Aliases("s", "status", "bal", "money", "credits")]
        [UsageExample("!bank balance @Someone")]
        public async Task GetBalanceAsync(CommandContext ctx,
                                         [Description("User.")] DiscordUser user = null)
        {
            if (user == null)
                user = ctx.User;

            long? balance = await Database.GetUserCreditAmountAsync(user.Id, ctx.Guild.Id)
                .ConfigureAwait(false);

            var emb = new DiscordEmbedBuilder() {
                Title = $"Account status for {user.Username}",
                Color = DiscordColor.Yellow
            };

            if (balance.HasValue) {
                emb.WithDescription($"Credit amount: {Formatter.Bold(balance.Value.ToWords())}");
                emb.AddField("Numeric value", $"{balance.Value:n0}");
            } else {
                emb.WithDescription($"No existing account! Use command {Formatter.InlineCode("bank register")} to open an account.");
            }
            emb.WithFooter("Your money is safe with us - WM Bank", user.AvatarUrl);

            await ctx.RespondAsync(embed: emb.Build())
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_BANK_GRANT
        [Command("grant"), Priority(1)]
        [Module(ModuleType.Currency)]
        [Description("Magically give funds to some user.")]
        [Aliases("give")]
        [UsageExample("!bank grant @Someone 1000")]
        [UsageExample("!bank grant 1000 @Someone")]
        [RequirePriviledgedUser]
        public async Task GrantAsync(CommandContext ctx,
                                    [Description("User.")] DiscordUser user,
                                    [Description("Amount.")] long amount)
        {
            if (amount <= 0 || amount > 10000000000)
                throw new InvalidCommandUsageException($"Invalid amount! Needs to be in range [1 - {1000000000:n0}]");

            if (!await Database.BankContainsUserAsync(user.Id, ctx.Guild.Id).ConfigureAwait(false))
                throw new CommandFailedException("Given user does not have a WM bank account!");

            await Database.GiveCreditsToUserAsync(user.Id, ctx.Guild.Id, amount)
                .ConfigureAwait(false);
            await ctx.RespondWithIconEmbedAsync($"{Formatter.Bold(user.Mention)} won {Formatter.Bold($"{amount:n0}")} credits on the Serbian lottery! (seems legit)", ":moneybag:")
                .ConfigureAwait(false);
        }

        [Command("grant"), Priority(0)]
        public Task GrantAsync(CommandContext ctx,
                              [Description("Amount.")] long amount,
                              [Description("User.")] DiscordUser user)
            => GrantAsync(ctx, user, amount);
        #endregion

        #region COMMAND_BANK_REGISTER
        [Command("register"), Module(ModuleType.Currency)]
        [Description("Create an account for you in WM bank.")]
        [Aliases("r", "signup", "activate")]
        [UsageExample("!bank register")]
        public async Task RegisterAsync(CommandContext ctx)
        {
            if (await Database.BankContainsUserAsync(ctx.User.Id, ctx.Guild.Id).ConfigureAwait(false))
                throw new CommandFailedException("You already own an account in WM bank!");

            await Database.OpenBankAccountForUserAsync(ctx.User.Id, ctx.Guild.Id)
                .ConfigureAwait(false);
            await ctx.RespondWithIconEmbedAsync($"Account opened for you, {ctx.User.Mention}! Since WM bank is so generous, you get 10000 credits for free.", ":moneybag:")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_BANK_TOP
        [Command("top"), Module(ModuleType.Currency)]
        [Description("Print the richest users.")]
        [Aliases("leaderboard", "elite")]
        [UsageExample("!bank top")]
        public async Task GetLeaderboardAsync(CommandContext ctx)
        {
            var top = await Database.GetTenRichestUsersAsync(ctx.Guild.Id)
                .ConfigureAwait(false);

            StringBuilder sb = new StringBuilder();
            foreach (var row in top) {
                try {
                    if (!ulong.TryParse(row["uid"], out ulong uid))
                        continue;
                    var u = await ctx.Client.GetUserAsync(uid)
                        .ConfigureAwait(false);
                    sb.AppendLine($"{Formatter.Bold(u.Username)} : {row["balance"]}");
                } catch (Exception e) {
                    Shared.LogProvider.LogException(LogLevel.Warning, e);
                }
            }

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Title = $"Wealthiest users for guild {ctx.Guild.Name}",
                Description = sb.ToString(),
                Color = DiscordColor.Gold
            }.Build()).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_BANK_TOPGLOBAL
        [Command("topglobal"), Module(ModuleType.Currency)]
        [Description("Print the globally richest users.")]
        [Aliases("globalleaderboard", "globalelite", "gtop", "topg", "globaltop")]
        [UsageExample("!bank gtop")]
        public async Task GetGlobalLeaderboardAsync(CommandContext ctx)
        {
            var top = await Database.GetTenRichestUsersAsync()
                .ConfigureAwait(false);

            StringBuilder sb = new StringBuilder();
            foreach (var row in top) {
                try {
                    if (!ulong.TryParse(row["uid"], out ulong uid))
                        continue;
                    var u = await ctx.Client.GetUserAsync(uid)
                        .ConfigureAwait(false);
                    sb.AppendLine($"{Formatter.Bold(u.Username)} : {row["total_balance"]}");
                } catch (Exception e) {
                    Shared.LogProvider.LogException(LogLevel.Warning, e);
                }
            }

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Title = "Globally wealthiest users:",
                Description = sb.ToString(),
                Color = DiscordColor.Gold
            }.Build()).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_BANK_TRANSFER
        [Command("transfer"), Priority(1)]
        [Module(ModuleType.Currency)]
        [Description("Transfer funds from your account to another one.")]
        [Aliases("lend")]
        [UsageExample("!bank transfer @Someone 40")]
        [UsageExample("!bank transfer 40 @Someone")]
        public async Task TransferCreditsAsync(CommandContext ctx,
                                              [Description("User to send credits to.")] DiscordUser user,
                                              [Description("Amount.")] long amount)
        {
            if (amount <= 0)
                throw new CommandFailedException("The amount must be positive integer.");

            if (user.Id == ctx.User.Id)
                throw new CommandFailedException("You can't transfer funds to yourself.");

            await Database.TransferCreditsAsync(ctx.User.Id, user.Id, ctx.Guild.Id, amount)
                .ConfigureAwait(false);

            await ctx.RespondWithIconEmbedAsync($"Transfer from {Formatter.Bold(ctx.User.Username)} to {Formatter.Bold(user.Username)} is complete.", ":moneybag:")
                .ConfigureAwait(false);
        }

        [Command("transfer"), Priority(0)]
        public Task TransferCreditsAsync(CommandContext ctx,
                                        [Description("Amount.")] long amount,
                                        [Description("User to send credits to.")] DiscordUser user)
            => TransferCreditsAsync(ctx, user, amount);
        #endregion

        #region COMMAND_BANK_UNREGISTER
        [Command("unregister"), Module(ModuleType.Currency)]
        [Description("Delete an account from WM bank.")]
        [Aliases("ur", "signout", "deleteaccount", "delacc", "disable", "deactivate")]
        [UsageExample("!bank unregister @Someone")]
        [RequirePriviledgedUser]
        public async Task UnregisterAsync(CommandContext ctx,
                                         [Description("User whose account to delete.")] DiscordUser user)
        {
            if (!await Database.BankContainsUserAsync(user.Id, ctx.Guild.Id).ConfigureAwait(false))
                throw new CommandFailedException("There is no account registered for that user in WM bank!");

            await Database.CloseBankAccountForUserAsync(user.Id, ctx.Guild.Id)
                .ConfigureAwait(false);
            await ctx.RespondWithIconEmbedAsync()
                .ConfigureAwait(false);
        }
        #endregion
    }
}
