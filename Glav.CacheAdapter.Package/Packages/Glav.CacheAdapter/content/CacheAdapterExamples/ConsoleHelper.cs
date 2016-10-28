using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glav.CacheAdapter.ExampleUsage
{
    static class ConsoleHelper
    {
        static ConsoleColor _originalColor = Console.ForegroundColor;

        public static void WriteErrMsgToConsole(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msg);
            Console.ForegroundColor = _originalColor;
        }

        public static void WriteHeadingSeparator(string heading)
        {
            var headingText = "*** " + heading + " ***";
            var line = new string('*', headingText.Length);

            Console.WriteLine("\n\n");
            Console.WriteLine(line);
            Console.WriteLine(headingText);
            Console.WriteLine(line);
            Console.WriteLine("\n");
        }

        public static void Wait(double timeInSeconds)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("Waiting for {0} seconds...", timeInSeconds);
            System.Threading.Thread.Sleep((int)(timeInSeconds * 1000));
            Console.ForegroundColor = _originalColor;
        }

    }
}
