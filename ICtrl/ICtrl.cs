using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace ICtrl
{
    public partial class ICtrl : ServiceBase
    {
        private ICtrlDLL.ICtrl iCtrl;

        public ICtrl()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            iCtrl = new ICtrlDLL.ICtrl();
            iCtrl.Run();
        }

        protected override void OnStop()
        {
            if (iCtrl != null)
            {
       //         iCtrl.settings.SaveToLocalDB();

                ICtrlDLL.DBConnection.Instance.Disconnect();
                ICtrlDLL.ICtrl.ipConnection.Disconnect();
                //iCtrl.ipConnection.Disconnect();
            }
        }
    }
}
