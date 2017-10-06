﻿#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Collections;
using System.Threading.Tasks;

using TheGodfatherBot.Exceptions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfatherBot.Commands.Admin
{
    [Group("guild", CanInvokeWithoutSubcommand = false)]
    [Description("Miscellaneous guild control commands.")]
    [Aliases("server")]
    public class CommandsGuild
    {
        #region COMMAND_GUILD_EMOJI
        [Command("emoji")]
        [Description("Print list of guild emojis.")]
        [Aliases("emojis")]
        public async Task GetEmoji(CommandContext ctx)
        {
            string s = "";
            foreach (var emoji in ctx.Guild.Emojis)
                s += $"{Formatter.Bold(emoji.Name)} ";

            await ctx.RespondAsync("", embed: new DiscordEmbedBuilder() {
                Title = "Available emoji:",
                Description = s,
                Color = DiscordColor.CornflowerBlue
            });
        }
        #endregion

        #region COMMAND_GUILD_LISTMEMBERS
        [Command("listmembers")]
        [Description("Rename guild.")]
        [Aliases("memberlist", "listm", "lm", "mem", "members", "memlist", "mlist")]
        [RequirePermissions(Permissions.ManageGuild)]
        public async Task ListMembers(CommandContext ctx, 
                                     [Description("Page.")] int page = 1)
        {
            var members = await ctx.Guild.GetAllMembersAsync();

            if (page < 1 || page > members.Count / 20 + 1)
                throw new CommandFailedException("No members on that page.");

            string s = "";
            int starti = (page - 1) * 20;
            int endi = starti + 20 < members.Count ? starti + 20 : members.Count;
            var membersarray = members.Take(page * 20).ToArray();
            for (var i = starti; i < endi; i++)
                s += $"{Formatter.Bold(membersarray[i].Username)} , joined at: {Formatter.Bold(membersarray[i].JoinedAt.ToString())}\n";

            await ctx.RespondAsync("", embed: new DiscordEmbedBuilder() {
                Title = $"Members (page {Formatter.Bold(page.ToString())}) :",
                Description = s,
                Color = DiscordColor.SapGreen
            });
        }
        #endregion

        #region COMMAND_GUILD_GETLOGS
        [Command("log")]
        [Description("Get audit logs.")]
        [Aliases("auditlog", "viewlog", "getlog", "getlogs", "logs")]
        [RequirePermissions(Permissions.ViewAuditLog)]
        public async Task GetAuditLogs(CommandContext ctx, 
                                      [Description("Page.")] int page = 1)
        {
            var log = await ctx.Guild.GetAuditLogsAsync();

            if (page < 1 || page > log.Count / 20 + 1)
                throw new CommandFailedException("No members on that page.");

            string s = "";
            int starti = (page - 1) * 20;
            int endi = starti + 20 < log.Count ? starti + 20 : log.Count;
            var logarray = log.Take(page * 20).ToArray();
            for (var i = starti; i < endi; i++)
                s += $"{Formatter.Bold(logarray[i].CreationTimestamp.ToUniversalTime().ToString())} UTC : Action " +
                     $"{Formatter.Bold(logarray[i].ActionType.ToString())} by {Formatter.Bold(logarray[i].UserResponsible.Username)}\n";

            await ctx.RespondAsync("", embed: new DiscordEmbedBuilder() {
                Title = $"Audit log (page {Formatter.Bold(page.ToString())}) :",
                Description = s,
                Color = DiscordColor.Brown
            });
        }
        #endregion

        #region COMMAND_GUILD_PRUNE
        [Command("prune")]
        [Description("Prune guild members who weren't active in given ammount of days.")]
        [Aliases("p", "clean", "lm", "mem", "members")]
        [RequirePermissions(Permissions.KickMembers)]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task PruneMembers(CommandContext ctx, 
                                      [Description("Days.")] int days = 365)
        {
            if (days <= 0 || days > 1000)
                throw new InvalidCommandUsageException("Number of days not in valid range! [1-1000]");
            
            int count = await ctx.Guild.GetPruneCountAsync(days);
            await ctx.RespondAsync($"Pruning will remove {Formatter.Bold(count.ToString())} members. Continue?");
            
            var interactivity = ctx.Client.GetInteractivityModule();
            var msg = await interactivity.WaitForMessageAsync(
                xm => (xm.Author.Id == ctx.User.Id) &&
                      (xm.Content.ToLower().StartsWith("yes") || xm.Content.ToLower().StartsWith("no")),
                TimeSpan.FromMinutes(1)
            );
            if (msg == null || msg.Message.Content.StartsWith("no")) {
                await ctx.RespondAsync("Alright, cancelling...");
                return;
            }
            
            await ctx.Guild.PruneAsync(days);
            await ctx.RespondAsync("Pruning complete!");
        }
        #endregion

        #region COMMAND_GUILD_RENAME
        [Command("rename")]
        [Description("Rename guild.")]
        [Aliases("r", "name", "setname")]
        [RequirePermissions(Permissions.ManageGuild)]
        public async Task RenameGuild(CommandContext ctx,
                                     [RemainingText, Description("New name.")] string name = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException("Missing new guild name.");

            await ctx.Guild.ModifyAsync(name: name);
            await ctx.RespondAsync("Guild successfully renamed.");
        }
        #endregion
    }
}

