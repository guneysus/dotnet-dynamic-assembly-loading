using System;
using System.Reflection;
using TinyPlugin;

namespace HelloPlugin
{
    public class HelloPlugin : IPlugin
    {

        public void Run()
        {
            Console.WriteLine($"Hello from {Assembly.GetExecutingAssembly().FullName}");
            var msg = Newtonsoft.Json.JsonConvert.SerializeObject("OK");
            //var _ = Resources.ResourceManager.GetObject("win32_shellcode");
        }

        public static void Main(string[] args)
        {
            //System.Windows.Forms.MessageBox.Show($"Hello from {Assembly.GetExecutingAssembly().FullName}");
            Console.WriteLine($"Hello from {Assembly.GetExecutingAssembly().FullName}");
            //var msg = Newtonsoft.Json.JsonConvert.SerializeObject("OK");
        }


    }
}
