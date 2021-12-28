﻿using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Attributes;
using TheGodfather.Common;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Modules.Chickens.Common;
using TheGodfather.Modules.Chickens.Services;
using TheGodfather.Modules.Currency.Services;
using TheGodfather.Services;
using TheGodfather.Services.Common;

namespace TheGodfather.Modules.Chickens
{
    public partial class ChickenModule
    {
        [Group("train"), UsesInteractivity]
        [Aliases("tr", "t", "exercise")]
        public sealed class TrainModule : TheGodfatherServiceModule<ChickenService>
        {
            #region chicken train
            [GroupCommand]
            public Task ExecuteGroupAsync(CommandContext ctx)
                => this.StrengthAsync(ctx);
            #endregion

            #region chicken train strength
            [Command("strength")]
            [Aliases("str", "st", "s")]
            public async Task StrengthAsync(CommandContext ctx)
            {
                Chicken? chicken = await this.PreTrainCheckAsync(ctx, "STR");
                if (chicken is null)
                    return;

                bool success = chicken.TrainStrength();
                chicken.Stats.BareVitality--;

                await this.Service.UpdateAsync(chicken);

                TranslationKey msg = success
                    ? TranslationKey.fmt_chicken_train_succ(ctx.User.Mention, "STR", chicken.Stats.TotalStrength)
                    : TranslationKey.fmt_chicken_train_fail(ctx.User.Mention, "STR", chicken.Stats.TotalStrength);
                await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Chicken, msg);
            }
            #endregion

            #region chicken train vitality
            [Command("vitality")]
            [Aliases("vit", "vi", "v")]
            public async Task VitalityAsync(CommandContext ctx)
            {
                Chicken? chicken = await this.PreTrainCheckAsync(ctx, "VIT");
                if (chicken is null)
                    return;

                bool success = chicken.TrainVitality();
                chicken.Stats.BareVitality--;

                await this.Service.UpdateAsync(chicken);

                TranslationKey msg = success
                    ? TranslationKey.fmt_chicken_train_succ(ctx.User.Mention, "VIT", chicken.Stats.TotalMaxVitality)
                    : TranslationKey.fmt_chicken_train_fail(ctx.User.Mention, "VIT", chicken.Stats.TotalMaxVitality);
                await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Chicken, msg);
            }
            #endregion


            #region internals
            private async Task<Chicken?> PreTrainCheckAsync(CommandContext ctx, string stat)
            {
                if (ctx.Services.GetRequiredService<ChannelEventService>().IsEventRunningInChannel(ctx.Channel.Id, out ChickenWar _))
                    throw new CommandFailedException(ctx, TranslationKey.cmd_err_chicken_war);

                Chicken? chicken = await this.Service.GetCompleteAsync(ctx.Guild.Id, ctx.User.Id);
                if (chicken is null)
                    throw new CommandFailedException(ctx, TranslationKey.cmd_err_chicken_none);
                chicken.Owner = ctx.User;

                if (chicken.Stats.TotalVitality < Chicken.MinVitalityToFight)
                    throw new CommandFailedException(ctx, TranslationKey.cmd_err_chicken_weak(ctx.User.Mention));

                CachedGuildConfig gcfg = ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id);
                long price = stat switch {
                    "STR" => chicken.TrainStrengthPrice,
                    "VIT" => chicken.TrainVitalityPrice,
                    _ => throw new CommandFailedException(ctx),
                };

                if (!await ctx.WaitForBoolReplyAsync(TranslationKey.q_chicken_train(ctx.User.Mention, stat, price, gcfg.Currency)))
                    return null;

                if (!await ctx.Services.GetRequiredService<BankAccountService>().TryDecreaseBankAccountAsync(ctx.Guild.Id, ctx.User.Id, price))
                    throw new CommandFailedException(ctx, TranslationKey.cmd_err_funds(gcfg.Currency, price));

                return chicken;
            }
            #endregion
        }
    }
}
