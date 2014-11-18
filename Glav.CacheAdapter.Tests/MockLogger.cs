using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Glav.CacheAdapter.Core.Diagnostics;

namespace Glav.CacheAdapter.Tests
{
    class MockLogger : ILogging
    {
        public void WriteInfoMessage(string message)
        {
            System.Diagnostics.Trace.WriteLine(message);
        }

        public void WriteErrorMessage(string message)
        {
            System.Diagnostics.Trace.WriteLine("ERROR:"+message);
        }

        public void WriteException(Exception ex)
        {
            System.Diagnostics.Trace.WriteLine(ex.GetBaseException().Message);
        }
    }
}
