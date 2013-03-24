using System;
using Errordite.Client.Configuration;

namespace Errordite.Client.Interfaces
{
    public interface IDataCollector
    {
        ErrorData Collect(Exception e, IErrorditeConfiguration configuration);
    }
}