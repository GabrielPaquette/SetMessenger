using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;

namespace ChatSystemService
{
    [RunInstaller(true)]
    public partial class ChatServerInstaller : System.Configuration.Install.Installer
    {
        public ChatServerInstaller()
        {
            InitializeComponent();
        }
    }
}
