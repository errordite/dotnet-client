using Errordite.Client.Configuration;

namespace Errordite.Client.DataCollectors
{
    using System;

    public class ScopedDataCollector 
    {
        public ErrorData Collect(Exception e, IErrorditeConfiguration configuration)
        {
            if (ErrorditeScope.Data == null)
                return null;

            var ret = new ErrorData();

            foreach (var dataItem in ErrorditeScope.Data)
            {
                ret.Add(dataItem.Key, dataItem.Value);
            }
            return ret;
        }
    }
}