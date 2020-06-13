using System;
using System.Reflection;
using TinyPlugin;

namespace HelloPlugin
{
    public class HelloPlugin : IPlugin
    {
        public void Load()
        {
            throw new NotImplementedException();
        }

        public void Run()
        {
            throw new NotImplementedException();
        }

        public static void Main(string[] args)
        {
            //System.Windows.Forms.MessageBox.Show($"Hello from {Assembly.GetExecutingAssembly().FullName}");
            Console.WriteLine($"Hello from {Assembly.GetExecutingAssembly().FullName}");
            var msg = Newtonsoft.Json.JsonConvert.SerializeObject("OK");
        }


    }

    public static class Application
    {
        public static void Main(string[] args)
        {
            Console.WriteLine($"Hello from {Assembly.GetExecutingAssembly().FullName}");
        }
    }
}
