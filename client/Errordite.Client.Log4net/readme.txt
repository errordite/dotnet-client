The Errordite log4net library is designed to allow errordite to receive any diagnostics 
output send to a log4net logger from configured applications.

Logging is initially configured by calling ErrorditeLogger.Initialise(bool enabled, params string[] loggers) 
in your application startup code.

Only web applications are currently supported as the logging relies on the HttpContext 
to persist and share logging information during a request.

It works by adding an appender to each of the loggers passed to the ErrorditeLogger.Initialise method. This appender
has its debug level set to Debug so it captures all output.

An ErrorditeLog4NetLogger is created and added to the http context on each request if logging is enabled.
This logger subscribes to the AppendLoggingEvent event on the ErrorditeAppender so it is notified of any
logging events. Each logging event results in a message being added to a list of messages contained in the
HttpContext.Current.Items dictionary.

The client will now check the HttpContext.Current.Items dictionary for any log messages prior to publishing 
the error to errordite if any are found they are converted to a LogMessage type and sent along with the error
to Errordite.