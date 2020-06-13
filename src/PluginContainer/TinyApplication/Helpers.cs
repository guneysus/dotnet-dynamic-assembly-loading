using System;
using System.Reflection;
using System.Runtime.Loader;

namespace TinyApplication
{
    public static class Helpers
    {
        public static void Log(string message)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("o")}] {message}");
        }


    }
}
