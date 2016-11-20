/*
Project: ChatSystemService - Logger.cs
Developer(s): Gabriel Paquette, Nathaniel Bray
Date: November 19, 2016
Description: The file contains the code that handles logging messages to the event log
*/

using System.Diagnostics;

namespace ChatSystemService
{
    public static class Logger
    {
        private const string eventSourceName = "ChatSource";
        private const string eventLogName = "SETMessengerLogs";
        private static EventLog serviceEventLog = new EventLog();


        /*
        Name: Log
        Parameters: string message -> this is the message that will be writen to the log
        Description: The function takes a string and logs it into the serviceEvertLog
        */
        public static void Log(string message)
        {
            
            if (!EventLog.SourceExists(eventSourceName))
            {
                EventLog.CreateEventSource(eventSourceName, eventLogName);
            }
            serviceEventLog.Source = eventSourceName;
            serviceEventLog.Log = eventLogName;
            serviceEventLog.WriteEntry(message);
        }


        /* 
        Name: removeLog
        Description: This function deletes the EventLog
        */
        public static void removeLog()
        {
            if (EventLog.SourceExists(eventSourceName))
            {
                EventLog.DeleteEventSource(eventSourceName, eventLogName);
                EventLog.Delete(eventLogName);
            }
        }
    }
}
