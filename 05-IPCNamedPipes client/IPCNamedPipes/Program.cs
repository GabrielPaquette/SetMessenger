using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Pipes;

namespace IPCNamedPipes
{
    class Program
    {
        static void Main(string[] args)
        {
            

            bool done = false;
            NamedPipeServerStream server = new NamedPipeServerStream("testpipe");
            server.WaitForConnection();
            StreamReader input = new StreamReader(server);
            while (!done)
            {
                String inp = input.ReadLine();
                Console.WriteLine(inp);
                if (inp == "Shutdown")
                {
                    done = true;
                }
            }
            Console.WriteLine("Program ended.");
        }
    }
}
