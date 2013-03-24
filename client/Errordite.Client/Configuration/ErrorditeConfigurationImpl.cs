using System.Collections.Generic;
using Errordite.Client.Interfaces;

namespace Errordite.Client.Configuration
{
    internal class ErrorditeConfigurationImpl : IErrorditeConfiguration
    {
        public ErrorditeConfigurationImpl(ErrorditeConfigurationSection section)
        {
            Token = section.Token;
            Enabled = section.Enabled;
            GZip = section.GZip;
            Endpoint = string.IsNullOrEmpty(section.Endpoint) ? "https://www.errordite.com/receiveerror" : section.Endpoint;
            
            SanitisedParams = new List<SanitiseParamConfig>();
            foreach (SanitiseParamElement sanitiseParamElem in section.SanitisedParams)
            {
                SanitisedParams.Add(new SanitiseParamConfig(sanitiseParamElem.Name, sanitiseParamElem.Match));
            }
            
            DataCollectors = new List<IDataCollectorFactory>();
            foreach (DataCollectorElement dataCollectorElement in section.DataCollectors)
            {
                DataCollectors.Add(dataCollectorElement);
            }

            UnwrapExceptions = new List<string>();
            foreach (UnwrapExceptionElement unwrapExceptionElement in section.UnwrapExceptions)
            {
                UnwrapExceptions.Add(unwrapExceptionElement.TypeName);
            }

            IgnoreExceptions = new List<string>();
            foreach (IgnoreExceptionElement ignoreExceptionElement in section.IgnoreExceptions)
            {
                IgnoreExceptions.Add(ignoreExceptionElement.TypeName);
            }
        }

        public string Token { get; set; }
        public bool Enabled { get; set; }
        public bool GZip { get; set; }
        public string Endpoint { get; set; }
        public IList<SanitiseParamConfig> SanitisedParams { get; set; }
        public IList<IDataCollectorFactory> DataCollectors { get; set; }
        public IList<string> UnwrapExceptions { get; set; }
        public IList<string> IgnoreExceptions { get; set; }
    }
}