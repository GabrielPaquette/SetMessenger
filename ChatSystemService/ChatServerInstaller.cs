using System.ComponentModel;
using System.Configuration.Install;

namespace ChatSystemService
{
    [RunInstaller(true)]
    public partial class ChatServerInstaller : System.Configuration.Install.Installer
    {
        public ChatServerInstaller()
        {
            InitializeComponent();
        }

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
