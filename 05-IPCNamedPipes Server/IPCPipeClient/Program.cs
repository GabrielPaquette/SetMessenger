using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Pipes;

namespace IPCPipeClient
{
    class Program
    {
        static void Main(string[] args)
        {
            bool done = false;
            NamedPipeClientStream client = new NamedPipeClientStream("testpipe");
            client.Connect();
            StreamWriter output = new StreamWriter(client);
            while (!done)
            {
                Console.Write("Enter data, or blank line to end: ");
                String outp = Console.ReadLine();
                output.WriteLine(outp);
                output.Flush();
                if (outp == "Shutdown")
                {
                    done = true;
                }
            }
            Console.WriteLine("Program ended...press any key.");
            Console.ReadKey();
        }
    }
}
