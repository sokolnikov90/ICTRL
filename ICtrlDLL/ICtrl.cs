using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.XPath;

using NLog;

namespace ICtrlDLL
{
    public class ICtrl
    {
        private static volatile ICtrl instance;
        public static object syncRoot = new Object();

        public static ICtrl Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new ICtrl();
                    }
                }

                return instance;
            }
        }

        public static volatile IPConnection ipConnection;
        public static IPConnection IpConnection
        {
            get
            {
                if (ipConnection == null)
                {
                    lock (syncRoot)
                    {
                        if (ipConnection == null)
                            ipConnection = new IPConnection();
                    }
                }

                return ipConnection;
            }
        }

        public static volatile List<Incident> incidentTasks = new List<Incident>();

        public static Logger logger = LogManager.GetCurrentClassLogger();

        public Settings settings = new Settings();

        private DbGateway dbgateway = new DbGateway();

        public static volatile ReportTimer reportTimer = new ReportTimer();

        public ICtrl()
        {
            settings.ConfigureNlog(logger);
            settings.Read();
        }

        public void Run()
        {
            ipConnection = IpConnection;
            reportTimer.dicInventoryInterval = dbgateway.GetEscalationDictionary();
            reportTimer.monitoringStatuses = dbgateway.GetMonitoringStatuses();
            reportTimer.monitoringTypes = dbgateway.GetMonitoringTypes();

            foreach (string ms in reportTimer.monitoringStatuses)
            {
                ICtrl.logger.Info("Monitoring status: " + ms);
            }

            foreach (string mt in reportTimer.monitoringTypes)
            {
                ICtrl.logger.Info("Monitoring type: " + mt);
            }

            SetTimersForDbIncidentTasks(dbgateway.GetIncidentsFromDataBase(reportTimer.monitoringStatuses, reportTimer.monitoringTypes).ToArray());

            IncidentListLogger();

            ipConnection.ReadEvent += new IPConnection.ReadDelegate(IPRead);

            try
            {
                ipConnection.Connect(Settings.ip, Settings.port);
            }
            catch (Exception e)
            {
                ICtrl.logger.Info("Connect(...) exception:");
                ICtrl.logger.Info(e.Message);
                ICtrl.logger.Info(e.Source);
                ICtrl.logger.Info(e.StackTrace);

                DBConnection.Instance.Disconnect();

                Environment.Exit(0);
            }

            ipConnection.Write(Messages.SayHello(Settings.selfName));

            for (int i = 0; i < Settings.subscribeNames.Length; i++)
            {
                ipConnection.Write(Messages.SaySubscribe(Settings.subscribeNames[i]));
            }

            ipConnection.Write(Messages.GetEscalation());
        }

        private void SetTimersForDbIncidentTasks(Incident[] incidentTasksArray)
        {
            ICtrl.logger.Info("Start SetTimersForDbIncidentTasks()...");
            ICtrl.logger.Info("Settings.incidents.Count  = " + incidentTasks.Count());
            try
            {
                for (int i = 0; i < incidentTasksArray.Count(); i++)
                {

                    if ((reportTimer.monitoringStatuses.Contains(incidentTasksArray[i].statusId.ToString())) && (reportTimer.monitoringTypes.Contains(incidentTasksArray[i].typeId.ToString())))
                    {
                        lock (syncRoot)
                        {
                            ICtrl.logger.Info("Starting AddIncidentTask(" + incidentTasksArray[i].id + ")");
                            reportTimer.SetTimers(ref incidentTasksArray[i]);

                            if (incidentTasksArray[i].timers.Count != 0)
                                incidentTasks.Add(incidentTasksArray[i]);
                        }
                        ICtrl.logger.Info("IncidentTask {id = " + incidentTasksArray[i].id + "} has been created.");
                    }
                }
            }

            catch
            {
                ICtrl.logger.Info("SetTimersForDbIncidentTasks()... ERROR");
            }
        }

        private void IPRead(string message, bool isComplite)
        {
            ICtrl.logger.Info("IPRead(...) message: " + message);

            XmlDocument document = new XmlDocument();

            try
            {
                    document.LoadXml(message);

                    if (document.SelectSingleNode("SystemCommand") != null)
                    {

                        string incidentId = document.SelectSingleNode("/Message/SystemCommand/Id").InnerText;
                        string incidentTableName = document.SelectSingleNode("/Message/SystemCommand/Table").InnerText;
                        switch (document.SelectSingleNode("/Message/SystemCommand/Status").InnerText)
                        {
                            case "CREATED":
                                ThreadPool.QueueUserWorkItem(new WaitCallback(this.ReportTaskCreate), new object[] { incidentId, incidentTableName });
                                break;
                            case "CHANGED":
                                ThreadPool.QueueUserWorkItem(new WaitCallback(this.ReportTaskChange), new object[] { incidentId, incidentTableName });
                                break;
                        }
                    }
             }
            catch (Exception e)
            {
                ICtrl.logger.Info("IPRead(...) exception:");
                ICtrl.logger.Info(e.Message);
                ICtrl.logger.Info(e.Source);
                ICtrl.logger.Info(e.StackTrace);
            }
        }

        private void ReportTaskCreate(object state)
        {
            try
            {
                ICtrl.logger.Info("Creating incident.");

                object[] array = state as object[];

                string id = array[0] as string;
                string table = array[1] as string;
                Incident incidentTask = dbgateway.GetIncident(id, table);
                
                //Если инцидент не закрыт и статус не АТМ Locked
                //Аналогично условию: if ((new[] { 1, 2, 3, 4, 7 }.Contains(incidentTask.statusId)) && (new[] { 8, 9, 11 }.Contains(incidentTask.typeId)))
                if ((reportTimer.monitoringStatuses.Contains(incidentTask.statusId.ToString())) && (reportTimer.monitoringTypes.Contains(incidentTask.typeId.ToString())))
                {
                    lock (syncRoot)
                    {
                        ICtrl.logger.Info("Starting AddIncidentTask(" + incidentTask.id + ")");
                        reportTimer.SetTimers(ref incidentTask);

                        if (incidentTask.timers.Count != 0)
                        {
                            incidentTasks.Add(incidentTask);
                            ICtrl.logger.Info("IncidentTask {id = " + incidentTask.id + "} has been created.");
                        }
                        else
                            ICtrl.logger.Info("IncidentTask {id = " + incidentTask.id + "}. All timers is out.");
                    }
                }

                IncidentListLogger();

            }
            catch (Exception e)
            {
                ICtrl.logger.Info("Creating incident Error: " + e.ToString());
            }
        }

        private void ReportTaskChange(object state)
        {
            try
            {
                ICtrl.logger.Info("ReportTaskChange.");

                object[] array = state as object[];

                string id = array[0] as string;
                string table = array[1] as string;
                Incident incidentTask = dbgateway.GetIncident(id, table);

                    lock (syncRoot)
                    {
                        int index = incidentTasks.FindIndex(x => x.id == incidentTask.id && x.table == incidentTask.table);
                        if (index == -1)
                        {   //Если инцидента нет в нашем списке, то вызываем функцию его добавления
                            ICtrl.logger.Info("Инцидента нет в ICTRL. Переходим к ReportTaskCreate");
                            ReportTaskCreate(state);
                        }
                        else
                        {
                            ICtrl.logger.Info("Инцидент имеется в ICTRL...");
                            //Если инцидент имеется в списке, првоеряем его тип и статус и удаляем, иначе ничего не делаем
                            //Аналогично условию: if ((new[] { 5, 6 }.Contains(incidentTask.statusId)) || incidentTask.typeId == 10)
                            if ((!reportTimer.monitoringStatuses.Contains(incidentTask.statusId.ToString())) || (!reportTimer.monitoringTypes.Contains(incidentTask.typeId.ToString())))
                            {
                                incidentTask = incidentTasks[index];
                                reportTimer.ClearTimers(ref incidentTask);
                                incidentTasks.RemoveAt(index);
                                ICtrl.logger.Info("Incident {id = " + incidentTask.id + "} has been deleted.");
                            }
                            IncidentListLogger();
                        }
                    }
                
            }
            catch (Exception e)
            {
                ICtrl.logger.Info("ReportTaskChange Error: " + e.ToString());
            }
        }

        private void AddIncidentTask(Incident inc)
        {
            ICtrl.logger.Info("Starting AddIncidentTask(" + inc.id + ")");
            lock (syncRoot)
            {
                reportTimer.SetTimers(ref inc);

                incidentTasks.Add(inc);

                ICtrl.logger.Info("IncidentTask {id = " + inc.id + "} has been created.");
            }
        }

        public void CheckIncident(object state)
        {
            ICtrl.logger.Info("Starting CheckIncident()...");
            try
            {
                object[] array = state as object[];
                Incident incidentTask = array[0] as Incident;
                string escalation = array[1] as String;
                DateTime dateTime = DateTime.Parse(incidentTask.timeCreated);
                string tableName = "INCIDENTS" + dateTime.Year + ((dateTime.Month < 10) ? "0" + dateTime.Month.ToString() : dateTime.Month.ToString());

                incidentTask = dbgateway.GetIncident(incidentTask.id.ToString(), tableName);

                //Если статус инцидента равен 5(выполнен) или 6(отменён), удаляем его из incidentTasks
                //Аналогично условию: if (new[] { 5, 6 }.Contains(incidentTask.statusId))
                if ((!reportTimer.monitoringStatuses.Contains(incidentTask.statusId.ToString())) || (!reportTimer.monitoringTypes.Contains(incidentTask.typeId.ToString())))           
                {
                    lock (syncRoot)
                    {
                        var index = incidentTasks.FindIndex(x => x.id == incidentTask.id);

                        incidentTask = incidentTasks[index];
                        reportTimer.ClearTimers(ref incidentTask);
                        incidentTasks.RemoveAt(index);

                        ICtrl.logger.Info("Incident {id = " + incidentTask.id + "} has been deleted.");
                    }
                }
                else
                {
                    try
                    {
                        ipConnection.Write(Messages.SayReply(incidentTask, reportTimer.GetHours(Convert.ToInt32(escalation)).ToString()));
                        
                        //Если выполнили задачу, у которой был максимальный уровень эскалации, то удаляем задачу
                        if (escalation == reportTimer.dicInventoryInterval.Count.ToString()) 
                        {
                            lock (syncRoot)
                            {
                                var index = incidentTasks.FindIndex(x => x.id == incidentTask.id);
                                ICtrl.logger.Info("incidentTasks id = " + incidentTask.id + "; index = " + index);
                                incidentTasks.RemoveAt(index);

                                ICtrl.logger.Info("Incident {id = " + index + "} has been deleted. Escalation = 5.");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        ICtrl.logger.Info("Connect(...) exception:");
                        ICtrl.logger.Info(e.Message);
                        ICtrl.logger.Info(e.Source);
                        ICtrl.logger.Info(e.StackTrace);
                    }
                }
                
            }
            catch (Exception e)
            {
                ICtrl.logger.Info("CheckIncident(...) exception:");
                ICtrl.logger.Info(e.Message);
                ICtrl.logger.Info(e.Source);
                ICtrl.logger.Info(e.StackTrace);
            }
            ICtrl.logger.Info("Finish CheckIncident()");
        }

        private void IncidentListLogger()
        {
            ICtrl.logger.Info("IncidentListLogger()...");
            for (int i = 0; i < incidentTasks.Count(); i++)
                ICtrl.logger.Info(string.Format("{0}. {1}", i + 1, incidentTasks[i]));
        }
    }
}
