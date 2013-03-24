

namespace Errordite.Client.Log4net
{
    public class ErrorditeLogger
    {
        public static bool Enabled { get; set; }

        public static void Initialise(params string[] loggers)
        {
            Enabled = true;

            if (loggers.Length > 0)
                ErrorditeLog4NetLogger.Initialise(loggers);
        }
    }
}
