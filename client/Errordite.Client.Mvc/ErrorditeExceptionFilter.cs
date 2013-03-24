using System.Web.Mvc;

namespace Errordite.Client.Mvc
{
    /// <summary>
    /// Filter that sends exceptions to Errordite if they have been handled or if Custom Errors are enabled. 
    /// If they are unhandled and no custom errors, they will be caught by the OnError event on the HttpApplication.
    /// 
    /// In order to ensure we know whether the exception has been handled, we use the FilterScope.Last enumeration when
    /// registering this Filter.
    /// </summary>
    public class ErrorditeExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext filterContext)
        {
            if (!ErrorditeAspHelper.LogException(filterContext.Exception))
                return;

            //the idea here is that if the exception has been marked as handled or if the custom errors are enabled, then
            //the OnError handler in the module will not be triggered and hence we want to do our logging here instead
            //Of course it's possible that setting ExceptionHandled = true is supposed to imply it's not an error we care
            //about.... needs further thought at some point.
            if (filterContext.ExceptionHandled  || 
                filterContext.HttpContext.IsCustomErrorEnabled)
            {
                ErrorditeClient.ReportException(filterContext.Exception);
            }
        }
    }
}

