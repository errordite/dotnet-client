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
            {
                //make sure we dont get duplicate keys
                if (data.ContainsKey(key))
                {
                    int index = 1;
                    while (data.ContainsKey(key))
                    {
                        key = string.Format("{0}-{1}", key, index);
                        index++;
                    }
                }   
                
                data.Add(key, value);
            }   
        }
    }
}
