﻿#region USING_DIRECTIVES
using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis;

using TheGodfatherBot.Exceptions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion


namespace TheGodfatherBot.Modules.Admin
{
    [Group("admin")]
    [Description("Bot administration commands.")]
    [Hidden]
    public class CommandsAdmin
    {
        #region COMMAND_CLEARLOG
        [Command("clearlog")]
        [Description("Clear application logs.")]
        [Aliases("clearlogs", "deletelogs", "deletelog")]
        [RequireOwner]
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

        #region COMMAND_EVAL
        // Code created by Emzi
        [Command("eval")]
        [Description("Evaluates a snippet of C# code, in context.")]
        [RequireOwner]
        public async Task EvaluateAsync(CommandContext ctx,
                                       [RemainingText, Description("Code to evaluate.")] string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new InvalidCommandUsageException("Code missing.");

            var cs1 = code.IndexOf("```") + 3;
            cs1 = code.IndexOf('\n', cs1) + 1;
            var cs2 = code.LastIndexOf("```");

            if (cs1 == -1 || cs2 == -1)
                throw new InvalidCommandUsageException("You need to wrap the code into a code block.");

            code = code.Substring(cs1, cs2 - cs1);

            var embed = new DiscordEmbedBuilder {
                Title = "Evaluating...",
                Color = DiscordColor.Aquamarine
            };
            var msg = await ctx.RespondAsync("", embed: embed.Build()).ConfigureAwait(false);

            var globals = new EvaluationEnvironment(ctx);
            var sopts = ScriptOptions.Default
                .WithImports("System", "System.Collections.Generic", "System.Linq", "System.Net.Http", "System.Net.Http.Headers", "System.Reflection", "System.Text", "System.Threading.Tasks",
                    "DSharpPlus", "DSharpPlus.CommandsNext", "DSharpPlus.Interactivity")
                .WithReferences(AppDomain.CurrentDomain.GetAssemblies().Where(xa => !xa.IsDynamic && !string.IsNullOrWhiteSpace(xa.Location)));

            var sw1 = Stopwatch.StartNew();
            var cs = CSharpScript.Create(code, sopts, typeof(EvaluationEnvironment));
            var csc = cs.Compile();
            sw1.Stop();

            if (csc.Any(xd => xd.Severity == DiagnosticSeverity.Error)) {
                embed = new DiscordEmbedBuilder {
                    Title = "Compilation failed",
                    Description = string.Concat("Compilation failed after ", sw1.ElapsedMilliseconds.ToString("#,##0"), "ms with ", csc.Length.ToString("#,##0"), " errors."),
                    Color = DiscordColor.Aquamarine
                };
                foreach (var xd in csc.Take(3)) {
                    var ls = xd.Location.GetLineSpan();
                    embed.AddField(string.Concat("Error at ", ls.StartLinePosition.Line.ToString("#,##0"), ", ", ls.StartLinePosition.Character.ToString("#,##0")), Formatter.InlineCode(xd.GetMessage()), false);
                }
                if (csc.Length > 3) {
                    embed.AddField("Some errors ommited", string.Concat((csc.Length - 3).ToString("#,##0"), " more errors not displayed"), false);
                }
                await msg.ModifyAsync(embed: embed.Build()).ConfigureAwait(false);
                return;
            }

            Exception rex = null;
            ScriptState<object> css = null;
            var sw2 = Stopwatch.StartNew();
            try {
                css = await cs.RunAsync(globals).ConfigureAwait(false);
                rex = css.Exception;
            } catch (Exception ex) {
                rex = ex;
            }
            sw2.Stop();

            if (rex != null) {
                embed = new DiscordEmbedBuilder {
                    Title = "Execution failed",
                    Description = string.Concat("Execution failed after ", sw2.ElapsedMilliseconds.ToString("#,##0"), "ms with `", rex.GetType(), ": ", rex.Message, "`."),
                    Color = DiscordColor.Aquamarine
                };
                await msg.ModifyAsync(embed: embed.Build()).ConfigureAwait(false);
                return;
            }

            // execution succeeded
            embed = new DiscordEmbedBuilder {
                Title = "Evaluation successful",
                Color = DiscordColor.Aquamarine
            };

            embed.AddField("Result", css.ReturnValue != null ? css.ReturnValue.ToString() : "No value returned", false)
                .AddField("Compilation time", string.Concat(sw1.ElapsedMilliseconds.ToString("#,##0"), "ms"), true)
                .AddField("Execution time", string.Concat(sw2.ElapsedMilliseconds.ToString("#,##0"), "ms"), true);

            if (css.ReturnValue != null)
                embed.AddField("Return type", css.ReturnValue.GetType().ToString(), true);

            await msg.ModifyAsync(embed: embed.Build()).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_LEAVEGUILDS
        [Command("leave")]
        [Description("Leave guilds given as IDs.")]
        [RequireOwner]
        public async Task LeaveGuilds(CommandContext ctx,
                                     [Description("Guild ID list.")] params ulong[] ids)
        {
            if (!ids.Any())
                throw new InvalidCommandUsageException("IDs missing.");

            string s = $"Left:\n";
            foreach (var id in ids) {
                try {
                    var guild = ctx.Client.Guilds[id];
                    await guild.LeaveAsync();
                    s += $"{guild.Name} owned by {guild.Owner.Username}#{guild.Owner.Discriminator}\n";
                } catch {

                }
            }
            await ctx.RespondAsync(s);
        }
        #endregion

        #region COMMAND_SHUTDOWN
        [Command("shutdown")]
        [Description("Triggers the dying in the vineyard scene.")]
        [Aliases("disable", "poweroff", "exit", "quit")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task ShutDown(CommandContext ctx)
        {
            await ctx.RespondAsync("https://www.youtube.com/watch?v=4rbfuw0UN2A");
            await ctx.Client.DisconnectAsync();
            Environment.Exit(0);
        }
        #endregion

        #region COMMAND_SUDO
        [Command("sudo")]
        [Description("Executes a command as another user.")]
        [Aliases("execas", "as")]
        [RequireOwner]
        public async Task Sudo(CommandContext ctx,
                              [Description("Member to execute as.")] DiscordMember member = null,
                              [RemainingText, Description("Command text to execute.")] string command = null)
        {
            if (member == null || command == null)
                throw new InvalidCommandUsageException();

            await ctx.Client.GetCommandsNext().SudoAsync(member, ctx.Channel, command);
        }
        #endregion


        [Group("status", CanInvokeWithoutSubcommand = false)]
        [Description("Bot status manipulation.")]
        public class CommandsStatus
        {
            #region COMMAND_STATUS_ADD
            [Command("add")]
            [Description("Add a status to running queue.")]
            [Aliases("+")]
            [RequireOwner]
            public async Task AddStatus(CommandContext ctx,
                                       [RemainingText, Description("Status.")] string status = null)
            {
                if (string.IsNullOrWhiteSpace(status))
                    throw new InvalidCommandUsageException("Invalid status.");

                TheGodfather._statuses.Add(status);
                await ctx.RespondAsync("Status added!");
            }
            #endregion

            #region COMMAND_STATUS_DELETE
            [Command("delete")]
            [Description("Remove status from running queue.")]
            [Aliases("-", "remove")]
            [RequireOwner]
            public async Task DeleteStatus(CommandContext ctx,
                                          [RemainingText, Description("Status.")] string status = null)
            {
                if (string.IsNullOrWhiteSpace(status))
                    throw new InvalidCommandUsageException("Invalid status.");

                if (status == "!help")
                    throw new InvalidCommandUsageException("Cannot delete help status!");

                TheGodfather._statuses.Remove(status);
                await ctx.RespondAsync("Status removed!");
            }
            #endregion

            #region COMMAND_STATUS_LIST
            [Command("list")]
            [Description("List all statuses.")]
            [RequireUserPermissions(Permissions.Administrator)]
            public async Task ListStatuses(CommandContext ctx)
            {
                string s = "Statuses:\n\n";
                foreach (var status in TheGodfather._statuses)
                    s += status + " ";
                await ctx.RespondAsync(s);
            }
            #endregion
        }
    }

    public sealed class EvaluationEnvironment
    {
        public CommandContext Context { get; }
        public DiscordMessage Message { get { return this.Context.Message; } }
        public DiscordChannel Channel { get { return this.Context.Channel; } }
        public DiscordGuild Guild { get { return this.Context.Guild; } }
        public DiscordUser User { get { return this.Context.User; } }
        public DiscordMember Member { get { return this.Context.Member; } }
        public DiscordClient Client { get { return this.Context.Client; } }
        public TheGodfather Godfather { get { return Context.Dependencies.GetDependency<TheGodfather>(); } }

        public EvaluationEnvironment(CommandContext ctx)
        {
            this.Context = ctx;
        }
    }
}
