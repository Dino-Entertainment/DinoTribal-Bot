﻿#region USING_DIRECTIVES
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
#endregion

namespace TheGodfatherBot
{
    [Group("user", CanInvokeWithoutSubcommand = false)]
    [Description("Miscellaneous user control commands.")]
    [Aliases("users", "u", "usr")]
    public class CommandsUsers
    {
        #region COMMAND_USER_KICK
        [Command("kick")]
        [Description("Kicks the user from server.")]
        [RequirePermissions(Permissions.KickMembers)]
        public async Task List(CommandContext ctx, [Description("User")] DiscordMember u = null)
        {
            if (u == null)
                throw new ArgumentNullException("You need to mention a user to kick.");

            await ctx.Guild.RemoveMemberAsync(u);
        }
        #endregion
    }
}
