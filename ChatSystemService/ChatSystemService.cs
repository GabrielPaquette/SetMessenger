using System.ServiceProcess;
using System.Threading;

namespace ChatSystemService
{
    public partial class ChatSystemService : ServiceBase
    {
        ChatServer chat;
        Thread program;

        public ChatSystemService()
        {
            InitializeComponent();
            chat = new ChatServer();
            program = new Thread(chat.startServer);
            program.Start();
        }

        protected override void OnStart(string[] args)
        {
            Logger.Log("Initailizing Chat Server Service");        
             
        }

        protected override void OnStop()
        {
            
            Logger.Log("Stopped Chat Server Service");
        }
    }
}
