using System.Web;
using System.Web.Mvc;

namespace Errordite.Client.Mvc
{
    /// <summary>
    /// Sends exceptions to Errordite.  They are logged from a Global Filter if they will not otherwise make it to
    /// the Error event on the HttpApplication.  
    /// </summary>
    public class ErrorditeModule : IHttpModule
    {
        //it might seem that this should be declared volatile.  In fact for .net 2.0 and later,
        //this is unnecessary so long as you have a lock around the writing of the value
        //http://blogs.msdn.com/b/ericlippert/archive/2011/06/16/atomicity-volatility-and-immutability-are-different-part-three.aspx
        private static bool _initialised = false;
        private static readonly object _lock = new object();

        #region IHttpModule Members

        public void Init(HttpApplication context)
        {
            //the event handler needs to be registered on each request
            context.Error += (sender, e) =>
            {
                var exception = HttpContext.Current.Server.GetLastError();
                if (ErrorditeAspHelper.LogException(exception))
                    ErrorditeClient.ReportException(exception);
            };

            //we only want this to run once, though this method will be called once for each worker thread 
            //http://www.dominicpettifer.co.uk/Blog/41/ihttpmodule-gotchas---the-init---method-can-get-called-multiple-times
            if (_initialised)
                return;

            lock (_lock)
            {
                if (_initialised)
                    return;

                _initialised = true;

                FilterProviders.Providers.Add(new ErrorditeFilterProvider());
            }
        }

        public void Dispose()
        {}

        #endregion

    }
}
