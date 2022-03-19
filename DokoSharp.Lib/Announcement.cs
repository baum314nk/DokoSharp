using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DokoSharp.Lib;

/// <summary>
/// An enumeration of Doko announcements.
/// </summary>
public enum Announcement
{
    None,
    Re,
    Contra,
    Under90,
    Under60,
    Under30,
    Black
}

public static class AnnouncementExtensions
{
    public static readonly IReadOnlyDictionary<Announcement, string?> AnnouncementNames;

    static AnnouncementExtensions()
    {
        AnnouncementNames = new Dictionary<Announcement, string?>()
        {
            {Announcement.None, null},
            {Announcement.Re, "Re"},
            {Announcement.Contra, "Contra"},
            {Announcement.Under90, "Unter 90"},
            {Announcement.Under60, "Unter 60"},
            {Announcement.Under30, "Unter 30"},
            {Announcement.Black, "Schwarz"},
        };
    }

    /// <summary>
    /// Gets the name of the announcement.
    /// </summary>
    /// <param name="announcement"></param>
    /// <returns></returns>
    public static string? GetName(this Announcement announcement)
    {
        return AnnouncementNames[announcement];
    }
}