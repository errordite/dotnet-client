using System;
using System.Collections.Generic;
#if (!NET2)
using System.Linq;
#endif
using System.Reflection;
using System.Threading;
using System.Web;
using Errordite.Client.Configuration;
using Errordite.Client.DataCollectors;
using Errordite.Client.Interfaces;
using Errordite.Client.Web;

namespace Errordite.Client
{
    /// <summary>
    /// The gateway for all Errordite operations.
    /// </summary>
    public static class ErrorditeClient
    {
        public const string ErrorditeLogMessagesHttpContextKey = "errordite-logmessages-httpcontext-key";
        private static Action<Exception> _exceptionNotificationAction = e => System.Diagnostics.Trace.Write(e.ToString());
        private static readonly Type BaseExceptionType = typeof(Exception);
        private static Action<IErrorditeConfiguration> _configurationAugmenter = c => { };
        private static bool _configurationAugmented;
        private static IErrorditeConfiguration _configuration;

        /// <summary>
        /// If you want to augment Errordite configuration using code rather than just rely
        /// on the XML configuration, set this action to adjust the configuration however you like.
        /// </summary>
        public static Action<IErrorditeConfiguration> ConfigurationAugmenter
        {
            set
            {
                _configurationAugmented = false;
                _configurationAugmenter = value;
            }
        }

        private static IErrorditeConfiguration Configuration
        {
            get
            {
                if (!_configurationAugmented)
                {
                    var configurationFromXml = ErrorditeConfigurationSection.Current;
                    var configuration = new ErrorditeConfigurationImpl(configurationFromXml);

                    _configurationAugmenter(configuration);
                    _configuration = configuration;
                    _configurationAugmented = true;
                }

                return _configuration;
            }
        }

        public static void SetErrorNotificationAction(Action<Exception> errorNotificationAction)
        {
            _exceptionNotificationAction = errorNotificationAction;
        }
 
        public static void ReportException(Exception exception)
        {
            ReportException(exception, true);
        }

        /// <summary>
        /// Call this method to send an exception to Errordite explicitly, rather than relying 
        /// on the HttpModule or similar to do it for you.
        /// </summary>
        /// <param name="exception">The exception to send.</param>
        /// <param name="async">If true then the sending will be queued as a Thread Pool work item.  If false the exception will be sent
        /// before flow returns to your code.</param>
        public static void ReportException(Exception exception, bool async)
        {
            if (exception == null)
                return;

            try
            {
                if (ShouldReportException(exception))
                {
                    exception = UnwrapException(exception);

                    var clientError = GetClientError(exception);

                    PostToErrordite(clientError, async);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    if (_exceptionNotificationAction != null)
                        _exceptionNotificationAction(ex);
                }
                catch
                {}
            }
        }

        private static void PostToErrordite(ClientError error, bool async)
        {
            if (async)
                ThreadPool.QueueUserWorkItem(cb => DoPostToErrordite(error, Configuration));
            else
                DoPostToErrordite(error, Configuration);
        }

        private static void DoPostToErrordite(ClientError error, IErrorditeConfiguration configuration)
        {
            try
            {
                ErrorditeWebRequest.To(configuration.Endpoint)
                    .WithError(error)
                    .TimeoutIn(60)
                    .Send(configuration.GZip);
            }
            catch (Exception ex)
            {
                try
                {
                    if (_exceptionNotificationAction != null)
                        _exceptionNotificationAction(ex);
                }
                catch
                { }
            }
        }

        public static ClientError GetClientError(Exception exception)
        {
            var clientError = new ClientError
            {
                TimestampUtc = DateTime.UtcNow,
                MachineName = Environment.MachineName,
                Token = Configuration.Token,
                Url = HttpContext.Current == null ? null : HttpContext.Current.Request.Url.AbsoluteUri,
                UserAgent = HttpContext.Current == null ? null : HttpContext.Current.Request.UserAgent,
                ExceptionInfo = GetExceptionInfo(exception, Configuration, true),
                Messages = HttpContext.Current == null ? null : HttpContext.Current.Items[ErrorditeLogMessagesHttpContextKey] as List<LogMessage>
            };

            return clientError;
        }

        private static ExceptionInfo GetExceptionInfo(Exception exception, IErrorditeConfiguration configuration, bool rootException)
        {
            var exceptionInfo = new ExceptionInfo
            {
                StackTrace = exception.StackTrace,
                ExceptionType = exception.GetType().FullName,
                Message = exception.Message,
                Source = exception.Source,
                Data = rootException 
                    ? GetCustomData(exception, configuration) 
                    : GetExceptionData(exception, configuration),
            };

            MethodBase method = exception.TargetSite;
            if (method != null)
            {
                exceptionInfo.MethodName = string.Format("{0}.{1}", method.DeclaringType == null ? "Unknown" : method.DeclaringType.FullName, method.Name);
            }

            if (exception.InnerException != null)
            {
                exceptionInfo.InnerExceptionInfo = GetExceptionInfo(exception.InnerException, configuration, false);
            }

            return exceptionInfo;
        }

        private static ErrorData GetExceptionData(Exception exception, IErrorditeConfiguration configuration)
        {
            var exceptionData = new ExceptionDataCollector().Collect(exception, configuration);
            return exceptionData ?? new ErrorData();
        }

        private static ErrorData GetCustomData(Exception exception, IErrorditeConfiguration configuration)
        {
            var exceptionData = GetExceptionData(exception, configuration);

            var items = exceptionData ?? new ErrorData();

            var scopedItems = new ScopedDataCollector().Collect(exception, configuration);

            if (scopedItems != null)
            {
                foreach (var scopedItem in scopedItems)
                {
                    items.Add(scopedItem.Key, scopedItem.Value);
                }
            }

            foreach (var dataCollectorFactory in configuration.DataCollectors)
            {
                var item = CollectExceptionData(exception, configuration, dataCollectorFactory);
                    
                if (item != null)
                    items.Add(item);
            }

            return items;
        }

        private static ErrorData CollectExceptionData(Exception exception, IErrorditeConfiguration configuration, IDataCollectorFactory dataCollectorFactory)
        {
            try
            {
                var dataCollector = dataCollectorFactory.Create();
                if (dataCollector == null)
                {
                    return null;
                }

                var exceptionData = dataCollector.Collect(exception, configuration);

                if (exceptionData != null)
                {
                    var ret = new ErrorData();
                    foreach (var kvp in exceptionData)
                    {
                        ret.Add(string.Format("{0}.{1}", dataCollectorFactory.Prefix, kvp.Key), kvp.Value);
                    }

                    return ret;
                }
            }
            catch (Exception e)
            {
                return new ErrorData{{"Error", e.Message}};
            }

            return null;
        }

        private static Exception UnwrapException(Exception exception)
        {
            foreach (var exceptionType in Configuration.UnwrapExceptions)
            {
                var fullName = exception.GetType().FullName;

                if (fullName.ToLowerInvariant() == exceptionType.ToLowerInvariant())
                {
                    return exception.InnerException ?? exception;
                }
            }

            return exception;
        }

        private static bool ShouldReportException(Exception exception)
        {
            if (!Configuration.Enabled)
                return false;

            var exceptionTypeHierarchy = new List<string>();
            var exceptionType = exception.GetType();

            while (exceptionType != null && exceptionType.IsAssignableFrom(BaseExceptionType))
            {
                exceptionTypeHierarchy.Add(exceptionType.FullName);
                exceptionType = exceptionType.BaseType;
            }

            foreach (var ignoreException in Configuration.IgnoreExceptions)
            {
                if (exceptionTypeHierarchy.Contains(ignoreException))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
