﻿#region USING_DIRECTIVES
using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis;

using TheGodfather.Helpers;
using TheGodfather.Helpers.DataManagers;
using TheGodfather.Exceptions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Commands.Administration
{
    [Group("admin")]
    [Description("Owner-only administration commands.")]
    [Aliases("owner", "o")]
    [RequireOwner]
    [Hidden]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public class CommandsOwner
    {
        #region COMMAND_BOTNAME
        [Command("botname")]
        [Description("Set bot name.")]
        [Aliases("setbotname", "setname")]
        [CheckIgnore]
        public async Task SetBotNameAsync(CommandContext ctx,
                                         [Description("New name.")] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException("Name missing.");

            await ctx.Client.EditCurrentUserAsync(username: name)
                .ConfigureAwait(false);
            await ctx.RespondAsync("Done.")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_CLEARLOG
        [Command("clearlog")]
        [Description("Clear application logs.")]
        [Aliases("clearlogs", "deletelogs", "deletelog")]
        [CheckIgnore]
        public async Task ClearLogAsync(CommandContext ctx)
        {
            try {
                ctx.Dependencies.GetDependency<TheGodfather>().LogHandle.ClearLogFile();
            } catch (Exception e) {
                ctx.Client.DebugLogger.LogMessage(LogLevel.Error, "TheGodfather", e.Message, DateTime.Now);
                throw e;
            }

            await ctx.RespondAsync("Logs cleared.")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_EVAL
        // Code created by Emzi
        [Command("eval")]
        [Description("Evaluates a snippet of C# code, in context.")]
        [Aliases("compile", "run")]
        [CheckIgnore]
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
            var msg = await ctx.RespondAsync(embed: embed.Build()).ConfigureAwait(false);

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
        [Command("leaveguilds")]
        [Description("Leave guilds given as IDs.")]
        [CheckIgnore]
        public async Task LeaveGuildsAsync(CommandContext ctx,
                                          [Description("Guild ID list.")] params ulong[] ids)
        {
            if (!ids.Any())
                throw new InvalidCommandUsageException("IDs missing.");

            string s = $"Left:\n";
            foreach (var id in ids) {
                try {
                    var guild = ctx.Client.Guilds[id];
                    await guild.LeaveAsync();
                    s += $"{Formatter.Bold(guild.Name)} owned by {Formatter.Bold(guild.Owner.Username)}#{guild.Owner.Discriminator}\n";
                } catch {

                }
            }
            await ctx.RespondAsync(s)
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_SENDMESSAGE
        [Command("sendmessage")]
        [Description("Sends a message to a user or channel.")]
        [Aliases("send")]
        [CheckIgnore]
        public async Task SendAsync(CommandContext ctx,
                                   [Description("u/c (for user or channel.)")] string desc,
                                   [Description("User/Channel ID.")] ulong xid,
                                   [RemainingText, Description("Message.")] string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new InvalidCommandUsageException();

            if (desc == "u") {
                var user = await ctx.Client.GetUserAsync(xid);
                var dm = await ctx.Client.CreateDmAsync(user);
                await ctx.Client.SendMessageAsync(dm, content: message)
                    .ConfigureAwait(false);
            } else if (desc == "c") {
                var channel = await ctx.Client.GetChannelAsync(xid)
                    .ConfigureAwait(false);
                await ctx.Client.SendMessageAsync(channel, content: message)
                    .ConfigureAwait(false);
            } else {
                throw new InvalidCommandUsageException("Descriptor can only be 'u' or 'c'.");
            }

            await ctx.RespondAsync("Message sent.")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_SHUTDOWN
        [Command("shutdown")]
        [Description("Triggers the dying in the vineyard scene.")]
        [Aliases("disable", "poweroff", "exit", "quit")]
        [CheckIgnore]
        public async Task ExitAsync(CommandContext ctx)
        {
            await ctx.RespondAsync("https://www.youtube.com/watch?v=4rbfuw0UN2A")
                .ConfigureAwait(false);
            await ctx.Client.DisconnectAsync()
                .ConfigureAwait(false);
            Environment.Exit(0);
        }
        #endregion

        #region COMMAND_SUDO
        [Command("sudo")]
        [Description("Executes a command as another user.")]
        [Aliases("execas", "as")]
        [CheckIgnore]
        public async Task SudoAsync(CommandContext ctx,
                                   [Description("Member to execute as.")] DiscordMember member,
                                   [RemainingText, Description("Command text to execute.")] string command)
        {
            if (member == null || command == null)
                throw new InvalidCommandUsageException();

            await ctx.Client.GetCommandsNext().SudoAsync(member, ctx.Channel, command)
                .ConfigureAwait(false);
        }
        #endregion
        
        #region COMMAND_TOGGLEIGNORE
        [Command("toggleignore")]
        [Description("Toggle bot's reaction to commands.")]
        [Aliases("ti")]
        public async Task ToggleIgnoreAsync(CommandContext ctx)
        {
            ctx.Dependencies.GetDependency<TheGodfather>().ToggleListening();
            await ctx.RespondAsync("Done!")
                .ConfigureAwait(false);
        }
        #endregion


        [Group("status", CanInvokeWithoutSubcommand = false)]
        [Description("Bot status manipulation.")]
        [CheckIgnore]
        public class CommandsStatus
        {
            #region COMMAND_STATUS_ADD
            [Command("add")]
            [Description("Add a status to running queue.")]
            [Aliases("+")]
            public async Task AddAsync(CommandContext ctx,
                                      [RemainingText, Description("Status.")] string status)
            {
                if (string.IsNullOrWhiteSpace(status))
                    throw new InvalidCommandUsageException("Invalid status.");

                ctx.Dependencies.GetDependency<StatusManager>().AddStatus(status);

                await ctx.RespondAsync("Status added!")
                    .ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_STATUS_DELETE
            [Command("delete")]
            [Description("Remove status from running queue.")]
            [Aliases("-", "remove")]
            public async Task DeleteAsync(CommandContext ctx,
                                         [RemainingText, Description("Status.")] string status)
            {
                if (string.IsNullOrWhiteSpace(status))
                    throw new InvalidCommandUsageException("Invalid status.");

                if (status == "!help")
                    throw new InvalidCommandUsageException("Cannot delete help status!");

                ctx.Dependencies.GetDependency<StatusManager>().DeleteStatus(status);
                await ctx.RespondAsync("Status removed!")
                    .ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_STATUS_LIST
            [Command("list")]
            [Description("List all statuses.")]
            public async Task ListAsync(CommandContext ctx)
            {
                await ctx.RespondAsync("My current statuses:\n" + string.Join("\n", ctx.Dependencies.GetDependency<StatusManager>().Statuses))
                    .ConfigureAwait(false);
            }
            #endregion
        }
    }
}
