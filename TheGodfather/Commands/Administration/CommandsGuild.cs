﻿#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Net;
using System.IO;
using System.Drawing;
using System.Threading.Tasks;

using TheGodfather.Helpers.DataManagers;
using TheGodfather.Exceptions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
#endregion

namespace TheGodfather.Commands.Administration
{
    [Group("guild", CanInvokeWithoutSubcommand = false)]
    [Description("Miscellaneous guild control commands.")]
    [Aliases("server", "g")]
    [Cooldown(3, 5, CooldownBucketType.Guild)]
    public class CommandsGuild
    {
        #region COMMAND_GUILD_INFO
        [Command("info")]
        [Description("Get guild information.")]
        [Aliases("i", "information")]
        public async Task GuildInfoAsync(CommandContext ctx)
        {
            var em = new DiscordEmbedBuilder() {
                Title = ctx.Guild.Name,
                ThumbnailUrl = ctx.Guild.IconUrl,
                Color = DiscordColor.MidnightBlue
            };
            em.AddField("Members", ctx.Guild.MemberCount.ToString(), inline: true);
            em.AddField("Owner", ctx.Guild.Owner.Username, inline: true);
            em.AddField("Region ID", ctx.Guild.RegionId, inline: true);
            em.AddField("Creation date", ctx.Guild.CreationTimestamp.ToString(), inline: true);

            await ctx.RespondAsync(embed: em.Build())
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_GUILD_LISTMEMBERS
        [Command("listmembers")]
        [Description("Get guild member list.")]
        [Aliases("memberlist", "lm", "members")]
        [RequirePermissions(Permissions.ManageGuild)]
        public async Task ListMembersAsync(CommandContext ctx, 
                                          [Description("Page.")] int page = 1)
        {
            var members = await ctx.Guild.GetAllMembersAsync()
                .ConfigureAwait(false);

            if (page < 1 || page > members.Count / 20 + 1)
                throw new CommandFailedException("No members on that page.");

            string s = "";
            int starti = (page - 1) * 20;
            int endi = starti + 20 < members.Count ? starti + 20 : members.Count;
            var membersarray = members.Take(page * 20).ToArray();
            for (var i = starti; i < endi; i++)
                s += $"{Formatter.Bold(membersarray[i].Username)} , joined at: {Formatter.Bold(membersarray[i].JoinedAt.ToString())}\n";

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Title = $"Members (page {Formatter.Bold(page.ToString())}) :",
                Description = s,
                Color = DiscordColor.SapGreen
            }.Build()).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_GUILD_GETLOGS
        [Command("log")]
        [Description("Get audit logs.")]
        [Aliases("auditlog", "viewlog", "getlog", "getlogs", "logs")]
        [RequirePermissions(Permissions.ViewAuditLog)]
        public async Task GetAuditLogsAsync(CommandContext ctx, 
                                           [Description("Page.")] int page = 1)
        {
            var log = await ctx.Guild.GetAuditLogsAsync()
                .ConfigureAwait(false);

            if (page < 1 || page > log.Count / 20 + 1)
                throw new CommandFailedException("No members on that page.");

            string desc = "";
            int starti = (page - 1) * 20;
            int endi = (starti + 20 < log.Count) ? starti + 20 : log.Count;
            var logarray = log.Take(page * 20).ToArray();
            for (var i = starti; i < endi; i++)
                desc += $"{Formatter.Bold(logarray[i].CreationTimestamp.ToUniversalTime().ToString())} UTC : Action " +
                     $"{Formatter.Bold(logarray[i].ActionType.ToString())} by {Formatter.Bold(logarray[i].UserResponsible.Username)}\n";

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Title = $"Audit log (page {page.ToString()}) :",
                Description = desc,
                Color = DiscordColor.Brown
            }.Build()).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_GUILD_PRUNE
        [Command("prune")]
        [Description("Kick guild members who weren't active in given ammount of days (1-7).")]
        [Aliases("p", "clean")]
        [RequirePermissions(Permissions.KickMembers)]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task PruneMembersAsync(CommandContext ctx, 
                                           [Description("Days.")] int days = 7)
        {
            if (days <= 0 || days > 7)
                throw new InvalidCommandUsageException("Number of days not in valid range! [1-7]");
            
            int count = await ctx.Guild.GetPruneCountAsync(days)
                .ConfigureAwait(false);
            await ctx.RespondAsync($"Pruning will remove {Formatter.Bold(count.ToString())} member(s). Continue?")
                .ConfigureAwait(false);
            
            var interactivity = ctx.Client.GetInteractivityModule();
            var msg = await interactivity.WaitForMessageAsync(
                xm => (xm.Author.Id == ctx.User.Id) &&
                      (xm.Content.ToLower().StartsWith("yes") || xm.Content.ToLower().StartsWith("no")),
                TimeSpan.FromMinutes(1)
            ).ConfigureAwait(false);
            if (msg == null || msg.Message.Content.StartsWith("no")) {
                await ctx.RespondAsync("Alright, cancelling...")
                    .ConfigureAwait(false);
                return;
            }
            
            await ctx.Guild.PruneAsync(days)
                .ConfigureAwait(false);
            await ctx.RespondAsync("Pruning complete!")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_GUILD_RENAME
        [Command("rename")]
        [Description("Rename guild.")]
        [Aliases("r", "name", "setname")]
        [RequirePermissions(Permissions.ManageGuild)]
        public async Task RenameGuildAsync(CommandContext ctx,
                                          [RemainingText, Description("New name.")] string name = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException("Missing new guild name.");

            await ctx.Guild.ModifyAsync(name: name)
                .ConfigureAwait(false);
            await ctx.RespondAsync("Guild successfully renamed.")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_GUILD_GETWELCOMECHANNEL
        [Command("getwelcomechannel")]
        [Description("Get current welcome message channel for this guild.")]
        [Aliases("getwelcomec", "getwc", "getwelcome", "welcomechannel", "wc")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task GetWelcomeChannelAsync(CommandContext ctx)
        {
            ulong cid = ctx.Dependencies.GetDependency<ChannelManager>().GetWelcomeChannelId(ctx.Guild.Id);
            if (cid != 0) {
                var c = ctx.Guild.GetChannel(cid);
                if (c == null)
                    throw new CommandFailedException($"Welcome channel was set but does not exist anymore (id: {cid}).");
                await ctx.RespondAsync($"Default welcome message channel: {Formatter.Bold(ctx.Guild.GetChannel(cid).Name)}.")
                    .ConfigureAwait(false);
            } else {
                await ctx.RespondAsync("Default welcome message channel isn't set for this guild.")
                    .ConfigureAwait(false);
            }
        }
        #endregion

        #region COMMAND_GUILD_GETLEAVECHANNEL
        [Command("getleavechannel")]
        [Description("Get current leave message channel for this guild.")]
        [Aliases("getleavec", "getlc", "getleave", "leavechannel", "lc")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task GetLeaveChannelAsync(CommandContext ctx)
        {
            ulong cid = ctx.Dependencies.GetDependency<ChannelManager>().GetLeaveChannelId(ctx.Guild.Id);
            
            if (cid != 0) {
                var c = ctx.Guild.GetChannel(cid);
                if (c == null)
                    throw new CommandFailedException($"Leave channel was set but does not exist anymore (id: {cid}).");
                await ctx.RespondAsync($"Default leave message channel: {Formatter.Bold(c.Name)}.")
                    .ConfigureAwait(false);
            } else {
                await ctx.RespondAsync("Default leave message channel isn't set for this guild.")
                    .ConfigureAwait(false);
            }
        }
        #endregion

        #region COMMAND_GUILD_SETWELCOMECHANNEL
        [Command("setwelcomechannel")]
        [Description("Set welcome message channel for this guild.")]
        [Aliases("setwc", "setwelcomec", "setwelcome")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task SetWelcomeChannelAsync(CommandContext ctx,
                                                [Description("Channel.")] DiscordChannel c = null)
        {
            if (c == null)
                c = ctx.Channel;

            if (c.Type != ChannelType.Text)
                throw new CommandFailedException("Channel must be text type.");

            if (ctx.Dependencies.GetDependency<ChannelManager>().TryAddWelcomeChannel(ctx.Guild.Id, c.Id)) {
                await ctx.RespondAsync($"Default welcome message channel set to {Formatter.Bold(c.Name)}.")
                    .ConfigureAwait(false);
            } else {
                throw new CommandFailedException("Failed to set welcome channel.");
            }
        }
        #endregion

        #region COMMAND_GUILD_SETLEAVECHANNEL
        [Command("setleavechannel")]
        [Description("Set leave message channel for this guild.")]
        [Aliases("leavec", "setlc", "setleave")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task SetLeaveChannelAsync(CommandContext ctx,
                                              [Description("Channel.")] DiscordChannel c = null)
        {
            if (c == null)
                c = ctx.Channel;

            if (c.Type != ChannelType.Text)
                throw new CommandFailedException("Channel must be text type.");

            if (ctx.Dependencies.GetDependency<ChannelManager>().TryAddLeaveChannel(ctx.Guild.Id, c.Id)) {
                await ctx.RespondAsync($"Default leave message channel set to {Formatter.Bold(c.Name)}.")
                    .ConfigureAwait(false);
            } else {
                throw new CommandFailedException("Failed to set welcome channel.");
            }
        }
        #endregion

        #region COMMAND_GUILD_DELETEWELCOMECHANNEL
        [Command("deletewelcomechannel")]
        [Description("Remove welcome message channel for this guild.")]
        [Aliases("delwelcomec", "delwc", "delwelcome", "dwc", "deletewc")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task RemoveWelcomeChannelAsync(CommandContext ctx)
        {
            if (ctx.Dependencies.GetDependency<ChannelManager>().TryRemoveWelcomeChannel(ctx.Guild.Id)) {
                await ctx.RespondAsync("Default welcome message removed.")
                    .ConfigureAwait(false);
            } else {
                throw new CommandFailedException("Failed to remove welcome channel.");
            }
        }
        #endregion

        #region COMMAND_GUILD_DELETELEAVECHANNEL
        [Command("deleteleavechannel")]
        [Description("Remove leave message channel for this guild.")]
        [Aliases("delleavec", "dellc", "delleave", "dlc")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task DeleteLeaveChannelAsync(CommandContext ctx)
        {
            if (ctx.Dependencies.GetDependency<ChannelManager>().TryRemoveLeaveChannel(ctx.Guild.Id)) {
                await ctx.RespondAsync("Default leave message removed.")
                    .ConfigureAwait(false);
            } else {
                throw new CommandFailedException("Failed to remove leave channel.");
            }
        }
        #endregion


        [Group("emoji", CanInvokeWithoutSubcommand = true)]
        [Description("Manipulate guild emoji.")]
        [Aliases("emojis", "e")]
        public class CommandsGuildEmoji
        {
            public async Task ExecuteGroupAsync(CommandContext ctx)
            {
                await ListEmojiAsync(ctx);
            }


            #region COMMAND_GUILD_EMOJI_ADD
            [Command("add")]
            [Description("Add emoji.")]
            [Aliases("create", "a", "+")]
            [RequirePermissions(Permissions.ManageEmojis)]
            public async Task AddEmojiAsync(CommandContext ctx,
                                           [Description("Name.")] string name = null,
                                           [Description("URL.")] string url = null)
            {
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(url))
                    throw new InvalidCommandUsageException("Name or URL missing or invalid.");

                string filename = $"Temp/tmp-emoji-{DateTime.Now.Ticks}.png";
                try {
                    if (!Directory.Exists("Temp"))
                        Directory.CreateDirectory("Temp");
                    using (WebClient webClient = new WebClient()) {
                        byte[] data = webClient.DownloadData(url);

                        using (MemoryStream mem = new MemoryStream(data))
                        using (Image image = Image.FromStream(mem)) {
                            image.Save(filename, System.Drawing.Imaging.ImageFormat.Png);
                            FileStream fs = new FileStream(filename, FileMode.Open);
                            await ctx.Guild.CreateEmojiAsync(name, fs, reason: $"Added by Godfather : {ctx.User.Username} ({ctx.User.Id})")
                                .ConfigureAwait(false);
                            await ctx.RespondAsync($"Emoji {Formatter.Bold(name)} successfully added!")
                                .ConfigureAwait(false);
                        }
                    }
                    if (File.Exists(filename))
                        File.Delete(filename);
                } catch (WebException e) {
                    throw new CommandFailedException("URL error.", e);
                } catch (BadRequestException e) {
                    throw new CommandFailedException("Bad request. Possibly emoji slots are full for this guild?", e);
                } catch (Exception e) {
                    throw new CommandFailedException("IO error. Contact owner please.", e);
                }
            }
            #endregion

            #region COMMAND_GUILD_EMOJI_DELETE
            [Command("delete")]
            [Description("Remove emoji.")]
            [Aliases("remove", "del", "-", "d")]
            [RequirePermissions(Permissions.ManageEmojis)]
            public async Task DeleteEmojiAsync(CommandContext ctx,
                                              [Description("Emoji.")] DiscordEmoji e = null)
            {
                if (e == null)
                    throw new InvalidCommandUsageException("Emoji missing.");

                try {
                    var emoji = await ctx.Guild.GetEmojiAsync(e.Id)
                        .ConfigureAwait(false);
                    string name = emoji.Name;
                    await ctx.Guild.DeleteEmojiAsync(emoji, $"Deleted by Godfather : {ctx.User.Username} ({ctx.User.Id})")
                        .ConfigureAwait(false);
                    await ctx.RespondAsync($"Emoji {Formatter.Bold(name)} successfully deleted!")
                        .ConfigureAwait(false);
                } catch (NotFoundException ex) {
                    throw new CommandFailedException("Can't find that emoji in list of emoji that I made for this guild.", ex);
                }
            }
            #endregion

            #region COMMAND_GUILD_EMOJI_LIST
            [Command("list")]
            [Description("Print list of guild emojis.")]
            [Aliases("print", "show", "l", "p")]
            public async Task ListEmojiAsync(CommandContext ctx)
            {
                string desc = "";
                foreach (var emoji in ctx.Guild.Emojis)
                    desc += $"{Formatter.Bold(emoji.Name)} ";

                await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                    Title = "Available guild emoji:",
                    Description = desc,
                    Color = DiscordColor.CornflowerBlue
                }.Build()).ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_GUILD_EMOJI_MODIFY
            [Command("modify")]
            [Description("Remove emoji.")]
            [Aliases("edit", "mod", "e", "m")]
            [RequirePermissions(Permissions.ManageEmojis)]
            public async Task ModifyEmojiAsync(CommandContext ctx,
                                              [Description("Emoji.")] DiscordEmoji e = null,
                                              [Description("Name.")] string name = null)
            {
                if (e == null)
                    throw new InvalidCommandUsageException("Emoji missing.");

                if (string.IsNullOrWhiteSpace(name))
                    throw new InvalidCommandUsageException("Name missing.");

                try {
                    var emoji = await ctx.Guild.GetEmojiAsync(e.Id)
                        .ConfigureAwait(false);
                    await ctx.Guild.ModifyEmojiAsync(emoji, name: name, reason: $"Edited by Godfather : {ctx.User.Username} ({ctx.User.Id})")
                        .ConfigureAwait(false);
                    await ctx.RespondAsync("Emoji successfully edited!")
                        .ConfigureAwait(false);
                } catch (NotFoundException ex) {
                    throw new CommandFailedException("Can't find that emoji in list of emoji that I made for this guild.", ex);
                }
            }
            #endregion
        }
    }
}

