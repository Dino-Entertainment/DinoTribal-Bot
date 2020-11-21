﻿using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Services;

namespace TheGodfather.Modules.Administration
{
    public partial class ConfigModule
    {
        [Group("instantleave")]
        [Aliases("il")]
        public class AntiInstantLeaveModule : TheGodfatherServiceModule<AntiInstantLeaveService>
        {
            public AntiInstantLeaveModule(AntiInstantLeaveService service)
                : base(service) { }


            #region config instantleave
            [GroupCommand, Priority(2)]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description("desc-enable")] bool enable,
                                               [Description("desc-sens")] short cooldown)
            {
                if (cooldown < AntiInstantLeaveSettings.MinCooldown || cooldown > AntiInstantLeaveSettings.MaxCooldown)
                    throw new CommandFailedException(ctx, "cmd-err-range-cd", AntiInstantLeaveSettings.MinCooldown, AntiInstantLeaveSettings.MaxCooldown);

                var settings = new AntiInstantLeaveSettings {
                    Enabled = enable,
                    Cooldown = cooldown
                };

                await ctx.Services.GetRequiredService<GuildConfigService>().ModifyConfigAsync(ctx.Guild.Id, gcfg => gcfg.AntiInstantLeaveSettings = settings);

                await ctx.GuildLogAsync(emb => {
                    emb.WithLocalizedTitle("evt-cfg-upd");
                    emb.WithColor(this.ModuleColor);
                    if (enable) {
                        emb.WithLocalizedDescription("evt-il-enabled");
                        emb.AddLocalizedTitleField("str-cooldown", settings.Cooldown, inline: true);
                    } else {
                        emb.WithLocalizedDescription("evt-il-disabled");
                    }
                });

                await ctx.InfoAsync(enable ? "evt-il-enabled" : "evt-il-disabled");
            }

            [GroupCommand, Priority(1)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description("desc-enable")] bool enable)
                => this.ExecuteGroupAsync(ctx, enable, 5);

            [GroupCommand, Priority(0)]
#pragma warning disable CA1822 // Mark members as static
            public Task ExecuteGroupAsync(CommandContext ctx)
#pragma warning restore CA1822 // Mark members as static
            {
                return ctx.WithGuildConfigAsync(gcfg => {
                    LocalizationService lcs = ctx.Services.GetRequiredService<LocalizationService>();
                    return ctx.InfoAsync("fmt-settings-rl", gcfg.AntiInstantLeaveSettings.ToEmbedFieldString(ctx.Guild.Id, lcs));
                });
            }
            #endregion

            #region config instantleave cooldown
            [Command("cooldown")]
            [Aliases("setcooldown", "setcool", "cd", "c")]
            public async Task SetCooldownAsync(CommandContext ctx,
                                              [Description("desc-sens")] short? cooldown = null)
            {
                if (cooldown is null) {
                    await ctx.WithGuildConfigAsync(gcfg => ctx.InfoAsync("evt-il-cd", gcfg.AntiInstantLeaveCooldown ));
                    return;
                }

                if (cooldown < AntiInstantLeaveSettings.MinCooldown || cooldown > AntiInstantLeaveSettings.MaxCooldown)
                    throw new CommandFailedException(ctx, "cmd-err-range-cd", AntiInstantLeaveSettings.MinCooldown, AntiInstantLeaveSettings.MaxCooldown);

                await ctx.Services.GetRequiredService<GuildConfigService>().ModifyConfigAsync(ctx.Guild.Id, cfg => {
                    cfg.AntiInstantLeaveCooldown = cooldown.Value;
                });

                await ctx.GuildLogAsync(emb => {
                    emb.WithLocalizedTitle("evt-cfg-upd");
                    emb.WithColor(this.ModuleColor);
                    emb.WithLocalizedDescription("evt-il-cd", cooldown.Value);
                });

                await ctx.InfoAsync("evt-il-cd", cooldown.Value);
            }
            #endregion

            #region config instantleave reset
            [Command("reset"), UsesInteractivity]
            [Aliases("default", "def", "s", "rr")]
            public async Task ResetAsync(CommandContext ctx)
            {
                await ctx.WithGuildConfigAsync(gcfg => {
                    return !gcfg.AntiInstantLeaveEnabled ? throw new CommandFailedException(ctx, "cmd-err-reset-il-off") : Task.CompletedTask;
                });

                if (!await ctx.WaitForBoolReplyAsync("q-setup-reset"))
                    return;

                var settings = new AntiInstantLeaveSettings();
                await this.ExecuteGroupAsync(ctx, true, settings.Cooldown);
            }
            #endregion
        }
    }
}
