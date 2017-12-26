﻿#region USING_DIRECTIVES
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

using DSharpPlus;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Helpers
{
    public class GameStats
    {
        private IReadOnlyDictionary<string, string> Stats { get; }
        public ulong UserId {
            get {
                bool succ = ulong.TryParse(Stats["uid"], out ulong uid); return succ ? uid : 0;
            }
        }


        public GameStats(IReadOnlyDictionary<string, string> statdict)
        {
            Stats = statdict;
        }

        public DiscordEmbedBuilder GetEmbeddedStats()
        {
            var eb = new DiscordEmbedBuilder() { Color = DiscordColor.Chartreuse };
            eb.AddField("Duel stats", DuelStatsString());
            eb.AddField("Tic-Tac-Toe stats", TTTStatsString());
            eb.AddField("Connect4 stats", Chain4StatsString());
            eb.AddField("Caro stats", CaroStatsString());
            eb.AddField("Nunchi stats", NunchiStatsString(), inline: true);
            eb.AddField("Quiz stats", QuizStatsString(), inline: true);
            eb.AddField("Race stats", RaceStatsString(), inline: true);
            eb.AddField("Hangman stats", HangmanStatsString(), inline: true);
            return eb;
        }

        public string DuelStatsString()
            => $"W: {Stats["duels_won"]} L: {Stats["duels_lost"]} ({Formatter.Bold($"{CalculateWinPercentage(Stats["duels_won"], Stats["duels_lost"])}")}%)";

        public string TTTStatsString()
            => $"W: {Stats["ttt_won"]} L: {Stats["ttt_lost"]} ({Formatter.Bold($"{CalculateWinPercentage(Stats["ttt_won"], Stats["ttt_lost"])}")}%)";

        public string Chain4StatsString()
            => $"W: {Stats["chain4_won"]} L: {Stats["chain4_lost"]} ({Formatter.Bold($"{CalculateWinPercentage(Stats["chain4_won"], Stats["chain4_lost"])}")}%)";

        public string CaroStatsString()
            => $"W: {Stats["caro_won"]} L: {Stats["caro_lost"]} ({Formatter.Bold($"{CalculateWinPercentage(Stats["caro_won"], Stats["caro_lost"])}")}%)";

        public string NunchiStatsString()
            => $"W: {Stats["nunchis_won"]}";

        public string QuizStatsString()
            => $"W: {Stats["quizes_won"]}";

        public string RaceStatsString()
            => $"W: {Stats["races_won"]}";

        public string HangmanStatsString()
            => $"W: {Stats["hangman_won"]}";

        public uint CalculateWinPercentage(string won, string lost)
        {
            int.TryParse(won, out int w);
            int.TryParse(lost, out int l);

            if (w + l == 0)
                return 0;

            return (uint)Math.Round((double)w / (w + l) * 100);
        }
    }
}
