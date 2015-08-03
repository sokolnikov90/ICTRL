using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;

namespace ICtrlDLL
{
    class DbGateway
    {
        public  Incident GetIncident(string sId, string sIncidentTable)
        {
            ICtrl.logger.Info("Start GetIncident(" + sId + "," + sIncidentTable + ")...");

            using (OdbcDataReader dataReader = DBConnection.Instance.SelectRows("SELECT ID, TIME_CREATED, TIME_CHANGED, TIME_CLOSED, STATUS_ID, TYPE_ID, ATM_ID, ASSIGNED_TO_ID FROM " + sIncidentTable + " WHERE ID=" + sId))
            {
                if (dataReader != null)
                {
                    Settings.incidents.Clear();
                    while (dataReader.Read())
                    {
                        try
                        {
                            return (new Incident
                            {
                                id = dataReader.IsDBNull(0) ? 0 : Convert.ToInt32(dataReader.GetValue(0)),
                                table = sIncidentTable,
                                timeCreated = dataReader.IsDBNull(1) ? string.Empty : dataReader.GetValue(1).ToString(),
                                timeChanged = dataReader.IsDBNull(2) ? string.Empty : dataReader.GetValue(2).ToString(),
                                timeClosed = dataReader.IsDBNull(3) ? string.Empty : dataReader.GetValue(3).ToString(),
                                statusId = dataReader.IsDBNull(4) ? 0 : Convert.ToInt32(dataReader.GetValue(4)),
                                typeId = dataReader.IsDBNull(5) ? 0 : Convert.ToInt32(dataReader.GetValue(5)),
                                atmId = dataReader.IsDBNull(6) ? 0 : Convert.ToInt32(dataReader.GetValue(6)),
                                assignedToId = dataReader.IsDBNull(7) ? 0 : Convert.ToInt32(dataReader.GetValue(7))
                            });
                        }
                        catch (Exception e)
                        {
                            ICtrl.logger.Info("ReadFromDataBase(...) adding data from " + sIncidentTable + " where id=" + sId + " to incidents list exception:");
                            ICtrl.logger.Info(e.Message);
                            ICtrl.logger.Info(e.Source);
                            ICtrl.logger.Info(e.StackTrace);

                            DBConnection.Instance.Disconnect();

                            Environment.Exit(0);
                        }
                    }
                }
            }
            return null;
        }

        public string GetTableName(DateTime dateTime)
        {
            return "INCIDENTS" + dateTime.Year + ((dateTime.Month < 10) ? "0" + dateTime.Month.ToString() : dateTime.Month.ToString());
        }

        public List<Incident> GetIncidentsFromDataBase(List<string> monitoringStatuses, List<string> monitoringTypes)
        {
            ICtrl.logger.Info("Reading incidents from database.");
            List<Incident> incidentReadTasks = new List<Incident>();

            string[] tableNames = new string[2];

            DateTime dateTime = DateTime.Now;

            tableNames[0] = GetTableName(dateTime);
            dateTime = dateTime.AddMonths(-1);
            tableNames[1] = GetTableName(dateTime);
            
            string statusId = "";
            for (int i = 0; i < monitoringStatuses.Count(); i++)
            {
                statusId += monitoringStatuses[i];
                if (i != monitoringStatuses.Count() - 1)
                    statusId += ",";
            }

            string typeId = "";
            for (int i = 0; i < monitoringTypes.Count(); i++)
            {
                typeId += monitoringTypes[i];
                if (i != monitoringTypes.Count() - 1)
                    typeId += ",";
            }
            ICtrl.logger.Info("SELECT ID, TIME_CREATED, TIME_CHANGED, TIME_CLOSED, STATUS_ID, TYPE_ID, ATM_ID, ASSIGNED_TO_ID  FROM " + tableNames[0] + " WHERE STATUS_ID IN (" + statusId + ") AND TYPE_ID IN (" + typeId + ")");
            using (OdbcDataReader dataReader = DBConnection.Instance.SelectRows("SELECT ID, TIME_CREATED, TIME_CHANGED, TIME_CLOSED, STATUS_ID, TYPE_ID, ATM_ID, ASSIGNED_TO_ID  FROM " + tableNames[0] + " WHERE STATUS_ID IN (" + statusId + ") AND TYPE_ID IN (" + typeId + ")"))                                                                       
            {

                if (dataReader != null)
                {

                    while (dataReader.Read())
                    {
                        try
                        {
                            incidentReadTasks.Add(new Incident
                            {
                                id = dataReader.IsDBNull(0) ? 0 : Convert.ToInt32(dataReader.GetValue(0)),
                                table = tableNames[0],
                                timeCreated = dataReader.IsDBNull(1) ? string.Empty : dataReader.GetValue(1).ToString(),
                                timeChanged = dataReader.IsDBNull(2) ? string.Empty : dataReader.GetValue(2).ToString(),
                                timeClosed = dataReader.IsDBNull(3) ? string.Empty : dataReader.GetValue(3).ToString(),
                                statusId = dataReader.IsDBNull(4) ? 0 : Convert.ToInt32(dataReader.GetValue(4)),
                                typeId = dataReader.IsDBNull(5) ? 0 : Convert.ToInt32(dataReader.GetValue(5)),
                                atmId = dataReader.IsDBNull(6) ? 0 : Convert.ToInt32(dataReader.GetValue(6)),
                                assignedToId = dataReader.IsDBNull(7) ? 0 : Convert.ToInt32(dataReader.GetValue(7)),
                            });
                        }
                        catch (Exception e)
                        {
                            ICtrl.logger.Info("ReadFromDataBase(...) adding data from " + tableNames[0] + " to incidents list exception:");
                            ICtrl.logger.Info(e.Message);
                            ICtrl.logger.Info(e.Source);
                            ICtrl.logger.Info(e.StackTrace);

                            DBConnection.Instance.Disconnect();

                            Environment.Exit(0);
                        }
                    }
                }
            }

            ICtrl.logger.Info("SELECT ID, TIME_CREATED, TIME_CHANGED, TIME_CLOSED, STATUS_ID, TYPE_ID, ATM_ID, ASSIGNED_TO_ID  FROM " + tableNames[1] + " WHERE STATUS_ID IN (" + statusId + ") AND TYPE_ID IN (" + typeId + ")");
            using (OdbcDataReader dataReader = DBConnection.Instance.SelectRows("SELECT ID, TIME_CREATED, TIME_CHANGED, TIME_CLOSED, STATUS_ID, TYPE_ID, ATM_ID, ASSIGNED_TO_ID  FROM " + tableNames[1] + " WHERE STATUS_ID IN (" + statusId + ") AND TYPE_ID IN (" + typeId + ")"))
            {
                if (dataReader != null)
                {

                    while (dataReader.Read())
                    {
                        try
                        {
                            incidentReadTasks.Add(new Incident
                            {
                                id = dataReader.IsDBNull(0) ? 0 : Convert.ToInt32(dataReader.GetValue(0)),
                                table = tableNames[1],
                                timeCreated = dataReader.IsDBNull(1) ? string.Empty : dataReader.GetValue(1).ToString(),
                                timeChanged = dataReader.IsDBNull(2) ? string.Empty : dataReader.GetValue(2).ToString(),
                                timeClosed = dataReader.IsDBNull(3) ? string.Empty : dataReader.GetValue(3).ToString(),
                                statusId = dataReader.IsDBNull(4) ? 0 : Convert.ToInt32(dataReader.GetValue(4)),
                                typeId = dataReader.IsDBNull(5) ? 0 : Convert.ToInt32(dataReader.GetValue(5)),
                                atmId = dataReader.IsDBNull(6) ? 0 : Convert.ToInt32(dataReader.GetValue(6)),
                                assignedToId = dataReader.IsDBNull(7) ? 0 : Convert.ToInt32(dataReader.GetValue(7)),
                            });
                        }
                        catch (Exception e)
                        {
                            ICtrl.logger.Info("ReadFromDataBase(...) adding data from " + tableNames[0] + " to incidents list exception:");
                            ICtrl.logger.Info(e.Message);
                            ICtrl.logger.Info(e.Source);
                            ICtrl.logger.Info(e.StackTrace);

                            DBConnection.Instance.Disconnect();

                            Environment.Exit(0);
                        }
                    }
                }
            }
            ICtrl.logger.Info("Settings from database has been read.");
            return incidentReadTasks;
        }

        public Dictionary<string, string> GetEscalationDictionary()
        {
            Dictionary<string, string> ret = new Dictionary<string, string>();
            ICtrl.logger.Info("Start GetEscalationDictionary()...");

            using (OdbcDataReader dataReader = DBConnection.Instance.SelectRows("SELECT ID, NAME FROM ESCALATIONS"))
            {
                if (dataReader != null)
                {

                    while (dataReader.Read())
                    {
                        try
                        {
                            string key = dataReader.IsDBNull(0) ? "0" : dataReader.GetValue(0).ToString();
                            string value = dataReader.IsDBNull(1) ? "0" : dataReader.GetValue(1).ToString();
                            ret[key] = value;
                        }
                        catch (Exception e)
                        {
                            ICtrl.logger.Info("GetEscalationDictionary(...) exception:");
                            ICtrl.logger.Info(e.Message);
                            ICtrl.logger.Info(e.Source);
                            ICtrl.logger.Info(e.StackTrace);

                            DBConnection.Instance.Disconnect();

                            Environment.Exit(0);
                        }
                    }
                }
            }
            return ret;
        }

        public List<string> GetMonitoringStatuses()
        {
            List<string> ret = new List<string>();
            ICtrl.logger.Info("Start GetEscalationDictionary()...");

            using (OdbcDataReader dataReader = DBConnection.Instance.SelectRows("SELECT ID FROM STATUSES WHERE IS_CLOSED != 2 ORDER BY ID ASC"))
            {
                if (dataReader != null)
                {

                    while (dataReader.Read())
                    {
                        try
                        {
                            string key = dataReader.IsDBNull(0) ? "0" : dataReader.GetValue(0).ToString();
                            ret.Add(key);
                        }
                        catch (Exception e)
                        {
                            ICtrl.logger.Info("GetEscalationDictionary(...) exception:");
                            ICtrl.logger.Info(e.Message);
                            ICtrl.logger.Info(e.Source);
                            ICtrl.logger.Info(e.StackTrace);

                            DBConnection.Instance.Disconnect();

                            Environment.Exit(0);
                        }
                    }
                }
            }
            return ret;
        }

        public List<string> GetMonitoringTypes()
        {
            List<string> ret = new List<string>();
            ICtrl.logger.Info("Start GetEscalationDictionary()...");

            using (OdbcDataReader dataReader = DBConnection.Instance.SelectRows("SELECT ID FROM TYPES WHERE TEXT!='ATMLocked' ORDER BY ID ASC"))
            {
                if (dataReader != null)
                {

                    while (dataReader.Read())
                    {
                        try
                        {
                            string key = dataReader.IsDBNull(0) ? "0" : dataReader.GetValue(0).ToString();
                            ret.Add(key);
                        }
                        catch (Exception e)
                        {
                            ICtrl.logger.Info("GetEscalationDictionary(...) exception:");
                            ICtrl.logger.Info(e.Message);
                            ICtrl.logger.Info(e.Source);
                            ICtrl.logger.Info(e.StackTrace);

                            DBConnection.Instance.Disconnect();

                            Environment.Exit(0);
                        }
                    }
                }
            }
            return ret;
        }
    }
}




















            /*



            object[] obj = new object[5];

            lock (syncRoot)
            {
                obj[0] = "0";

                OdbcDataReader dataReader = DBConnection.Instance.SelectRows("SELECT STATUS_ID, TYPE_ID, TIME_CREATED FROM " + sIncidentTable + " WHERE ID=" + sId);

                if (dataReader != null)
                {
                    while (dataReader.Read())
                    {
                        try
                        {
                            obj[2] = dataReader.GetInt32(0);
                            obj[3] = dataReader.GetInt32(1);
                            obj[4] = dataReader.GetInt32(2);
                        }
                        catch (Exception e)
                        {
                            ICtrl.logger.Info("GetIncident(...) exception:");
                            ICtrl.logger.Info(e.Message);
                            ICtrl.logger.Info(e.Source);
                            ICtrl.logger.Info(e.StackTrace);
                        }
                    }
                    obj[0] = sId;
                    obj[1] = sIncidentTable;
                }

                for (int i = 0; i <6; i++)
                    ICtrl.logger.Info("obj[" + i + "] = " + obj[i]);

                return obj;
            }*/
