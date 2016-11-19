using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatSystemService
{
    public partial class ChatSystemService : ServiceBase
    {
        ChatServer chat;
        Thread program;

        public ChatSystemService()
        {
            InitializeComponent();
            CanPauseAndContinue = true;
            chat = new ChatServer();
            program = new Thread(chat.startServer);
        }

        protected override void OnStart(string[] args)
        {
            Logger.Log("Initailizing Chat Server Service");
            
            
            program.Start(); 
        }

        protected override void OnStop()
        {
            
            Logger.Log("Stopped Chat Server Service");
        }

        protected override void OnContinue()
        {
            Logger.Log("Continuing Chat Server Service");
        }

        protected override void OnPause()
        {
            Logger.Log("Chat Server Service Paused");
        }
    }
}
