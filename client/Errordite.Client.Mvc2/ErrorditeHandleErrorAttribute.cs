
using System.Web.Mvc;

namespace Errordite.Client.Mvc2
{
    public class ErrorditeHandleErrorAttribute : IExceptionFilter
    {
        public void OnException(ExceptionContext filterContext)
        {
            ErrorditeClient.ReportException(filterContext.Exception);
        }
    }
}
