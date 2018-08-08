﻿#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Collections;
using TheGodfather.Extensions;
#endregion

namespace TheGodfather.Modules.Games.Common
{
    public class Quiz : ChannelEvent
    {
        public ConcurrentDictionary<DiscordUser, int> Results { get; }

        private readonly IReadOnlyList<QuizQuestion> questions;


        public Quiz(InteractivityExtension interactivity, DiscordChannel channel, IReadOnlyList<QuizQuestion> questions)
            : base(interactivity, channel)
        {
            this.questions = questions;
            this.Results = new ConcurrentDictionary<DiscordUser, int>();
        }


        public override async Task RunAsync()
        {
            int timeouts = 0;
            int currentQuestionIndex = 1;
            foreach (QuizQuestion question in this.questions) {
                var emb = new DiscordEmbedBuilder {
                    Title = $"Question #{currentQuestionIndex}",
                    Description = Formatter.Bold(question.Content),
                    Color = DiscordColor.Teal
                };
                emb.AddField("Category", question.Category, inline: false);

                var answers = new List<string>(question.IncorrectAnswers) {
                    question.CorrectAnswer
                };
                answers.Shuffle();

                for (int index = 0; index < answers.Count; index++)
                    emb.AddField($"Answer #{index + 1}:", answers[index], inline: true);

                await this.Channel.TriggerTypingAsync();
                await this.Channel.SendMessageAsync(embed: emb.Build());

                bool timeout = true;
                var failed = new ConcurrentHashSet<ulong>();
                var answerRegex = new Regex($@"\b{question.CorrectAnswer}\b", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                MessageContext mctx = await this.Interactivity.WaitForMessageAsync(
                    xm => {
                        if (xm.ChannelId != this.Channel.Id || xm.Author.IsBot || failed.Contains(xm.Author.Id))
                            return false;
                        if (int.TryParse(xm.Content, out int index) && index > 0 && index <= answers.Count) {
                            timeout = false;
                            if (answers[index - 1] == question.CorrectAnswer)
                                return true;
                            else
                                failed.Add(xm.Author.Id);
                        }
                        return false;
                    },
                    TimeSpan.FromSeconds(10)
                ); ;
                if (mctx == null) {
                    if (timeout)
                        timeouts++;
                    else
                        timeouts = 0;

                    if (timeouts == 3) {
                        this.IsTimeoutReached = true;
                        return;
                    }

                    await this.Channel.SendMessageAsync($"Time is out! The correct answer was: {Formatter.Bold(question.CorrectAnswer)}");
                } else {
                    await this.Channel.SendMessageAsync($"GG {mctx.User.Mention}, you got it right!");
                    this.Results.AddOrUpdate(mctx.User, u => 1, (u, v) => v + 1);
                }

                await Task.Delay(TimeSpan.FromSeconds(2));

                currentQuestionIndex++;
            }
        }
    }
}


