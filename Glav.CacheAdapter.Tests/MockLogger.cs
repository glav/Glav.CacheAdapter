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
        }

        public void WriteErrorMessage(string message)
        {
        }

        public void WriteException(Exception ex)
        {
        }
    }
}
