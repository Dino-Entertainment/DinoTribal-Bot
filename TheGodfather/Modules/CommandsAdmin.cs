﻿#region USING_DIRECTIVES
using System;
using System.IO;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfatherBot
{
    [Group("admin"), Description("Administrative owner commands."), Hidden]
    [RequirePermissions(Permissions.Administrator)]
    [RequireOwner]
    public class CommandsAdmin
    {
        #region COMMAND_CLEARLOG
        [Command("clearlog"), Description("Clear application logs.")]
        [Aliases("clearlogs", "deletelogs", "deletelog")]
        public async Task ChangeNickname(CommandContext ctx)
        {
            try {
                TheGodfather.CloseLogFile();
                File.Delete("log.txt");
                TheGodfather.OpenLogFile();
            } catch (Exception e) {
                ctx.Client.DebugLogger.LogMessage(LogLevel.Error, "TheGodfather", e.Message, DateTime.Now);
                throw e;
            }

            await ctx.RespondAsync("Logs cleared.");
        }
        #endregion
       
        #region COMMAND_SHUTDOWN
        [Command("shutdown"), Description("Triggers the dying in the vineyard scene.")]
        [Aliases("disable", "poweroff", "exit", "quit")]
        public async Task ShutDown(CommandContext ctx)
        {
            await ctx.RespondAsync("https://www.youtube.com/watch?v=4rbfuw0UN2A");
            await ctx.Client.DisconnectAsync();
            Environment.Exit(0);
        }
        #endregion

        #region COMMAND_SUDO
        [Command("sudo"), Description("Executes a command as another user."), Hidden]
        public async Task Sudo(CommandContext ctx, 
                              [Description("Member to execute as.")] DiscordMember member, 
                              [RemainingText, Description("Command text to execute.")] string command)
        {
            ctx.Client.DebugLogger.LogMessage(LogLevel.Info, "TheGodfather",
                   $"{ctx.User.Username} attempts to execute !sudo {member.Username} {command}",
                   DateTime.Now);
            await ctx.TriggerTypingAsync();
            var cmds = ctx.Client.GetCommandsNext();
            await cmds.SudoAsync(member, ctx.Channel, command);
        }
        #endregion
    }
}
