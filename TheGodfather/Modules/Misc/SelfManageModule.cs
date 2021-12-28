﻿using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Modules.Administration.Services;

namespace TheGodfather.Modules.Misc;

[Group("grant")][Module(ModuleType.Misc)][NotBlocked]
[Aliases("give")]
[RequireGuild][Cooldown(3, 5, CooldownBucketType.Guild)]
public sealed class GrantModule : TheGodfatherModule
{
    #region grant
    [GroupCommand][Priority(0)]
    public Task ExecuteGroupAsync(CommandContext ctx,
        [Description(TranslationKey.desc_roles_add)] params DiscordRole[] roles)
    {
        if (ctx.Guild.CurrentMember is null)
            throw new ChecksFailedException(ctx.Command, ctx, new[] { new RequireBotPermissionsAttribute(Permissions.ManageRoles) });

        if (ctx.Channel.PermissionsFor(ctx.Guild.CurrentMember).HasPermission(Permissions.Administrator | Permissions.ManageRoles))
            return this.GiveRoleAsync(ctx, roles);
        throw new ChecksFailedException(ctx.Command, ctx, new[] { new RequireBotPermissionsAttribute(Permissions.ManageRoles) });
    }
    #endregion

    #region grant role
    [Command("role")]
    [Aliases("roles", "rl", "r")]
    [RequireBotPermissions(Permissions.ManageRoles)]
    public async Task GiveRoleAsync(CommandContext ctx,
        [Description(TranslationKey.desc_roles_add)] params DiscordRole[] roles)
    {
        SelfRoleService service = ctx.Services.GetRequiredService<SelfRoleService>();

        var failedRoles = new List<DiscordRole>();
        foreach (DiscordRole role in roles.Distinct())
            if (await service.ContainsAsync(ctx.Guild.Id, role.Id))
                await ctx.Member.GrantRoleAsync(role, ctx.BuildInvocationDetailsString("_gf: Self-granted"));
            else
                failedRoles.Add(role);

        if (failedRoles.Any())
            await ctx.ImpInfoAsync(this.ModuleColor, TranslationKey.cmd_err_grant_fail(failedRoles.Select(r => r.Mention).JoinWith()));
        else
            await ctx.InfoAsync(this.ModuleColor);
    }
    #endregion

    #region grant nickname
    [Command("nickname")]
    [Aliases("nick", "name", "n")]
    [RequireBotPermissions(Permissions.ManageNicknames)]
    public async Task GiveNameAsync(CommandContext ctx,
        [RemainingText][Description(TranslationKey.desc_name)] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_missing_name);

        ForbiddenNamesService service = ctx.Services.GetRequiredService<ForbiddenNamesService>();
        if (service.IsNameForbidden(ctx.Guild.Id, name, out _))
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_fn_match);

        await ctx.Member.ModifyAsync(m => {
            m.Nickname = name;
            m.AuditLogReason = this.Localization.GetString(ctx.Guild.Id, TranslationKey.rsn_grant_name);
        });

        await ctx.InfoAsync(this.ModuleColor);
    }
    #endregion
}

[Group("revoke")][Module(ModuleType.Misc)][NotBlocked]
[Aliases("take")]
[RequireGuild][Cooldown(3, 5, CooldownBucketType.Guild)]
public sealed class RevokeModule : TheGodfatherModule
{
    #region revoke
    [GroupCommand][Priority(0)]
    public async Task ExecuteGroupAsync(CommandContext ctx,
        [Description(TranslationKey.desc_roles_del)] params DiscordRole[] roles)
    {
        if (ctx.Guild.CurrentMember is null)
            throw new ChecksFailedException(ctx.Command, ctx, new[] { new RequireBotPermissionsAttribute(Permissions.ManageRoles) });

        if (ctx.Channel.PermissionsFor(ctx.Guild.CurrentMember).HasPermission(Permissions.Administrator | Permissions.ManageRoles))
            await this.RevokeRoleAsync(ctx, roles);
        else
            throw new ChecksFailedException(ctx.Command, ctx, new[] { new RequireBotPermissionsAttribute(Permissions.ManageRoles) });
    }
    #endregion

    #region revoke role
    [Command("role")]
    [Aliases("rl", "r")]
    [RequireBotPermissions(Permissions.ManageRoles)]
    public async Task RevokeRoleAsync(CommandContext ctx,
        [Description(TranslationKey.desc_roles_del)] params DiscordRole[] roles)
    {
        SelfRoleService service = ctx.Services.GetRequiredService<SelfRoleService>();

        var failedRoles = new List<DiscordRole>();
        foreach (DiscordRole role in roles.Distinct())
            if (await service.ContainsAsync(ctx.Guild.Id, role.Id))
                await ctx.Member.RevokeRoleAsync(role, ctx.BuildInvocationDetailsString("_gf: Self-revoke"));
            else
                failedRoles.Add(role);

        if (failedRoles.Any())
            await ctx.ImpInfoAsync(this.ModuleColor, TranslationKey.cmd_err_revoke_fail(failedRoles.Select(r => r.Mention).JoinWith()));
        else
            await ctx.InfoAsync(this.ModuleColor);
    }
    #endregion
}