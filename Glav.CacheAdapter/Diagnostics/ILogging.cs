using System;

namespace Glav.CacheAdapter.Core.Diagnostics
{
    public interface ILogging
    {
        /// <summary>
        /// Write an informational message to the trace log
        /// </summary>
        /// <param name="message">The informational message</param>
        void WriteInfoMessage(string message);
        /// <summary>
        /// Write an error message to the trace log
        /// </summary>
        /// <param name="message">The error message</param>
        void WriteErrorMessage(string message);
        /// <summary>
        /// Write exception details to the trace log
        /// </summary>
        /// <param name="ex">The exception to be written</param>
        void WriteException(Exception ex);

    }
}
