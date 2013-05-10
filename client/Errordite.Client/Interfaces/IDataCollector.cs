using System;

namespace Errordite.Client.Interfaces
{
    public interface IDataCollector
    {
        ErrorData Collect(Exception e);
    }
}