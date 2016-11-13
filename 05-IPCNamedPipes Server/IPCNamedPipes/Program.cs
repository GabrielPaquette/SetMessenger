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
        static bool running;
        static Thread runningThread;
        static string pipeName = "testpipe";
        static EventWaitHandle terminateHandle = new EventWaitHandle(false, EventResetMode.AutoReset);

        static Dictionary<string, StreamWriter> users = new Dictionary<string, StreamWriter>();
        static void Main(string[] args)
        {
            running = true;
            runningThread = new Thread(ServerLoop);
            runningThread.Start();
        }
       
        public string PipeName { get; set; }

        static void ServerLoop()
        {
            while (running)
            {
                ProcessNextClient();
            }

            terminateHandle.Set();
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
            while (!done)
            {
                try
                {
                    if(pipeStream.IsConnected == false)
                    {
                        pipeStream.WaitForConnection();
                    }
                    
                    String inp = input.ReadLine();

                    messageTo = processMessage(inp, output, out inp);
                    Console.WriteLine(inp);

                    messageTo.WriteLine(inp);
                    messageTo.Flush();
                    pipeStream.WaitForPipeDrain();
                }
                catch(Exception e)
                {
                   
                    Console.WriteLine(e.Message);
                    
                }
                finally
                {
                    pipeStream.WaitForPipeDrain();
                }



                //if (inp == "Shutdown")
                //{
                    //foreach (object thread in list)
                    //{
                    //    Console.WriteLine(list.Count);
                    //}
                //} 
            }

            pipeStream.Close();
            pipeStream.Dispose();
        }

        static StreamWriter processMessage(String inp, StreamWriter output, out string input)
        {
            string[] messageInfo = inp.Split(':');
            StreamWriter messageTo = output;
            string temp = "";
            switch (messageInfo[0])
            {
                case "-1":
                    //adds this user to the user list
                    users.Add(messageInfo[1], output);
                    temp = messageInfo[1] + " has started a conversation with you";
                    break;
                case "1":
                    if (users.TryGetValue(messageInfo[2], out messageTo) == true)
                    {
                        temp = messageInfo[1] + ": " + messageInfo[3];
                    }
                    break;
                case "2":
                    break;

            }
            input = temp;
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
