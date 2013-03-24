using System;
using System.Web;

namespace Errordite.Client.Mvc
{
    internal static class ErrorditeAspHelper
    {
        public static bool LogException(Exception ex)
        {
            //if the exception is something other than a 500, we won't log it as it is implicitly 
            //handled by dint of it resulting in a "known" error code
            return ex != null 
                   && new HttpException(null, ex).GetHttpCode() == 500;
        }
    }
}