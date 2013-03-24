using System.Collections.Generic;
using Errordite.Client.Interfaces;

namespace Errordite.Client.Configuration
{
    public interface IErrorditeConfiguration
    {
        string Token { get; set; }
        bool Enabled { get; set; }
        bool GZip { get; set; }
        string Endpoint { get; set; }

        IList<SanitiseParamConfig> SanitisedParams { get; }
        IList<IDataCollectorFactory> DataCollectors { get; }
        IList<string> UnwrapExceptions { get; }
        IList<string> IgnoreExceptions { get; }
    }
}