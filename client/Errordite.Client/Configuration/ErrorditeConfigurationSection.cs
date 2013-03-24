
using System;
using System.Configuration;
using Errordite.Client.Interfaces;

namespace Errordite.Client.Configuration
{
    internal class ErrorditeConfigurationSection : ConfigurationSection
    {
        public static ErrorditeConfigurationSection Current
        {
            get { return (ErrorditeConfigurationSection) ConfigurationManager.GetSection("errordite"); }
        }

        [ConfigurationProperty("token", DefaultValue = "", IsRequired = true)]
        public string Token
        {
            get
            {
                return (string)this["token"];
            }
            set
            {
                this["token"] = value;
            }
        }

        [ConfigurationProperty("enabled", DefaultValue = true, IsRequired = true)]
        public bool Enabled
        {
            get
            {
                return (bool)this["enabled"];
            }
            set
            {
                this["enabled"] = value;
            }
        }

        [ConfigurationProperty("gzip", DefaultValue = false, IsRequired = false)]
        public bool GZip
        {
            get
            {
                return (bool)this["gzip"];
            }
            set
            {
                this["gzip"] = value;
            }
        }

        [ConfigurationProperty("endpoint", DefaultValue = "https://www.errordite.com/receiveerror", IsRequired = false)]
        public string Endpoint
        {
            get
            {
                return (string)this["endpoint"];
            }
            set
            {
                this["endpoint"] = value;
            }
        }
        
        [ConfigurationProperty("sanitiseParams", IsDefaultCollection = false, IsKey = false, IsRequired = false)]
        public SanitiseParamElementCollection SanitisedParams
        {
            get
            {
                return base["sanitiseParams"] as SanitiseParamElementCollection;
            }
        }
            
        [ConfigurationProperty("dataCollectors", IsDefaultCollection = false, IsKey = false, IsRequired = false)]
        public DataCollectorElementCollection DataCollectors
        {
            get
            {
                return base["dataCollectors"] as DataCollectorElementCollection;
            }
        }
            
        [ConfigurationProperty("unwrapExceptions", IsDefaultCollection = false, IsKey = false, IsRequired = false)]
        public UnwrapExceptionElementCollection UnwrapExceptions
        {
            get
            {
                return base["unwrapExceptions"] as UnwrapExceptionElementCollection;
            }
        }
        
        [ConfigurationProperty("ignoreExceptions", IsDefaultCollection = false, IsKey = false, IsRequired = false)]
        public IgnoreExceptionElementCollection IgnoreExceptions
        {
            get
            {
                return base["ignoreExceptions"] as IgnoreExceptionElementCollection;
            }
        }
    }

    #region Data Collectors

    public class DataCollectorElementCollection : ConfigurationElementCollection
    {
        public DataCollectorElement this[int index]
        {
            get
            {
                return BaseGet(index) as DataCollectorElement;
            }
        }

        protected override string ElementName
        {
            get
            {
                return "dataCollector";
            }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new DataCollectorElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((DataCollectorElement)element).Prefix;
        }

        protected override bool IsElementName(string elementName)
        {
            return !string.IsNullOrEmpty(elementName) && elementName == "dataCollector";
        }

        
    }

    public class DataCollectorElement : ConfigurationElement, IDataCollectorFactory
    {
        [ConfigurationProperty("prefix", DefaultValue = "", IsRequired = true)]
        public string Prefix
        {
            get
            {
                return (string)this["prefix"];
            }
            set
            {
                this["prefix"] = value;
            }
        }

        [ConfigurationProperty("type", DefaultValue = "", IsRequired = true)]
        public string Type
        {
            get
            {
                return (string)this["type"];
            }
            set
            {
                this["type"] = value;
            }
        }

        public IDataCollector Create()
        {
            var type = System.Type.GetType(Type);
            if (type == null)
                return null;
            return Activator.CreateInstance(type) as IDataCollector;
        }
    }

    #endregion

    #region Sanitise Params

    public enum SanitiseMatchType
    {
        Exact,
        Contains
    }

    public class SanitiseParamElementCollection : ConfigurationElementCollection
    {
        public SanitiseParamElement this[int index]
        {
            get
            {
                return BaseGet(index) as SanitiseParamElement;
            }
        }

        protected override string ElementName
        {
            get
            {
                return "param";
            }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new SanitiseParamElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((SanitiseParamElement)element).Name;
        }

        protected override bool IsElementName(string elementName)
        {
            return !string.IsNullOrEmpty(elementName) && elementName == "param";
        }
    }

    public class SanitiseParamConfig
    {
        public SanitiseParamConfig(string name, SanitiseMatchType match)
        {
            Name = name;
            Match = match;
        }

        public string Name { get; private set; }
        public SanitiseMatchType Match { get; private set; }
    }

    public class SanitiseParamElement : ConfigurationElement
    {
        [ConfigurationProperty("name", DefaultValue = "", IsRequired = true)]
        public string Name
        {
            get
            {
                return (string)this["name"];
            }
            set
            {
                this["name"] = value;
            }
        }

        [ConfigurationProperty("match", DefaultValue = SanitiseMatchType.Exact, IsRequired = true)]
        public SanitiseMatchType Match
        {
            get
            {
                return (SanitiseMatchType)this["match"];
            }
            set
            {
                this["match"] = value;
            }
        }
    }

#endregion

    #region Unwrap Exceptions



    public class UnwrapExceptionElementCollection : ConfigurationElementCollection
    {
        public UnwrapExceptionElement this[int index]
        {
            get
            {
                return BaseGet(index) as UnwrapExceptionElement;
            }
        }

        protected override string ElementName
        {
            get
            {
                return "exception";
            }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new UnwrapExceptionElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((UnwrapExceptionElement)element).TypeName;
        }

        protected override bool IsElementName(string elementName)
        {
            return !string.IsNullOrEmpty(elementName) && elementName == "exception";
        }
    }

    public interface IUnwrapExceptionConfig
    {
        string TypeName { get; }
    }

    public class UnwrapExceptionElement : ConfigurationElement, IUnwrapExceptionConfig
    {
        [ConfigurationProperty("typeName", DefaultValue = "", IsRequired = true)]
        public string TypeName
        {
            get
            {
                return (string)this["typeName"];
            }
            set
            {
                this["typeName"] = value;
            }
        }
    }

    #endregion

    #region Ignore Exceptions

    public class IgnoreExceptionElementCollection : ConfigurationElementCollection
    {
        public IgnoreExceptionElement this[int index]
        {
            get
            {
                return BaseGet(index) as IgnoreExceptionElement;
            }
        }

        protected override string ElementName
        {
            get
            {
                return "exception";
            }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new IgnoreExceptionElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((IgnoreExceptionElement)element).TypeName;
        }

        protected override bool IsElementName(string elementName)
        {
            return !string.IsNullOrEmpty(elementName) && elementName == "exception";
        }
    }

    public interface IIgnoreExceptionConfig
    {
        string TypeName { get; set; }
    }

    public class IgnoreExceptionElement : ConfigurationElement, IIgnoreExceptionConfig
    {
        [ConfigurationProperty("typeName", DefaultValue = "", IsRequired = true)]
        public string TypeName
        {
            get
            {
                return (string)this["typeName"];
            }
            set
            {
                this["typeName"] = value;
            }
        }
    }

    #endregion
}
