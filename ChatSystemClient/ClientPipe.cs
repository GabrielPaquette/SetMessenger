using System;
using System.Collections.Generic;
using System.IO.Pipes;
using BWCS;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Messaging;

namespace ChatSystemClient
{
    class ClientPipe
    {        
        public static bool connected = false;
        private const int timeoutTime = 5000;
        public static string ServerName { get; set; }
        private static NamedPipeClientStream clientStream { get; set; }

        public static int connectToServer()
        {
            int retCode = 0;
            clientStream = new NamedPipeClientStream(ServerName, SETMessengerUtilities.pipeName, PipeDirection.Out);
            try
            {
                clientStream.Connect(timeoutTime);
            }
            catch (TimeoutException)
            {
                retCode = 1;

            }
            catch (IOException)
            {
                retCode = 2;
            }
            return retCode;
        }

        public static void sendMessage(string message)
        {
            try
            {
                if (!clientStream.IsConnected)
                {
                    connectToServer();
                }

                clientStream.Write(Encoding.ASCII.GetBytes(message),0,message.Length);
                clientStream.WaitForPipeDrain();
            }
            catch (Exception)
            {
                connected = false;
            }
        }

        public static void disconnect()
        {
            clientStream.Dispose();
        }
    }
}