﻿#region USING_DIRECTIVES
using System.Collections.Immutable;
using System.Text;
using DSharpPlus;
using DSharpPlus.Entities;
using Humanizer;
using TheGodfather.Common;
#endregion

namespace TheGodfather.Modules.Currency.Common
{
    public class SlotMachine
    {
        private static ImmutableArray<DiscordEmoji> _emoji = new DiscordEmoji[] {
            Emojis.LargeBlueDiamond,
            Emojis.Seven,
            Emojis.MoneyBag,
            Emojis.Trophy,
            Emojis.Gift,
            Emojis.Cherries
        }.ToImmutableArray();

        private static ImmutableArray<int> _multipliers = new[] {
            10,
            7,
            5,
            4,
            3,
            2
        }.ToImmutableArray();


        public static DiscordEmbed RollToDiscordEmbed(DiscordUser user, long bid, string currency, out long won)
        {
            int[,] res = Roll();
            won = EvaluateSlotResult(res, bid);

            var emb = new DiscordEmbedBuilder {
                Title = $"{Emojis.LargeOrangeDiamond} SLUT MACHINE {Emojis.LargeOrangeDiamond}",
                Description = MakeStringFromResult(res),
                Color = DiscordColor.DarkGreen,
                ThumbnailUrl = user.AvatarUrl
            };

            var sb = new StringBuilder();
            for (int i = 0; i < _emoji.Length; i++)
                sb.Append(_emoji[i]).Append(Formatter.InlineCode($" x{_multipliers[i]}")).Append(" ");

            emb.AddField("Multipliers", sb.ToString());
            emb.AddField("Result", $"{user.Mention} won {Formatter.Bold(won.ToWords())} ({won:n0}) {currency}");

            return emb.Build();
        }


        private static int[,] Roll()
        {
            var rng = new SecureRandom();
            int[,] result = new int[3, 3];
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    result[i, j] = rng.Next(_emoji.Length);

            return result;
        }

        private static string MakeStringFromResult(int[,] res)
        {
            var sb = new StringBuilder();

            sb.Append(Emojis.BlackSquare);
            for (int i = 0; i < 5; i++)
                sb.Append(Emojis.SmallOrangeDiamond);
            sb.AppendLine();

            for (int i = 0; i < 3; i++) {
                if (i % 2 == 1)
                    sb.Append(Emojis.Joystick);
                else
                    sb.Append(Emojis.BlackSquare);
                sb.Append(Emojis.SmallOrangeDiamond);
                for (int j = 0; j < 3; j++)
                    sb.Append(_emoji[res[i, j]]);
                sb.AppendLine(Emojis.SmallOrangeDiamond);
            }

            sb.Append(Emojis.BlackSquare);
            for (int i = 0; i < 5; i++)
                sb.Append(Emojis.SmallOrangeDiamond);

            return sb.ToString();
        }

        private static long EvaluateSlotResult(int[,] res, long bid)
        {
            long pts = bid;

            for (int i = 0; i < 3; i++)
                if (res[i, 0] == res[i, 1] && res[i, 1] == res[i, 2])
                    pts *= _multipliers[res[i, 0]];

            for (int i = 0; i < 3; i++)
                if (res[0, i] == res[1, i] && res[1, i] == res[2, i])
                    pts *= _multipliers[res[0, i]];

            return pts == bid ? 0L : pts;
        }
    }
}
