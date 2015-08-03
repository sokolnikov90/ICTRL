using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

using NLog;
using NLog.Targets;
using NLog.Targets.Wrappers;

namespace ICtrlDLL
{
    public class Settings
    {
        public struct User
        {
            public int id;
            public string email;
            public string fname;
            public string lname;
            public string surname;
            public string phone;
            public int escalation;
        }

        public static int port;
        public static string ip;

        public static int logOn;

        public static string selfName;
        public static string[] subscribeNames;

        public static List<Incident> incidents = new List<Incident>();

    
        public void Read()
        {
            ReadFromRegistry();
        }

        private void ReadFromRegistry()
        {
            ICtrl.logger.Info("Reading settings from registry.");

            string paramValue = "";

            if (Registry.GetValue(Registry.REG_BASE, "DSN", ref paramValue))
            {
                DBConnection.dsn = paramValue;
            }
            else DBConnection.dsn = "";

            if (Registry.GetValue(Registry.REG_BASE, "DBUSER", ref paramValue))
            {
                DBConnection.uid = paramValue;
            }
            else DBConnection.uid = "";

            if (Registry.GetValue(Registry.REG_BASE, "DBPASSWORD", ref paramValue))
            {
                DBConnection.pwd = paramValue;
            }
            else DBConnection.pwd = "";

            if (Registry.GetValue(Registry.REG_BASE, "PORT", ref paramValue))
            {
                port = Convert.ToInt32(paramValue);
            }
            else port = 0;

            if (Registry.GetValue(Registry.REG_BASE, "IP", ref paramValue))
            {
                ip = paramValue;
            }
            else ip = "";

            if (Registry.GetValue(Registry.REG_BASE + "\\SUBSCRIBE", "SELFNAME", ref paramValue))
            {
                selfName = paramValue;
            }
            else selfName = "ICtrl";

            if (Registry.GetValue(Registry.REG_BASE + "\\SUBSCRIBE", "SUBSCRIBE", ref paramValue))
            {
                subscribeNames = paramValue.Split(';');
            }
            else subscribeNames = new string[] { };

            #if DEBUG
                DBConnection.dsn = "ATMMONITOR";
                DBConnection.pwd = "0c080713011b";
                DBConnection.uid = "atmmonitor";
                port = 2882;
                ip = "172.23.100.209";
            #endif

            ICtrl.logger.Info("Settings from registry has been read.");
        }

        public void ConfigureNlog(Logger logger)
        {
            FileTarget target = new FileTarget();

            target.FileName = "${basedir}\\logs\\ICtrl.Log_${date:format=ddMMyyyy}.txt";
            target.KeepFileOpen = false;
            target.Encoding = "windows-1251";
            target.Layout = "${date:format=HH\\:mm\\:ss.fff}|${level:padding=5:uppercase=true}|${message}";

            AsyncTargetWrapper wrapper = new AsyncTargetWrapper();

            wrapper.WrappedTarget = target;
            wrapper.QueueLimit = 5000;
            wrapper.OverflowAction = AsyncTargetWrapperOverflowAction.Block; 

            logOn = GetLogLevel();

            switch (logOn)
            {
                case 1:
                    NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(wrapper, LogLevel.Info);
                    break;
                default:
                    NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(wrapper, LogLevel.Off);
                    break;
            }
        }

        private int GetLogLevel()
        {
            string paramValue = "";

            if (Registry.GetValue(Registry.REG_BASE, "LOGON", ref paramValue))
            {
                logOn = Convert.ToInt32(paramValue);
            }
            else logOn = 1;
            return logOn;
        }
    }
}