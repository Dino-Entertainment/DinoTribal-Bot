﻿#region USING_DIRECTIVES
using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Services;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.VoiceNext;
#endregion

namespace TheGodfather.Modules.Voice
{
    public partial class VoiceModule
    {
        [Group("play")]
        [Description("Commands for playing music. If invoked without subcommand, plays given URL or searches YouTube for given query and plays the first result.")]
        [Aliases("music", "p")]
        [UsageExample("!play https://www.youtube.com/watch?v=dQw4w9WgXcQ")]
        [UsageExample("!play what is love?")]
        [RequireBotPermissions(Permissions.Speak)]
        [ListeningCheck]
        public class PlayModule : VoiceModule
        {

            public PlayModule(YoutubeService yt, SharedData shared) : base(yt, shared) { }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [RemainingText, Description("URL or YouTube search query.")] string data_or_url)
            {
                if (!IsValidURL(data_or_url, out Uri uri))
                    data_or_url = await _Service.GetFirstVideoResultAsync(data_or_url)
                        .ConfigureAwait(false);

                if (string.IsNullOrWhiteSpace(data_or_url))
                    throw new CommandFailedException("No results found!");

                var si = await _Service.GetSongInfoAsync(data_or_url)
                    .ConfigureAwait(false);
                if (si == null)
                    throw new CommandFailedException("Failed to retrieve song information for that URL.");
                si.Queuer = ctx.User.Mention;

                var vnext = ctx.Client.GetVoiceNext();
                if (vnext == null)
                    throw new CommandFailedException("VNext is not enabled or configured.");

                var vnc = vnext.GetConnection(ctx.Guild);
                if (vnc == null) {
                    await ConnectAsync(ctx);
                    vnc = vnext.GetConnection(ctx.Guild);
                }
                
                if (vnc.IsPlaying)
                    return; // TODO

                await ctx.RespondAsync("Now playing: ", embed: si.Embed())
                    .ConfigureAwait(false);
                await PlayAsync(vnc, ctx.Guild.Id, si.Uri);
            }


            #region COMMAND_PLAY_FILE
            [Command("file")]
            [Description("Plays an audio file from the server filesystem.")]
            [Aliases("f")]
            [UsageExample("!play file test.mp3")]
            [RequireOwner]
            public async Task PlayFileAsync(CommandContext ctx,
                                           [RemainingText, Description("Full path to the file to play.")] string filename)
            {
                var vnext = ctx.Client.GetVoiceNext();
                if (vnext == null)
                    throw new CommandFailedException("VNext is not enabled or configured.");

                var vnc = vnext.GetConnection(ctx.Guild);
                if (vnc == null) {
                    await ConnectAsync(ctx);
                    vnc = vnext.GetConnection(ctx.Guild);
                }

                if (!File.Exists(filename))
                    throw new CommandFailedException($"File {Formatter.InlineCode(filename)} does not exist.", new FileNotFoundException());

                while (vnc.IsPlaying)
                    return; // TODO

                await ctx.RespondWithIconEmbedAsync(StaticDiscordEmoji.Headphones, $"Playing {Formatter.InlineCode(filename)}.");
                await PlayAsync(vnc, ctx.Guild.Id, filename);
            }
            #endregion


            #region HELPER_FUNCTIONS
            private async Task PlayAsync(VoiceNextConnection vnc, ulong gid, string url)
            {
                if (!Shared.PlayingVoiceIn.Add(gid))
                    throw new CommandFailedException("Failed to setup the voice playing settings");

                await vnc.SendSpeakingAsync(true);
                try {
                    var ffmpeg_inf = new ProcessStartInfo {
                        FileName = "ffmpeg",
                        Arguments = $"-i \"{url}\" -ac 2 -f s16le -ar 48000 pipe:1",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };
                    var ffmpeg = Process.Start(ffmpeg_inf);
                    var ffout = ffmpeg.StandardOutput.BaseStream;

                    using (var ms = new MemoryStream()) {
                        await ffout.CopyToAsync(ms);
                        ms.Position = 0;

                        var buff = new byte[3840];
                        var br = 0;
                        while (Shared.PlayingVoiceIn.Contains(gid) && (br = ms.Read(buff, 0, buff.Length)) > 0) {
                            if (br < buff.Length)
                                for (var i = br; i < buff.Length; i++)
                                    buff[i] = 0;

                            await vnc.SendAsync(buff, 20);
                        }
                    }
                } catch (Exception e) {
                    TheGodfather.LogHandle.LogException(LogLevel.Error, e);
                } finally {
                    await vnc.SendSpeakingAsync(false);
                    Shared.PlayingVoiceIn.TryRemove(gid);
                }
            }
            #endregion
        }
    }
}

