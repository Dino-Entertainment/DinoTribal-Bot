﻿using DSharpPlus.Entities;

namespace TheGodfather.Modules.Administration.Extensions;

public static class DiscordClientStatusExtensions
{
    public static string ToUserFriendlyString(this DiscordClientStatus status)
    {
        if (status.Desktop.HasValue)
            return "Desktop";
        if (status.Mobile.HasValue)
            return "Mobile";
        if (status.Web.HasValue)
            return "Web";
        return "Unknown";
    }
}