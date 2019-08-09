﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using DSharpPlus.Entities;

namespace TheGodfather.Common
{
    public static class Emojis
    {
        #region Animals
        public static class Animals
        {
            public static IReadOnlyList<DiscordEmoji> All => _animals;
            public static readonly ImmutableList<DiscordEmoji> _animals = new List<DiscordEmoji> {
                DiscordEmoji.FromUnicode("\U0001f436"),     // :dog:
                DiscordEmoji.FromUnicode("\U0001f431"),     // :cat: 
                DiscordEmoji.FromUnicode("\U0001f42d"),     // :mouse:
                DiscordEmoji.FromUnicode("\U0001f439"),     // :hamster:
                DiscordEmoji.FromUnicode("\U0001f430"),     // :rabbit:
                DiscordEmoji.FromUnicode("\U0001f43b"),     // :bear:
                DiscordEmoji.FromUnicode("\U0001f437"),     // :pig:
                DiscordEmoji.FromUnicode("\U0001f42e"),     // :cow:
                DiscordEmoji.FromUnicode("\U0001f428"),     // :koala:
                DiscordEmoji.FromUnicode("\U0001f42f"),     // :tiger:
            }.ToImmutableList();
        }
        #endregion

        #region Cards
        public static class Cards
        {
            public static IReadOnlyList<DiscordEmoji> Values => _values;
            private static readonly ImmutableList<DiscordEmoji> _values = new List<DiscordEmoji> {
                DiscordEmoji.FromUnicode("\U0001f1e6"),     // :regional_indicator_a:
                Numbers.Get(1),
                Numbers.Get(2),
                Numbers.Get(3),
                Numbers.Get(4),
                Numbers.Get(5),
                Numbers.Get(6),
                Numbers.Get(7),
                Numbers.Get(8),
                DiscordEmoji.FromUnicode("\U0001f1f9"),     // :regional_indicator_t:
                DiscordEmoji.FromUnicode("\U0001f1ef"),     // :regional_indicator_j:
                DiscordEmoji.FromUnicode("\U0001f1f6"),     // :regional_indicator_q:
                DiscordEmoji.FromUnicode("\U0001f1f0"),     // :regional_indicator_k:
                DiscordEmoji.FromUnicode("\U0001f1e6"),     // :regional_indicator_a:
            }.ToImmutableList();

            public static IReadOnlyList<DiscordEmoji> Suits => _suits;
            public static ImmutableList<DiscordEmoji> _suits = new List<DiscordEmoji> {
                DiscordEmoji.FromUnicode("\u2660"),         // :spades:
                DiscordEmoji.FromUnicode("\u2663"),         // :clubs:
                DiscordEmoji.FromUnicode("\u2665"),         // :hearts:
                DiscordEmoji.FromUnicode("\u2666"),         // :diamonds:
            }.ToImmutableList();
        }
        #endregion

        #region Casino
        public static DiscordEmoji Cherries { get; } = DiscordEmoji.FromUnicode("\U0001f352");
        public static DiscordEmoji Gift { get; } = DiscordEmoji.FromUnicode("\U0001f381");
        public static DiscordEmoji LargeBlueDiamond { get; } = DiscordEmoji.FromUnicode("\U0001f537");
        public static DiscordEmoji LargeOrangeDiamond { get; } = DiscordEmoji.FromUnicode("\U0001f536");
        public static DiscordEmoji MoneyBag { get; } = DiscordEmoji.FromUnicode("\U0001f4b0");
        public static DiscordEmoji Peach { get; } = DiscordEmoji.FromUnicode("\U0001f351");
        public static DiscordEmoji Seven { get; } = DiscordEmoji.FromUnicode("7\u20e3");
        public static DiscordEmoji SmallBlueDiamond { get; } = DiscordEmoji.FromUnicode("\U0001f539");
        public static DiscordEmoji SmallOrangeDiamond { get; } = DiscordEmoji.FromUnicode("\U0001f538");
        #endregion

        #region Games
        public static DiscordEmoji Bicyclist { get; } = DiscordEmoji.FromUnicode("\U0001f6b4");
        public static DiscordEmoji BlackSquare { get; } = DiscordEmoji.FromUnicode("\u2b1b");
        public static DiscordEmoji Blast { get; } = DiscordEmoji.FromUnicode("\U0001f4a2");
        public static DiscordEmoji BoardPieceBlueCircle { get; } = DiscordEmoji.FromUnicode("\U0001f535");
        public static DiscordEmoji BoardPieceRedCircle { get; } = DiscordEmoji.FromUnicode("\U0001f534");
        public static DiscordEmoji BoardSquare { get; } = DiscordEmoji.FromUnicode("\u25fb");
        public static DiscordEmoji Dead { get; } = DiscordEmoji.FromUnicode("\U0001f635");
        public static DiscordEmoji Dice { get; } = DiscordEmoji.FromUnicode("\U0001f3b2");
        public static DiscordEmoji DuelSwords { get; } = DiscordEmoji.FromUnicode("\u2694");
        public static DiscordEmoji Gun { get; } = DiscordEmoji.FromUnicode("\U0001f52b");
        public static DiscordEmoji Headphones { get; } = DiscordEmoji.FromUnicode("\U0001f3a7");
        public static DiscordEmoji Joystick { get; } = DiscordEmoji.FromUnicode("\U0001f579");
        public static DiscordEmoji O { get; } = DiscordEmoji.FromUnicode("\u2b55");
        public static DiscordEmoji Relieved { get; } = DiscordEmoji.FromUnicode("\U0001f616");
        public static DiscordEmoji Syringe { get; } = DiscordEmoji.FromUnicode("\U0001f489");
        public static DiscordEmoji Trophy { get; } = DiscordEmoji.FromUnicode("\U0001f3c6");
        public static DiscordEmoji WhiteSquare { get; } = DiscordEmoji.FromUnicode("\u2b1c");
        public static DiscordEmoji X { get; } = DiscordEmoji.FromUnicode("\u274c");
        #endregion

        #region Misc
        public static DiscordEmoji AlarmClock { get; } = DiscordEmoji.FromUnicode("\u23f0");
        public static DiscordEmoji ArrowDown { get; } = DiscordEmoji.FromUnicode("\u2b07");
        public static DiscordEmoji ArrowUp { get; } = DiscordEmoji.FromUnicode("\u2b06");
        public static DiscordEmoji Cake { get; } = DiscordEmoji.FromUnicode("\U0001f370");
        public static DiscordEmoji CheckMarkSuccess { get; } = DiscordEmoji.FromUnicode("\u2705");
        public static DiscordEmoji Chicken { get; } = DiscordEmoji.FromUnicode("\U0001f414");
        public static DiscordEmoji Clock1 { get; } = DiscordEmoji.FromUnicode("\U0001f550");
        public static DiscordEmoji Information { get; } = DiscordEmoji.FromUnicode("\u2139");
        public static DiscordEmoji Medal { get; } = DiscordEmoji.FromUnicode("\U0001f3c5");
        public static DiscordEmoji NoEntry { get; } = DiscordEmoji.FromUnicode("\u26d4");
        public static DiscordEmoji Question { get; } = DiscordEmoji.FromUnicode("\u2753");
        public static DiscordEmoji Tada { get; } = DiscordEmoji.FromUnicode("\U0001f389");
        public static DiscordEmoji Wave { get; } = DiscordEmoji.FromUnicode("\U0001f44b");
        #endregion

        #region Numbers
        public static class Numbers
        {
            public static IReadOnlyList<DiscordEmoji> All => _numbers;
            private static readonly ImmutableList<DiscordEmoji> _numbers = new List<DiscordEmoji> {
                DiscordEmoji.FromUnicode("1\u20e3"),
                DiscordEmoji.FromUnicode("2\u20e3"),
                DiscordEmoji.FromUnicode("3\u20e3"),
                DiscordEmoji.FromUnicode("4\u20e3"),
                DiscordEmoji.FromUnicode("5\u20e3"),
                DiscordEmoji.FromUnicode("6\u20e3"),
                DiscordEmoji.FromUnicode("7\u20e3"),
                DiscordEmoji.FromUnicode("8\u20e3"),
                DiscordEmoji.FromUnicode("9\u20e3"),
                DiscordEmoji.FromUnicode("\U0001f51f"),
            }.ToImmutableList();

            public static DiscordEmoji Get(int index)
            {
                if (index < 1 || index > 10)
                    throw new ArgumentOutOfRangeException("Index can't be lower than 1 or greater than 10");
                return _numbers[index];
            }
        }
        #endregion

        #region Weapons
        public static class Weapons
        {
            public static IReadOnlyList<DiscordEmoji> All => _weapons;
            private static readonly ImmutableList<DiscordEmoji> _weapons = new List<DiscordEmoji> {
                DiscordEmoji.FromUnicode("\U0001f528"),     // :hammer:
                DiscordEmoji.FromUnicode("\U0001f5e1"),     // :dagger:
                DiscordEmoji.FromUnicode("\u26cf"),         // :pick:
                DiscordEmoji.FromUnicode("\U0001f4a3"),     // :bomb:
                DiscordEmoji.FromUnicode("\U0001f525"),     // :fire:
                DiscordEmoji.FromUnicode("\U0001f3f9"),     // :bow_and_arrow:
                DiscordEmoji.FromUnicode("\U0001f529"),     // :nut_and_bolt:
            }.ToImmutableList();

            public static DiscordEmoji GetRandomDuelWeapon()
                => All[GFRandom.Generator.Next(All.Count)];
        }
        #endregion

        #region Weather
        public static DiscordEmoji Cloud { get; } = DiscordEmoji.FromUnicode("\u2601");
        public static DiscordEmoji Drops { get; } = DiscordEmoji.FromUnicode("\U0001f4a6");
        public static DiscordEmoji Globe { get; } = DiscordEmoji.FromUnicode("\U0001f30d");
        public static DiscordEmoji Ruler { get; } = DiscordEmoji.FromUnicode("\U0001f4cf");
        public static DiscordEmoji Thermometer { get; } = DiscordEmoji.FromUnicode("\U0001f321");
        public static DiscordEmoji Wind { get; } = DiscordEmoji.FromUnicode("\U0001f4a8");
        #endregion
    }
}
