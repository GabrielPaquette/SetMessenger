using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BWCS
{
    public enum StatusCode
    {
        ClientConnected,
        ClientDisconnected,
        Whisper,        
        ServerClosing,
        SendUserList
    }
    public abstract class PipeClass
    {
        public const string pipeName = "BWCSSetPipe";


        public static string makeMessage(bool client,StatusCode status, params string[] msgParam)
        {
            int state = (int)status;
            string msg = "";
            if (client)
            {
                msg += Environment.MachineName;
            }
            msg += ":"+state.ToString();
            foreach (string param in msgParam)
            {
                msg += ":" + param;
            }
            return msg;
        }
    }
}
