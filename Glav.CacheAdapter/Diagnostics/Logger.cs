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
        public void WriteInfoMessage(string message)
        {
            Trace.TraceInformation(ConstructTraceInfo(message));
        }

        public void WriteErrorMessage(string message)
        {
            Trace.TraceError(ConstructTraceInfo(message));
        }

        public void WriteException(Exception ex)
        {
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
