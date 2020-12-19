﻿using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace TheGodfather.Common
{
    public sealed class IPAddressRange
    {
        private static readonly Regex _formatRegex
            = new Regex(
                @"^(?<range>\d{1,3}\.(\*\.\*\.\*|\d{1,3}\.\*\.\*|\d{1,3}\.\d{1,3}\.\*|\d{1,3}\.\d{1,3}\.\d{1,3}|(\d{1,3}\.){0,2}\d{1,3}))(:(?<port>[1-9]\d{0,4}))?$",
                RegexOptions.Compiled
            );

        public static bool TryParse(string str, out IPAddressRange? res)
        {
            res = null;

            Match m = _formatRegex.Match(str);
            if (!m.Success || !m.Groups.TryGetValue("range", out Group? rangeGroup) || !rangeGroup.Success)
                return false;

            string[] quartets = rangeGroup.Value.Split(".", StringSplitOptions.RemoveEmptyEntries);
            if (quartets.First().All(c => c == '0' || c == '*'))
                return false;
            foreach (string quartet in quartets) {
                if (quartet != "*" && !byte.TryParse(quartet, out byte v))
                    return false;
            }

            if (m.Groups.TryGetValue("port", out Group? portGroup) && portGroup.Success) {
                if (!ushort.TryParse(portGroup.Value, out ushort port) || port < 10)
                    return false;
            }

            res = new IPAddressRange(str, rangeGroup.Value, portGroup?.Value);
            return true;
        }


        public string CompleteRange { get; }
        public string Range { get; }
        public ushort Port { get; }


        private IPAddressRange(string str, string rstr, string? pstr = null)
        {
            this.CompleteRange = str;
            this.Range = rstr;
            if (ushort.TryParse(pstr, out ushort port))
                this.Port = port;
        }
    }
}
