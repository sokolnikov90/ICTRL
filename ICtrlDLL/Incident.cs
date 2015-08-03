using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ICtrlDLL
{
    public class Incident
    {
        public int id { get; set; }
        public string table { get; set; }
        public int statusId { get; set; }
        public int typeId { get; set; }
        public int assignedToId { get; set; }
        public int atmId { get; set; }
        public string timeCreated { get; set; }
        public string timeChanged { get; set; }
        public string timeClosed { get; set; }
        public List<Timer> timers { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Id: " + id + Environment.NewLine);
            sb.Append("Table: " + table + Environment.NewLine);
            sb.Append("StatusId: " + statusId + Environment.NewLine);
            sb.Append("AssignedToId: " + assignedToId + Environment.NewLine);
            sb.Append("AtmId: " + atmId + Environment.NewLine);
            sb.Append("TimeCreated: " + timeCreated + Environment.NewLine);
            sb.Append("TimeChanged: " + timeChanged + Environment.NewLine);
            sb.Append("TimeClosed: " + timeClosed + Environment.NewLine);
            sb.Append("Timers.Count(): " + timers.Count());
            return sb.ToString();
        }
    }
}
