﻿#region USING_DIRECTIVES
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Common.Attributes;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Modules.Administration.Services;
#endregion

namespace TheGodfather.Modules.Administration
{
    [Group("forbiddennames"), Module(ModuleType.Administration), NotBlocked]
    [Description("Manage forbidden names for this guild. Group call shows all the forbidden nicknames for this guild.")]
    [Aliases("forbiddenname", "forbiddennicknames", "fn", "disallowednames")]
    
    [RequireUserPermissions(Permissions.ManageGuild)]
    [RequirePermissions(Permissions.ManageNicknames)]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public class ForbiddenNamesModule : TheGodfatherModule
    {

        public ForbiddenNamesModule(DatabaseContextBuilder db)
            : base(db)
        {
            
        }


        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => this.ListAsync(ctx);

        [GroupCommand, Priority(0)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [RemainingText, Description("Forbidden name list (can be regexes)")] params string[] names)
            => this.AddAsync(ctx, names);


        #region COMMAND_FORBIDDENNAMES_ADD
        [Command("add")]
        [Description("Add nicknames to the forbidden list (can be a regex).")]
        [Aliases("addnew", "create", "a", "+", "+=", "<", "<<")]
        
        public async Task AddAsync(CommandContext ctx,
                                  [RemainingText, Description("Name list.")] params string[] names)
        {
            if (names is null || !names.Any())
                throw new InvalidCommandUsageException("Names missing.");

            var eb = new StringBuilder();

            using (DatabaseContext db = this.Database.CreateContext()) {
                var dbNames = new List<DatabaseForbiddenName>();
                foreach (string regexString in names) {
                    if (regexString.Length < 3 || regexString.Length > 60) {
                        eb.AppendLine($"Error: Name or regex {Formatter.InlineCode(regexString)} doesn't fit the size requirement (3-60).");
                        continue;
                    }

                    if (!regexString.TryParseRegex(out Regex regex))
                        regex = regexString.ToRegex(escape: true);
                    
                    if (!db.ForbiddenNames.Any(n => n.RegexString == regexString))
                        dbNames.Add(new DatabaseForbiddenName { GuildId = ctx.Guild.Id, RegexString = regexString });
                }
                db.ForbiddenNames.AddRange(dbNames);
                await db.SaveChangesAsync();

                DiscordMember bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
                foreach (DiscordMember member in ctx.Guild.Members.Select(kvp => kvp.Value).Where(m => !m.IsBot && m.Hierarchy < bot.Hierarchy)) {
                    if (dbNames.Any(name => name.Regex.IsMatch(member.DisplayName))) {
                        try {
                            await member.ModifyAsync(m => {
                                m.Nickname = "Temporary nickname";
                                m.AuditLogReason = "_gf: Forbidden name match";
                            });
                            await member.SendMessageAsync($"The nickname you have in the guild {ctx.Guild.Name} is now forbidden by the guild administrator and I have set a temporary nickname for you. Please set a different name.");
                        } catch (UnauthorizedException) {

                        }
                    }
                }
            }

            DiscordChannel logchn = ctx.Services.GetService<GuildConfigService>().GetLogChannelForGuild(ctx.Guild);
            if (!(logchn is null)) {
                var emb = new DiscordEmbedBuilder {
                    Title = "Forbidden name addition occured",
                    Color = this.ModuleColor
                };
                emb.AddField("User responsible", ctx.User.Mention, inline: true);
                emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                emb.AddField("Tried adding forbidden names", string.Join("\n", names.Select(rgx => Formatter.InlineCode(rgx))));
                if (eb.Length > 0)
                    emb.AddField("Errors", eb.ToString());
                await logchn.SendMessageAsync(embed: emb.Build());
            }

            if (eb.Length > 0)
                await this.InformFailureAsync(ctx, $"Action finished with warnings/errors:\n\n{eb.ToString()}");
            else
                await this.InformAsync(ctx, "Successfully registered all given forbidden names!", important: false);
        }
        #endregion

        #region COMMAND_FORBIDDENNAMES_DELETE
        [Command("delete"), Priority(1)]
        [Description("Removes forbidden name either by ID or plain text match.")]
        [Aliases("remove", "rm", "del", "d", "-", "-=", ">", ">>")]
        
        public async Task DeleteAsync(CommandContext ctx,
                                     [RemainingText, Description("Forbidden name IDs to remove.")] params int[] ids)
        {
            if (ids is null || !ids.Any())
                throw new CommandFailedException("No IDs given.");

            using (DatabaseContext db = this.Database.CreateContext()) {
                db.ForbiddenNames.RemoveRange(db.ForbiddenNames.Where(fn => fn.GuildId == ctx.Guild.Id && ids.Any(id => id == fn.Id)));
                await db.SaveChangesAsync();
            }

            DiscordChannel logchn = ctx.Services.GetService<GuildConfigService>().GetLogChannelForGuild(ctx.Guild);
            if (!(logchn is null)) {
                var emb = new DiscordEmbedBuilder {
                    Title = "Forbidden name deletion occured",
                    Color = this.ModuleColor
                };
                emb.AddField("User responsible", ctx.User.Mention, inline: true);
                emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                emb.AddField("Tried deleting forbidden names with IDs", string.Join("\n", ids.Select(id => id.ToString())));
                await logchn.SendMessageAsync(embed: emb.Build());
            }

            await this.InformAsync(ctx, "Done!", important: false);
        }

        [Command("delete"), Priority(0)]
        public async Task DeleteAsync(CommandContext ctx,
                                     [RemainingText, Description("Forbidden name IDs to remove.")] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException("Missing name.");

            using (DatabaseContext db = this.Database.CreateContext()) {
                DatabaseForbiddenName fn = db.ForbiddenNames.SingleOrDefault(n => n.GuildId == ctx.Guild.Id && n.RegexString == name);
                if (fn is null)
                    throw new CommandFailedException("Such name is not forbidden.");
                db.ForbiddenNames.Remove(fn);
                await db.SaveChangesAsync();
            }

            DiscordChannel logchn = ctx.Services.GetService<GuildConfigService>().GetLogChannelForGuild(ctx.Guild);
            if (!(logchn is null)) {
                var emb = new DiscordEmbedBuilder {
                    Title = "Forbidden name deletion occured",
                    Color = this.ModuleColor
                };
                emb.AddField("User responsible", ctx.User.Mention, inline: true);
                emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                emb.AddField("Tried deleting forbidden name", name);
                await logchn.SendMessageAsync(embed: emb.Build());
            }

            await this.InformAsync(ctx, "Done!", important: false);
        }
        #endregion

        #region COMMAND_FORBIDDENNAMES_DELETEALL
        [Command("deleteall"), UsesInteractivity]
        [Description("Delete all forbidden names for the current guild.")]
        [Aliases("removeall", "rmrf", "rma", "clearall", "clear", "delall", "da")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task DeleteAllAsync(CommandContext ctx)
        {
            if (!await ctx.WaitForBoolReplyAsync("Are you sure you want to delete all forbidden names for this guild?"))
                return;

            using (DatabaseContext db = this.Database.CreateContext()) {
                db.ForbiddenNames.RemoveRange(db.ForbiddenNames.Where(n => n.GuildId == ctx.Guild.Id));
                await db.SaveChangesAsync();
            }

            DiscordChannel logchn = ctx.Services.GetService<GuildConfigService>().GetLogChannelForGuild(ctx.Guild);
            if (!(logchn is null)) {
                var emb = new DiscordEmbedBuilder {
                    Title = "All forbidden names have been deleted",
                    Color = this.ModuleColor
                };
                emb.AddField("User responsible", ctx.User.Mention, inline: true);
                emb.AddField("Invoked in", ctx.Channel.Mention, inline: true);
                await logchn.SendMessageAsync(embed: emb.Build());
            }

            await this.InformAsync(ctx, "Successfully deleted all guild forbidden names!", important: false);
        }
        #endregion

        #region COMMAND_FORBIDDENNAMES_LIST
        [Command("list")]
        [Description("Show all forbidden names for this guild.")]
        [Aliases("ls", "l")]
        public async Task ListAsync(CommandContext ctx)
        {
            List<DatabaseForbiddenName> names;
            using (DatabaseContext db = this.Database.CreateContext()) {
                names = await db.ForbiddenNames
                    .Where(n => n.GuildId == ctx.Guild.Id)
                    .OrderBy(n => n.Id)
                    .ToListAsync();
            }

            if (!names.Any())
                throw new CommandFailedException("No forbidden names registered in this guild!");

            await ctx.SendCollectionInPagesAsync(
                $"Forbidden names registered for {ctx.Guild.Name}",
                names,
                n => $"{Formatter.InlineCode($"{n.Id:D3}")} | {Formatter.InlineCode(n.RegexString)}",
                this.ModuleColor
            );
        }
        #endregion
    }
}
