﻿#region USING_DIRECTIVES
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.EntityFrameworkCore;
using TheGodfather.Attributes;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Extensions;
#endregion

namespace TheGodfather.Modules.Owner
{
    public partial class OwnerModule
    {
        [Group("privilegedusers"), NotBlocked]
        [Description("Manipulate privileged users. Privileged users can invoke commands marked with RequirePrivilegedUsers permission.")]
        [Aliases("pu", "privu", "privuser", "pusers", "puser", "pusr")]
        [RequireOwner]
        public class PrivilegedUsersModule : TheGodfatherModule
        {

            public PrivilegedUsersModule(DbContextBuilder db)
                : base(db)
            {

            }


            [GroupCommand, Priority(1)]
            public Task ExecuteGroupAsync(CommandContext ctx)
                => this.ListAsync(ctx);

            [GroupCommand, Priority(0)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [Description("Users to grant privilege to.")] params DiscordUser[] users)
                => this.AddAsync(ctx, users);


            #region COMMAND_PRIVILEGEDUSERS_ADD
            [Command("add")]
            [Description("Add users to privileged users list.")]
            [Aliases("+", "a", "<", "<<", "+=")]

            public async Task AddAsync(CommandContext ctx,
                                      [Description("Users to grant privilege to.")] params DiscordUser[] users)
            {
                if (users is null || !users.Any())
                    throw new InvalidCommandUsageException("Missing users to grant privilege to.");

                using (TheGodfatherDbContext db = this.Database.CreateContext()) {
                    db.PrivilegedUsers.AddRange(users.Select(u => new PrivilegedUser {
                        UserId = u.Id
                    }));
                    await db.SaveChangesAsync();
                }

                await this.InformAsync(ctx, "Granted privilege to all given users.", important: false);
            }
            #endregion

            #region COMMAND_PRIVILEGEDUSERS_DELETE
            [Command("delete")]
            [Description("Remove users from privileged users list.")]
            [Aliases("-", "remove", "rm", "del", ">", ">>", "-=")]

            public async Task DeleteAsync(CommandContext ctx,
                                         [Description("Users to revoke privileges from.")] params DiscordUser[] users)
            {
                if (users is null || !users.Any())
                    throw new InvalidCommandUsageException("Missing users.");

                using (TheGodfatherDbContext db = this.Database.CreateContext()) {
                    // FIXME
                    db.PrivilegedUsers.RemoveRange(db.PrivilegedUsers.Where(pu => users.Any(u => u.Id == pu.UserId)));
                    await db.SaveChangesAsync();
                }

                await this.InformAsync(ctx, "Revoked privilege from all given users.", important: false);
            }
            #endregion

            #region COMMAND_PRIVILEGEDUSERS_LIST
            [Command("list")]
            [Description("List all privileged users.")]
            [Aliases("ls", "l", "print")]
            public async Task ListAsync(CommandContext ctx)
            {
                List<PrivilegedUser> privileged;
                using (TheGodfatherDbContext db = this.Database.CreateContext())
                    privileged = await db.PrivilegedUsers.ToListAsync();

                var valid = new List<DiscordUser>();
                foreach (PrivilegedUser usr in privileged) {
                    try {
                        DiscordUser user = await ctx.Client.GetUserAsync(usr.UserId);
                        valid.Add(user);
                    } catch (NotFoundException) {
                        LogExt.Debug(ctx, "Removing 404 privileged user {UserId}", usr.UserId);
                        using TheGodfatherDbContext db = this.Database.CreateContext();
                        db.PrivilegedUsers.Remove(new PrivilegedUser { UserIdDb = usr.UserIdDb });
                        await db.SaveChangesAsync();
                    }
                }

                if (!valid.Any())
                    throw new CommandFailedException("No privileged users registered!");

                await ctx.PaginateAsync(
                    "Privileged users",
                    valid,
                    user => user.ToString(),
                    this.ModuleColor,
                    10
                );
            }
            #endregion
        }
    }
}
