using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Glav.CacheAdapter.Diagnostics
{
    public enum LoggingLevel
    {
        None,
        ErrorsOnly,
        Information
    }
     
    public static class LoggingLevelExtensioins
    {
        public static LoggingLevel ToLoggingLevel(this string loggingLevelText)
        {
            if (string.IsNullOrWhiteSpace(loggingLevelText))
            {
                return LoggingLevel.Information;
            }

            var normalisedText = loggingLevelText.ToLowerInvariant();
            if (normalisedText == "none")
            {
                return LoggingLevel.None;
            }
            if (normalisedText == "errorsonly")
            {
                return LoggingLevel.ErrorsOnly;
            }
            return LoggingLevel.Information;
        }
    }

}
