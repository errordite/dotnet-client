using System;
using log4net.Core;

namespace Errordite.Client.Log4net
{
    public class AppendLoggingEventEventArgs : EventArgs
    {
        public LoggingEvent LoggingEvent;

        public AppendLoggingEventEventArgs(LoggingEvent loggingEvent)
        {
            LoggingEvent = loggingEvent;
        }
    }
}
