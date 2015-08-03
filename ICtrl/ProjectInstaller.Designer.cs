namespace ICtrl
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ICtrlServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.ICtrlInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // ICtrlServiceProcessInstaller
            // 
            this.ICtrlServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.ICtrlServiceProcessInstaller.Password = null;
            this.ICtrlServiceProcessInstaller.Username = null;
            // 
            // ICtrlInstaller
            // 
            this.ICtrlInstaller.DisplayName = "LANIT-ICTRL";
            this.ICtrlInstaller.ServiceName = "LANIT-ICTRL";
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.ICtrlServiceProcessInstaller,
            this.ICtrlInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller ICtrlServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller ICtrlInstaller;
    }
}