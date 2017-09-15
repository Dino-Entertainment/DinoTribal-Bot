﻿#region USING_DIRECTIVES
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.VoiceNext;
using DSharpPlus.EventArgs;
using DSharpPlus.Net.WebSocket;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
#endregion

namespace TheGodfatherBot
{
    class TheGodfather
    {
        #region STATIC_FIELDS
        static DiscordClient _client { get; set; }
        static CommandsNextModule _commands { get; set; }
        static InteractivityModule _interactivity { get; set; }
        static VoiceNextClient _voice { get; set; }
        public static List<string> _statuses = new List<string> { "!help" , "worldmafia.net", "worldmafia.net/discord" };
        #endregion

        #region PRIVATE_FIELDS
        private static StreamWriter _logstream = null;
        private static EventWaitHandle _logwritelock = null;
        #endregion


        public static void Main(string[] args) =>
            new TheGodfather().MainAsync(args).GetAwaiter().GetResult();
        

        ~TheGodfather()
        {
            if (_logstream != null)
                _logstream.Close();
            _client.DisconnectAsync();
            _client.Dispose();
        }


        public async Task MainAsync(string[] args)
        {
            SetupClient();
            OpenLogFile();
            SetupCommands();
            SetupInteractivity();
            SetupVoice();

            await _client.ConnectAsync();

            await Task.Delay(-1);
        }


        #region HELPER_FUNCTIONS
        public static void OpenLogFile()
        {
            try {
                _logstream = new StreamWriter("log.txt", append: true);
                _logwritelock = new EventWaitHandle(true, EventResetMode.AutoReset, "SHARED_BY_ALL_PROCESSES");
            } catch (Exception e) {
                Console.WriteLine("Cannot open log file. Details: " + e.Message);
                return;
            }

            try {
                _logwritelock.WaitOne();
                _logstream.WriteLine();
                _logstream.WriteLine($"*** NEW INSTANCE STARTED AT {DateTime.Now.ToLongDateString()} : {DateTime.Now.ToLongTimeString()} ***");
                _logstream.WriteLine();
                _logstream.Flush();
                _logwritelock.Set();
            } catch (Exception e) {
                _client.DebugLogger.LogMessage(LogLevel.Error, "TheGodfather", "Cannot write to log file. Details: " + e.Message, DateTime.Now);
            }
        }

        public static void CloseLogFile()
        {
            if (_logwritelock != null && _logstream != null) {
                _logstream.Close();
                _logwritelock.Dispose();
                _logstream = null;
                _logwritelock = null;
            }
        }

        private string GetToken(string filename)
        {
            if (!File.Exists(filename))
                return null;
            else
                return File.ReadAllLines(filename)[0].Trim();
        }

        private void SetupClient()
        {
            _client = new DiscordClient(new DiscordConfiguration {
                LargeThreshold = 250,
                AutoReconnect = true,
                Token = GetToken("Resources/token.txt"),
                TokenType = TokenType.Bot,
                UseInternalLogHandler = true,
                LogLevel = LogLevel.Debug
            });
            _client.Ready += Client_Ready;
            _client.GuildAvailable += Client_GuildAvailable;
            _client.ClientErrored += Client_ClientError;
            _client.GuildMemberAdded += Client_GuildMemberAdd;
            _client.GuildMemberRemoved += Client_GuildMemberRemove;
            _client.MessageCreated += Client_MessageCreated;
            _client.Heartbeated += Client_Heartbeated;
            _client.DebugLogger.LogMessageReceived += Client_LogMessage;

            // Windows 7 specific
            _client.SetWebSocketClient<WebSocket4NetClient>();
        }

        private void SetupCommands()
        {
            _commands = _client.UseCommandsNext(new CommandsNextConfiguration {
                StringPrefix = "!",
                EnableDms = false,
                CaseSensitive = false,
                EnableMentionPrefix = true
            });
            CommandsAlias.LoadAliases(_client.DebugLogger);
            CommandsMemes.LoadMemes(_client.DebugLogger);
            CommandsSwat.LoadServers(_client.DebugLogger);
            CommandsInsult.LoadInsults(_client.DebugLogger);
            _commands.RegisterCommands<CommandsAdmin>();
            _commands.RegisterCommands<CommandsAlias>();
            _commands.RegisterCommands<CommandsBank>();
            _commands.RegisterCommands<CommandsCards>();
            _commands.RegisterCommands<CommandsChannels>();
            _commands.RegisterCommands<CommandsGamble>();
            _commands.RegisterCommands<CommandsGames>();
            _commands.RegisterCommands<CommandsGuild>();
            _commands.RegisterCommands<CommandsImgur>();
            _commands.RegisterCommands<CommandsInsult>();
            _commands.RegisterCommands<CommandsMemes>();
            _commands.RegisterCommands<CommandsMessages>();
            _commands.RegisterCommands<CommandsMisc>();
            _commands.RegisterCommands<CommandsRace>();
            _commands.RegisterCommands<CommandsRanking>();
            //_commands.RegisterCommands<CommandsReddit>();
            _commands.RegisterCommands<CommandsRoles>();
            _commands.RegisterCommands<CommandsRSS>();
            _commands.RegisterCommands<CommandsSwat>();
            _commands.RegisterCommands<CommandsUsers>();
            //_commands.RegisterCommands<CommandsVoice>();
            _commands.CommandExecuted += Commands_CommandExecuted;
            _commands.CommandErrored += Commands_CommandErrored;
        }

        private void SetupInteractivity()
        {
            _interactivity = _client.UseInteractivity();
        }

        private void SetupVoice()
        {
            _voice = _client.UseVoiceNext();
        }
        #endregion


        #region CLIENT_EVENTS
        private async Task Client_Ready(ReadyEventArgs e)
        {
            e.Client.DebugLogger.LogMessage(LogLevel.Info, "TheGodfather", "Ready.", DateTime.Now);
            await _client.UpdateStatusAsync(new Game(_statuses[0]) { StreamType = GameStreamType.NoStream });
        }

        private Task Client_GuildAvailable(GuildCreateEventArgs e)
        {
            e.Client.DebugLogger.LogMessage(LogLevel.Info, "TheGodfather", $"Guild available: {e.Guild.Name}", DateTime.Now);
            return Task.CompletedTask;
        }

        private Task Client_ClientError(ClientErrorEventArgs e)
        {
            e.Client.DebugLogger.LogMessage(LogLevel.Error, "TheGodfather", $"Exception occured: {e.Exception.GetType()}: {e.Exception.Message}", DateTime.Now);
            return Task.CompletedTask;
        }

        private Task Client_GuildMemberAdd(GuildMemberAddEventArgs e)
        {
            e.Guild.GetDefaultChannel().SendMessageAsync($"Welcome to {e.Guild.Name}, {e.Member.Mention}!");
            return Task.CompletedTask;
        }

        private Task Client_GuildMemberRemove(GuildMemberRemoveEventArgs e)
        {
            e.Guild.GetDefaultChannel().SendMessageAsync($"{e.Member?.Username ?? "<unknown>"} left {e.Guild?.Name ?? "<unknown>"}. Bye!");
            return Task.CompletedTask;
        }

        private Task Client_MessageCreated(MessageCreateEventArgs e)
        {
            if (e.Message.Author.IsBot)
                return Task.CompletedTask;

            if (e.Channel.IsPrivate) {
                e.Client.DebugLogger.LogMessage(LogLevel.Info, "TheGodfather", $"IGNORED DM: {e.Author.Username} : {e.Message}", DateTime.Now);
                return Task.CompletedTask;
            }

            CommandsRanking.UpdateMessageCount(e.Channel, e.Author);

            // Check if message has an alias
            var response = CommandsAlias.FindAlias(e.Guild.Id, e.Message.Content);
            if (response != null) {
                e.Client.DebugLogger.LogMessage(
                    LogLevel.Info,
                    "TheGodfather",
                    $"Alias triggered: {e.Message.Content}\n" +
                    $" User: {e.Message.Author.ToString()}\n" +
                    $" Location: '{e.Guild.Name}' ({e.Guild.Id}) ; {e.Channel.ToString()}"
                    , DateTime.Now
                );
                e.Channel.SendMessageAsync(response);
            }

            return Task.CompletedTask;
        }

        private async Task Client_Heartbeated(HeartbeatEventArgs e)
        {
            var rnd = new Random();
            await _client.UpdateStatusAsync(new Game(_statuses[rnd.Next(_statuses.Count)]) { StreamType = GameStreamType.NoStream });
        }

        private void Client_LogMessage(object sender, DebugLogMessageEventArgs e)
        {
            if (_logstream == null)
                return;

            try {
                _logwritelock.WaitOne();
                _logstream.WriteLine($"*[{e.Timestamp}] [{e.Level}]\t{e.Message}");
                _logstream.Flush();
                _logwritelock.Set();
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
        }
        #endregion


        #region COMMAND_EVENTS
        private Task Commands_CommandExecuted(CommandExecutionEventArgs e)
        {
            e.Context.Client.DebugLogger.LogMessage(
                LogLevel.Info, 
                "TheGodfather",
                $"Executed: {e.Command?.QualifiedName ?? "<unknown command>"}\n" +
                $" User: {e.Context.User.ToString()}\n" +
                $" Location: '{e.Context.Guild.Name}' ({e.Context.Guild.Id}) ; {e.Context.Channel.ToString()}"
                , DateTime.Now
            );
            return Task.CompletedTask;
        }

        private async Task Commands_CommandErrored(CommandErrorEventArgs e)
        {
            e.Context.Client.DebugLogger.LogMessage(
                LogLevel.Error, 
                "TheGodfather",
                $"Tried executing: {e.Command?.QualifiedName ?? "<unknown command>"}\n" +
                $" User: {e.Context.User.ToString()}\n" +
                $" Location: '{e.Context.Guild.Name}' ({e.Context.Guild.Id}) ; {e.Context.Channel.ToString()}\n" +
                $" Exception: {e.Exception.GetType()}\n" +
                $" Message: {e.Exception.Message ?? "<no message>"}"
                , DateTime.Now
            );

            var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");
            var embed = new DiscordEmbedBuilder {
                Title = "Error",
                Color = DiscordColor.Red
            };

            if (e.Exception is ChecksFailedException ex)
                embed.Description = $"{emoji} Either you or I don't have the permissions required to execute this command.";
            else if (e.Exception is UnauthorizedException)
                embed.Description = $"{emoji} I am not authorized to do that.";
            else
                embed.Description = $"{emoji} {e.Exception.Message}";

            await e.Context.RespondAsync("", embed: embed);
        }
        #endregion
    }
}