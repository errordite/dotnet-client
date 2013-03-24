using System;
using System.Collections.Generic;

namespace Errordite.Client
{
    [Serializable]
    public class ClientError
    {
        public string Token { get; set; }
        public string MachineName { get; set; }
        public string Url { get; set; }
        public string UserAgent { get; set; }
        public string Version { get; set; }
        public List<LogMessage> Messages { get; set; }
        public DateTime TimestampUtc { get; set; }
        public ExceptionInfo ExceptionInfo { get; set; }
    }

    [Serializable]
    public class ExceptionInfo
    {
        public string Message { get; set; }
        public string Source { get; set; }
        public string ExceptionType { get; set; }
        public string StackTrace { get; set; }
        public string MethodName { get; set; }
        public ErrorData Data { get; set; }
        public ExceptionInfo InnerExceptionInfo { get; set; }
    }

    [Serializable]
    public class LogMessage
    {
        public DateTime TimestampUtc { get; set; }
        public string Message { get; set; }
    }

#if NET2
    
    /// <summary>
    /// .net v2 version of custom ErrorData; the standard XmlSerializer can't
    /// serialize dictionaries and the JSONSerializer doesn't exist so we resort 
    /// to a list of items (key/value pairs).
    /// </summary>
    [Serializable]
    public class ErrorData : List<ErrorDataItem>
    {
        public void Add(string key, string value)
        {
            Add(new ErrorDataItem(key, value));
        }

        public void Add(ErrorData errorData)
        {
            AddRange(errorData);
        }
    }
    
#else
    [Serializable]
    public class ErrorData : Dictionary<string, string>
    {
        public void Add(ErrorData errorData)
        {
            foreach (var item in errorData)
                Add(item.Key, item.Value);
        }
    }
#endif

    /// <summary>
    /// Only used for XML serialisation; just acting as a serialisable KeyValuePair&lt;string,string>.
    /// </summary>
    [Serializable]
    public class ErrorDataItem
    {
        public ErrorDataItem(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public ErrorDataItem()
        {}

        public string Key { get; set; }
        public string Value { get; set; }
    }
}
