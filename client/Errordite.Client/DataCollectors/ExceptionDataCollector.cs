using System;
using System.Collections.Generic;
using Errordite.Client.Configuration;

namespace Errordite.Client.DataCollectors
{
    /// <summary>
    /// This data collector does not implement IDataCollector as it should not be specified in errordite configuration, its
    /// used by default to pull andy items from the Data property of an exception.
    /// </summary>
    public class ExceptionDataCollector : DataCollectorBase
    {
        public ErrorData Collect(Exception exception, IErrorditeConfiguration configuration)
        {
            if (exception == null)
                return new ErrorData();

            var data = new ErrorData();

            foreach (var key in exception.Data.Keys)
            {
                AddIfNotEmpty(
                    string.Format("Exception.{0}", key),
                    Sanitise(configuration, key.ToString(), exception.Data[key].ToString()),
                    data);
            }

            return data;
        }
    }
}
