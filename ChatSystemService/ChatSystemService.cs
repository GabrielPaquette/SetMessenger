/*
Project: ChatSystemService - ChatSystemService.cs
Developer(s): Gabriel Paquette, Nathaniel Bray
Date: November 19, 2016
Description: This file contains the code that runs when the service starts and stops.
             It also starts the server in a thread and a new ChatSystemService object
             is instantiated.
*/

using System.ServiceProcess;
using System.Threading;

namespace ChatSystemService
{
    public partial class ChatSystemService : ServiceBase
    {
        ChatServer chat;
        Thread program;


        /*
        Name:ChatSystemService
        Description: This is the contructor for the ChatSystemService class.
                     It starts the server in a new thread.
        */
        public ChatSystemService()
        {
            InitializeComponent();
            chat = new ChatServer();
            program = new Thread(chat.startServer);
            program.Start();
        }


        /*
        Name: OnStart
        Description: This function runs when the server service starts
        */
        protected override void OnStart(string[] args)
        {
            Logger.Log("Initailizing Chat Server Service");        
             
        }


        /*
        Name: OnStop
        Description: This function runs when the server service stops
        */
        protected override void OnStop()
        {
            
            Logger.Log("Stopped Chat Server Service");
        }
    }
}
