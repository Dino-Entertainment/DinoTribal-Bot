﻿#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Extensions;

using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Polls.Common
{
    public class ReactionsPoll : Poll
    {
        private static Dictionary<string, int> _emojiid = new Dictionary<string, int>() {
            { "1\u20e3" , 0 }, { ":one:" , 0 },
            { "2\u20e3" , 1 }, { ":two:" , 1 },
            { "3\u20e3" , 2 }, { ":three:" , 2 },
            { "4\u20e3" , 3 }, { ":four:" , 3 },
            { "5\u20e3" , 4 }, { ":five:" , 4 },
            { "6\u20e3" , 5 }, { ":six:" , 5 },
            { "7\u20e3" , 6 }, { ":seven:" , 6 },
            { "8\u20e3" , 7 }, { ":eight:" , 7 },
            { "9\u20e3" , 8 }, { ":nine:" , 8 },
            { "\U0001f51f" , 9 }, { ":keycap_ten:" , 9 },
        };


        private DiscordMessage _message;
        private ReactionCollectionContext _result;


        public ReactionsPoll(InteractivityExtension interactivity, DiscordChannel channel, string question)
            : base(interactivity, channel, question) { }


        public override async Task RunAsync(TimeSpan timespan)
        {
            Running = true;
            _message = await _channel.SendMessageAsync(embed: EmbedPoll("Vote by clicking on the reactions!"))
                .ConfigureAwait(false);
            _result = await _interactivity.CreatePollAsync(_message, EmojiUtil.Numbers.Take(OptionCount), timespan)
                .ConfigureAwait(false);
            await _channel.SendMessageAsync(embed: EmbedPollResults())
                .ConfigureAwait(false);
            Running = false;
        }

        public override DiscordEmbed EmbedPollResults()
        {
            var emb = new DiscordEmbedBuilder() {
                Title = Question + " (results)",
                Color = DiscordColor.Orange
            };
            foreach (var kvp in _result.Reactions) 
                emb.AddField(_options[_emojiid[kvp.Key.Name]], kvp.Value.ToString(), inline: true);

            return emb.Build();
        }
    }
}
