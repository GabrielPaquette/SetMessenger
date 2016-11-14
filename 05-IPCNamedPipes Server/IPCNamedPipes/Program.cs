using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Pipes;
using System.Threading;
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
        static Dictionary<string, StreamWriter> userList = new Dictionary<string, StreamWriter>();
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

            StreamReader input = new StreamReader(pipeStream);
            StreamWriter output = new StreamWriter(pipeStream);
            StreamWriter messageTo = null;
            output.AutoFlush = true;
            bool done = false;


            while (exitFlag == false && done == false)
            {
                try
                {
                    if (pipeStream.IsConnected == false)
                    {
                        pipeStream.WaitForConnection();
                    }

                    String inp = input.ReadLine();

                    messageTo = processMessage(inp, output, out inp, out done);
                    if (exitFlag == true)
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine(inp);

<<<<<<< HEAD
                        messageTo.WriteLine(inp);
                        messageTo.Flush();
                    }
=======
                    Console.WriteLine(inp);

                    messageTo.WriteLine(inp);
                    messageTo.Flush();
                    //pipeStream.WaitForPipeDrain();
>>>>>>> 390d24249304b20204769c78760abe9017cd7f06
                }
                catch (Exception e)
                {
                    done = true;
                    Console.WriteLine(e.Message);
                    done = true;
                }
            }

            Console.WriteLine("closing");
            pipeStream.Close();
            Console.WriteLine("disposing");
            pipeStream.Dispose();
        }

        static StreamWriter processMessage(String inp, StreamWriter output, out string input, out bool done)
        {
            char[] delim = { ':' };
            string[] messageInfo = inp.Split(delim, 4, StringSplitOptions.RemoveEmptyEntries);
            StreamWriter messageTo = output;
            string temp = "";
            bool finish = false;


            switch (messageInfo[0])
            {
                case "1":
                    //adds this user to the user list
                    userList.Add(messageInfo[1], output);
<<<<<<< HEAD
                    Console.WriteLine(messageInfo[1] + " has connected to the server");
                    sendConnectMessage(messageInfo[1] + ":");
=======

>>>>>>> 390d24249304b20204769c78760abe9017cd7f06

                    sendUserlist(messageTo);
                    break;
                case "2":
                    //send message from messageInfo[1] to messageInfo[2]
                    if (userList.TryGetValue(messageInfo[2], out messageTo) == true)
                    {
                        temp = (int)StatusCode.Whisper + messageInfo[1] + ": " + messageInfo[3];
                    }
                    break;
                case "9":
                    //delete user when disconnect
                    if (userList.ContainsKey(messageInfo[1]))
                    {
                        Console.WriteLine(messageInfo[1] + " disconected from the server");
                        //gabe:gabe disconected from the server
                        sendDisconectMessage(messageInfo[1] + ":");
                        userList.Remove(messageInfo[1]);
                    }
                    finish = true;
                    break;
                case "-1":
                    //close server
                    sendServerCloseMessage();
                    exitFlag = true;
                    break;
            }
            input = temp;
            done = finish;
            return messageTo;
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
                StreamWriter writer = item.Value;
                writer.WriteLine((int)StatusCode.ClientDisconnected + ":" + disconnectMessage);
                writer.Flush();
            }
        }

        static void sendConnectMessage(string connectMessage)
        {
            foreach (var item in userList)
            {
                StreamWriter writer = item.Value;
                writer.WriteLine((int)StatusCode.ClientConnected + ":" + connectMessage);
                writer.Flush();
            }
        }

        static void sendServerCloseMessage()
        {
            foreach (var item in userList)
            {
                StreamWriter writer = item.Value;
                writer.WriteLine((int)StatusCode.ServerClosing );
                writer.Flush();
            }
        }

        static void sendUserlist(StreamWriter messageTo)
        {
            string nameToAdd = "";
            string updatedUserList = (int)StatusCode.SendUserList + ":";

            foreach(string name in userList.Keys)
            {
                nameToAdd = name + ":";
                updatedUserList += nameToAdd;
            }
            messageTo.WriteLine(updatedUserList);
            messageTo.Flush();
        }
    }
    //http://stackoverflow.com/questions/4570653/multithreaded-namepipeserver-in-c-sharp
}
