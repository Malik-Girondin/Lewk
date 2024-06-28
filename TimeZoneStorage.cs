using System.Collections.Generic;
using System.IO;

public static class TimeZoneStorage
{
    private static Dictionary<int, string> timeZones = new Dictionary<int, string>();
    private static readonly string storagePath = "timezones.txt"; // Path to store time zones

    static TimeZoneStorage()
    {
        LoadTimeZones();
    }

    public static void SetTimeZone(int appointmentId, string timeZone)
    {
        timeZones[appointmentId] = timeZone;
        SaveTimeZones();
    }

    public static string GetTimeZone(int appointmentId)
    {
        return timeZones.TryGetValue(appointmentId, out string timeZone) ? timeZone : null;
    }

    private static void SaveTimeZones()
    {
        using (StreamWriter writer = new StreamWriter(storagePath))
        {
            foreach (var entry in timeZones)
            {
                writer.WriteLine($"{entry.Key},{entry.Value}");
            }
        }
    }

    private static void LoadTimeZones()
    {
        if (!File.Exists(storagePath))
        {
            return;
        }

        using (StreamReader reader = new StreamReader(storagePath))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var parts = line.Split(',');
                if (parts.Length == 2 && int.TryParse(parts[0], out int appointmentId))
                {
                    timeZones[appointmentId] = parts[1];
                }
            }
        }
    }
}
