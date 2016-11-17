using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Messaging;
using System.Collections;
using System.Diagnostics;


namespace IPCNamedPipes
{

    public enum StatusCode
    {
        ClientConnected,
        ClientDisconnected,
        Whisper,
        ServerClosing,
        SendUserList
    }

    class Program
    {
        static Thread ServerThread;
        static bool closeServerFlag = false;
        static string pipeName = "BWCSSetPipe";
        static EventWaitHandle terminateHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
        static Dictionary<string, string> userList = new Dictionary<string, string>();

        public string PipeName { get; set; }

        static void Main(string[] args)
        {
            ServerThread = new Thread(serverThread);
            ServerThread.Start();
        }
        
        static void serverThread()
        {
            do
            {
                ProcessNextClient();
            } while (!closeServerFlag);
            
            terminateHandle.Set();

            Console.ReadKey();
        }
        
        public static void ProcessClientThread(object pStream)
        {
            NamedPipeServerStream pipeStream = (NamedPipeServerStream)pStream;
            
            var recievedByteMessage = new byte[1024];
            bool closeClientThreadFlag = false;
            
            while (closeServerFlag == false && closeClientThreadFlag == false)
            {
                try
                {
                    //if the pipe is not connected, then wait for a connection
                    if (pipeStream.IsConnected == false)
                    {
                        pipeStream.WaitForConnection();
                    }
                    
                    //read the message sent through the pipe
                    pipeStream.Read(recievedByteMessage, 0, 1024);
                    //convert the message into a string and cut out the \0s at the end of the string
                    String message = Encoding.ASCII.GetString(recievedByteMessage).TrimEnd('\0');

                    //message = message.Substring(0, message.IndexOf('\0'));
                    //determine what to do with the message recieved and does the action needed
                    processMessageRecieved(message, out closeClientThreadFlag);

                }
                catch (Exception e)
                {
                    closeClientThreadFlag = true;
                    Debug.WriteLine(e.Message);
                }
            }
            
            pipeStream.Close();
            pipeStream.Dispose();
        }

        static void processMessageRecieved(string message, out bool ct)
        {
            bool closeThread = false;
            char[] delim = { ':' };
            string[] messageInfo = message.Split(delim, 5, StringSplitOptions.RemoveEmptyEntries);
            StatusCode sc = (StatusCode)int.Parse(messageInfo[1]);

            switch (sc)
            {
                case StatusCode.ClientConnected:
                    processClientConnect(messageInfo[2], messageInfo[0]);
                    break;
                case StatusCode.Whisper:
                    //send message from messageInfo[2] to messageInfo[3]
                    processWhisper(messageInfo[2], messageInfo[3], messageInfo[4]);
                    break;
                case StatusCode.ClientDisconnected:
                    processClientDisconnect(messageInfo[2]);
                    closeThread = true;
                    break;
                case StatusCode.ServerClosing:
                    //close server
                    processServerClose();
                    break;
            }

            ct = closeThread;
        }


        public static void ProcessNextClient()
        {
            try
            {
                NamedPipeServerStream pipeStream = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 254);
                pipeStream.WaitForConnection();

                //Spawn a new thread for each request and continues waiting
                Thread t = new Thread(ProcessClientThread);
                t.Start(pipeStream);
            }
            catch (Exception e)
            {//If there are no more avail connections (254 is in use already) then just keep looping until one is avail
                throw e;
            }
        }


        static void processWhisper(string from, string to, string message)
        {
            string machineName = "";
            string messageToSend = "";

            if (userList.TryGetValue(to, out machineName) == true)
            {
                //contruct the whisper to send
                messageToSend = (int)StatusCode.Whisper + ":" + from + ": " + message;
                //send the message
                sendWhisper(messageToSend, machineName);
            }
        }


        static void processClientDisconnect(string who)
        {
            string disconnectMessage = (int)StatusCode.ClientDisconnected + ":" + who;
            if (userList.ContainsKey(who))
            {
                //delete user when disconnect
                userList.Remove(who);
                sendBroadcastMessage(disconnectMessage);
            }
        }


        static void processClientConnect(string name, string machineName)
        {
            //adds this user to the user list
            userList.Add(name, machineName);

            string connectMessage = (int)StatusCode.ClientConnected + ":" + name + ":";
            sendBroadcastMessage(connectMessage);

            sendUserlist(name);
        }

        static void processServerClose()
        {
            string serverClosingMessage = "Server is now closed";
            sendBroadcastMessage(serverClosingMessage);
            closeServerFlag = true;
        }

        static void sendWhisper(string whisper, string machineName)
        {
            MessageQueue mq = new MessageQueue("FormatName:DIRECT=OS:" + machineName + "\\Private$\\SETQueue");
            mq.Send(whisper);
        }


        static void sendBroadcastMessage (string messageToBroadcast)
        {
            foreach (var item in userList)
            {
                MessageQueue mq = new MessageQueue("FormatName:DIRECT=OS:" + item.Value + "\\Private$\\SETQueue");
                mq.Send(messageToBroadcast);
            }
        }

        static void sendUserlist(string machineName)
        {
            string nameToAdd = "";
            string updatedUserList = (int)StatusCode.SendUserList + ":";

            foreach(string name in userList.Keys)
            {
                nameToAdd = name + ":";
                updatedUserList += nameToAdd;
            }

            MessageQueue mq = new MessageQueue("FormatName:DIRECT=OS:" + machineName + "\\Private$\\SETQueue");
            mq.Send(updatedUserList);
            //send updatedUserList to the message queue using machineName
        }
    }
    //http://stackoverflow.com/questions/4570653/multithreaded-namepipeserver-in-c-sharp
}
