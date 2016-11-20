/*
Project: ChatSystemService - ChatServerInstaller.cs
Developer(s): Gabriel Paquette, Nathaniel Bray
Date: November 19, 2016
Description: This file contains the code that is ran when the server 
             is install and uninstalled.
*/

using System.ComponentModel;
using System.Configuration.Install;

namespace ChatSystemService
{
    [RunInstaller(true)]
    public partial class ChatServerInstaller : System.Configuration.Install.Installer
    {
        /*
        Name:ChatServerInstaller
        Description: This function initializes the components for the server program
        */
        public ChatServerInstaller()
        {
            InitializeComponent();
        }


        /*
        Name:ChatServerServiceInstaller_BeforeUninstall
        Description: This function deletes the log file when the service is uninstalled
        */
        private void ChatServerServiceInstaller_BeforeUninstall(object sender, InstallEventArgs e)
        {
            Logger.removeLog();
        }

        private void ChatServerServiceInstaller_AfterInstall(object sender, InstallEventArgs e)
        {
            Logger.Log("SET Messenger Service Installed");
        }
    }
}
