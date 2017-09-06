﻿#region USING_DIRECTIVES
using System;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
#endregion

namespace TheGodfatherBot
{
    [Group("messages", CanInvokeWithoutSubcommand = false)]
    [Description("Commands to manipulate messages on the channel.")]
    [RequirePermissions(Permissions.ManageMessages)]
    [Aliases("m", "msg", "msgs")]
    public class CommandsMessages
    {
        #region COMMAND_DELETE
        [Command("delete")]
        [Aliases("prune", "del", "d")]
        [RequirePermissions(Permissions.Administrator)]
        public async Task Delete(CommandContext ctx, [Description("Ammount")] int n = 0)
        {
            if (n <= 0 || n > 10000)
                throw new Exception("Invalid number of messages to delete (must be in range [1, 10000].");

            await ctx.Channel.GetMessagesAsync(n).ContinueWith(
                async t => await ctx.Channel.DeleteMessagesAsync(t.Result)
            );
        }
        #endregion
    }
}