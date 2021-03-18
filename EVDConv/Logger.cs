using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace EVDConv
{
    static class Logger
    {
        //public static StreamWriter pen = null;

        public static void Log(string msg)
        {
            Console.WriteLine("[INFO] " + msg);
        }

        public static void Warn(string msg)
        {
            Console.WriteLine("[WARN] " + msg);
        }

        public static void Error(string msg)
        {
            Console.WriteLine("[ERROR] " + msg);
        }

        public static void Debug(string msg)
        {
            Console.WriteLine("[DEBUG] " + msg);
        }
    }
}
