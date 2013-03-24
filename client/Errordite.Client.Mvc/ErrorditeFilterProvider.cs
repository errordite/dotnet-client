using System.Collections.Generic;
using System.Web.Mvc;

namespace Errordite.Client.Mvc
{
    /// <summary>
    /// Custom FilterProvider that simply returns an instance of the ErrorditeExceptionFilter.  
    /// Exception Filters run in reverse order to that specified.  So specifying First actually means 
    /// it will run Last!  Sadly, it will always run before the OnException method on the base controller
    /// so if it is this that is handling the Exception (as in setting ErrorHandled = true), 
    /// the ErrorditeExceptionFilter will not do anything.
    /// </summary>
    public class ErrorditeFilterProvider : IFilterProvider
    {
        public IEnumerable<Filter> GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
        {
            yield return new Filter(new ErrorditeExceptionFilter(), FilterScope.First, int.MinValue);
        }
    }
}