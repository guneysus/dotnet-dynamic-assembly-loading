using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Loader;

namespace TinyAgent
{

    public static class AssemblyLoadContextExtensions
    {
        public static AssemblyLoadContext Load(this MemoryStream pluginMemoryStream, AssemblyLoadContext context)
        {
            context.LoadFromStream(pluginMemoryStream);
            return context;
        }
    }
}
