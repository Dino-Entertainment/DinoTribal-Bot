﻿#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Extensions;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
#endregion

namespace TheGodfather
{
    internal sealed partial class TheGodfatherShard
    {
        private async Task Client_ChannelCreated(ChannelCreateEventArgs e)
        {
            var logchn = await GetLogChannelForGuild(e.Guild.Id)
                .ConfigureAwait(false);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "Channel created",
                    Description = e.Channel.ToString(),
                    Color = DiscordColor.Aquamarine
                };

                var entry = await GetFirstLogEntryAsync(e.Guild, AuditLogActionType.ChannelCreate)
                    .ConfigureAwait(false);
                if (entry == null || !(entry is DiscordAuditLogChannelEntry centry)) {
                    emb.AddField("Error", "Failed to read audit log information. Please check my permissions");
                } else {
                    emb.AddField("User responsible", centry.UserResponsible.Mention, inline: true);
                    emb.AddField("Channel type", centry.Target.Type.ToString(), inline: true);
                    if (!string.IsNullOrWhiteSpace(centry.Reason))
                        emb.AddField("Reason", centry.Reason);
                    emb.WithFooter($"At {centry.CreationTimestamp.ToUniversalTime().ToString()} UTC", centry.UserResponsible.AvatarUrl);
                }

                await logchn.SendMessageAsync(embed: emb.Build())
                    .ConfigureAwait(false);
            }
        }

        private async Task Client_ChannelDeleted(ChannelDeleteEventArgs e)
        {
            var logchn = await GetLogChannelForGuild(e.Guild.Id)
                .ConfigureAwait(false);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "Channel deleted",
                    Description = e.Channel.ToString(),
                    Color = DiscordColor.Aquamarine
                };

                var entry = await GetFirstLogEntryAsync(e.Guild, AuditLogActionType.ChannelCreate)
                    .ConfigureAwait(false);
                if (entry == null || !(entry is DiscordAuditLogChannelEntry centry)) {
                    emb.AddField("Error", "Failed to read audit log information. Please check my permissions");
                } else {
                    emb.AddField("User responsible", centry.UserResponsible.Mention, inline: true);
                    emb.AddField("Channel type", centry.Target.Type.ToString(), inline: true);
                    if (!string.IsNullOrWhiteSpace(centry.Reason))
                        emb.AddField("Reason", centry.Reason);
                    emb.WithFooter($"At {centry.CreationTimestamp.ToUniversalTime().ToString()} UTC", centry.UserResponsible.AvatarUrl);
                }

                await logchn.SendMessageAsync(embed: emb.Build())
                    .ConfigureAwait(false);
            }
        }

        private async Task Client_ChannelPinsUpdated(ChannelPinsUpdateEventArgs e)
        {
            var logchn = await GetLogChannelForGuild(e.Channel.Guild.Id)
                .ConfigureAwait(false);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "Channel pins updated",
                    Description = e.Channel.ToString(),
                    Color = DiscordColor.Aquamarine
                };
                emb.AddField("Last pin timestamp", e.LastPinTimestamp.ToUniversalTime().ToString(), inline: true);
                await logchn.SendMessageAsync(embed: emb.Build())
                    .ConfigureAwait(false);
            }
        }

        private async Task Client_ChannelUpdated(ChannelUpdateEventArgs e)
        {
            if (e.ChannelBefore.Position != e.ChannelAfter.Position)
                return;

            var logchn = await GetLogChannelForGuild(e.Guild.Id)
                .ConfigureAwait(false);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "Channel updated",
                    Color = DiscordColor.Aquamarine
                };

                var entry = await GetFirstLogEntryAsync(e.Guild, AuditLogActionType.ChannelCreate)
                    .ConfigureAwait(false);
                if (entry == null || !(entry is DiscordAuditLogChannelEntry centry)) {
                    emb.AddField("Error", "Failed to read audit log information. Please check my permissions");
                    emb.AddField("Channel", e.ChannelBefore?.ToString() ?? "<unknown>");
                } else {
                    emb.WithDescription(centry.Target.ToString());
                    emb.AddField("User responsible", centry.UserResponsible.Mention, inline: true);
                    if (centry.BitrateChange != null)
                        emb.AddField("Bitrate changed to", centry.BitrateChange.After.Value.ToString(), inline: true);
                    if (centry.NameChange != null)
                        emb.AddField("Name changed to", centry.NameChange.After, inline: true);
                    if (centry.NsfwChange != null)
                        emb.AddField("NSFW flag changed to", centry.NsfwChange.After.Value.ToString(), inline: true);
                    if (centry.OverwriteChange != null)
                        emb.AddField("Permissions overwrites changed", $"{centry.OverwriteChange.After.Count} overwrites after changes");
                    if (centry.TopicChange != null)
                        emb.AddField("Topic changed to", centry.TopicChange.After);
                    if (centry.TypeChange.After.HasValue)
                        emb.AddField("Type changed to", centry.TypeChange.After.Value.ToString());
                    if (!string.IsNullOrWhiteSpace(centry.Reason))
                        emb.AddField("Reason", centry.Reason);
                    emb.WithFooter($"At {centry.CreationTimestamp.ToUniversalTime().ToString()} UTC", centry.UserResponsible.AvatarUrl);
                }

                await logchn.SendMessageAsync(embed: emb.Build())
                    .ConfigureAwait(false);
            }
        }

        private Task Client_Errored(ClientErrorEventArgs e)
        {
            Log(LogLevel.Critical, $"Client errored: {e.Exception.GetType()}: {e.Exception.Message}");
            return Task.CompletedTask;
        }

        private Task Client_GuildAvailable(GuildCreateEventArgs e)
        {
            Log(LogLevel.Info, $"Guild available: {e.Guild.ToString()}");
            return Task.CompletedTask;
        }

        private async Task Client_GuildBanAdded(GuildBanAddEventArgs e)
        {
            var logchn = await GetLogChannelForGuild(e.Guild.Id)
                .ConfigureAwait(false);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "Member banned",
                    Color = DiscordColor.DarkRed
                };

                var entry = await GetFirstLogEntryAsync(e.Guild, AuditLogActionType.Ban)
                    .ConfigureAwait(false);
                if (entry == null || !(entry is DiscordAuditLogBanEntry bentry)) {
                    emb.AddField("Error", "Failed to read audit log information. Please check my permissions");
                    emb.AddField("Member", e.Member?.ToString() ?? "<unknown>");
                } else {
                    emb.WithDescription(bentry.Target.ToString());
                    emb.AddField("User responsible", bentry.UserResponsible.Mention, inline: true);
                    if (!string.IsNullOrWhiteSpace(bentry.Reason))
                        emb.AddField("Reason", bentry.Reason);
                    emb.WithFooter($"At {bentry.CreationTimestamp.ToUniversalTime().ToString()} UTC", bentry.UserResponsible.AvatarUrl);
                }

                await logchn.SendMessageAsync(embed: emb.Build())
                    .ConfigureAwait(false);
            }
        }

        private async Task Client_GuildBanRemoved(GuildBanRemoveEventArgs e)
        {
            var logchn = await GetLogChannelForGuild(e.Guild.Id)
                .ConfigureAwait(false);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "Member unbanned",
                    Color = DiscordColor.DarkRed
                };

                var entry = await GetFirstLogEntryAsync(e.Guild, AuditLogActionType.Unban)
                    .ConfigureAwait(false);
                if (entry == null || !(entry is DiscordAuditLogBanEntry bentry)) {
                    emb.AddField("Error", "Failed to read audit log information. Please check my permissions");
                    emb.AddField("Member", e.Member?.ToString() ?? "<unknown>");
                } else {
                    emb.WithDescription(bentry.Target.ToString());
                    emb.AddField("User responsible", bentry.UserResponsible.Mention, inline: true);
                    if (!string.IsNullOrWhiteSpace(bentry.Reason))
                        emb.AddField("Reason", bentry.Reason);
                    emb.WithFooter($"At {bentry.CreationTimestamp.ToUniversalTime().ToString()} UTC", bentry.UserResponsible.AvatarUrl);
                }
                await logchn.SendMessageAsync(embed: emb.Build())
                    .ConfigureAwait(false);
            }
        }

        private async Task Client_GuildCreated(GuildCreateEventArgs e)
        {
            Log(LogLevel.Info, $"Joined guild: {e.Guild.ToString()}");

            await _db.RegisterGuildAsync(e.Guild.Id)
                .ConfigureAwait(false);
            _shared.GuildConfigurations.TryAdd(e.Guild.Id, PartialGuildConfig.Default);

            var emoji = DiscordEmoji.FromName(e.Client, ":small_blue_diamond:");
            await e.Guild.GetDefaultChannel().SendIconEmbedAsync(
                $"{Formatter.Bold("Thank you for adding me!")}\n\n" +
                $"{emoji} The default prefix for commands is {Formatter.Bold(_shared.BotConfiguration.DefaultPrefix)}, but it can be changed using {Formatter.Bold("prefix")} command.\n" +
                $"{emoji} I advise you to run the configuration wizard for this guild in order to quickly configure functions like logging, notifications etc. The wizard can be invoked using {Formatter.Bold("guild config setup")} command.\n" +
                $"{emoji} You can use the {Formatter.Bold("help")} command as a guide, though it is recommended to read the documentation @ https://github.com/ivan-ristovic/the-godfather\n" +
                $"{emoji} If you have any questions or problems, feel free to use the {Formatter.Bold("report")} command in order send a message to the bot owner ({e.Client.CurrentApplication.Owner.Username}#{e.Client.CurrentApplication.Owner.Discriminator}). Alternatively, you can create an issue on GitHub or join WorldMafia discord server for quick support (https://discord.me/worldmafia).\n"
                , StaticDiscordEmoji.Wave
            ).ConfigureAwait(false);
        }

        private async Task Client_GuildDeleted(GuildDeleteEventArgs e)
        {
            Log(LogLevel.Info, $"Left guild: {e.Guild.ToString()}");

            await _db.UnregisterGuildAsync(e.Guild.Id)
                .ConfigureAwait(false);
            _shared.GuildConfigurations.TryRemove(e.Guild.Id, out _);
        }

        private async Task Client_GuildEmojisUpdated(GuildEmojisUpdateEventArgs e)
        {
            var logchn = await GetLogChannelForGuild(e.Guild.Id)
                .ConfigureAwait(false);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Color = DiscordColor.Gold
                };

                DiscordAuditLogEntry entry = null;
                if (e.EmojisAfter.Count > e.EmojisBefore.Count)
                    entry = await GetFirstLogEntryAsync(e.Guild, AuditLogActionType.EmojiCreate).ConfigureAwait(false);
                else if (e.EmojisAfter.Count < e.EmojisBefore.Count)
                    entry = await GetFirstLogEntryAsync(e.Guild, AuditLogActionType.EmojiDelete).ConfigureAwait(false);
                else
                    entry = await GetFirstLogEntryAsync(e.Guild, AuditLogActionType.EmojiUpdate).ConfigureAwait(false);
                if (entry == null || !(entry is DiscordAuditLogEmojiEntry eentry)) {
                    emb.WithTitle($"Guild emojis updated");
                    emb.AddField("Error", "Failed to read audit log information. Please check my permissions");
                    emb.AddField("Emojis before", e.EmojisBefore?.Count.ToString() ?? "<unknown>", inline: true);
                    emb.AddField("Emojis after", e.EmojisAfter?.Count.ToString() ?? "<unknown>", inline: true);
                } else {
                    emb.WithTitle($"Guild emoji acton occured: {eentry.ActionCategory.ToString()}");
                    emb.WithDescription(eentry.Target?.ToString() ?? "No description provided");
                    emb.AddField("User responsible", eentry.UserResponsible.Mention, inline: true);
                    if (eentry.NameChange != null)
                        emb.AddField("Name changes", $"{eentry.NameChange.Before ?? "None"} -> {eentry.NameChange.After ?? "None"}", inline: true);
                    if (!string.IsNullOrWhiteSpace(eentry.Reason))
                        emb.AddField("Reason", eentry.Reason);
                    emb.WithFooter($"At {eentry.CreationTimestamp.ToUniversalTime().ToString()} UTC", eentry.UserResponsible.AvatarUrl);
                }

                await logchn.SendMessageAsync(embed: emb.Build())
                    .ConfigureAwait(false);
            }
        }

        private async Task Client_GuildIntegrationsUpdated(GuildIntegrationsUpdateEventArgs e)
        {
            var logchn = await GetLogChannelForGuild(e.Guild.Id)
                .ConfigureAwait(false);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "Guild integrations updated",
                    Color = DiscordColor.DarkGreen
                };
                await logchn.SendMessageAsync(embed: emb.Build())
                    .ConfigureAwait(false);
            }
        }

        private async Task Client_GuildMemberAdded(GuildMemberAddEventArgs e)
        {
            if (!TheGodfather.Listening)
                return;

            Log(LogLevel.Info,
                $"Member joined: {e.Member.ToString()}<br>" +
                $"{e.Guild.ToString()}"
            );

            try {
                var cid = await _db.GetWelcomeChannelIdAsync(e.Guild.Id)
                    .ConfigureAwait(false);
                if (cid != 0) {
                    try {
                        var chn = e.Guild.GetChannel(cid);
                        if (chn != null) {
                            var msg = await _db.GetWelcomeMessageAsync(e.Guild.Id)
                                .ConfigureAwait(false);
                            if (string.IsNullOrWhiteSpace(msg))
                                await chn.SendIconEmbedAsync($"Welcome to {Formatter.Bold(e.Guild.Name)}, {e.Member.Mention}!", DiscordEmoji.FromName(Client, ":wave:")).ConfigureAwait(false);
                            else
                                await chn.SendIconEmbedAsync(msg.Replace("%user%", e.Member.Mention), DiscordEmoji.FromName(Client, ":wave:")).ConfigureAwait(false);
                        }
                    } catch (Exception exc) {
                        while (exc is AggregateException)
                            exc = exc.InnerException;
                        Log(LogLevel.Debug,
                            $"Failed to send a welcome message!<br>" +
                            $"Channel ID: {cid}<br>" +
                            $"{e.Guild.ToString()}<br>" +
                            $"Exception: {exc.GetType()}<br>" +
                            $"Message: {exc.Message}"
                        );
                        if (exc is NotFoundException)
                            await _db.RemoveWelcomeChannelAsync(e.Guild.Id)
                                .ConfigureAwait(false);
                    }
                }
            } catch (Exception exc) {
                TheGodfather.LogHandle.LogException(LogLevel.Debug, exc);
            }

            try {
                var rids = await _db.GetAutomaticRolesForGuildAsync(e.Guild.Id)
                    .ConfigureAwait(false);
                foreach (var rid in rids) {
                    try {
                        var role = e.Guild.GetRole(rid);
                        if (role == null) {
                            await _db.RemoveAutomaticRoleAsync(e.Guild.Id, rid)
                                .ConfigureAwait(false);
                        } else {
                            await e.Member.GrantRoleAsync(role)
                                .ConfigureAwait(false);
                        }
                    } catch (Exception exc) {
                        Log(LogLevel.Debug,
                            $"Failed to assign an automatic role to a new member!<br>" +
                            $"{e.Guild.ToString()}<br>" +
                            $"Exception: {exc.GetType()}<br>" +
                            $"Message: {exc.Message}"
                        );
                    }
                }
            } catch (Exception exc) {
                TheGodfather.LogHandle.LogException(LogLevel.Debug, exc);
            }

            var logchn = await GetLogChannelForGuild(e.Guild.Id)
                .ConfigureAwait(false);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "Member joined",
                    Description = e.Member.ToString(),
                    Color = DiscordColor.White,
                    ThumbnailUrl = e.Member.AvatarUrl
                };
                emb.AddField("Registered at", $"{e.Member.CreationTimestamp.ToUniversalTime().ToString()} UTC", inline: true);
                if (!string.IsNullOrWhiteSpace(e.Member.Email))
                    emb.AddField("Email", e.Member.Email);

                await logchn.SendMessageAsync(embed: emb.Build())
                    .ConfigureAwait(false);
            }
        }

        private async Task Client_GuildMemberRemoved(GuildMemberRemoveEventArgs e)
        {
            if (!TheGodfather.Listening || e.Member.Id == e.Client.CurrentUser.Id)
                return;

            Log(LogLevel.Info,
                $"Member left: {e.Member.ToString()}<br>" +
                e.Guild.ToString()
            );

            ulong cid = 0;
            try {
                cid = await _db.GetLeaveChannelIdAsync(e.Guild.Id)
                    .ConfigureAwait(false);
            } catch (Exception exc) {
                TheGodfather.LogHandle.LogException(LogLevel.Debug, exc);
            }

            if (cid == 0)
                return;

            try {
                var chn = e.Guild.GetChannel(cid);
                if (chn != null) {
                    var msg = await _db.GetLeaveMessageAsync(e.Guild.Id)
                        .ConfigureAwait(false);
                    if (string.IsNullOrWhiteSpace(msg))
                        await chn.SendIconEmbedAsync($"{Formatter.Bold(e.Member?.Username ?? "<unknown>")} left the server! Bye!", StaticDiscordEmoji.Wave).ConfigureAwait(false);
                    else
                        await chn.SendIconEmbedAsync(msg.Replace("%user%", e.Member.Mention), DiscordEmoji.FromName(Client, ":wave:")).ConfigureAwait(false);
                }
            } catch (Exception exc) {
                while (exc is AggregateException)
                    exc = exc.InnerException;
                Log(LogLevel.Debug,
                    $"Failed to send a leaving message!<br>" +
                    $"Channel ID: {cid}<br>" +
                    $"{e.Guild.ToString()}<br>" +
                    $"Exception: {exc.GetType()}<br>" +
                    $"Message: {exc.Message}"
                );
                if (exc is NotFoundException)
                    await _db.RemoveLeaveChannelAsync(e.Guild.Id)
                        .ConfigureAwait(false);
            }

            var logchn = await GetLogChannelForGuild(e.Guild.Id)
                .ConfigureAwait(false);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "Member left",
                    Description = e.Member.ToString(),
                    Color = DiscordColor.White,
                    ThumbnailUrl = e.Member.AvatarUrl
                };
                emb.AddField("Registered at", $"{e.Member.CreationTimestamp.ToUniversalTime().ToString()} UTC", inline: true);
                if (!string.IsNullOrWhiteSpace(e.Member.Email))
                    emb.AddField("Email", e.Member.Email);

                await logchn.SendMessageAsync(embed: emb.Build())
                    .ConfigureAwait(false);
            }
        }

        private async Task Client_GuildMemberUpdated(GuildMemberUpdateEventArgs e)
        {
            var logchn = await GetLogChannelForGuild(e.Guild.Id)
                .ConfigureAwait(false);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "Member updated",
                    Description = e.Member.ToString(),
                    Color = DiscordColor.White,
                    ThumbnailUrl = e.Member.AvatarUrl
                };

                DiscordAuditLogEntry entry = null;
                if (e.RolesBefore.Count == e.RolesAfter.Count)
                    entry = await GetFirstLogEntryAsync(e.Guild, AuditLogActionType.MemberUpdate).ConfigureAwait(false);
                else
                    entry = await GetFirstLogEntryAsync(e.Guild, AuditLogActionType.MemberRoleUpdate).ConfigureAwait(false);
                if (entry == null || !(entry is DiscordAuditLogMemberUpdateEntry mentry)) {
                    emb.AddField("Error", "Failed to read audit log information. Please check my permissions");
                    emb.AddField("Name before", e.NicknameBefore ?? "<unknown>", inline: true);
                    emb.AddField("Name after", e.NicknameAfter ?? "<unknown>", inline: true);
                    emb.AddField("Roles before", e.RolesBefore?.Count.ToString() ?? "<unknown>", inline: true);
                    emb.AddField("Roles after", e.RolesAfter?.Count.ToString() ?? "<unknown>", inline: true);
                } else {
                    emb.AddField("User responsible", mentry.UserResponsible.Mention, inline: true);
                    if (mentry.NicknameChange != null)
                        emb.AddField("Nickname change", $"{mentry.NicknameChange.Before} -> {mentry.NicknameChange.After}", inline: true);
                    if (mentry.AddedRoles != null && mentry.AddedRoles.Any())
                        emb.AddField("Added roles", string.Join(",", mentry.AddedRoles.Select(r => r.Name)), inline: true);
                    if (mentry.RemovedRoles != null && mentry.RemovedRoles.Any())
                        emb.AddField("Removed roles", string.Join(",", mentry.RemovedRoles.Select(r => r.Name)), inline: true);
                    if (!string.IsNullOrWhiteSpace(mentry.Reason))
                        emb.AddField("Reason", mentry.Reason);
                    emb.WithFooter($"At {mentry.CreationTimestamp.ToUniversalTime().ToString()} UTC", mentry.UserResponsible.AvatarUrl);
                }

                await logchn.SendMessageAsync(embed: emb.Build())
                    .ConfigureAwait(false);
            }
        }

        private async Task Client_GuildRoleCreated(GuildRoleCreateEventArgs e)
        {
            var logchn = await GetLogChannelForGuild(e.Guild.Id)
                .ConfigureAwait(false);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "Role created",
                    Description = e.Role.ToString(),
                    Color = DiscordColor.Magenta,
                };

                var entry = await GetFirstLogEntryAsync(e.Guild, AuditLogActionType.RoleCreate)
                    .ConfigureAwait(false);
                if (entry == null || !(entry is DiscordAuditLogRoleUpdateEntry rentry)) {
                    emb.AddField("Error", "Failed to read audit log information. Please check my permissions");
                } else {
                    emb.AddField("User responsible", rentry.UserResponsible.Mention, inline: true);
                    if (rentry.NameChange != null)
                        emb.AddField("Name change", $"{rentry.NameChange.Before ?? "unknown"} -> {rentry.NameChange.After ?? "unknown"}", inline: true);
                    if (rentry.ColorChange != null)
                        emb.AddField("Color changed", $"{rentry.ColorChange.Before?.ToString() ?? "unknown"} -> {rentry.ColorChange.After?.ToString() ?? "unknown"}", inline: true);
                    if (rentry.HoistChange != null)
                        emb.AddField("Hoist", rentry.HoistChange.After?.ToString() ?? "unknown", inline: true);
                    if (rentry.MentionableChange != null)
                        emb.AddField("Mentionable", rentry.MentionableChange.After?.ToString() ?? "unknown", inline: true);
                    if (rentry.PermissionChange != null)
                        emb.AddField("Permissions changed to", rentry.PermissionChange.After?.ToPermissionString() ?? "unknown", inline: true);
                    if (rentry.PositionChange != null)
                        emb.AddField("Position changed to", rentry.PositionChange.After?.ToString() ?? "unknown", inline: true);
                    if (!string.IsNullOrWhiteSpace(rentry.Reason))
                        emb.AddField("Reason", rentry.Reason);
                    emb.WithFooter($"At {rentry.CreationTimestamp.ToUniversalTime().ToString()} UTC", rentry.UserResponsible.AvatarUrl);
                }

                await logchn.SendMessageAsync(embed: emb.Build())
                    .ConfigureAwait(false);
            }
        }

        private async Task Client_GuildRoleDeleted(GuildRoleDeleteEventArgs e)
        {
            var logchn = await GetLogChannelForGuild(e.Guild.Id)
                .ConfigureAwait(false);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "Role deleted",
                    Description = e.Role.ToString(),
                    Color = DiscordColor.Magenta,
                };

                var entry = await GetFirstLogEntryAsync(e.Guild, AuditLogActionType.RoleDelete)
                    .ConfigureAwait(false);
                if (entry == null || !(entry is DiscordAuditLogRoleUpdateEntry rentry)) {
                    emb.AddField("Error", "Failed to read audit log information. Please check my permissions");
                } else {
                    emb.AddField("User responsible", rentry.UserResponsible.Mention, inline: true);
                    if (rentry.NameChange != null)
                        emb.AddField("Name change", $"{rentry.NameChange.Before ?? "unknown"} -> {rentry.NameChange.After ?? "unknown"}", inline: true);
                    if (rentry.ColorChange != null)
                        emb.AddField("Color changed", $"{rentry.ColorChange.Before?.ToString() ?? "unknown"} -> {rentry.ColorChange.After?.ToString() ?? "unknown"}", inline: true);
                    if (rentry.HoistChange != null)
                        emb.AddField("Hoist", rentry.HoistChange.After?.ToString() ?? "unknown", inline: true);
                    if (rentry.MentionableChange != null)
                        emb.AddField("Mentionable", rentry.MentionableChange.After?.ToString() ?? "unknown", inline: true);
                    if (rentry.PermissionChange != null)
                        emb.AddField("Permissions changed to", rentry.PermissionChange.After?.ToPermissionString() ?? "unknown", inline: true);
                    if (rentry.PositionChange != null)
                        emb.AddField("Position changed to", rentry.PositionChange.After?.ToString() ?? "unknown", inline: true);
                    if (!string.IsNullOrWhiteSpace(rentry.Reason))
                        emb.AddField("Reason", rentry.Reason);
                    emb.WithFooter($"At {rentry.CreationTimestamp.ToUniversalTime().ToString()} UTC", rentry.UserResponsible.AvatarUrl);
                }

                await logchn.SendMessageAsync(embed: emb.Build())
                    .ConfigureAwait(false);
            }
        }

        private async Task Client_GuildRoleUpdated(GuildRoleUpdateEventArgs e)
        {
            if (e.RoleBefore.Position != e.RoleAfter.Position)
                return;

            var logchn = await GetLogChannelForGuild(e.Guild.Id)
                .ConfigureAwait(false);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "Role updated",
                    Color = DiscordColor.Magenta,
                };

                var entry = await GetFirstLogEntryAsync(e.Guild, AuditLogActionType.RoleUpdate)
                    .ConfigureAwait(false);
                if (entry == null || !(entry is DiscordAuditLogRoleUpdateEntry rentry)) {
                    emb.AddField("Error", "Failed to read audit log information. Please check my permissions");
                    emb.AddField("Role", e.RoleBefore?.ToString() ?? "<unknown>");
                } else {
                    emb.WithDescription(rentry.Target.ToString());
                    emb.AddField("User responsible", rentry.UserResponsible.Mention, inline: true);
                    if (rentry.NameChange != null)
                        emb.AddField("Name change", $"{rentry.NameChange.Before ?? "unknown"} -> {rentry.NameChange.After ?? "unknown"}", inline: true);
                    if (rentry.ColorChange != null)
                        emb.AddField("Color changed", $"{rentry.ColorChange.Before?.ToString() ?? "unknown"} -> {rentry.ColorChange.After?.ToString() ?? "unknown"}", inline: true);
                    if (rentry.HoistChange != null)
                        emb.AddField("Hoist", rentry.HoistChange.After?.ToString() ?? "unknown", inline: true);
                    if (rentry.MentionableChange != null)
                        emb.AddField("Mentionable", rentry.MentionableChange.After?.ToString() ?? "unknown", inline: true);
                    if (rentry.PermissionChange != null)
                        emb.AddField("Permissions changed to", rentry.PermissionChange.After?.ToPermissionString() ?? "unknown", inline: true);
                    if (rentry.PositionChange != null)
                        emb.AddField("Position changed to", rentry.PositionChange.After?.ToString() ?? "unknown", inline: true);
                    if (!string.IsNullOrWhiteSpace(rentry.Reason))
                        emb.AddField("Reason", rentry.Reason);
                    emb.WithFooter($"At {rentry.CreationTimestamp.ToUniversalTime().ToString()} UTC", rentry.UserResponsible.AvatarUrl);
                }

                await logchn.SendMessageAsync(embed: emb.Build())
                    .ConfigureAwait(false);
            }
        }

        private Task Client_GuildUnavailable(GuildDeleteEventArgs e)
        {
            Log(LogLevel.Info, $"Guild unavailable: {e.Guild.ToString()}");
            return Task.CompletedTask;
        }

        private async Task Client_GuildUpdated(GuildUpdateEventArgs e)
        {
            var logchn = await GetLogChannelForGuild(e.Guild.Id)
                .ConfigureAwait(false);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "Guild settings updated",
                    Color = DiscordColor.Magenta,
                };

                var entry = await GetFirstLogEntryAsync(e.Guild, AuditLogActionType.GuildUpdate)
                    .ConfigureAwait(false);
                if (entry == null || !(entry is DiscordAuditLogGuildEntry gentry)) {
                    emb.AddField("Error", "Failed to read audit log information. Please check my permissions");
                } else {
                    emb.AddField("User responsible", gentry.UserResponsible.Mention, inline: true);
                    if (gentry.NameChange != null)
                        emb.AddField("Name change", $"{gentry.NameChange.Before ?? "unknown"} -> {gentry.NameChange.After ?? "unknown"}", inline: true);
                    if (gentry.AfkChannelChange != null)
                        emb.AddField("AFK channel changed to", gentry.AfkChannelChange.After?.ToString() ?? "unknown", inline: true);
                    if (gentry.EmbedChannelChange != null)
                        emb.AddField("Embed channel changed to", gentry.EmbedChannelChange.After?.ToString() ?? "unknown", inline: true);
                    if (gentry.IconChange != null)
                        emb.AddField("Icon changed to", gentry.IconChange.After ?? "unknown", inline: true);
                    if (gentry.NotificationSettingsChange != null)
                        emb.AddField("Notifications changed to", gentry.NotificationSettingsChange.After.HasFlag(DefaultMessageNotifications.AllMessages) ? "All messages" : "Mentions only", inline: true);
                    if (gentry.OwnerChange != null)
                        emb.AddField("Owner changed to", gentry.OwnerChange.After?.ToString() ?? "unknown", inline: true);
                    if (!string.IsNullOrWhiteSpace(gentry.Reason))
                        emb.AddField("Reason", gentry.Reason);
                    emb.WithFooter($"At {gentry.CreationTimestamp.ToUniversalTime().ToString()} UTC", gentry.UserResponsible.AvatarUrl);
                }

                await logchn.SendMessageAsync(embed: emb.Build())
                    .ConfigureAwait(false);
            }
        }

        private async Task Client_MessagesBulkDeleted(MessageBulkDeleteEventArgs e)
        {
            var logchn = await GetLogChannelForGuild(e.Channel.Guild.Id)
                   .ConfigureAwait(false);
            if (logchn != null) {
                await logchn.SendMessageAsync(embed: new DiscordEmbedBuilder() {
                    Title = $"Bulk message deletion occured ({e.Messages.Count} total)",
                    Description = $"In channel {e.Channel.Mention}",
                    Color = DiscordColor.SpringGreen
                }).ConfigureAwait(false);
            }
        }

        private async Task Client_MessageCreated(MessageCreateEventArgs e)
        {
            if (e.Author.IsBot || !TheGodfather.Listening)
                return;

            if (e.Channel.IsPrivate) {
                Log(LogLevel.Info, $"Ignored DM from {e.Author.ToString()}:<br>{e.Message}");
                return;
            }

            if (_shared.BlockedChannels.Contains(e.Channel.Id))
                return;

            // Check if message contains filter
            if (e.Message.Content != null && _shared.MessageContainsFilter(e.Guild.Id, e.Message.Content)) {
                try {
                    await e.Channel.DeleteMessageAsync(e.Message, "_gf: Filter hit")
                        .ConfigureAwait(false);
                    Log(LogLevel.Debug,
                        $"Filter triggered in message: {e.Message.Content.Replace('\n', ' ')}<br>" +
                        $"{e.Message.Author.ToString()}<br>" +
                        $"{e.Guild.ToString()} | {e.Channel.ToString()}"
                    );
                } catch (UnauthorizedException) {
                    Log(LogLevel.Debug,
                        $"Filter triggered in message but missing permissions to delete!<br>" +
                        $"Message: {e.Message.Content.Replace('\n', ' ')}<br>" +
                        $"{e.Message.Author.ToString()}<br>" +
                        $"{e.Guild.ToString()} | {e.Channel.ToString()}"
                    );
                }
                return;
            }

            // If the user is blocked, ignore
            if (_shared.BlockedUsers.Contains(e.Author.Id))
                return;

            // Since below actions require SendMessages permission, checking it now
            if (!e.Channel.PermissionsFor(e.Guild.CurrentMember).HasFlag(Permissions.SendMessages))
                return;

            // Update message count for the user that sent the message
            int rank = _shared.UpdateMessageCount(e.Author.Id);
            if (rank != -1) {
                var ranks = _shared.Ranks;
                await e.Channel.SendIconEmbedAsync($"GG {e.Author.Mention}! You have advanced to level {rank} ({(rank < ranks.Count ? ranks[rank] : "Low")})!", DiscordEmoji.FromName(Client, ":military_medal:"))
                    .ConfigureAwait(false);
            }

            // Check if message has a text reaction
            if (_shared.TextReactions.ContainsKey(e.Guild.Id)) {
                var tr = _shared.TextReactions[e.Guild.Id]?.FirstOrDefault(r => r.Matches(e.Message.Content));
                if (tr != null && tr.CanSend()) {
                    Log(LogLevel.Debug,
                        $"Text reaction detected: {tr.Response}<br>" +
                        $"Message: {e.Message.Content.Replace('\n', ' ')}<br>" +
                        $"{e.Message.Author.ToString()}<br>" +
                        $"{e.Guild.ToString()} | {e.Channel.ToString()}"
                    );
                    await e.Channel.SendMessageAsync(tr.Response.Replace("%user%", e.Author.Mention))
                        .ConfigureAwait(false);
                }
            }

            if (!e.Channel.PermissionsFor(e.Guild.CurrentMember).HasFlag(Permissions.AddReactions))
                return;

            // Check if message has an emoji reaction
            if (_shared.EmojiReactions.ContainsKey(e.Guild.Id)) {
                var ereactions = _shared.EmojiReactions[e.Guild.Id].Where(er => er.Matches(e.Message.Content));
                foreach (var er in ereactions) {
                    Log(LogLevel.Debug,
                        $"Emoji reaction detected: {er.Response}<br>" +
                        $"Message: {e.Message.Content.Replace('\n', ' ')}<br>" +
                        $"{e.Message.Author.ToString()}<br>" +
                        $"{e.Guild.ToString()} | {e.Channel.ToString()}"
                    );
                    try {
                        var emoji = DiscordEmoji.FromName(Client, er.Response);
                        await e.Message.CreateReactionAsync(emoji)
                            .ConfigureAwait(false);
                    } catch (ArgumentException) {
                        await _db.RemoveAllEmojiReactionTriggersForReactionAsync(e.Guild.Id, er.Response)
                            .ConfigureAwait(false);
                    } catch (UnauthorizedException) {
                        Log(LogLevel.Debug,
                            $"Emoji reaction trigger found but missing permissions to add reactions!<br>" +
                            $"Message: '{e.Message.Content.Replace('\n', ' ')}<br>" +
                            $"{e.Message.Author.ToString()}<br>" +
                            $"{e.Guild.ToString()} | {e.Channel.ToString()}"
                        );
                        break;
                    }
                }
            }
        }

        private async Task Client_MessageDeleted(MessageDeleteEventArgs e)
        {
            if (e.Channel.IsPrivate)
                return;

            var logchn = await GetLogChannelForGuild(e.Guild.Id)
                .ConfigureAwait(false);
            if (logchn != null && e.Message != null) {
                var emb = new DiscordEmbedBuilder() {
                    Description = $"From {e.Message.Author.ToString()}",
                    Color = DiscordColor.SpringGreen
                };

                var entry = await GetFirstLogEntryAsync(e.Guild, AuditLogActionType.MessageDelete)
                    .ConfigureAwait(false);
                if (entry == null || !(entry is DiscordAuditLogMessageEntry mentry)) {
                    emb.AddField("Error", "Failed to read audit log information. Please check my permissions");
                    emb.WithTitle($"Messages deleted");
                } else {
                    emb.WithTitle($"Messages deleted ({mentry.MessageCount ?? 1} total)");
                    emb.AddField("User responsible", mentry.UserResponsible.Mention, inline: true);
                    if (!string.IsNullOrWhiteSpace(mentry.Reason))
                        emb.AddField("Reason", mentry.Reason);
                    else if (_shared.MessageContainsFilter(e.Guild.Id, e.Message.Content))
                        emb.AddField("Reason", "Filter triggered");
                    emb.WithFooter($"At {mentry.CreationTimestamp.ToUniversalTime().ToString()} UTC", mentry.UserResponsible.AvatarUrl);
                }
                
                if (e.Message.Embeds.Count > 0)
                    emb.AddField("Embeds", e.Message.Embeds.Count.ToString(), inline: true);
                if (e.Message.Reactions.Count > 0)
                    emb.AddField("Reactions", e.Message.Reactions.Count.ToString(), inline: true);
                if (e.Message.Attachments.Count > 0)
                    emb.AddField("Attachments", e.Message.Attachments.Count.ToString(), inline: true);
                emb.AddField("Created at", e.Message.CreationTimestamp != null ? e.Message.CreationTimestamp.ToUniversalTime().ToString() : "<unknown timestamp>", inline: true);
                emb.AddField("Content", $"{Formatter.BlockCode(string.IsNullOrWhiteSpace(e.Message.Content) ? "<empty content>" : e.Message.Content)}");

                await logchn.SendMessageAsync(embed: emb.Build())
                    .ConfigureAwait(false);
            }
        }

        private async Task Client_MessageUpdated(MessageUpdateEventArgs e)
        {
            if (e.Author == null || e.Message == null || !TheGodfather.Listening || e.Channel.IsPrivate)
                return;

            if (_shared.BlockedChannels.Contains(e.Channel.Id))
                return;

            // Check if message contains filter
            if (!e.Author.IsBot && e.Message.Content != null && _shared.MessageContainsFilter(e.Guild.Id, e.Message.Content)) {
                try {
                    await e.Channel.DeleteMessageAsync(e.Message, "_gf: Filter hit after update")
                        .ConfigureAwait(false);

                    Log(LogLevel.Debug,
                        $"Filter triggered after message edit:<br>" +
                        $"Message: {e.Message.Content.Replace('\n', ' ')}<br>" +
                        $"{e.Message.Author.ToString()}<br>" +
                        $"{e.Guild.ToString()} | {e.Channel.ToString()}"
                    );
                } catch (UnauthorizedException) {
                    Log(LogLevel.Debug,
                        $"Filter triggered in edited message but missing permissions to delete!<br>" +
                        $"Message: '{e.Message.Content.Replace('\n', ' ')}<br>" +
                        $"{e.Message.Author.ToString()}<br>" +
                        $"{e.Guild.ToString()} | {e.Channel.ToString()}"
                    );
                }
            }

            try {
                var logchn = await GetLogChannelForGuild(e.Guild.Id)
                    .ConfigureAwait(false);
                if (logchn != null && !e.Author.IsBot && e.Message.EditedTimestamp != null) {
                    var detailspre = $"{Formatter.BlockCode(string.IsNullOrWhiteSpace(e.MessageBefore?.Content) ? "<empty content>" : e.MessageBefore.Content)}\nCreated at: {(e.Message.CreationTimestamp != null ? e.Message.CreationTimestamp.ToUniversalTime().ToString() : "<unknown>")}, embeds: {e.MessageBefore.Embeds.Count}, reactions: {e.MessageBefore.Reactions.Count}, attachments: {e.MessageBefore.Attachments.Count}";
                    var detailsafter = $"{Formatter.BlockCode(string.IsNullOrWhiteSpace(e.Message?.Content) ? "<empty content>" : e.Message.Content)}\nEdited at: {(e.Message.EditedTimestamp != null ? e.Message.EditedTimestamp.ToUniversalTime().ToString() : "<unknown>")}, embeds: {e.Message.Embeds.Count}, reactions: {e.Message.Reactions.Count}, attachments: {e.Message.Attachments.Count}";
                    await logchn.SendMessageAsync(embed: new DiscordEmbedBuilder() {
                        Title = "Message updated",
                        Description = $"In channel {e.Channel.Mention}\n\nBefore update: {detailspre}\n\nAfter update: {detailsafter}",
                        Color = DiscordColor.SpringGreen
                    }).ConfigureAwait(false);
                }
            } catch {

            }
        }

        private async Task Client_VoiceServerUpdated(VoiceServerUpdateEventArgs e)
        {
            var logchn = await GetLogChannelForGuild(e.Guild.Id)
                .ConfigureAwait(false);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "Voice server updated",
                    Color = DiscordColor.DarkGray
                };
                emb.AddField("Endpoint", Formatter.Bold(e.Endpoint));

                await logchn.SendMessageAsync(embed: emb.Build())
                    .ConfigureAwait(false);
            }
        }

        private async Task Client_WebhooksUpdated(WebhooksUpdateEventArgs e)
        {
            var logchn = await GetLogChannelForGuild(e.Guild.Id)
                .ConfigureAwait(false);
            if (logchn != null) {
                await logchn.SendMessageAsync(embed: new DiscordEmbedBuilder() {
                    Title = "Webhooks updated",
                    Description = $"For {e.Channel.ToString()}",
                    Color = DiscordColor.DarkGray
                }.Build()).ConfigureAwait(false);
            }
        }

        private async Task<DiscordAuditLogEntry> GetFirstLogEntryAsync(DiscordGuild guild, AuditLogActionType type)
        {
            try {
                var entries = await guild.GetAuditLogsAsync(1, action_type: type)
                    .ConfigureAwait(false);
                return entries.Any() ? entries.FirstOrDefault() : null;
            } catch {
                return null;
            }
        }
    }
}
