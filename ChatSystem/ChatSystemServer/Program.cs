using BWCS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Messaging;
using System.Text;
using System.Threading;

namespace ChatSystemServer
{
    class Program
    {
        static Thread ServerThread;
        static bool closeServerFlag = false;
        static EventWaitHandle terminateHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
        static Dictionary<string, string> userList = new Dictionary<string, string>();

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
                catch (IOException e)
                { Debug.WriteLine("Pipe connection error: "+e.Message); }
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
                    // Machine:status:from:to:message
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
                NamedPipeServerStream pipeStream = new NamedPipeServerStream(PipeClass.pipeName, PipeDirection.In, 254);
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

            if (userList.TryGetValue(to, out machineName) == true)
            {
                //contruct the whisper to send
                string messageToSend = PipeClass.makeMessage(false, StatusCode.Whisper, from, message);
                //send the message
                sendMsg(messageToSend, machineName);
            }
        }


        static void processClientDisconnect(string who)
        {
            if (userList.ContainsKey(who))
            {
                string disconnectMessage = PipeClass.makeMessage(false, StatusCode.ClientDisconnected, who);
                //delete user when disconnect
                userList.Remove(who);
                sendBroadcastMessage(disconnectMessage);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="machineName"></param>
        static void processClientConnect(string name, string machineName)
        {
            //adds this user to the user list
            userList.Add(name, machineName);

            string connectMessage = PipeClass.makeMessage(false, StatusCode.ClientConnected, name);
            sendBroadcastMessage(connectMessage);

            sendUserlist(machineName);
        }


        /// <summary>
        /// 
        /// </summary>
        static void processServerClose()
        {
            string serverClosingMessage = PipeClass.makeMessage(false,StatusCode.ServerClosing, "Closing server");
            sendBroadcastMessage(serverClosingMessage);
            closeServerFlag = true;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageToBroadcast"></param>
        static void sendBroadcastMessage(string messageToBroadcast)
        {
            foreach (var item in userList)
            {
                sendMsg(messageToBroadcast, item.Value);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="machineName"></param>
        static void sendUserlist(string machineName)
        {
            string userListMessage = ":" + (int)StatusCode.SendUserList + ":";

            foreach (string name in userList.Keys)
            {
                userListMessage += name + ":";

            }
            //send updatedUserList to the message queue using machineName
            sendMsg(userListMessage, machineName);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="machineName"></param>
        private static void sendMsg(string message, string machineName)
        {
            MessageQueue mq = new MessageQueue("FormatName:DIRECT=OS:" + machineName + "\\Private$\\SETQueue");
            mq.Send(message);
        }
    }
    //http://stackoverflow.com/questions/4570653/multithreaded-namepipeserver-in-c-sharp
}

