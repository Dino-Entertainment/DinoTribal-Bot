﻿#region USING_DIRECTIVES
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using TheGodfather.Exceptions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Commands.Games
{
    public partial class CommandsGames
    {
        [Group("quiz", CanInvokeWithoutSubcommand = false)]
        [Description("Start a quiz!")]
        [Aliases("trivia")]
        [Cooldown(2, 5, CooldownBucketType.User), Cooldown(3, 5, CooldownBucketType.Channel)]
        [CheckListeningAttribute]
        class CommandsQuiz
        {
            #region PRIVATE_FIELDS
            private enum QuizType { Countries };
            private Dictionary<string, string> _countries = null;
            private ConcurrentDictionary<ulong, bool> _quizrunning = new ConcurrentDictionary<ulong, bool>();
            #endregion


            #region COMMAND_QUIZ_COUNTRIES
            [Command("countries")]
            [Description("Country flags quiz.")]
            [Aliases("flags")]
            public async Task CountriesQuizAsync(CommandContext ctx)
            {
                if (_countries == null)
                    LoadCountries();

                if (_quizrunning.ContainsKey(ctx.Channel.Id))
                    throw new CommandFailedException("Quiz is already running in this channel!");

                _quizrunning.TryAdd(ctx.Channel.Id, true);

                await StartQuizAsync(QuizType.Countries, ctx)
                    .ConfigureAwait(false);

                _quizrunning.TryRemove(ctx.Channel.Id, out _);
            }
            #endregion


            #region HELPER_FUNCTIONS
            private async Task StartQuizAsync(QuizType t, CommandContext ctx)
            {
                var questions = new List<string>(_countries.Keys);
                var participants = new SortedDictionary<ulong, int>();

                await ctx.RespondAsync("Quiz will start in 10s! Get ready!")
                    .ConfigureAwait(false);
                await Task.Delay(10000)
                    .ConfigureAwait(false);

                var rnd = new Random();
                for (int i = 1; i < 10; i++) {
                    string question = questions[rnd.Next(questions.Count)];

                    await ctx.TriggerTypingAsync()
                        .ConfigureAwait(false);

                    if (t == QuizType.Countries) {
                        try {
                            await ctx.RespondWithFileAsync(new FileStream(question, FileMode.Open), content: $"Question {Formatter.Bold(i.ToString())}:")
                                .ConfigureAwait(false); ;
                        } catch (IOException e) {
                            throw e;
                        }
                    } else {
                        await ctx.RespondAsync(question)
                            .ConfigureAwait(false);
                    }

                    var interactivity = ctx.Client.GetInteractivityModule();
                    var msg = await interactivity.WaitForMessageAsync(
                        // TODO check enum when you add more quiz commands
                        xm => xm.Content.ToLower() == _countries[question].ToLower()
                    ).ConfigureAwait(false);
                    if (msg == null) {
                        await ctx.RespondAsync($"Time is out! The correct answer was: {Formatter.Bold(_countries[question])}")
                            .ConfigureAwait(false);
                    } else {
                        await ctx.RespondAsync($"GG {msg.User.Mention}, you got it right!")
                            .ConfigureAwait(false);
                        if (participants.ContainsKey(msg.User.Id))
                            participants[msg.User.Id]++;
                        else
                            participants.Add(msg.User.Id, 1);
                    }
                    questions.Remove(question);

                    await Task.Delay(2000)
                        .ConfigureAwait(false);
                }

                var em = new DiscordEmbedBuilder() { Title = "Results", Color = DiscordColor.Azure };
                foreach (var participant in participants) {
                    var m = await ctx.Guild.GetMemberAsync(participant.Key);
                    em.AddField(m.Username, participant.Value.ToString(), inline: true);
                }
                await ctx.RespondAsync(embed: em.Build())
                    .ConfigureAwait(false);
            }

            private void LoadCountries()
            {
                DirectoryInfo di = new DirectoryInfo("Resources/quiz-flags");
                FileInfo[] files = di.GetFiles("*.png");
                _countries = new Dictionary<string, string>();
                foreach (var f in files)
                    _countries.Add(f.FullName, f.Name.Split('.')[0]);
            }
            #endregion
        }
    }
}
