/* Filename     :SETMessengerUtilities.cs
 * Project      : ChatSystem/WinProgA0(4,5)
 * Author(s)    : Nathan Bray, Gabe Paquette
 * Date Created : 2016-11-12
 * Description  : This file contains information that is shared between applications, 
 *          such as the status codes
 */
using System;

namespace BWCS
{
    /// <summary>
    /// this enum represents the various types of messages that can be sent to and from clients/server
    /// </summary>
    public enum StatusCode
    {
        ClientConnected,
        ClientDisconnected,
        Whisper,        
        ServerClosing,
        SendUserList,
        All
    }

    /// <summary>
    /// This class is for the shared methods between client and server systems
    /// </summary>
    public abstract class SETMessengerUtilities
    {
        public const string pipeName = "BWCSSetPipe";


        /// <summary>
        /// this method takes all the parameters and makes a string message that both a client and the server can understand and parse through
        /// </summary>
        /// <param name="client"> a bool to determine if the machine name needs to be sent in the message</param>
        /// <param name="status"> the statuscode of the message</param>
        /// <param name="msgParam">an array of strings containing more message onformation such as who it is from,
        /// who it goes to, and what the actual message is</param>
        /// <returns>the message in the form of colon-separated values</returns>
        public static string makeMessage(bool client,StatusCode status, params string[] msgParam)
        {
            int state = (int)status;
            string msg = "";
            if (client)
            {
                msg += Environment.MachineName;
            }
            msg += ":" + state.ToString();
            foreach (string param in msgParam)
            {
                msg += ":" + param;
            }
            return msg;
        }
    }
}
