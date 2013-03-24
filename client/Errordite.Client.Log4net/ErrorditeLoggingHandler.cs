using System.Web;

namespace Errordite.Client.Log4net
{
    public class ErrorditeLoggingHandler : IHttpHandler
    {
        public const string QueryStringKey = "enable";

        public void ProcessRequest(HttpContext context)
        {
            bool enable;
            if(bool.TryParse(context.Request.QueryString[QueryStringKey], out enable))
            {
                ErrorditeLogger.Enabled = enable;
            }
        }

        public bool IsReusable
        {
            get { return true; }
        }
    }
}
