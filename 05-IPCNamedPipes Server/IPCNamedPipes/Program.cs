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
    class Program
    {
        static Thread runningThread;
        static string pipeName = "testpipe";
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
            //TODO FOR YOU: Write code for handling pipe client here
            while (!exitFlag && !done)
            {
                try
                {
                    if(pipeStream.IsConnected == false)
                    {
                        pipeStream.WaitForConnection();
                    }
                    
                    String inp = input.ReadLine();
                    
                    messageTo = processMessage(inp, output, out inp, out done);
                    if (exitFlag == true)
                    {
                        break;
                    }

                    Console.WriteLine(inp);

                    messageTo.WriteLine(inp);
                    messageTo.Flush();
                    pipeStream.WaitForPipeDrain();
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
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
                    if (exitFlag == false)
                    {
                        exitFlag = true;
                    }

                    temp = messageInfo[1] + " has connected to the server";
                    break;
                case "2":
                    //send message from messageInfo[1] to messageInfo[2]
                    if (userList.TryGetValue(messageInfo[2], out messageTo) == true)
                    {
                        temp = messageInfo[1] + ": " + messageInfo[3];
                    }
                    break;
                case "9":
                    //delete user when disconnect
                    if (userList.ContainsKey(messageInfo[1]))
                    {
                        Console.WriteLine("Deleteing " + messageInfo[1]);
                        userList.Remove(messageInfo[1]);
                    }
                    finish = true;
                    break;
                case "-1":
                    //close server
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
    }

    //http://stackoverflow.com/questions/4570653/multithreaded-namepipeserver-in-c-sharp
}
