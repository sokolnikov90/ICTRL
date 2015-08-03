using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ICtrlDLL
{
    public class Report
    {
        public int taskId { get; set; }
        public string userId { get; set; }
        public string runTime { get; set; }
        public string from { get; set; }
        public string to { get; set; }
        public string path { get; set; }
        public string format { get; set; }
        public string type { get; set; }

        public List<string> atmsId { get; set; }
        public List<string> atmsGroupsId { get; set; }
    }
}
