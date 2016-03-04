using System;
using System.Text;
using System.Diagnostics;

namespace Glav.CacheAdapter.Core.Diagnostics
{
    /// <summary>
    /// A very basic logging implementation
    /// </summary>
    public class Logger : ILogging
    {
        private CacheConfig _config;

        public Logger(CacheConfig config = null)
        {
            if (config == null)
            {
                config = new CacheConfig();
            }
            _config = config;
        }
        public void WriteInfoMessage(string message)
        {
            if (_config.LoggingLevel != CacheAdapter.Diagnostics.LoggingLevel.Information)
            {
                return;
            }
            Trace.TraceInformation(ConstructTraceInfo(message));
        }

        public void WriteErrorMessage(string message)
        {
            if (_config.LoggingLevel == CacheAdapter.Diagnostics.LoggingLevel.None)
            {
                return;
            }
            Trace.TraceError(ConstructTraceInfo(message));
        }

        public void WriteException(Exception ex)
        {
            if (_config.LoggingLevel == CacheAdapter.Diagnostics.LoggingLevel.None)
            {
                return;
            }
            Trace.TraceError(FormatExceptionAsString(ex));
        }

        private string ConstructTraceInfo(string message)
        {
            return string.Format("{0} {1}: {2}{3}",
                        DateTime.Now.ToString("dd/MM/yyyy"),
                        DateTime.Now.ToString("hh:mm:ss")
                        , message
                        , Environment.NewLine);
        }

        private string FormatExceptionAsString(Exception ex)
        {
            StringBuilder errMsg = new StringBuilder();
            errMsg.AppendFormat("Exception: {0}{1}", ex.GetType(), Environment.NewLine);
            errMsg.AppendFormat("Message: {0}{1}", ex.Message, Environment.NewLine);
            errMsg.AppendFormat("Stack Trace: {0}{1}", ex.StackTrace, Environment.NewLine);
            if (ex.InnerException != null)
            {
                errMsg.AppendFormat("Inner Exception: {0}{1}", ex.InnerException.GetType(), Environment.NewLine);
                errMsg.AppendFormat("Inner Message: {0}{1}", ex.InnerException.Message, Environment.NewLine);
                errMsg.AppendFormat("Inner Stack Trace: {0}{1}", ex.InnerException.StackTrace, Environment.NewLine);
            }

            return ConstructTraceInfo(errMsg.ToString());
        }
    }
}
