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
        static Thread runningThread;
        static string pipeName = "BWCSSetPipe";
        static EventWaitHandle terminateHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
        static bool exitFlag = false;
        static Dictionary<string, string> userList = new Dictionary<string, string>();
        static void Main(string[] args)
        {
            runningThread = new Thread(ServerLoop);
            runningThread.Start();

        }

        public string PipeName { get; set; }

        static void ServerLoop()
        {
            do
            {
                Console.WriteLine("starting to process a client");
                ProcessNextClient();

            } while (!exitFlag);

            Console.WriteLine("the user list is empty -- exiting");
            terminateHandle.Set();

            Console.ReadKey();


        }

        public static void ProcessClientThread(object pStream)
        {
            NamedPipeServerStream pipeStream = (NamedPipeServerStream)pStream;
            
            var byteMessage = new byte[1024];
            bool done = false;
            string machineName;
            while (exitFlag == false && done == false)
            {
                machineName = "";
                try
                {
                    if (pipeStream.IsConnected == false)
                    {
                        pipeStream.WaitForConnection();
                    }
                    
                    pipeStream.Read(byteMessage, 0, 1024);
                    String message = Encoding.ASCII.GetString(byteMessage);

                    message = message.Substring(0, message.IndexOf('\0'));

                    machineName = processMessage(message, pipeStream, out message, out done);

                    if (exitFlag == true)
                    {
                        break;
                    }
                    else
                    {
                        if (message != "")
                        {
                            Console.WriteLine(message);
                            MessageQueue mq = new MessageQueue("FormatName:DIRECT=OS:" + machineName + "\\Private$\\SETQueue");
                            mq.Send(message);
                        }
                    }
                }
                catch (Exception e)
                {
                    done = true;
                    Console.WriteLine(e.Message);
                }
            }

            Console.WriteLine("closing");
            pipeStream.Close();
            Console.WriteLine("disposing");
            pipeStream.Dispose();
        }

        static string processMessage(string message, NamedPipeServerStream pipeStream, out string input, out bool done)
        {
            char[] delim = { ':' };
            string[] messageInfo = message.Split(delim, 5, StringSplitOptions.RemoveEmptyEntries);

            //machineName:code:name
            //machinename:code:from:to

            string tempMessage = "";
            bool finish = false;

            string machineName = "";

            StatusCode sc = (StatusCode)int.Parse(messageInfo[0]);

            switch (sc)
            {
                case StatusCode.ClientConnected:
                

                    //adds this user to the user list
                    userList.Add(messageInfo[2], messageInfo[0]);
                    Console.WriteLine(messageInfo[2] + " has connected to the server");
                    sendConnectMessage(messageInfo[2] + ":");

                    sendUserlist(messageInfo[2]);
                    break;
                case StatusCode.Whisper:
                    //send message from messageInfo[2] to messageInfo[3]
                
                    if (userList.TryGetValue(messageInfo[3], out machineName) == true)
                    {
                        tempMessage = (int)StatusCode.Whisper + ":" + messageInfo[2] + ": " + messageInfo[4];
                    }

                    break;
                case StatusCode.ClientDisconnected:
                    //delete user when disconnect
                    if (userList.ContainsKey(messageInfo[1]))
                    {
                        Console.WriteLine(messageInfo[2] + " disconected from the server");
                        //gabe:gabe disconected from the server
                        sendDisconectMessage(messageInfo[2] + ":");
                        userList.Remove(messageInfo[2]);
                    }
                    finish = true;
                    break;
                case StatusCode.ServerClosing:
                    //close server
                    sendServerCloseMessage();
                    exitFlag = true;

                    break;
            }

            input = tempMessage;
            done = finish;
            return machineName;
        }


        public static void ProcessNextClient()
        {
            try
            {
                NamedPipeServerStream pipeStream = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 254);
                pipeStream.WaitForConnection();

                //Spawn a new thread for each request and continue waiting
                Thread t = new Thread(ProcessClientThread);
                Console.WriteLine("starting new thread for client");
                t.Start(pipeStream);
            }
            catch (Exception e)
            {//If there are no more avail connections (254 is in use already) then just keep looping until one is avail
                throw e;
            }
        }


        static void sendDisconectMessage(string disconnectMessage)
        {
            foreach (var item in userList)
            {
                MessageQueue mq = new MessageQueue("FormatName:DIRECT=OS:" + item.Value + "\\Private$\\SETQueue");
                mq.Send(disconnectMessage);
            }
        }

        static void sendConnectMessage(string connectMessage)
        {
            foreach (var item in userList)
            {
                string message = (int)StatusCode.ClientConnected + ":" + connectMessage;
                //send this message to the machinename(item.value) through a message queue
                MessageQueue mq = new MessageQueue("FormatName:DIRECT=OS:" + item.Value + "\\Private$\\SETQueue");
                mq.Send(message);
            }
        }

        static void sendServerCloseMessage()
        {
            string closingMessage = "Server is now closed";

            foreach (var item in userList)
            {
                MessageQueue mq = new MessageQueue("FormatName:DIRECT=OS:" + item.Value + "\\Private$\\SETQueue");
                mq.Send(closingMessage);
                //send closing message to message queue using item.value(machineName)
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
