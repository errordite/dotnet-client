using System.Web;

namespace Errordite.Client.Log4net
{
    public class ErrorditeLoggingModule : IHttpModule
    {
        private const string ErrorditeLoggerHttpContextKey = "errordite-logger-httpcontext-key";

        public void Init(HttpApplication application)
        {
            application.BeginRequest += (sender, args) =>
            {
                if (ErrorditeLogger.Enabled)
                {
                    var logger = new ErrorditeLog4NetLogger();
                    logger.Start();
                    application.Context.Items.Add(ErrorditeLoggerHttpContextKey, logger);
                }
            };

            application.EndRequest += (sender, args) => Stop(sender as HttpApplication);
            application.Error += (sender, args) => Stop(sender as HttpApplication);
        }

        private void Stop(HttpApplication application)
        {
            if (application != null && ErrorditeLogger.Enabled)
            {
                var logger = application.Context.Items[ErrorditeLoggerHttpContextKey] as ErrorditeLog4NetLogger;

                if (logger != null)
                {
                    logger.Stop();
                }
            }
        }

        public void Dispose()
        {}
    }
}
