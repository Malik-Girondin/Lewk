using System.Collections.Generic;

namespace C969
{
    public static class TimeZoneStorage
    {
        public static Dictionary<int, string> AppointmentTimeZones { get; private set; } = new Dictionary<int, string>();

        public static void SaveTimeZone(int appointmentId, string timeZone)
        {
            AppointmentTimeZones[appointmentId] = timeZone;
        }

        public static string GetTimeZone(int appointmentId)
        {
            return AppointmentTimeZones.TryGetValue(appointmentId, out string timeZone) ? timeZone : null;
        }
    }
}

