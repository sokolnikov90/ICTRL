using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Text;

namespace ICtrlDLL
{
    public class DBConnection
    {
        private static volatile DBConnection instance;
        private static object syncRoot = new Object();

        private string connectionString;
        private OdbcConnection connection;

        public static string dsn;
        public static string uid;
        public static string pwd;

        private DBConnection()
        {
            if (DBConnection.dsn != null && DBConnection.uid != null && DBConnection.pwd != null)
            {
                connectionString = "DSN=" + DBConnection.dsn + ";UID=" + DBConnection.uid + ";PWD=" + CryptoPWD.XorMask(CryptoPWD.Str2Hex(DBConnection.pwd), "bgszg");

                this.Connect();
            }
            else
            {
                ICtrl.logger.Info("Connection string can not be formed.");

                ICtrl.logger.Info("DSN: " + DBConnection.dsn);
                ICtrl.logger.Info("UID: " + DBConnection.uid);
                ICtrl.logger.Info("PWD: " + DBConnection.pwd);

                DBConnection.Instance.Disconnect();

                Environment.Exit(0);
            }
        }

        public static DBConnection Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new DBConnection();
                    }
                }

                return instance;
            }
        }

        public void Connect()
        {
            if (connection == null)
                connection = new OdbcConnection(connectionString);

            try
            {
                connection.Open();
            }
            catch (OdbcException e)
            {
                ICtrl.logger.Info("Connect(...) exception:");
                ICtrl.logger.Info(e.Message);
                ICtrl.logger.Info(e.Source);
                ICtrl.logger.Info(e.StackTrace);

                DBConnection.Instance.Disconnect();

                Environment.Exit(0);
            }
        }

        public void Reconnect()
        {
            this.Disconnect();
            this.Connect();
        }

        public void Disconnect()
        {
            if (connection != null)
                connection.Close();
        }

        public OdbcDataReader SelectRows(Object stateInfo)
        {
            try
            {
                if (connection != null)
                {
                    if (connection.State != ConnectionState.Open)
                        this.Reconnect();
                }
                else this.Connect();

                using (OdbcCommand command = connection.CreateCommand())
                {
                    command.CommandText = stateInfo.ToString();

                    return command.ExecuteReader();
                }
            }
            catch (Exception e)
            {
                ICtrl.logger.Info("SelectRows(...) exception:");
                ICtrl.logger.Info("Sql command text: " + stateInfo.ToString());
                ICtrl.logger.Info(e.Message);
                ICtrl.logger.Info(e.Source);
                ICtrl.logger.Info(e.StackTrace);
            }

            return null;
        }

        public int InsertRows(Object stateInfo)
        {
            try
            {
                if (connection != null)
                {
                    if (connection.State != ConnectionState.Open)
                        this.Reconnect();
                }
                else this.Connect();

                using (OdbcCommand command = connection.CreateCommand())
                {
                    command.CommandText = stateInfo.ToString();

                    return command.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                ICtrl.logger.Info("InsertRows(...) exception:");
                ICtrl.logger.Info("Sql command text: " + stateInfo.ToString());
                ICtrl.logger.Info(e.Message);
                ICtrl.logger.Info(e.Source);
                ICtrl.logger.Info(e.StackTrace);
            }

            return 0;
        }
    }
}
