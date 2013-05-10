
namespace Errordite.Client.DataCollectors
{
    using System;

    internal class ScopedDataCollector 
    {
        public ErrorData Collect(Exception e)
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