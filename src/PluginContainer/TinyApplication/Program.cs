using System;
using System.Linq;
using System.Reflection;
using static TinyApplication.Helpers;

namespace TinyApplication
{

    public class Program
    {
        private static void Main(string[] args)
        {

            Execute(new AssemblyName("HelloPlugin"));
            Execute("HelloPlugin", "1.0.0");
            Execute("HelloPlugin", new Version(1, 0));
            Execute("HelloPlugin", new Version(1, 0, 0));
            Execute("HelloPlugin", new Version(1, 0, 0, 0));
        }


        public static void Execute(AssemblyName assemblyName)
        {
            var context = new PluginLoadContext(isCollectible: true);
            context.LoadPluginFromFile(assemblyName);
            var plugin = context.Assemblies.First(x => x.GetName().Name == assemblyName.Name);

            //Log($"LOADED ASSEMBLY {plugin.FullName}");
            //Log($"INVOKING ENTRYPOINT of {plugin.FullName}");

            var instance = plugin.ExportedTypes.First().GetConstructor(new Type[] { }).Invoke(new object[] { });
            instance.GetType().GetMethod("Run").Invoke(instance, new object[] { });

            //plugin.EntryPoint.Invoke(null, new object[] { new string[] { } });

            context.UnregisterResolvingEvents();
            context.Unload();
        }

        public static void Execute(string pluginName) => Execute(new AssemblyName(pluginName));

        public static void Execute(string pluginName, string version) => Execute(new AssemblyName(pluginName)
        {
            Version = Version.Parse(version)
        });

        public static void Execute(string pluginName, Version version) => Execute(new AssemblyName(pluginName)
        {
            Version = version
        });
    }




}
