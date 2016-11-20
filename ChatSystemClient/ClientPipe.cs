/* Filename     : ClientPipe.cs
 * Project      : ChatSystem/WinProgA0(4,5)
 * Author(s)    : Nathan Bray, Gabe Paquette
 * Date Created : 2016-11-17
 * Description  : This class handles methods pertaining to the Named pipe connection between the client and the server
 *  it contains connecting, disconnecting and sending methods for the application.
 *  Note: this was not made to be an API as of yet (i.e.) it does not handle its' own exceptions and must
 *   be wrapped in a try-catch block to be safely handled
 */
using System;
using System.IO.Pipes;
using BWCS;
using System.Text;

namespace ChatSystemClient
{
    class ClientPipe
    {        
        public static bool connected = false;

        // If the connection can't be made within this amount of time, the connection will timeout
        private const int timeoutTime = 5000;
        public static string ServerName { get; set; }
        private static NamedPipeClientStream clientStream { get; set; }


        /// <summary>
        /// this method will create an instance of the pipe strem connection to send messages to the server with
        /// and then will attemp to connect to it. 
        /// Note: if this fails, it is to be caught in the program calling this 
        /// </summary>
        public static void connectToServer()
        {
            clientStream = new NamedPipeClientStream(ServerName, SETMessengerUtilities.pipeName, PipeDirection.Out);
            // If the connection can't be made within this amount of time, the connection will timeout
            clientStream.Connect(timeoutTime);            
        }


        /// <summary>
        /// this method will write an already formed message to the pipe in bytes using
        /// the provided pipe.write functionality
        /// </summary>
        /// <param name="message"></param>
        public static void sendMessage(string message)
        {
            try
            {
                clientStream.Write(Encoding.ASCII.GetBytes(message),0,message.Length);
                clientStream.WaitForPipeDrain();
            }
            catch (Exception)
            {
                connected = false;
            }
        }


        /// <summary>
        /// this method clears all resources used for the named pipe connection
        /// </summary>
        public static void disconnect()
        {
            clientStream.Dispose();
        }
    }
}