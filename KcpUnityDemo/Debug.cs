using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KcpUnityDemo
{
    public static class Debug
    {
        public static void Log(string message)
        {
            Console.WriteLine(message);
        }

        public static void LogWarning(string message) 
        { 
            Console.WriteLine("Warn:" + message);
        }

        public static void LogError(string message)
        {
            Console.WriteLine("Error:" + message + "\n" + GetTrace());
        }

        public static string GetTrace()
        {
            StackTrace stackTrace = new StackTrace(true);
            return stackTrace.ToString();
            //StackFrame[] stackFrames = stackTrace.GetFrames();
            //int maxLines = Math.Min(stackFrames.Length, 3);
            //StringBuilder sb = new StringBuilder();
            //for (int i = 0; i < maxLines; i++)
            //{
            //    StackFrame frame = stackFrames[i];
            //    sb.Append($"{frame.ToString()}");
            //}
            //return sb.ToString();
        }
    }
}
