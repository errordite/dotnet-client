using System;
using System.Web;
using Errordite.Client.Configuration;
using Errordite.Client.Interfaces;

namespace Errordite.Client.DataCollectors
{
    public class HttpContextDataCollector : DataCollectorBase, IDataCollector
    {
        public ErrorData Collect(Exception e, IErrorditeConfiguration configuration)
        {
            var context = HttpContext.Current;

            if (context == null)
                return null;

            var data = new ErrorData();

            AddIfNotEmpty("HttpMethod", context.Request.HttpMethod, data);

            if(context.Request.UrlReferrer != null)
                AddIfNotEmpty("Referrer", context.Request.UrlReferrer.AbsoluteUri, data);

            if (context.Request.Form.Count > 0)
            {
                foreach(var key in context.Request.Form.Keys)
                {
                    AddIfNotEmpty(string.Format("Form.{0}", key), 
                        Sanitise(configuration, key.ToString(), context.Request.Form[key.ToString()]), 
                        data);
                }
            }

            if (context.Request.Headers.Count > 0)
            {
                foreach (string key in context.Request.Headers.Keys)
                {
                    switch (key)
                    {
                        case "User-Agent":
                            continue;
                    }

                    AddIfNotEmpty(string.Format("Header.{0}", key),
                        Sanitise(configuration, key, context.Request.Headers[key]),
                        data);
                }
            }

            return data;
        }

        public ErrorData Collect(Exception e)
        {
            return null;
        }
    }
}
