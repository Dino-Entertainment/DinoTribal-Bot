﻿#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Chickens.Common;
using TheGodfather.Services.Database;
using TheGodfather.Services.Database.Bank;
using TheGodfather.Services.Database.Chickens;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.Chickens
{
    [Group("chicken"), Module(ModuleType.Chickens)]
    [Description("Manage your chicken. If invoked without subcommands, prints out your chicken information.")]
    [Aliases("cock", "hen", "chick", "coc", "cc")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    [UsageExamples("!chicken",
                   "!chicken @Someone")]
    [NotBlocked]
    public partial class ChickenModule : TheGodfatherModule
    {

        public ChickenModule(DBService db) : base(db: db) { }


        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("User.")] DiscordUser user = null)
            => InfoAsync(ctx, user);


        #region COMMAND_CHICKEN_FIGHT
        [Command("fight"), Module(ModuleType.Chickens)]
        [Description("Make your chicken and another user's chicken fight eachother!")]
        [Aliases("f", "duel", "attack")]
        [UsageExamples("!chicken duel @Someone")]
        public async Task FightAsync(CommandContext ctx,
                                    [Description("User.")] DiscordUser user)
        {
            if (ChannelEvent.GetEventInChannel(ctx.Channel.Id) is ChickenWar ambush)
                throw new CommandFailedException("There is a chicken war running in this channel. No fights are allowed before the war finishes.");

            if (user.Id == ctx.User.Id)
                throw new CommandFailedException("You can't fight against your own chicken!");

            if (!ctx.Guild.Members.Any(m => m.Id == user.Id))
                throw new CommandFailedException("That user is not a member of this guild so you cannot fight his chicken.");

            var chicken1 = await Database.GetChickenAsync(ctx.User.Id, ctx.Guild.Id)
                .ConfigureAwait(false);
            if (chicken1 != null) {
                if (chicken1.Stats.TotalVitality < 25)
                    throw new CommandFailedException($"{ctx.User.Mention}, your chicken is too weak to start a fight with another chicken! Heal it using {Formatter.InlineCode("chicken heal")} command.");
            } else {
                throw new CommandFailedException("You do not own a chicken!");
            }

            var chicken2 = await Database.GetChickenAsync(user.Id, ctx.Guild.Id)
                .ConfigureAwait(false);
            if (chicken2 == null)
                throw new CommandFailedException("The specified user does not own a chicken!");

            if (Math.Abs(chicken1.Stats.TotalStrength - chicken2.Stats.TotalStrength) > 50)
                throw new CommandFailedException("The bare strength difference is too big (50 max)! Please find a more suitable opponent.");

            string header = $"{Formatter.Bold(chicken1.Name)} ({chicken1.Stats.ToShortString()}) {StaticDiscordEmoji.DuelSwords} {Formatter.Bold(chicken2.Name)} ({chicken2.Stats.ToShortString()}) {StaticDiscordEmoji.Chicken}\n\n";

            var winner = chicken1.Fight(chicken2);
            winner.Owner = winner.OwnerId == ctx.User.Id ? ctx.User : user;
            var loser = winner == chicken1 ? chicken2 : chicken1;
            int gain = winner.DetermineStrengthGain(loser);
            winner.Stats.BareStrength += gain;
            winner.Stats.BareMaxVitality -= gain;
            if (winner.Stats.TotalVitality == 0)
                winner.Stats.BareVitality = 1;
            loser.Stats.BareVitality -= 50;

            await Database.ModifyChickenAsync(winner, ctx.Guild.Id)
                .ConfigureAwait(false);
            if (loser.Stats.TotalVitality > 0)
                await Database.ModifyChickenAsync(loser, ctx.Guild.Id).ConfigureAwait(false);
            else
                await Database.RemoveChickenAsync(loser.OwnerId, ctx.Guild.Id).ConfigureAwait(false);

            await Database.IncreaseBankAccountBalanceAsync(winner.OwnerId, ctx.Guild.Id, gain * 2000)
                .ConfigureAwait(false);

            await ctx.InformSuccessAsync(StaticDiscordEmoji.Chicken,
                header +
                $"{StaticDiscordEmoji.Trophy} Winner: {Formatter.Bold(winner.Name)}\n\n" +
                $"{Formatter.Bold(winner.Name)} gained {Formatter.Bold(gain.ToString())} strength!\n" +
                (loser.Stats.TotalVitality > 0 ? $"{Formatter.Bold(loser.Name)} lost {Formatter.Bold("50")} HP!" : $"{Formatter.Bold(loser.Name)} died in the battle!") +
                $"\n\n{winner.Owner.Mention} won {gain * 200} credits."
            ).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_CHICKEN_FLU
        [Command("flu"), Module(ModuleType.Chickens)]
        [Description("Pay a well-known scientist to create a disease that disintegrates weak chickens.")]
        [Aliases("cancer", "disease", "blackdeath")]
        [UsageExamples("!chicken flu")]
        [UsesInteractivity]
        public async Task FluAsync(CommandContext ctx)
        {
            if (ChannelEvent.GetEventInChannel(ctx.Channel.Id) is ChickenWar ambush)
                throw new CommandFailedException("There is a chicken war running in this channel. No actions are allowed before the war finishes.");

            if (!await ctx.WaitForBoolReplyAsync($"{ctx.User.Mention}, are you sure you want to pay {Formatter.Bold("1000000")} credits to create a disease?"))
                return;

            if (!await Database.DecreaseBankAccountBalanceAsync(ctx.User.Id, ctx.Guild.Id, 1000000).ConfigureAwait(false))
                throw new CommandFailedException($"You do not have enought credits to pay for the disease creation!");

            short threshold = (short)GFRandom.Generator.Next(50, 100);
            await Database.FilterChickensByVitalityAsync(ctx.Guild.Id, threshold)
                .ConfigureAwait(false);

            await ctx.InformSuccessAsync($"The deadly chicken flu killed all chickens with vitality less than or equal to {Formatter.Bold(threshold.ToString())}!")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_CHICKEN_HEAL
        [Command("heal"), Module(ModuleType.Chickens)]
        [Description("Heal your chicken (+100 HP). There is one medicine made each 10 minutes, so you need to grab it before the others do!")]
        [Aliases("+hp", "hp")]
        [Cooldown(1, 300, CooldownBucketType.Guild)]
        [UsageExamples("!chicken heal")]
        public async Task HealAsync(CommandContext ctx)
        {
            if (ChannelEvent.GetEventInChannel(ctx.Channel.Id) is ChickenWar)
                throw new CommandFailedException("There is a chicken war running in this channel. You are not allowed to heal your chicken before the war finishes.");

            var chicken = await Database.GetChickenAsync(ctx.User.Id, ctx.Guild.Id)
                .ConfigureAwait(false);
            if (chicken == null)
                throw new CommandFailedException("You do not own a chicken!");

            chicken.Stats.BareVitality += 100;
            await Database.ModifyChickenAsync(chicken, ctx.Guild.Id)
                .ConfigureAwait(false);

            await ctx.InformSuccessAsync(StaticDiscordEmoji.Chicken, $"{ctx.User.Mention} healed his chicken (+100 to current HP)!")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_CHICKEN_INFO
        [Command("info"), Module(ModuleType.Chickens)]
        [Description("View user's chicken info. If the user is not given, views sender's chicken info.")]
        [Aliases("information", "stats")]
        [UsageExamples("!chicken info @Someone")]
        public async Task InfoAsync(CommandContext ctx,
                                   [Description("User.")] DiscordUser user = null)
        {
            if (user == null)
                user = ctx.User;

            var chicken = await Database.GetChickenAsync(user.Id, ctx.Guild.Id)
                .ConfigureAwait(false);
            if (chicken == null)
                throw new CommandFailedException($"User {user.Mention} does not own a chicken in this guild! Use command {Formatter.InlineCode("chicken buy")} to buy a chicken (1000 credits).");

            await ctx.RespondAsync(embed: chicken.ToDiscordEmbed(user))
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_CHICKEN_RENAME
        [Command("rename"), Module(ModuleType.Chickens)]
        [Description("Rename your chicken.")]
        [Aliases("rn", "name")]
        [UsageExamples("!chicken name New Name")]
        public async Task RenameAsync(CommandContext ctx,
                                     [RemainingText, Description("Chicken name.")] string name = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException("New name for your chicken is missing.");

            if (name.Length < 2 || name.Length > 30)
                throw new InvalidCommandUsageException("Name cannot be shorter than 2 and longer than 30 characters.");

            if (!name.All(c => Char.IsLetterOrDigit(c) || Char.IsWhiteSpace(c)))
                throw new InvalidCommandUsageException("Name cannot contain characters that are not letters or digits.");

            if (ChannelEvent.GetEventInChannel(ctx.Channel.Id) is ChickenWar ambush)
                throw new CommandFailedException("There is a chicken war running in this channel. No renames are allowed before the war finishes.");

            var chicken = await Database.GetChickenAsync(ctx.User.Id, ctx.Guild.Id)
                .ConfigureAwait(false);
            if (chicken == null)
                throw new CommandFailedException("You do not own a chicken!");

            chicken.Name = name;
            await Database.ModifyChickenAsync(chicken, ctx.Guild.Id)
                .ConfigureAwait(false);

            await ctx.InformSuccessAsync(StaticDiscordEmoji.Chicken, $"{ctx.User.Mention} renamed his chicken to {Formatter.Bold(name)}")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_CHICKEN_SELL
        [Command("sell"), Module(ModuleType.Chickens)]
        [Description("Sell your chicken.")]
        [Aliases("s")]
        [UsageExamples("!chicken sell")]
        [UsesInteractivity]
        public async Task SellAsync(CommandContext ctx)
        {
            var chicken = await Database.GetChickenAsync(ctx.User.Id, ctx.Guild.Id)
                .ConfigureAwait(false);
            if (chicken == null)
                throw new CommandFailedException("You do not own a chicken!");

            if (ChannelEvent.GetEventInChannel(ctx.Channel.Id) is ChickenWar ambush)
                throw new CommandFailedException("There is a chicken war running in this channel. No sells are allowed before the war finishes.");

            var price = chicken.SellPrice;
            if (!await ctx.WaitForBoolReplyAsync($"{ctx.User.Mention}, are you sure you want to sell your chicken for {Formatter.Bold(price.ToString())} credits?"))
                return;

            await Database.RemoveChickenAsync(ctx.User.Id, ctx.Guild.Id)
                .ConfigureAwait(false);
            await Database.IncreaseBankAccountBalanceAsync(ctx.User.Id, ctx.Guild.Id, price)
                .ConfigureAwait(false);

            await ctx.InformSuccessAsync(StaticDiscordEmoji.Chicken, $"{ctx.User.Mention} sold {Formatter.Bold(chicken.Name)} for {Formatter.Bold(price.ToString())} credits!")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_CHICKEN_TOP
        [Command("top"), Module(ModuleType.Chickens)]
        [Description("View the list of strongest chickens in the current guild.")]
        [Aliases("best", "strongest")]
        [UsageExamples("!chicken top")]
        public async Task TopAsync(CommandContext ctx)
        {
            var chickens = await Database.GetStrongestChickensAsync(ctx.Guild.Id)
                .ConfigureAwait(false);
            if (chickens == null || !chickens.Any())
                throw new CommandFailedException("No chickens bought in this guild.");

            foreach (var chicken in chickens) {
                try {
                    chicken.Owner = await ctx.Client.GetUserAsync(chicken.OwnerId)
                        .ConfigureAwait(false);
                } catch (Exception e) {
                    Shared.LogProvider.LogException(LogLevel.Warning, e);
                }
            }

            await ctx.SendCollectionInPagesAsync(
                "Strongest bare strength chickens in this guild:",
                chickens,
                c => $"{Formatter.Bold(c.Name)} | {c.Owner.Mention} | {c.Stats.TotalStrength} STR"
            ).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_CHICKEN_TOPGLOBAL
        [Command("topglobal"), Module(ModuleType.Chickens)]
        [Description("View the list of strongest chickens globally.")]
        [Aliases("bestglobally", "globallystrongest", "globaltop", "topg", "gtop")]
        [UsageExamples("!chicken topglobal")]
        public async Task GlobalTopAsync(CommandContext ctx)
        {
            var chickens = await Database.GetStrongestChickensAsync()
                .ConfigureAwait(false);
            if (chickens == null || !chickens.Any())
                throw new CommandFailedException("No chickens bought.");

            foreach (var chicken in chickens) {
                try {
                    chicken.Owner = await ctx.Client.GetUserAsync(chicken.OwnerId)
                        .ConfigureAwait(false);
                } catch (Exception e) {
                    Shared.LogProvider.LogException(LogLevel.Warning, e);
                }
            }

            await ctx.SendCollectionInPagesAsync(
                "Strongest bare strength chickens (globally):",
                chickens,
                c => $"{Formatter.Bold(c.Name)} | {c.Owner.Mention} | {c.Stats.TotalStrength} STR"
            ).ConfigureAwait(false);
        }
        #endregion
    }
}
