using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Pipes;
using System.Threading;

namespace IPCPipeClient
{
    class Program
    {
<<<<<<< HEAD
        static NamedPipeClientStream client = new NamedPipeClientStream(".", "testpipe", PipeDirection.InOut, PipeOptions.Asynchronous);
=======
        static NamedPipeClientStream client = new NamedPipeClientStream(".", "BWCSSetPipe", PipeDirection.InOut, PipeOptions.Asynchronous);
>>>>>>> 390d24249304b20204769c78760abe9017cd7f06

        static void Main(string[] args)
        {
            bool nameApproved = false;
            string name = "";
            string sendto = "";
<<<<<<< HEAD

=======
            //char[] seper = { ':' };
            //string test = "1:two:-1:message:astest";
            //string[] testArray = test.Split(seper, 4, StringSplitOptions.RemoveEmptyEntries);
            //foreach (string item in testArray)
            //{
            //    Console.WriteLine(item);
            //}
>>>>>>> 390d24249304b20204769c78760abe9017cd7f06
            while (!nameApproved)
            {
                Console.WriteLine("Enter your Username");
                name = Console.ReadLine();
                nameApproved = true;
            }

            Console.WriteLine("start");

            client.Connect();


            Console.WriteLine("sendto:");
            sendto = Console.ReadLine();

            Thread readThread = new Thread(read);
            readThread.Start();

            write(name, sendto);

            Console.WriteLine("Program ended...press any key.");
            Console.ReadKey();
        }

        static void read(object data)
        {
            while (true)
            {
                StreamReader reader = new StreamReader(client);
                string read = "";
                if ((read = reader.ReadLine()) != null)
                {
                    Console.Write("\n" + read + "\n");
                    //client.WaitForPipeDrain();
                }
            }
        }

<<<<<<< HEAD
        static void readServerMessage()
        {
            StreamReader reader = new StreamReader(client);
    
            string read = "";
            if ((read = reader.ReadLine()) != null)
            {
                Console.Write("\n" + read + "\n");
                client.WaitForPipeDrain();
            }
        }
=======
        //static void readServerMessage()
        //{
           
        //}
>>>>>>> 390d24249304b20204769c78760abe9017cd7f06


        static void write(string name, string sendto)
        {
            StreamWriter output = new StreamWriter(client);

            output.AutoFlush = true;

<<<<<<< HEAD
            output.WriteLine("1:" + name + ":");
            client.WaitForPipeDrain();
=======
            output.WriteLine("1:" + name + ": connected:");
            //client.WaitForPipeDrain();
>>>>>>> 390d24249304b20204769c78760abe9017cd7f06

            String message = "";
            String formattedMessage = "";

            do
            {
                try
                {
                    if (client.IsConnected == false)
                    {
                        client.Connect();
                    }

                    Console.Write("{0}: ", name);
                        message = Console.ReadLine();
<<<<<<< HEAD
                    }
                    else
                    {
                        readServerMessage();
                    }
                    formattedMessage = ("2:" + name + ":" + sendto + ":" + message);
=======
                    formattedMessage = ("2:" + name + ":" + sendto + ":" + message + ":");
>>>>>>> 390d24249304b20204769c78760abe9017cd7f06
                    output.WriteLine(formattedMessage);
                    //client.WaitForPipeDrain();
                }
                catch (Exception e)
                {
                    throw e;
                }



            } while (true);

        }
    }
}
