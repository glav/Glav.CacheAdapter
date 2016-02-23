using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Glav.CacheAdapter.Core.Diagnostics;

namespace Glav.CacheAdapter.ExampleUsage
{
    public class InMemoryLogger : ILogging
    {
        private static StringBuilder _buffer = new StringBuilder();

        public void WriteInfoMessage(string message)
        {
            _buffer.AppendFormat("[Info]: {0}{1}",message,Environment.NewLine);
        }

        public void WriteErrorMessage(string message)
        {
            _buffer.AppendFormat("[Error]: {0}{1}", message, Environment.NewLine);
        }

        public void WriteException(Exception ex)
        {
            _buffer.AppendFormat("[Exception]: {0}{1}", ex.GetBaseException().Message, Environment.NewLine);
        }

        public static void FlushToDisk(string filename)
        {
            System.IO.File.WriteAllText(filename, _buffer.ToString());
            _buffer.Clear();
        }
    }
}
