using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C969
{
    public class UserReport
    {
        public int appointmentId { get; set; }
        public int UserID { get; set; }
        public int CustomerID { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public UserReport(int appointmentID, int userId, int customerID, DateTime start, DateTime end)
        {
            appointmentId = appointmentID;
            UserID = userId;
            CustomerID = customerID;
            Start = start;
            End = end;
        }
    }
}
