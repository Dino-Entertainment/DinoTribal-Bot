﻿#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Chickens.Common;
using TheGodfather.Modules.Chickens.Extensions;
using TheGodfather.Modules.Currency.Extensions;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Chickens
{
    public partial class ChickenModule
    {
        [Group("buy"), UsesInteractivity]
        [Description("Buy a new chicken in this guild using your credits from WM bank.")]
        [Aliases("b", "shop")]
        [UsageExamples("!chicken buy My Chicken Name")]
        public class BuyModule : TheGodfatherModule
        {

            public BuyModule(SharedData shared, DBService db)
                : base(shared, db)
            {
                this.ModuleColor = DiscordColor.Yellow;
            }


            [GroupCommand]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [RemainingText, Description("Chicken name.")] string name)
                => TryBuyInternalAsync(ctx, ChickenType.Default, name);


            #region COMMAND_CHICKEN_BUY_DEFAULT
            [Command("default")]
            [Description("Buy a chicken of default strength (cheapest).")]
            [Aliases("d", "def")]
            [UsageExamples("!chicken buy default My Chicken Name")]
            public Task DefaultAsync(CommandContext ctx,
                                    [RemainingText, Description("Chicken name.")] string name)
                => TryBuyInternalAsync(ctx, ChickenType.Default, name);
            #endregion

            #region COMMAND_CHICKEN_BUY_WELLFED
            [Command("wellfed")]
            [Description("Buy a well-fed chicken.")]
            [Aliases("wf", "fed")]
            [UsageExamples("!chicken buy wellfed My Chicken Name")]
            public Task WellFedAsync(CommandContext ctx,
                                    [RemainingText, Description("Chicken name.")] string name)
                => TryBuyInternalAsync(ctx, ChickenType.WellFed, name);
            #endregion

            #region COMMAND_CHICKEN_BUY_TRAINED
            [Command("trained")]
            [Description("Buy a trained chicken.")]
            [Aliases("tr", "train")]
            [UsageExamples("!chicken buy trained My Chicken Name")]
            public Task TrainedAsync(CommandContext ctx,
                                    [RemainingText, Description("Chicken name.")] string name)
                => TryBuyInternalAsync(ctx, ChickenType.Trained, name);
            #endregion

            #region COMMAND_CHICKEN_BUY_EMPOWERED
            [Command("steroidempowered")]
            [Description("Buy a steroid-empowered chicken.")]
            [Aliases("steroid", "empowered")]
            [UsageExamples("!chicken buy steroidempowered My Chicken Name")]
            public Task EmpoweredAsync(CommandContext ctx,
                                      [RemainingText, Description("Chicken name.")] string name)
                => TryBuyInternalAsync(ctx, ChickenType.SteroidEmpowered, name);
            #endregion

            #region COMMAND_CHICKEN_BUY_ALIEN
            [Command("alien")]
            [Description("Buy an alien chicken.")]
            [Aliases("a", "extraterrestrial")]
            [UsageExamples("!chicken buy alien My Chicken Name")]
            public Task AlienAsync(CommandContext ctx,
                                  [RemainingText, Description("Chicken name.")] string name)
                => TryBuyInternalAsync(ctx, ChickenType.Alien, name);
            #endregion

            #region COMMAND_CHICKEN_BUY_LIST
            [Command("list")]
            [Description("List all available chicken types.")]
            [Aliases("ls", "view")]
            [UsageExamples("!chicken buy list")]
            public Task ListAsync(CommandContext ctx)
            {
                var emb = new DiscordEmbedBuilder() {
                    Title = "Available chicken types",
                    Color = this.ModuleColor
                };

                emb.AddField($"Default ({Chicken.Price(ChickenType.Default)} credits)", Chicken.StartingStats[ChickenType.Default].ToString());
                emb.AddField($"Well-Fed ({Chicken.Price(ChickenType.WellFed)} credits)", Chicken.StartingStats[ChickenType.WellFed].ToString());
                emb.AddField($"Trained ({Chicken.Price(ChickenType.Trained)} credits)", Chicken.StartingStats[ChickenType.Trained].ToString());
                emb.AddField($"Steroid Empowered ({Chicken.Price(ChickenType.SteroidEmpowered)} credits)", Chicken.StartingStats[ChickenType.SteroidEmpowered].ToString());
                emb.AddField($"Alien ({Chicken.Price(ChickenType.Alien)} credits)", Chicken.StartingStats[ChickenType.Alien].ToString());

                return ctx.RespondAsync(embed: emb.Build());
            }
            #endregion


            #region HELPER_FUNCTIONS
            private async Task TryBuyInternalAsync(CommandContext ctx, ChickenType type, string name)
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new InvalidCommandUsageException("Name for your chicken is missing.");

                if (name.Length < 2 || name.Length > 30)
                    throw new InvalidCommandUsageException("Name cannot be shorter than 2 and longer than 30 characters.");

                if (!name.All(c => Char.IsLetterOrDigit(c) || Char.IsWhiteSpace(c)))
                    throw new InvalidCommandUsageException("Name cannot contain characters that are not letters or digits.");

                if (await this.Database.GetChickenAsync(ctx.User.Id, ctx.Guild.Id) != null)
                    throw new CommandFailedException("You already own a chicken!");

                if (!await ctx.WaitForBoolReplyAsync($"{ctx.User.Mention}, are you sure you want to buy a chicken for {Formatter.Bold(Chicken.Price(type).ToString())} credits?"))
                    return;

                if (!await this.Database.DecreaseBankAccountBalanceAsync(ctx.User.Id, ctx.Guild.Id, Chicken.Price(type)))
                    throw new CommandFailedException($"You do not have enought credits to buy a chicken ({Chicken.Price(type)} needed)!");

                await this.Database.AddChickenAsync(ctx.User.Id, ctx.Guild.Id, name, Chicken.StartingStats[type]);

                await InformAsync(ctx, StaticDiscordEmoji.Chicken, $"{ctx.User.Mention} bought a chicken named {Formatter.Bold(name)}");
            }
            #endregion
        }
    }
}
