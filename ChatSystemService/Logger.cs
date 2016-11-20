using System.Diagnostics;

namespace ChatSystemService
{
    public static class Logger
    {
        private const string eventSourceName = "ChatSource";
        private const string eventLogName = "SETMessengerLogs";
        private static EventLog serviceEventLog = new EventLog();

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

        public static void removeLog()
        {
            if (EventLog.SourceExists(eventSourceName))
            {
                EventLog.Delete(eventLogName);
            }
        }
    }
}
