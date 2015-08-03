using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ICtrlDLL
{
    public class ReportTimer
    {
        public Dictionary<string, string> dicInventoryInterval = new Dictionary<string, string>();

        public List<String> monitoringStatuses = new List<String>();
        public List<String> monitoringTypes = new List<String>();

        public Int32 GetHours(int word)
        {
            string result;
            if (dicInventoryInterval.TryGetValue(word.ToString(), out result))
            {
                ICtrl.logger.Info(string.Format("Starting GetHours({0}); result = {1}", word, result));
                return Convert.ToInt32(result);
            }
            else
            {
                ICtrl.logger.Info(string.Format("Starting GetHours({0}); result = {1}", word, 0));
                return 0;
            }
        }

        private Timer timer;

        public void SetTimers(ref Incident reportTask)
        {
            ICtrl.logger.Info("Starting SetTimers()...");
            DateTime nowDateTime = DateTime.Now;

            TimeSpan dueTimeSpan;
            TimeSpan periodTimeSpan;

            try
            {
                this.ClearTimers(ref reportTask);

                for (int i = 1; i <= dicInventoryInterval.Count; i++)
                {
                    long dueTime = this.SetDueTime(reportTask, i);
                    if (dueTime > 0)
                    {
                        dueTimeSpan = new TimeSpan(dueTime);
                        periodTimeSpan = new TimeSpan(0, 0, 0, 0, -1);
                        timer = new Timer(new TimerCallback(ICtrl.Instance.CheckIncident), new object[] { reportTask, i.ToString() }, dueTimeSpan, periodTimeSpan);
                        reportTask.timers.Add(timer);
                    }
                }
            }
            catch (Exception e)
            {
                ICtrl.logger.Info("SetTimers(...) exception:");
                ICtrl.logger.Info(e.Message);
                ICtrl.logger.Info(e.Source);
                ICtrl.logger.Info(e.StackTrace);
            }
            ICtrl.logger.Info("Finish SetTimers()...");
        }

        public void ClearTimers(ref Incident reportTask)
        {
            try
            {
                if (reportTask.timers != null)
                {
                    for (int i = 0; i < reportTask.timers.Count; i++)
                    {
                        reportTask.timers[i].Dispose();
                    }

                    reportTask.timers = new List<Timer>();
                }
                else reportTask.timers = new List<Timer>();
            }
            catch (Exception e)
            {
                ICtrl.logger.Info("ClearTimers(...) exception:");
                ICtrl.logger.Info(e.Message);
                ICtrl.logger.Info(e.Source);
                ICtrl.logger.Info(e.StackTrace);
            }
        }

        private long SetDueTime(Incident reportTask, int iEscalation)
        {
            long ticks = 0;
            long dtime = 0;

            DateTime nowDateTime = DateTime.Now;
            DateTime createDateTime = DateTime.Parse(reportTask.timeCreated);
            DateTime runDateTime;

            runDateTime = createDateTime + new TimeSpan(GetHours(iEscalation), 0, 0);
            dtime = (runDateTime - nowDateTime).Ticks;
            if (dtime > 0)
            {
                ticks = dtime;
            }

            return ticks;
        }
    }
}
