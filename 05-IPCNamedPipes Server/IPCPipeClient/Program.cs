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
        static NamedPipeClientStream client = new NamedPipeClientStream("2A314-B07", "testpipe", PipeDirection.InOut, PipeOptions.Asynchronous);

        static void Main(string[] args)
        {
            bool nameApproved = false;
            string name = "";
            string sendto = "";
            char[] seper = { ':' };
            string test = "1:two:-1:message:astest";
            string[] testArray = test.Split(seper, 4, StringSplitOptions.RemoveEmptyEntries);
            foreach (string item in testArray)
            {
                Console.WriteLine(item);
            }
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
                readServerMessage();
            }
        }

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


        static void write(string name, string sendto)
        {
            StreamWriter output = new StreamWriter(client);

            output.AutoFlush = true;

            output.WriteLine("-1:" + name + ": connected:");
            client.WaitForPipeDrain();

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
                    if (!Console.KeyAvailable)
                    {
                        message = Console.ReadLine();
                    }
                    else
                    {
                        readServerMessage();
                    }
                    formattedMessage = ("1:" + name + ":" + sendto + ":" + message + ":");
                    output.WriteLine(formattedMessage);
                    client.WaitForPipeDrain();
                }
                catch (Exception e)
                {
                    throw e;
                }



            } while (true);

        }
    }
}
