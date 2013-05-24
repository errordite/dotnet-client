using System;
using System.Collections.Generic;
using System.Web;
using log4net;
using log4net.Core;
using log4net.Repository.Hierarchy;
using System.Linq;

namespace Errordite.Client.Log4net
{
    public class ErrorditeLog4NetLogger
    {
        private static readonly IList<ErrorditeAppender> ProfilingAppenders = new List<ErrorditeAppender>();
        private bool _running;
        private List<LogMessage> _messages; 

        public static void Initialise(bool captureRootLoggerOutput, params string[] loggersToCaptureEventsFrom)
        {
            var repository = LogManager.GetRepository() as Hierarchy;

            if (repository == null)
                return;

            if (captureRootLoggerOutput)
            {
                if (repository.Root != null)
                {
                    var profilingAppender = new ErrorditeAppender
                    {
                        Threshold = Level.Debug,
                        Name = string.Format("{0}-Errordite", repository.Root.Name)
                    };

                    repository.Root.AddAppender(profilingAppender);
                    ProfilingAppenders.Add(profilingAppender);
                }
            }

            if (loggersToCaptureEventsFrom.Length > 0)
            {
                var attachLoggers = loggersToCaptureEventsFrom.Select(l => l.ToLowerInvariant()).ToList();

                foreach (var log in LogManager.GetCurrentLoggers())
                {
                    var logger = log.Logger as Logger;

                    if (logger != null && attachLoggers.Contains(logger.Name.ToLowerInvariant()))
                    {
                        var profilingAppender = new ErrorditeAppender
                        {
                            Threshold = Level.Debug,
                            Name = string.Format("{0}-Errordite", logger.Name)
                        };

                        logger.AddAppender(profilingAppender);
                        ProfilingAppenders.Add(profilingAppender);
                    }
                }
            }

            repository.Configured = true;
            repository.RaiseConfigurationChanged(EventArgs.Empty);
        }

        public void Start()
        {
            if (!_running)
            {
                _messages = new List<LogMessage>();

                foreach (var appender in ProfilingAppenders)
                    appender.AppendLoggingEvent += ProfilingAppenderAppendLoggingEvent;

                _running = true;

                if(HttpContext.Current != null)
                {
                    HttpContext.Current.Items.Add(ErrorditeClient.ErrorditeLogMessagesHttpContextKey, _messages);
                }
            }
        }

        public void Stop()
        {
            if (_running)   
            {
                foreach (var appender in ProfilingAppenders)
                    appender.AppendLoggingEvent -= ProfilingAppenderAppendLoggingEvent;

                _running = false;
            }
        }

        private void ProfilingAppenderAppendLoggingEvent(object sender, AppendLoggingEventEventArgs e)
        {
            _messages.Add(new LogMessage
            {
                Message = string.Format("{0}: {1}", e.LoggingEvent.LoggerName, e.LoggingEvent.RenderedMessage),
                TimestampUtc = DateTime.UtcNow
            });
        }
    }
}
