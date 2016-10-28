using System;
using Enyim.Caching;
using Glav.CacheAdapter.Core.Diagnostics;

namespace Glav.CacheAdapter.Distributed.memcached
{
    class LogFactoryAdapter : ILogFactory
    {
        private readonly ILogging _logger = new Logger();
        private readonly ILog _logAdapter;

        public LogFactoryAdapter() { _logAdapter = new LogAdapter(_logger); }

        public LogFactoryAdapter(ILogging logger)
        {
            _logger = logger;
            _logAdapter = new LogAdapter(_logger);
        }
        public ILog GetLogger(Type type)
        {
            return _logAdapter;
        }

        public ILog GetLogger(string name)
        {
            return _logAdapter;
        }
    }

    class LogAdapter : ILog
    {
        private readonly ILogging _logger = new Logger();

        public LogAdapter() { }
        public LogAdapter(ILogging logger)
        {
            _logger = logger;
        }

        public void Debug(object message, Exception exception)
        {
            _logger.WriteException(exception);
        }

        public void Debug(object message)
        {
            var msg = message as string;
            if (msg != null)
            {
                _logger.WriteInfoMessage(msg);
            }
        }

        public void DebugFormat(IFormatProvider provider, string format, params object[] args)
        {
            var msg = string.Format(provider, format, args);
            _logger.WriteInfoMessage(msg);
        }

        public void DebugFormat(string format, params object[] args)
        {
            var msg = string.Format(format, args);
            _logger.WriteInfoMessage(msg);
        }

        public void DebugFormat(string format, object arg0, object arg1, object arg2)
        {
            var msg = string.Format(format, arg0, arg1, arg2);
            _logger.WriteInfoMessage(msg);
        }

        public void DebugFormat(string format, object arg0, object arg1)
        {
            var msg = string.Format(format, arg0, arg1);
            _logger.WriteInfoMessage(msg);
        }

        public void DebugFormat(string format, object arg0)
        {
            var msg = string.Format(format, arg0);
            _logger.WriteInfoMessage(msg);
        }

        public void Error(object message, Exception exception)
        {
            _logger.WriteException(exception);
        }

        public void Error(object message)
        {
            var msg = message as string;
            if (msg != null)
            {
                _logger.WriteErrorMessage(msg);
            }
        }

        public void ErrorFormat(IFormatProvider provider, string format, params object[] args)
        {
            var msg = string.Format(provider, format, args);
            _logger.WriteErrorMessage(msg);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            var msg = string.Format(format, args);
            _logger.WriteErrorMessage(msg);
        }

        public void ErrorFormat(string format, object arg0, object arg1, object arg2)
        {
            var msg = string.Format(format, arg0, arg1, arg2);
            _logger.WriteErrorMessage(msg);
        }

        public void ErrorFormat(string format, object arg0, object arg1)
        {
            var msg = string.Format(format, arg0, arg1);
            _logger.WriteErrorMessage(msg);
        }

        public void ErrorFormat(string format, object arg0)
        {
            var msg = string.Format(format, arg0);
            _logger.WriteErrorMessage(msg);
        }

        public void Fatal(object message, Exception exception)
        {
            _logger.WriteException(exception);
        }

        public void Fatal(object message)
        {
            var msg = message as string;
            if (msg != null)
            {
                _logger.WriteErrorMessage(msg);
            }
        }

        public void FatalFormat(IFormatProvider provider, string format, params object[] args)
        {
            var msg = string.Format(provider, format, args);
            _logger.WriteErrorMessage(msg);
        }

        public void FatalFormat(string format, params object[] args)
        {
            var msg = string.Format(format, args);
            _logger.WriteErrorMessage(msg);
        }

        public void FatalFormat(string format, object arg0, object arg1, object arg2)
        {
            var msg = string.Format(format, arg0, arg1, arg2);
            _logger.WriteErrorMessage(msg);
        }

        public void FatalFormat(string format, object arg0, object arg1)
        {
            var msg = string.Format(format, arg0, arg1);
            _logger.WriteErrorMessage(msg);
        }

        public void FatalFormat(string format, object arg0)
        {
            var msg = string.Format(format, arg0);
            _logger.WriteErrorMessage(msg);
        }

        public void Info(object message, Exception exception)
        {
            throw new NotImplementedException();
        }

        public void Info(object message)
        {
            Debug(message);
        }

        public void InfoFormat(IFormatProvider provider, string format, params object[] args)
        {
            DebugFormat(provider, format, args);
        }

        public void InfoFormat(string format, params object[] args)
        {
            var msg = string.Format(format, args);
            _logger.WriteInfoMessage(msg);
        }

        public void InfoFormat(string format, object arg0, object arg1, object arg2)
        {
            DebugFormat(format, arg0, arg1, arg2);
        }

        public void InfoFormat(string format, object arg0, object arg1)
        {
            DebugFormat(format, arg0, arg1);
        }

        public void InfoFormat(string format, object arg0)
        {
            DebugFormat(format, arg0);
        }

        public bool IsDebugEnabled
        {
            get { return true; }
        }

        public bool IsErrorEnabled
        {
            get { return true; }
        }

        public bool IsFatalEnabled
        {
            get { return true; }
        }

        public bool IsInfoEnabled
        {
            get { return true; }
        }

        public bool IsWarnEnabled
        {
            get { return true; }
        }

        public void Warn(object message, Exception exception)
        {
            Debug(message, exception);
        }

        public void Warn(object message)
        {
            Debug(message);
        }

        public void WarnFormat(IFormatProvider provider, string format, params object[] args)
        {
            DebugFormat(provider, format, args);
        }

        public void WarnFormat(string format, params object[] args)
        {
            DebugFormat(format, args);
        }

        public void WarnFormat(string format, object arg0, object arg1, object arg2)
        {
            DebugFormat(format, arg0, arg1, arg2);
        }

        public void WarnFormat(string format, object arg0, object arg1)
        {
            DebugFormat(format, arg0, arg1);
        }

        public void WarnFormat(string format, object arg0)
        {
            DebugFormat(format, arg0);
        }
    }

}
