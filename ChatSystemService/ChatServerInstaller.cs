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
    }
}
