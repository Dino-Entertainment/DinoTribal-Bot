﻿#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
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
        [Group("train"), UsesInteractivity]
        [Description("Train your chicken using your credits from WM bank.")]
        [Aliases("tr", "t", "exercise")]
        [UsageExamples("!chicken train")]
        public class TrainModule : TheGodfatherModule
        {

            public TrainModule(SharedData shared, DBService db) 
                : base(shared, db)
            {
                this.ModuleColor = DiscordColor.Yellow;
            }


            [GroupCommand]
            public Task ExecuteGroupAsync(CommandContext ctx)
                => this.StrengthAsync(ctx);


            #region COMMAND_CHICKEN_TRAIN_STRENGTH
            [Command("strength")]
            [Description("Train your chicken's strength using your credits from WM bank.")]
            [Aliases("str", "st", "s")]
            [UsageExamples("!chicken train strength")]
            public async Task StrengthAsync(CommandContext ctx)
            {
                if (this.Shared.GetEventInChannel(ctx.Channel.Id) is ChickenWar)
                    throw new CommandFailedException("There is a chicken war running in this channel. No trainings are allowed before the war finishes.");

                string result;

                using (DatabaseContext db = this.DatabaseBuilder.CreateContext()) {
                    DatabaseChicken dbc = db.Chickens.SingleOrDefault(c => c.GuildId == ctx.Guild.Id && c.UserId == ctx.User.Id);
                    var chicken = Chicken.FromDatabaseChicken(dbc);
                    if (chicken is null)
                        throw new CommandFailedException("You do not own a chicken!");

                    if (chicken.Stats.TotalVitality < 25)
                        throw new CommandFailedException($"{ctx.User.Mention}, your chicken is too weak for that action! Heal it using {Formatter.BlockCode("chicken heal")} command.");

                    long price = chicken.TrainStrengthPrice;
                    if (!await ctx.WaitForBoolReplyAsync($"{ctx.User.Mention}, are you sure you want to train your chicken for {Formatter.Bold($"{price:n0}")} {this.Shared.GetGuildConfig(ctx.Guild.Id).Currency ?? "credits"}?\n\nNote: This action will also weaken the vitality of your chicken by 1."))
                        return;

                    if (!await this.Database.DecreaseBankAccountBalanceAsync(ctx.User.Id, ctx.Guild.Id, price))
                        throw new CommandFailedException($"You do not have enough {this.Shared.GetGuildConfig(ctx.Guild.Id).Currency ?? "credits"} to train a chicken ({price:n0} needed)!");

                    if (chicken.TrainStrength())
                        result = $"{ctx.User.Mention}'s chicken learned alot from the training. New strength: {chicken.Stats.TotalStrength}";
                    else
                        result = $"{ctx.User.Mention}'s chicken got tired and didn't learn anything. New strength: {chicken.Stats.TotalStrength}";
                    chicken.Stats.BareVitality--;

                    dbc.Strength = chicken.Stats.BareStrength;
                    dbc.Vitality--;
                    db.Chickens.Update(dbc);
                    await db.SaveChangesAsync();
                }

                await this.InformAsync(ctx, StaticDiscordEmoji.Chicken, result);
            }
            #endregion

            #region COMMAND_CHICKEN_TRAIN_VITALITY
            [Command("vitality")]
            [Description("Train your chicken's vitality using your credits from WM bank.")]
            [Aliases("vit", "vi", "v")]
            [UsageExamples("!chicken train vitality")]
            public async Task VitalityAsync(CommandContext ctx)
            {
                if (this.Shared.GetEventInChannel(ctx.Channel.Id) is ChickenWar)
                    throw new CommandFailedException("There is a chicken war running in this channel. No trainings are allowed before the war finishes.");

                string result;

                using (DatabaseContext db = this.DatabaseBuilder.CreateContext()) {
                    DatabaseChicken dbc = db.Chickens.SingleOrDefault(c => c.GuildId == ctx.Guild.Id && c.UserId == ctx.User.Id);
                    var chicken = Chicken.FromDatabaseChicken(dbc);
                    if (chicken is null)
                        throw new CommandFailedException("You do not own a chicken!");

                    if (chicken.Stats.TotalVitality < 25)
                        throw new CommandFailedException($"{ctx.User.Mention}, your chicken is too weak for that action! Heal it using {Formatter.BlockCode("chicken heal")} command.");

                    long price = chicken.TrainVitalityPrice;
                    if (!await ctx.WaitForBoolReplyAsync($"{ctx.User.Mention}, are you sure you want to train your chicken for {Formatter.Bold($"{price:n0}")} {this.Shared.GetGuildConfig(ctx.Guild.Id).Currency ?? "credits"}?\n\nNote: This action will also weaken the vitality of your chicken by 1."))
                        return;

                    if (!await this.Database.DecreaseBankAccountBalanceAsync(ctx.User.Id, ctx.Guild.Id, price))
                        throw new CommandFailedException($"You do not have enough {this.Shared.GetGuildConfig(ctx.Guild.Id).Currency ?? "credits"} to train a chicken ({price:n0} needed)!");

                    if (chicken.TrainVitality())
                        result = $"{ctx.User.Mention}'s chicken learned alot from the training. New max vitality: {chicken.Stats.TotalMaxVitality}";
                    else
                        result = $"{ctx.User.Mention}'s chicken got tired and didn't learn anything. New max vitality: {chicken.Stats.TotalMaxVitality}";
                    chicken.Stats.BareVitality--;

                    dbc.MaxVitality = chicken.Stats.BareMaxVitality;
                    dbc.Vitality--;
                    db.Chickens.Update(dbc);
                    await db.SaveChangesAsync();
                }

                await this.InformAsync(ctx, StaticDiscordEmoji.Chicken, result);
            }
            #endregion
        }
    }
}
