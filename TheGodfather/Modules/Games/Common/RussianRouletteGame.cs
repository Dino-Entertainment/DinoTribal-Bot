﻿#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using TheGodfather.Common;
using TheGodfather.Common.Collections;
#endregion

namespace TheGodfather.Modules.Games.Common
{
    public class RussianRouletteGame : BaseChannelGame
    {
        public int ParticipantCount => this.participants.Count;
        public bool Started { get; private set; }
        public IReadOnlyList<DiscordUser> Survivors { get; private set; }

        private readonly ConcurrentHashSet<DiscordUser> participants;


        public RussianRouletteGame(InteractivityExtension interactivity, DiscordChannel channel)
            : base(interactivity, channel)
        {
            this.Started = false;
            this.participants = new ConcurrentHashSet<DiscordUser>();
        }


        public override async Task RunAsync()
        {
            this.Started = true;

            for (int round = 1; round < 5 && this.ParticipantCount > 1; round++) {
                DiscordMessage msg = await this.Channel.SendMessageAsync($"Round #{round} starts in 5s!");

                await Task.Delay(TimeSpan.FromSeconds(5));

                var rng = new SecureRandom();
                var participants = this.participants.ToList();
                var eb = new StringBuilder();
                foreach (DiscordUser participant in participants) {
                    if (rng.Next(6) < round) {
                        eb.AppendLine($"{participant.Mention} {Emojis.Dead} {Emojis.Blast} {Emojis.Gun}");
                        this.participants.TryRemove(participant);
                    } else {
                        eb.AppendLine($"{participant.Mention} {Emojis.Relieved} {Emojis.Gun}");
                    }

                    msg = await msg.ModifyAsync(embed: new DiscordEmbedBuilder {
                        Title = $"ROUND #{round}",
                        Description = eb.ToString(),
                        Color = DiscordColor.DarkRed
                    }.Build());

                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
            }

            this.Survivors = this.participants.ToList().AsReadOnly();
        }

        public bool AddParticipant(DiscordUser user)
        {
            if (this.participants.Any(u => user.Id == u.Id))
                return false;
            return this.participants.Add(user);
        }
    }
}
