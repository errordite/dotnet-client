
using System.Collections.Generic;
using Errordite.Client.Configuration;

namespace Errordite.Client.DataCollectors
{
    public abstract class DataCollectorBase
    {
        protected string Sanitise(IErrorditeConfiguration configuration, string key, string value)
        {
            foreach (var paramToSanitise in configuration.SanitisedParams)
            {
                if (paramToSanitise.Match == SanitiseMatchType.Exact && paramToSanitise.Name.ToLowerInvariant() == key.ToLowerInvariant())
                    return "***SECRET***";
                if (paramToSanitise.Match == SanitiseMatchType.Contains && key.Contains(paramToSanitise.Name))
                    return "***SECRET***";
            }

            return value;
        }

        protected void AddIfNotEmpty(string key, string value, ErrorData data)
        {
            if (!string.IsNullOrEmpty(value))
                data.Add(key, value);
        }
    }
}
