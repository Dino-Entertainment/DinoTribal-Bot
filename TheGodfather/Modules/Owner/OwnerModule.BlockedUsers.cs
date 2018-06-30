﻿#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
#endregion

namespace TheGodfather.Modules.Owner
{
    public partial class OwnerModule
    {
        [Group("blockedusers"), Module(ModuleType.Owner)]
        [Description("Manipulate blocked users. Bot will not allow blocked users to invoke commands and will not react (either with text or emoji) to their messages.")]
        [Aliases("bu", "blockedu", "blockuser", "busers", "buser", "busr")]
        [RequirePriviledgedUser]
        [NotBlocked]
        public class BlockedUsersModule : TheGodfatherBaseModule
        {

            public BlockedUsersModule(SharedData shared, DBService db) : base(shared, db) { }


            [GroupCommand, Priority(3)]
            public Task ExecuteGroupAsync(CommandContext ctx)
                => ListAsync(ctx);

            [GroupCommand, Priority(2)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description("Users to block.")] params DiscordUser[] users)
                => AddAsync(ctx, null, users);

            [GroupCommand, Priority(1)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description("Reason (max 60 chars).")] string reason,
                                         [Description("Users to block.")] params DiscordUser[] users)
                => AddAsync(ctx, reason, users);

            [GroupCommand, Priority(0)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description("Users to block.")] DiscordUser user,
                                         [RemainingText, Description("Reason (max 60 chars).")] string reason)
                => AddAsync(ctx, reason, user);


            #region COMMAND_BLOCKEDUSERS_ADD
            [Command("add"), Priority(2)]
            [Module(ModuleType.Owner)]
            [Description("Add users to blocked users list.")]
            [Aliases("+", "a")]
            [UsageExample("!owner blockedusers add @Someone")]
            [UsageExample("!owner blockedusers add @Someone Troublemaker and spammer")]
            [UsageExample("!owner blockedusers add 123123123123123")]
            [UsageExample("!owner blockedusers add @Someone 123123123123123")]
            [UsageExample("!owner blockedusers add \"This is some reason\" @Someone 123123123123123")]
            public Task AddAsync(CommandContext ctx,
                                [Description("Users to block.")] params DiscordUser[] users)
                => AddAsync(ctx, null, users);

            [Command("add"), Priority(1)]
            public async Task AddAsync(CommandContext ctx,
                                      [Description("Reason (max 60 chars).")] string reason,
                                      [Description("Users to block.")] params DiscordUser[] users)
            {
                if (reason?.Length >= 60)
                    throw new InvalidCommandUsageException("Reason cannot exceed 60 characters");

                if (!users.Any())
                    throw new InvalidCommandUsageException("Missing users to block.");

                var sb = new StringBuilder("Action results:\n\n");
                foreach (var user in users) {
                    if (Shared.BlockedUsers.Contains(user.Id)) {
                        sb.AppendLine($"Error: {user.ToString()} is already blocked!");
                        continue;
                    }

                    if (!Shared.BlockedUsers.Add(user.Id)) {
                        sb.AppendLine($"Error: Failed to add {user.ToString()} to blocked users list!");
                        continue;
                    }

                    try {
                        await Database.AddBlockedUserAsync(user.Id, reason)
                            .ConfigureAwait(false);
                    } catch {
                        sb.AppendLine($"Warning: Failed to add blocked {user.ToString()} to the database!");
                        continue;
                    }

                    sb.AppendLine($"Blocked: {user.ToString()}!");
                }

                await ctx.RespondWithIconEmbedAsync(sb.ToString())
                    .ConfigureAwait(false);
            }

            [Command("add"), Priority(0)]
            public Task AddAsync(CommandContext ctx,
                                [Description("Users to block.")] DiscordUser user,
                                [RemainingText, Description("Reason (max 60 chars).")] string reason)
                => AddAsync(ctx, reason, user);
            #endregion

            #region COMMAND_BLOCKEDUSERS_DELETE
            [Command("delete"), Module(ModuleType.Owner)]
            [Description("Remove users from blocked users list..")]
            [Aliases("-", "remove", "rm", "del")]
            [UsageExample("!owner blockedusers remove @Someone")]
            [UsageExample("!owner blockedusers remove 123123123123123")]
            [UsageExample("!owner blockedusers remove @Someone 123123123123123")]
            public async Task DeleteAsync(CommandContext ctx,
                                         [Description("Users to unblock.")] params DiscordUser[] users)
            {
                if (!users.Any())
                    throw new InvalidCommandUsageException("Missing users to block.");

                var sb = new StringBuilder("Action results:\n\n");
                foreach (var user in users) {
                    if (!Shared.BlockedUsers.Contains(user.Id)) {
                        sb.AppendLine($"Warning: {user.ToString()} is not blocked!");
                        continue;
                    }

                    if (!Shared.BlockedUsers.TryRemove(user.Id)) {
                        sb.AppendLine($"Error: Failed to remove {user.ToString()} from blocked users list!");
                        continue;
                    }

                    try {
                        await Database.RemoveBlockedUserAsync(user.Id)
                            .ConfigureAwait(false);
                    } catch (Exception e) {
                        sb.AppendLine($"Warning: Failed to remove {user.ToString()} from the database!");
                        TheGodfather.LogProvider.LogException(LogLevel.Warning, e);
                        continue;
                    }

                    sb.AppendLine($"Unblocked: {user.ToString()}!");
                }

                await ctx.RespondWithIconEmbedAsync(sb.ToString())
                    .ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_BLOCKEDUSERS_LIST
            [Command("list"), Module(ModuleType.Owner)]
            [Description("List all blocked users.")]
            [Aliases("ls")]
            [UsageExample("!owner blockedusers list")]
            public async Task ListAsync(CommandContext ctx)
            {
                var blocked = await Database.GetAllBlockedUsersAsync()
                    .ConfigureAwait(false);

                List<string> lines = new List<string>();
                foreach (var tup in blocked) {
                    try {
                        var user = await ctx.Client.GetUserAsync(tup.Item1)
                            .ConfigureAwait(false);
                        lines.Add($"{user.ToString()} ({Formatter.Italic(string.IsNullOrWhiteSpace(tup.Item2) ? "No reason provided." : tup.Item2)})");
                    } catch (NotFoundException) {
                        TheGodfather.LogProvider.LogMessage(LogLevel.Warning, $"Removed 404 blocked user with ID {tup.Item1}");
                        await Database.RemoveBlockedUserAsync(tup.Item1)
                            .ConfigureAwait(false);
                    }
                }

                if (!lines.Any())
                    throw new CommandFailedException("No blocked users registered!");

                await ctx.SendPaginatedCollectionAsync(
                    "Blocked users (in database):",
                    lines,
                    line => line,
                    DiscordColor.Azure,
                    5
                ).ConfigureAwait(false);
            }
            #endregion
        }
    }
}
