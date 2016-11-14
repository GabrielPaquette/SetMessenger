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
        StartupConnect,
        ClientConnected,
        ClientDisconnected,
        Whisper,        
        ServerClosing
    }
    public class PipeClass
    {
        public const string pipeName = "BWCSSetPipe";
        public static string ServerName { get; set; }        
        public static NamedPipeClientStream clientStream { get; set; }
    }
}
