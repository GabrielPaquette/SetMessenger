namespace ChatSystemService
{
    partial class ChatServerInstaller
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
            this.ChatServerServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.ChatServerServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // ChatServerServiceProcessInstaller
            // 
            this.ChatServerServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.ChatServerServiceProcessInstaller.Password = null;
            this.ChatServerServiceProcessInstaller.Username = null;
            // 
            // ChatServerServiceInstaller
            // 
            this.ChatServerServiceInstaller.Description = "SET Messenger Server";
            this.ChatServerServiceInstaller.ServiceName = "ChatServerService";
            this.ChatServerServiceInstaller.BeforeUninstall += new System.Configuration.Install.InstallEventHandler(this.ChatServerServiceInstaller_BeforeUninstall);
            // 
            // ChatServerInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.ChatServerServiceInstaller,
            this.ChatServerServiceProcessInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller ChatServerServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller ChatServerServiceInstaller;
    }
}