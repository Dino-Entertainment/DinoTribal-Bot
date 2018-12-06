﻿#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TheGodfather.Common.Attributes;
using TheGodfather.Database;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
#endregion

namespace TheGodfather.Modules.Misc
{
    [Group("grant"), Module(ModuleType.Miscellaneous), NotBlocked]
    [Description("Requests to grant the sender a certain object (role for example).")]
    [Aliases("give")]
    [Cooldown(3, 5, CooldownBucketType.Guild)]
    public class GrantModule : TheGodfatherModule
    {

        public GrantModule(SharedData shared, DatabaseContextBuilder db)
            : base(shared, db)
        {
            this.ModuleColor = DiscordColor.Wheat;
        }


        #region COMMAND_GRANT_ROLE
        [Command("role")]
        [Description("Grants you a role from this guild's self-assignable roles list.")]
        [Aliases("rl", "r")]
        [UsageExamples("!grant role @Announcements")]
        [RequireBotPermissions(Permissions.ManageRoles)]
        public async Task GiveRoleAsync(CommandContext ctx,
                                       [Description("Role to grant.")] DiscordRole role)
        {
            using (DatabaseContext db = this.Database.CreateContext()) {
                if (!db.SelfAssignableRoles.Any(r => r.GuildId == ctx.Guild.Id && r.RoleId == role.Id))
                    throw new CommandFailedException("That role is not in this guild's self-assignable roles list.");
            }

            await ctx.Member.GrantRoleAsync(role, ctx.BuildInvocationDetailsString("Granted self-assignable role."));
            await this.InformAsync(ctx, "Successfully granted the required roles.", important: false);
        }
        #endregion

        #region COMMAND_GRANT_NAME
        [Command("nickname")]
        [Description("Grants you a given nickname.")]
        [Aliases("nick", "name", "n")]
        [UsageExamples("!grant name My New Display Name")]
        [RequireBotPermissions(Permissions.ManageNicknames)]
        public async Task GiveNameAsync(CommandContext ctx,
                                       [RemainingText, Description("Nickname to set.")] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException("Nickname missing.");
            
            using (DatabaseContext db = this.Database.CreateContext()) {
                if (db.ForbiddenNames.Any(n => n.GuildId == ctx.Guild.Id && n.Regex.IsMatch(name)))
                    throw new CommandFailedException($"Name {name} matches one of the forbidden names in this guild.");
            }

            await ctx.Member.ModifyAsync(m => {
                m.Nickname = name;
                m.AuditLogReason = "Self-rename";
            });
            await this.InformAsync(ctx, "Successfully granted the required nickname.", important: false);
        }
        #endregion
    }
}
