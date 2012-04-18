using System;
using System.Collections.Generic;
using System.Linq;
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
                        ,message
                        ,System.Environment.NewLine);
        }

        private string FormatExceptionAsString(Exception ex)
        {
            StringBuilder errMsg = new StringBuilder();
            errMsg.AppendFormat("Exception: {0}{1}", ex.GetType().ToString(), System.Environment.NewLine);
            errMsg.AppendFormat("Message: {0}{1}", ex.Message, System.Environment.NewLine);
            errMsg.AppendFormat("Stack Trace: {0}{1}", ex.StackTrace, System.Environment.NewLine);
            if (ex.InnerException != null)
            {
                errMsg.AppendFormat("Inner Exception: {0}{1}", ex.InnerException.GetType().ToString(), System.Environment.NewLine);
                errMsg.AppendFormat("Inner Message: {0}{1}", ex.InnerException.Message, System.Environment.NewLine);
                errMsg.AppendFormat("Inner Stack Trace: {0}{1}", ex.InnerException.StackTrace, System.Environment.NewLine);
            }

            return ConstructTraceInfo(errMsg.ToString());
        }
    }
}
