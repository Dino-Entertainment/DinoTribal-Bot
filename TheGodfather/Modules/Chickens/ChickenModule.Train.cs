﻿#region USING_DIRECTIVES
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Modules.Chickens.Common;
using TheGodfather.Modules.Currency.Extensions;
using TheGodfather.Services;
using TheGodfather.Services.Common;
#endregion

namespace TheGodfather.Modules.Chickens
{
    public partial class ChickenModule
    {
        [Group("train"), UsesInteractivity]
        [Description("Train your chicken using your credits from WM bank.")]
        [Aliases("tr", "t", "exercise")]
        public class TrainModule : TheGodfatherServiceModule<ChannelEventService>
        {

            public TrainModule(ChannelEventService service, DatabaseContextBuilder db) 
                : base(service, db)
            {
                
            }


            [GroupCommand]
            public Task ExecuteGroupAsync(CommandContext ctx)
                => this.StrengthAsync(ctx);


            #region COMMAND_CHICKEN_TRAIN_STRENGTH
            [Command("strength")]
            [Description("Train your chicken's strength using your credits from WM bank.")]
            [Aliases("str", "st", "s")]
            public async Task StrengthAsync(CommandContext ctx)
            {
                if (this.Service.IsEventRunningInChannel(ctx.Channel.Id, out ChickenWar _))
                    throw new CommandFailedException("There is a chicken war running in this channel. No trainings are allowed before the war finishes.");

                string result;

                using (DatabaseContext db = this.Database.CreateContext()) {
                    DatabaseChicken dbc = await db.Chickens.FindAsync((long)ctx.Guild.Id, (long)ctx.User.Id);
                    var chicken = Chicken.FromDatabaseChicken(dbc);
                    if (chicken is null)
                        throw new CommandFailedException("You do not own a chicken!");

                    if (chicken.Stats.TotalVitality < 25)
                        throw new CommandFailedException($"{ctx.User.Mention}, your chicken is too weak for that action! Heal it using {Formatter.BlockCode("chicken heal")} command.");


                    CachedGuildConfig gcfg = ctx.Services.GetService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id);
                    long price = chicken.TrainStrengthPrice;
                    if (!await ctx.WaitForBoolReplyAsync($"{ctx.User.Mention}, are you sure you want to train your chicken for {Formatter.Bold($"{price:n0}")} {gcfg.Currency}?\n\nNote: This action will also weaken the vitality of your chicken by 1."))
                        return;

                    if (!await db.TryDecreaseBankAccountAsync(ctx.User.Id, ctx.Guild.Id, price))
                        throw new CommandFailedException($"You do not have enough {gcfg.Currency} to train a chicken ({price:n0} needed)!");
                    
                    result = chicken.TrainStrength()
                        ? $"{ctx.User.Mention}'s chicken learned alot from the training. New strength: {chicken.Stats.TotalStrength}"
                        : $"{ctx.User.Mention}'s chicken got tired and didn't learn anything. New strength: {chicken.Stats.TotalStrength}";
                    chicken.Stats.BareVitality--;

                    dbc.Strength = chicken.Stats.BareStrength;
                    dbc.Vitality--;
                    db.Chickens.Update(dbc);

                    await db.SaveChangesAsync();
                }

                await this.InformAsync(ctx, Emojis.Chicken, result);
            }
            #endregion

            #region COMMAND_CHICKEN_TRAIN_VITALITY
            [Command("vitality")]
            [Description("Train your chicken's vitality using your credits from WM bank.")]
            [Aliases("vit", "vi", "v")]
            public async Task VitalityAsync(CommandContext ctx)
            {
                if (this.Service.IsEventRunningInChannel(ctx.Channel.Id, out ChickenWar _))
                    throw new CommandFailedException("There is a chicken war running in this channel. No trainings are allowed before the war finishes.");

                string result;

                using (DatabaseContext db = this.Database.CreateContext()) {
                    DatabaseChicken dbc = await db.Chickens.FindAsync((long)ctx.Guild.Id, (long)ctx.User.Id);
                    var chicken = Chicken.FromDatabaseChicken(dbc);
                    if (chicken is null)
                        throw new CommandFailedException("You do not own a chicken!");

                    if (chicken.Stats.TotalVitality < 25)
                        throw new CommandFailedException($"{ctx.User.Mention}, your chicken is too weak for that action! Heal it using {Formatter.BlockCode("chicken heal")} command.");

                    CachedGuildConfig gcfg = ctx.Services.GetService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id);
                    long price = chicken.TrainVitalityPrice;
                    if (!await ctx.WaitForBoolReplyAsync($"{ctx.User.Mention}, are you sure you want to train your chicken for {Formatter.Bold($"{price:n0}")} {gcfg.Currency}?\n\nNote: This action will also weaken the vitality of your chicken by 1."))
                        return;

                    if (!await db.TryDecreaseBankAccountAsync(ctx.User.Id, ctx.Guild.Id, price))
                        throw new CommandFailedException($"You do not have enough {gcfg.Currency} to train a chicken ({price:n0} needed)!");

                    result = chicken.TrainVitality()
                        ? $"{ctx.User.Mention}'s chicken learned alot from the training. New max vitality: {chicken.Stats.TotalMaxVitality}"
                        : $"{ctx.User.Mention}'s chicken got tired and didn't learn anything. New max vitality: {chicken.Stats.TotalMaxVitality}";
                    chicken.Stats.BareVitality--;

                    dbc.MaxVitality = chicken.Stats.BareMaxVitality;
                    dbc.Vitality--;
                    db.Chickens.Update(dbc);
                    await db.SaveChangesAsync();
                }

                await this.InformAsync(ctx, Emojis.Chicken, result);
            }
            #endregion
        }
    }
}
