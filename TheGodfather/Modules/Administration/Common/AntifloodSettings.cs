﻿using TheGodfather.Services;

namespace TheGodfather.Modules.Administration.Common
{
    public sealed class AntifloodSettings
    {
        public PunishmentAction Action { get; set; } = PunishmentAction.PermanentBan;
        public short Cooldown { get; set; } = 10;
        public bool Enabled { get; set; } = false;
        public short Sensitivity { get; set; } = 5;


        public string ToEmbedFieldString(ulong gid, LocalizationService lcs)
            => this.Enabled ? lcs.GetString(gid, "fmt-settings-af", this.Sensitivity, this.Cooldown, this.Action.ToTypeString()) : lcs.GetString(gid, "str-off");
    }
}
