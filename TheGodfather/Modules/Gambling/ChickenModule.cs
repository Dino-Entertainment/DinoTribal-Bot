﻿#region USING_DIRECTIVES
using System.Text;
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

namespace TheGodfather.Modules.Gambling
{
    [Group("chicken"), Module(ModuleType.Gambling)]
    [Description("Manage your chicken. If invoked without subcommands, prints out your chicken information.")]
    [Aliases("cock", "hen", "chick")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    [UsageExample("!chicken")]
    [UsageExample("!chicken @Someone")]
    [ListeningCheck]
    public class ChickenModule : TheGodfatherBaseModule
    {

        public ChickenModule(DBService db) : base(db: db) { }


        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("User.")] DiscordUser user = null)
            => InfoAsync(ctx, user);


        #region COMMAND_CHICKEN_BUY
        [Command("buy"), Module(ModuleType.Gambling)]
        [Description("Buy a new chicken.")]
        [Aliases("b")]
        [UsageExample("!chicken buy My Chicken Name")]
        public async Task BuyAsync(CommandContext ctx,
                                  [RemainingText, Description("Chicken name.")] string name = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException("Name for your chicken is missing.");

            if (name.Length < 2 || name.Length > 30)
                throw new InvalidCommandUsageException("Name cannot be shorter than 2 and longer than 30 characters.");

            if (await Database.GetChickenInfoAsync(ctx.User.Id).ConfigureAwait(false) != null)
                throw new CommandFailedException("You already own a chicken!");

            if (!await ctx.AskYesNoQuestionAsync($"Are you sure you want to buy a chicken for {Chicken.Price} credits?"))
                return;

            if (!await Database.TakeCreditsFromUserAsync(ctx.User.Id, Chicken.Price).ConfigureAwait(false))
                throw new CommandFailedException($"You do not have enought credits to buy a chicken ({Chicken.Price} needed)!");

            await Database.BuyChickenAsync(ctx.User.Id, name)
                .ConfigureAwait(false);

            await ctx.RespondWithIconEmbedAsync(StaticDiscordEmoji.Chicken, $"{ctx.User.Mention} bought a chicken named {Formatter.Italic(name)}")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_CHICKEN_FIGHT
        [Command("fight"), Module(ModuleType.Gambling)]
        [Description("Make your chicken and another user's chicken fight until death!")]
        [Aliases("f", "duel")]
        [UsageExample("!chicken duel @Someone")]
        public async Task TrainAsync(CommandContext ctx,
                                    [Description("User.")] DiscordUser user)
        {
            var chicken1 = await Database.GetChickenInfoAsync(ctx.User.Id)
                .ConfigureAwait(false);
            var chicken2 = await Database.GetChickenInfoAsync(user.Id)
                .ConfigureAwait(false);
            if (chicken1 == null || chicken2 == null)
                throw new CommandFailedException("One of you does not own a chicken!");

            var winner = chicken1.Fight(chicken2);
            var loser = winner == chicken1 ? chicken2 : chicken1;
            winner.IncreaseStrength();

            await Database.ModifyChickenAsync(winner)
                .ConfigureAwait(false);
            await Database.RemoveChickenAsync(loser.OwnerId)
                .ConfigureAwait(false);

            await ctx.RespondWithIconEmbedAsync(StaticDiscordEmoji.Chicken, $"{Formatter.Bold("Fight results:")}\n\n{StaticDiscordEmoji.Trophy} Winner: {Formatter.Bold(winner.Name)}\n\n{winner.Name} gained strength!\n\n{loser.Name} died in the battle!")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_CHICKEN_INFO
        [Command("info"), Module(ModuleType.Gambling)]
        [Description("View user's chicken info. If the user is not given, views sender's chicken info.")]
        [Aliases("information", "stats")]
        [UsageExample("!chicken info @Someone")]
        public async Task InfoAsync(CommandContext ctx,
                                   [Description("User.")] DiscordUser user = null)
        {
            if (user == null)
                user = ctx.User;

            var chicken = await Database.GetChickenInfoAsync(user.Id)
                .ConfigureAwait(false);
            if (chicken == null)
                throw new CommandFailedException($"User {user.Mention} does not own a chicken! Use command {Formatter.InlineCode("chicken buy")} to buy a chicken (1000 credits).");
            
            await ctx.RespondAsync(embed: chicken.Embed(user))
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_CHICKEN_RENAME
        [Command("rename"), Module(ModuleType.Gambling)]
        [Description("Rename your chicken.")]
        [Aliases("rn", "name")]
        [UsageExample("!chicken name New Name")]
        public async Task RenameAsync(CommandContext ctx,
                                     [RemainingText, Description("Chicken name.")] string name = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException("New name for your chicken is missing.");

            if (name.Length < 2 || name.Length > 30)
                throw new InvalidCommandUsageException("Name cannot be shorter than 2 and longer than 30 characters.");

            var chicken = await Database.GetChickenInfoAsync(ctx.User.Id)
                .ConfigureAwait(false);
            if (chicken == null)
                throw new CommandFailedException("You do not own a chicken!");

            chicken.Name = name;
            await Database.ModifyChickenAsync(chicken)
                .ConfigureAwait(false);

            await ctx.RespondWithIconEmbedAsync(StaticDiscordEmoji.Chicken, $"{ctx.User.Mention} renamed his chicken to {Formatter.Italic(name)}")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_CHICKEN_TRAIN
        [Command("train"), Module(ModuleType.Gambling)]
        [Description("Train your chicken.")]
        [Aliases("tr", "t", "exercise")]
        [UsageExample("!chicken train")]
        public async Task TrainAsync(CommandContext ctx)
        {
            var chicken = await Database.GetChickenInfoAsync(ctx.User.Id)
                .ConfigureAwait(false);
            if (chicken == null)
                throw new CommandFailedException("You do not own a chicken!");

            string result;
            if (GFRandom.Generator.GetBool()) {
                chicken.IncreaseStrength();
                result = $"Your chicken learned alot from the training. New strength: {chicken.Strength}";
            } else {
                chicken.DecreaseStrength();
                result = $"Your chicken got tired and didn't learn anything. New strength: {chicken.Strength}";
            }

            await Database.ModifyChickenAsync(chicken)
                .ConfigureAwait(false);

            await ctx.RespondWithIconEmbedAsync(StaticDiscordEmoji.Chicken, result)
                .ConfigureAwait(false);
        }
        #endregion


        // SO MANY IDEAS WTF IS THIS BRAINSTORM???
        // chicken sell - estimate price
        // chicken stats - strength, agility, hitpoints
        // chicken upgrades - weapons, armor etc
    }
}
