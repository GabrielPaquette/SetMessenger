using System.Diagnostics;

namespace ChatSystemService
{
    public static class Logger
    {
        private const string eventSourceName = "ChatSource";
        private const string eventLogName = "SETMessengerLogs";

        public static void Log(string message)
        {
            EventLog serviceEventLog = new EventLog();
            if (!EventLog.SourceExists(eventSourceName))
            {
                EventLog.CreateEventSource(eventSourceName, eventLogName);
            }
            serviceEventLog.Source = eventSourceName;
            serviceEventLog.Log = eventLogName;
            serviceEventLog.WriteEntry(message);
        }
    }
}
