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
            
        }

        protected override void OnStart(string[] args)
        {
            Logger.Log("Initailizing Chat Server Service");        
            program.Start();
        }

        protected override void OnStop()
        {
            chat.processServerClose();
            Logger.Log("Stopped Chat Server Service");
        }
    }
}
