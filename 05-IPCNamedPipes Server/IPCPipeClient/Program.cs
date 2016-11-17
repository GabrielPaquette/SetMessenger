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

        static NamedPipeClientStream client = new NamedPipeClientStream(".", "BWCSSetPipe", PipeDirection.InOut, PipeOptions.Asynchronous);


        static void Main(string[] args)
        {
            bool nameApproved = false;
            string name = "";
            string sendto = "";

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

                byte[] readMessage = new byte[1024];
                client.Read(readMessage, 0, 1024);

                string msg = Encoding.ASCII.GetString(readMessage);

                msg = msg.Substring(0, msg.IndexOf('\0'));
                if(msg.Length > 0)
                {
                    Console.Write(msg);
                }
                

            }
        }
        


        static void write(string name, string sendto)
        {
            string connectMessage = "0:" + name + ":";
            var byteMessage = Encoding.ASCII.GetBytes(connectMessage);
            client.Write(byteMessage, 0, byteMessage.Length);

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

                    formattedMessage = ("2:" + name + ":" + sendto + ":" + message + ":");
                    byteMessage = Encoding.ASCII.GetBytes(formattedMessage);
                    client.Write(byteMessage, 0, byteMessage.Length);
                }
                catch (Exception e)
                {
                    throw e;
                }
                
            } while (true);

        }
    }
}
