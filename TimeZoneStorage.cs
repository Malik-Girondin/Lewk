using System;
using System.Collections.Generic;

public static class TimeZoneStorage
{
    private static Dictionary<int, string> timeZoneDictionary = new Dictionary<int, string>();

    public static void SetTimeZone(int appointmentId, string timeZone)
    {
        if (timeZoneDictionary.ContainsKey(appointmentId))
        {
            timeZoneDictionary[appointmentId] = timeZone;
        }
        else
        {
            timeZoneDictionary.Add(appointmentId, timeZone);
        }
        // Debug Statement
        Console.WriteLine($"GetTimeZone: appointmentId={appointmentId}, timeZone={timeZone}");

    }

    public static string GetTimeZone(int appointmentId)
    {
        if (timeZoneDictionary.TryGetValue(appointmentId, out string timeZone))
        {
            // Debug statement
            Console.WriteLine($"GetTimeZone: appointmentId={appointmentId}, timeZone={timeZone}");

            return timeZone;
        }
        else
        {
            return null;
        }
    }
}
