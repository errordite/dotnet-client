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
		private static Action<Exception> _exceptionNotificationAction = e => System.Diagnostics.Trace.Write(e.ToString());
		private static Action<Exception> _preReportAction = null;
        private static readonly Type _baseExceptionType = typeof(Exception);
        private static Action<IErrorditeConfiguration> _configurationAugmenter = c => { };
        private static bool _configurationAugmented;
        private static IErrorditeConfiguration _configuration;

        public const string ErrorditeLogMessagesHttpContextKey = "errordite-logmessages-httpcontext-key";
        public static bool Debug { get; set; }

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

		public static void SetPreReportAction(Action<Exception> preReportAction)
		{
			_preReportAction = preReportAction;
		}
 
        public static void ReportException(Exception exception)
        {
            ReportException(exception, true, null);
        }

        public static void ReportException(Exception exception, bool async)
        {
            ReportException(exception, async, null);
        }

        /// <summary>
        /// Call this method to send an exception to Errordite explicitly, rather than relying 
        /// on the HttpModule or similar to do it for you.
        /// </summary>
        /// <param name="exception">The exception to send.</param>
        /// <param name="async">If true then the sending will be queued as a Thread Pool work item.  If false the exception will be sent
        /// before flow returns to your code.</param>
        /// <param name="customData">Any custom data you want sent woth your error</param>
        public static void ReportException(Exception exception, bool async, IDictionary<string, string> customData)
        {
            if (exception == null)
                return;

            try
            {
                Trace("New exception raised: type:={0}, async:={1}", exception.GetType().FullName, async);

                if (ShouldReportException(exception))
                {
					if (_preReportAction != null)
					{
						try
						{
							_preReportAction(exception);
						}
						catch(Exception e)
						{
							Trace(string.Format("Pre report action failed:={0}", e.Message));
						}
					}

                    Trace("...reporting exception to Errordite");

                    exception = UnwrapException(exception);

                    Trace("...Unwrapped exception");

                    var clientError = GetClientError(exception, customData);

                    Trace("...Got client error, posting to Errordite");

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
                ThreadPool.QueueUserWorkItem(cb => DoPostToErrordite(error));
            else
                DoPostToErrordite(error);
        }

        private static void DoPostToErrordite(ClientError error)
        {
            try
            {
                Trace("...Sending exception to:={0}", Configuration.Endpoint);

                ErrorditeWebRequest.To(Configuration.Endpoint)
                    .WithError(error)
                    .TimeoutIn(60)
                    .Send(Configuration.GZip);
            }
            catch (Exception ex)
            {
                Trace("...Failed to post to Errordite, ", error);

                try
                {
                    if (_exceptionNotificationAction != null)
                        _exceptionNotificationAction(ex);
                }
                catch
                { }
            }
        }

        public static ClientError GetClientError(Exception exception, IDictionary<string, string> customData)
        {
            var clientError = new ClientError
            {
                TimestampUtc = DateTime.UtcNow,
                MachineName = Environment.MachineName,
                Token = Configuration.Token,
                Url = HttpContext.Current == null ? null : HttpContext.Current.Request.Url.AbsoluteUri,
                UserAgent = HttpContext.Current == null ? null : HttpContext.Current.Request.UserAgent,
                ExceptionInfo = GetExceptionInfo(exception),
                Messages = HttpContext.Current == null ? null : HttpContext.Current.Items[ErrorditeLogMessagesHttpContextKey] as List<LogMessage>,
                ContextData = GetCustomData(exception, customData) 
            };

            return clientError;
        }

        private static ExceptionInfo GetExceptionInfo(Exception exception)
        {
            var exceptionInfo = new ExceptionInfo
            {
                StackTrace = exception.StackTrace,
                ExceptionType = exception.GetType().FullName,
                Message = exception.Message,
                Source = exception.Source,
                Data = GetExceptionData(exception),
            };

            MethodBase method = exception.TargetSite;
            if (method != null)
            {
                exceptionInfo.MethodName = string.Format("{0}.{1}", method.DeclaringType == null ? "Unknown" : method.DeclaringType.FullName, method.Name);
            }

            if (exception.InnerException != null)
            {
                exceptionInfo.InnerExceptionInfo = GetExceptionInfo(exception.InnerException);
            }

            return exceptionInfo;
        }

        private static ErrorData GetExceptionData(Exception exception)
        {
            var exceptionData = new ExceptionDataCollector().Collect(exception, Configuration);
            return exceptionData ?? new ErrorData();
        }

        private static ErrorData GetCustomData(Exception exception, IDictionary<string, string> customData)
        {
            var items = new ErrorData();

            if (customData != null)
            {
                foreach (var item in customData)
                {
                    items.Add("Custom." + item.Key, item.Value);
                }
            }

            var scopedItems = new ScopedDataCollector().Collect(exception);

            if (scopedItems != null)
            {
                foreach (var item in scopedItems)
                {
                    items.Add("Scoped." + item.Key, item.Value);
                }
            }

            var httpItems = new HttpContextDataCollector().Collect(exception, Configuration);

            if (httpItems != null)
            {
                foreach (var item in httpItems)
                {
                    items.Add("Request." + item.Key, item.Value);
                }
            }

            if (Configuration.DataCollectors == null)
                return items;

            foreach (var dataCollectorFactory in Configuration.DataCollectors)
            {
                var item = CollectData(exception, dataCollectorFactory);
                    
                if (item != null)
                    items.Add(item);
            }

            return items;
        }

        private static ErrorData CollectData(Exception exception, IDataCollectorFactory dataCollectorFactory)
        {
            try
            {
                var dataCollector = dataCollectorFactory.Create();
                if (dataCollector == null)
                {
                    return null;
                }

                var exceptionData = dataCollector.Collect(exception);

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

            while (exceptionType != null && exceptionType.IsAssignableFrom(_baseExceptionType))
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

        private static void Trace(string message, params object[] args)
        {
            if(Debug)
                System.Diagnostics.Trace.Write(string.Format("Errordite: {0}", string.Format(message, args)));
        }
    }
}
