/*
Project: ChatSystemService - Program.cs
Developer(s): Gabriel Paquette, Nathaniel Bray
Date: November 19, 2016
Description: This file contains the code that starts the service.
*/
using System.ServiceProcess;

namespace ChatSystemService
{
    static class Program
    {
        /*
        Name: Main
        Description: The function starts the service in a new thread
        */
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new ChatSystemService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
