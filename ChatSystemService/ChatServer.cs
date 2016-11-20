using System;
using System.Collections.Generic;
using System.Messaging;
using System.Text;
using BWCS;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO.Pipes;
using System.IO;

namespace ChatSystemService
{
    class ChatServer
    {
        //private Thread ServerThread;
        private bool closeServerFlag = false;
        //static EventWaitHandle terminateHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
        private Dictionary<string, string> userList = new Dictionary<string, string>();



        #region unmanaged
        // Declare the SetConsoleCtrlHandler function
        // as external and receiving a delegate.

        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);

        // A delegate type to be used as the handler routine
        // for SetConsoleCtrlHandler.
        private delegate bool HandlerRoutine(CtrlTypes CtrlType);

        // An enumerated type for the control messages
        // sent to the handler routine.
        private enum CtrlTypes
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT,
            CTRL_CLOSE_EVENT,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT
        }

        #endregion

        public void startServer()
        {
            SetConsoleCtrlHandler(new HandlerRoutine(ConsoleCtrlCheck), true);
            do
            {
                ProcessNextClient();
                Thread.Sleep(1000);
            } while (!closeServerFlag);
            //ServerThread = new Thread(serverThread);
            //ServerThread.Start();
        }



        private bool ConsoleCtrlCheck(CtrlTypes ctrlType)
        {
            // Put your own handler here
            switch (ctrlType)
            {
                case CtrlTypes.CTRL_C_EVENT:
                    processServerClose();
                    break;

                case CtrlTypes.CTRL_BREAK_EVENT:
                    processServerClose();
                    break;

                case CtrlTypes.CTRL_CLOSE_EVENT:
                    processServerClose();
                    break;
                case CtrlTypes.CTRL_SHUTDOWN_EVENT:
                    processServerClose();
                    break;

            }
            return true;
        }

        private void serverThread()
        {
            do
            {
                ProcessNextClient();
                Thread.Sleep(1000);
            } while (!closeServerFlag);

            //terminateHandle.Set();
        }

        private void ProcessClientThread(object pStream)
        {
            NamedPipeServerStream pipeStream = (NamedPipeServerStream)pStream;


            bool closeClientThreadFlag = false;

            while (closeServerFlag == false && closeClientThreadFlag == false)
            {
                try
                {
                    var recievedByteMessage = new byte[1024];
                    string message = "";
                    //if the pipe is not connected, then wait for a connection
                    //if (pipeStream.IsConnected == false)
                    //{
                    //    pipeStream.WaitForConnection();
                    //}

                    //read the message sent through the pipe
                    pipeStream.Read(recievedByteMessage, 0, 1024);
                    //convert the message into a string and cut out the \0s at the end of the string
                    message = Encoding.ASCII.GetString(recievedByteMessage).TrimEnd('\0');

                    //message = message.Substring(0, message.IndexOf('\0'));
                    //determine what to do with the message recieved and does the action needed
                    processMessageRecieved(message, out closeClientThreadFlag);

                }
                catch (IOException e)
                {
                    Logger.Log("Server-ProcessClientThread Filerror: " + e.Message);
                }
                catch (Exception e)
                {
                    closeClientThreadFlag = true;
                    Logger.Log("Server - ProcessClientThread Error" +e.Message);
                }
            }

            pipeStream.Close();
            pipeStream.Dispose();
        }

        private void processMessageRecieved(string message, out bool ct)
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
                case StatusCode.All:
                    processBroadcast(messageInfo[2], messageInfo[4]);
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


        private void ProcessNextClient()
        {
            try
            {
                NamedPipeServerStream pipeStream = new NamedPipeServerStream(SETMessengerUtilities.pipeName, PipeDirection.In, 254);
                pipeStream.WaitForConnection();

                //Spawn a new thread for each request and continues waiting
                Thread t = new Thread(ProcessClientThread);
                t.Start(pipeStream);
            }
            catch (Exception e)
            {
                //If there are no more avail connections (254 is in use already) then just keep looping until one is avail
                Logger.Log("Server-ProccesNextClient Error: " + e.Message);
            }
        }


        private void processWhisper(string from, string to, string message)
        {
            string machineName = "";

            if (userList.TryGetValue(to, out machineName) == true)
            {
                //contruct the whisper to send
                string messageToSend = SETMessengerUtilities.makeMessage(false, StatusCode.Whisper, from, message);
                //send the message
                sendMsg(messageToSend, machineName);
            }
        }

        private void processBroadcast(string from, string message)
        {
            string messageToSend = SETMessengerUtilities.makeMessage(false, StatusCode.All, from, message);
            sendBroadcastMessage(messageToSend);
        }


        private void processClientDisconnect(string who)
        {
            if (userList.ContainsKey(who))
            {
                string disconnectMessage = SETMessengerUtilities.makeMessage(false, StatusCode.ClientDisconnected, who);
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
        private void processClientConnect(string name, string machineName)
        {
            //adds this user to the user list
            userList.Add(name, machineName);

            string connectMessage = SETMessengerUtilities.makeMessage(false, StatusCode.ClientConnected, name);
            sendBroadcastMessage(connectMessage);

            sendUserlist(machineName);
        }


        /// <summary>
        /// 
        /// </summary>
        public void processServerClose()
        {
            string serverClosingMessage = SETMessengerUtilities.makeMessage(false, StatusCode.ServerClosing, "Closing server");
            sendBroadcastMessage(serverClosingMessage);
            closeServerFlag = true;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageToBroadcast"></param>
        private void sendBroadcastMessage(string messageToBroadcast)
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
        private void sendUserlist(string machineName)
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
        private void sendMsg(string message, string machineName)
        {
            MessageQueue mq = new MessageQueue("FormatName:DIRECT=OS:" + machineName + "\\Private$\\SETQueue");
            mq.Send(message);            
        }
    }
    //http://stackoverflow.com/questions/4570653/multithreaded-namepipeserver-in-c-sharp
}
