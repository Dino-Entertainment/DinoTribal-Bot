﻿#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Net;
using System.IO;
using System.Drawing;
using System.Threading.Tasks;

using TheGodfather.Attributes;
using TheGodfather.Services;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.Net.Models;
#endregion

namespace TheGodfather.Modules.Administration
{
    [Group("guild")]
    [Description("Miscellaneous guild control commands.")]
    [Aliases("server", "g")]
    [Cooldown(2, 5, CooldownBucketType.Guild)]
    [ListeningCheck]
    public class GuildAdminModule : GodfatherBaseModule
    {

        public GuildAdminModule(SharedData shared, DatabaseService db) : base(shared, db) { }
        

        #region COMMAND_GUILD_GETBANS
        [Command("bans")]
        [Description("Get guild ban list.")]
        [Aliases("banlist", "viewbanlist", "getbanlist", "getbans", "viewbans")]
        [UsageExample("!guild banlist")]
        [RequirePermissions(Permissions.ViewAuditLog)]
        public async Task GetBansAsync(CommandContext ctx)
        {
            var bans = await ctx.Guild.GetBansAsync()
                .ConfigureAwait(false);

            await InteractivityUtil.SendPaginatedCollectionAsync(
                ctx, 
                "Guild bans", 
                bans, 
                b => $"- {b.User.ToString()} | Reason: {b.Reason} ", 
                DiscordColor.Red
            ).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_GUILD_GETLOGS
        [Command("log")]
        [Description("Get audit logs.")]
        [Aliases("auditlog", "viewlog", "getlog", "getlogs", "logs")]
        [UsageExample("!guild logs")]
        [RequirePermissions(Permissions.ViewAuditLog)]
        public async Task GetAuditLogsAsync(CommandContext ctx)
        {
            var bans = await ctx.Guild.GetAuditLogsAsync()
                .ConfigureAwait(false);

            await InteractivityUtil.SendPaginatedCollectionAsync(
                ctx,
                "Audit log",
                bans,
                le => $"- {le.CreationTimestamp} {Formatter.Bold(le.UserResponsible.Username)} | {Formatter.Bold(le.ActionType.ToString())} | Reason: {le.Reason}",
                DiscordColor.Brown,
                5
            ).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_GUILD_INFO
        [Command("info")]
        [Description("Get guild information.")]
        [UsageExample("!guild info")]
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
            em.AddField("Creation date", ctx.Guild.CreationTimestamp.ToString(), inline: true);
            em.AddField("Voice region", ctx.Guild.VoiceRegion.Name, inline: true);
            em.AddField("Verification level", ctx.Guild.VerificationLevel.ToString(), inline: true);

            await ctx.RespondAsync(embed: em.Build())
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_GUILD_LISTMEMBERS
        [Command("listmembers")]
        [Description("Get guild member list.")]
        [UsageExample("!guild memberlist")]
        [Aliases("memberlist", "lm", "members")]
        public async Task ListMembersAsync(CommandContext ctx)
        {
            var members = await ctx.Guild.GetAllMembersAsync()
                .ConfigureAwait(false);

            var sorted = members.ToList();
            sorted.Sort((m1, m2) => string.Compare(m1.Username, m2.Username));

            await InteractivityUtil.SendPaginatedCollectionAsync(
                ctx,
                "Members",
                sorted,
                m => m.ToString(),
                DiscordColor.SapGreen
            ).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_GUILD_PRUNE
        [Command("prune")]
        [Description("Kick guild members who weren't active in given amount of days (1-7).")]
        [Aliases("p", "clean")]
        [UsageExample("!guild prune 5 Kicking inactives..")]
        [RequirePermissions(Permissions.KickMembers)]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task PruneMembersAsync(CommandContext ctx,
                                           [Description("Days.")] int days = 7,
                                           [RemainingText, Description("Reason.")] string reason = null)
        {
            if (days <= 0 || days > 7)
                throw new InvalidCommandUsageException("Number of days is not in valid range! [1-7]");

            int count = await ctx.Guild.GetPruneCountAsync(days)
                .ConfigureAwait(false);
            if (count == 0) {
                await ctx.RespondAsync("No members found to prune...")
                    .ConfigureAwait(false);
                return;
            }

            await ctx.RespondAsync($"Pruning will remove {Formatter.Bold(count.ToString())} member(s). Continue?")
                .ConfigureAwait(false);

            if (!await InteractivityUtil.WaitForConfirmationAsync(ctx)) {
                await ctx.RespondAsync("Alright, cancelling...")
                    .ConfigureAwait(false);
                return;
            }

            await ctx.Guild.PruneAsync(days, GetReasonString(ctx, reason))
                .ConfigureAwait(false);
            await ReplySuccessAsync(ctx)
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_GUILD_RENAME
        [Command("rename")]
        [Description("Rename guild.")]
        [Aliases("r", "name", "setname")]
        [UsageExample("!guild rename New guild name")]
        [UsageExample("!guild rename \"Reason for renaming\" New guild name")]
        [RequirePermissions(Permissions.ManageGuild)]
        public async Task RenameGuildAsync(CommandContext ctx,
                                          [Description("Reason.")] string reason,
                                          [RemainingText, Description("New name.")] string newname)
        {
            if (string.IsNullOrWhiteSpace(newname))
                throw new InvalidCommandUsageException("Missing new guild name.");

            await ctx.Guild.ModifyAsync(new Action<GuildEditModel>(m => {
                m.Name = newname;
                m.AuditLogReason = GetReasonString(ctx, reason);
            })).ConfigureAwait(false);
            await ReplySuccessAsync(ctx)
                .ConfigureAwait(false);
        }

        [Command("rename")]
        public async Task RenameGuildAsync(CommandContext ctx,
                                          [RemainingText, Description("New name.")] string newname)
            => await RenameGuildAsync(ctx, null, newname);
        #endregion

        #region COMMAND_GUILD_SETICON
        [Command("seticon")]
        [Description("Change icon of the guild.")]
        [Aliases("icon", "si")]
        [UsageExample("!guild seticon http://imgur.com/someimage.png")]
        [RequirePermissions(Permissions.ManageGuild)]
        public async Task SetIconAsync(CommandContext ctx,
                                      [Description("New icon URL.")] string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new InvalidCommandUsageException("URL missing.");
            
            if (!IsValidImageURL(url, out Uri uri))
                throw new CommandFailedException("URL must point to an image and use http or https protocols.");

            string filename = $"Temp/tmp-icon-{DateTime.Now.Ticks}.png";
            try {
                if (!Directory.Exists("Temp"))
                    Directory.CreateDirectory("Temp");

                using (var wc = new WebClient()) {
                    byte[] data = wc.DownloadData(uri.AbsoluteUri);

                    using (var ms = new MemoryStream(data))
                        await ctx.Guild.ModifyAsync(new Action<GuildEditModel>(e =>
                            e.Icon = ms
                        )).ConfigureAwait(false);
                }

                if (File.Exists(filename))
                    File.Delete(filename);
            } catch (WebException e) {
                throw new CommandFailedException("Error getting the image.", e);
            } catch (Exception e) {
                throw new CommandFailedException("Unknown error occured.", e);
            }

            await ReplySuccessAsync(ctx)
                .ConfigureAwait(false);
        }
        #endregion


        #region WELCOME_LEAVE_CHANNELS

        #region COMMAND_GUILD_GETWELCOMECHANNEL
        [Command("getwelcomechannel")]
        [Description("Get current welcome message channel for this guild.")]
        [Aliases("getwelcomec", "getwc", "getwelcome", "welcomechannel", "wc")]
        [UsageExample("!guild getwelcomechannel")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task GetWelcomeChannelAsync(CommandContext ctx)
        {
            ulong cid = await DatabaseService.GetGuildWelcomeChannelIdAsync(ctx.Guild.Id)
                .ConfigureAwait(false);
            if (cid != 0) {
                var c = ctx.Guild.GetChannel(cid);
                if (c == null)
                    throw new CommandFailedException($"Welcome channel was set but does not exist anymore (id: {cid}).");
                await ReplySuccessAsync(ctx, $"Default welcome message channel: {Formatter.Bold(ctx.Guild.GetChannel(cid).Name)}.")
                    .ConfigureAwait(false);
            } else {
                await ReplySuccessAsync(ctx, "Default welcome message channel isn't set for this guild.")
                    .ConfigureAwait(false);
            }
        }
        #endregion

        #region COMMAND_GUILD_GETLEAVECHANNEL
        [Command("getleavechannel")]
        [Description("Get current leave message channel for this guild.")]
        [Aliases("getleavec", "getlc", "getleave", "leavechannel", "lc")]
        [UsageExample("!guild getleavechannel")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task GetLeaveChannelAsync(CommandContext ctx)
        {
            ulong cid = await DatabaseService.GetGuildLeaveChannelIdAsync(ctx.Guild.Id)
                .ConfigureAwait(false);
            if (cid != 0) {
                var c = ctx.Guild.GetChannel(cid);
                if (c == null)
                    throw new CommandFailedException($"Leave channel was set but does not exist anymore (id: {cid}).");
                await ReplySuccessAsync(ctx, $"Default leave message channel: {Formatter.Bold(c.Name)}.")
                    .ConfigureAwait(false);
            } else {
                await ReplySuccessAsync(ctx, "Default leave message channel isn't set for this guild.")
                    .ConfigureAwait(false);
            }
        }
        #endregion

        #region COMMAND_GUILD_SETWELCOMECHANNEL
        [Command("setwelcomechannel")]
        [Description("Set welcome message channel for this guild. If the channel isn't given, uses the current one.")]
        [Aliases("setwc", "setwelcomec", "setwelcome")]
        [UsageExample("!guild setwelcomechannel")]
        [UsageExample("!guild setwelcomechannel #welcome")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task SetWelcomeChannelAsync(CommandContext ctx,
                                                [Description("Channel.")] DiscordChannel channel = null)
        {
            if (channel == null)
                channel = ctx.Channel;

            if (channel.Type != ChannelType.Text)
                throw new CommandFailedException("Given channel must be a text channel.");

            await DatabaseService.SetGuildWelcomeChannelAsync(ctx.Guild.Id, channel.Id)
                .ConfigureAwait(false);
            await ReplySuccessAsync(ctx, $"Default welcome message channel set to {Formatter.Bold(channel.Name)}.")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_GUILD_SETLEAVECHANNEL
        [Command("setleavechannel")]
        [Description("Set leave message channel for this guild. If the channel isn't given, uses the current one.")]
        [Aliases("leavec", "setlc", "setleave")]
        [UsageExample("!guild setleavechannel")]
        [UsageExample("!guild setleavechannel #bb")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task SetLeaveChannelAsync(CommandContext ctx,
                                              [Description("Channel.")] DiscordChannel channel = null)
        {
            if (channel == null)
                channel = ctx.Channel;

            if (channel.Type != ChannelType.Text)
                throw new CommandFailedException("Given channel must be a text channel.");

            await DatabaseService.SetGuildLeaveChannelAsync(ctx.Guild.Id, channel.Id)
                .ConfigureAwait(false);
            await ReplySuccessAsync(ctx, $"Default leave message channel set to {Formatter.Bold(channel.Name)}.")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_GUILD_DELETEWELCOMECHANNEL
        [Command("deletewelcomechannel")]
        [Description("Remove welcome message channel for this guild.")]
        [Aliases("delwelcomec", "delwc", "delwelcome", "dwc", "deletewc")]
        [UsageExample("!guild deletewelcomechannel")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task RemoveWelcomeChannelAsync(CommandContext ctx)
        {
            await DatabaseService.RemoveGuildWelcomeChannelAsync(ctx.Guild.Id)
                .ConfigureAwait(false);
            await ReplySuccessAsync(ctx, "Default welcome message channel removed.")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_GUILD_DELETELEAVECHANNEL
        [Command("deleteleavechannel")]
        [Description("Remove leave message channel for this guild.")]
        [Aliases("delleavec", "dellc", "delleave", "dlc")]
        [UsageExample("!guild deletewelcomechannel")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task DeleteLeaveChannelAsync(CommandContext ctx)
        {
            await DatabaseService.RemoveGuildLeaveChannelAsync(ctx.Guild.Id)
                .ConfigureAwait(false);
            await ReplySuccessAsync(ctx, "Default leave message channel removed.")
                .ConfigureAwait(false);
        }
        #endregion

        #endregion


        
    }
}

