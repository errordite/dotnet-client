
namespace Errordite.Client.Log4net
{
    public class ErrorditeLogger
    {
        public static bool Enabled { get; set; }

        public static void Initialise(bool captureRootLoggerOutput, params string[] loggersToCaptureEventsFrom)
        {
            Enabled = true;
            ErrorditeLog4NetLogger.Initialise(captureRootLoggerOutput, loggersToCaptureEventsFrom);
        }
    }
}
