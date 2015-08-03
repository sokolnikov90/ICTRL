using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ICtrlDLL
{
    class Messages
    {
        public static string SayHello(string name)
        {
            StringBuilder sb = new StringBuilder();
            return sb.AppendFormat("<Message><Hello>{0}</Hello></Message>", name).ToString();
        }

        public static string SaySubscribe(string name)
        {
            StringBuilder sb = new StringBuilder();
            return sb.AppendFormat("<Message><Subscribe>{0}</Subscribe></Message>", name).ToString();
        }

        public static string SayReply(Incident incident, string escalation)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<Message>");
                sb.Append("<IncidentEventICtrl>");
                    sb.AppendFormat("<Id>{0}</Id>", incident.id.ToString().PadLeft(5, '0'));
                    sb.AppendFormat("<Status>{0}</Status>", incident.statusId);
                    sb.AppendFormat("<Type>{0}</Type>", incident.typeId);
                    sb.AppendFormat("<AssignedTo>{0}</AssignedTo>", incident.assignedToId);
                    sb.AppendFormat("<TimeCreate>{0}</TimeCreate>", incident.timeCreated);
                    sb.AppendFormat("<TimeChanged>{0}</TimeChanged>", incident.timeChanged);
                    sb.AppendFormat("<TimeClosed>{0}</TimeClosed>", incident.timeClosed);
                    sb.AppendFormat("<Escalation>{0}</Escalation>", escalation);
                    sb.AppendFormat("<AtmId>{0}</AtmId>", incident.atmId);
                sb.Append("</IncidentEventICtrl>");
            sb.Append("</Message>");
            return sb.ToString();
        }

        public static string GetEscalation()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<Message>");
            sb.Append("<Request name=\"CUserEscalations\">");
            sb.Append("</Request>");
            sb.Append("</Message>");
            return sb.ToString();
        }
    }
}
