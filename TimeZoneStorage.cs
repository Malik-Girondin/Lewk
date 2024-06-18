using System.Collections.Generic;

namespace C969
{
    public static class TimeZoneStorage
    {
        private static Dictionary<int, string> timeZoneDictionary = new Dictionary<int, string>();

        public static void SetTimeZone(int appointmentId, string timeZone)
        {
            timeZoneDictionary[appointmentId] = timeZone;
        }

        public static string GetTimeZone(int appointmentId)
        {
            if (timeZoneDictionary.TryGetValue(appointmentId, out string timeZone))
            {
                return timeZone;
            }
            return null;
        }
    }
}
