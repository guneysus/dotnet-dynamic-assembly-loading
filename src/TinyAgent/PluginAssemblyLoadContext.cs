using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Loader;

namespace TinyAgent
{
    public static class AssemblyLoadContextFactory
    {
        public static AssemblyLoadContext GetDefaultAssemblyLoadContext(Func<AssemblyLoadContext, AssemblyName, Assembly?>? resolving = null)
        {
            AssemblyLoadContext.Default.Resolving += Dependency_Resolving;
            return AssemblyLoadContext.Default;
        }

        public static AssemblyLoadContext New(
            string? name = null,
            Func<AssemblyLoadContext, AssemblyName, Assembly?>? resolving = null)
        {
            var context = new AssemblyLoadContext(name, true);

            context.Resolving += (AssemblyLoadContext context, AssemblyName asm) =>
            {
                var path = Path.Combine("lib", asm.Name, $"{string.Join('.', asm.Version.Major, asm.Version.Minor)}", $"{asm.Name}.dll");

                using var fs = new FileStream(path, FileMode.Open);
                using var ms = new MemoryStream();
                fs.CopyTo(ms);
                ms.Position = 0;

                return context.LoadFromStream(ms);
            };

            return context;
        }

        public static Assembly Dependency_Resolving(AssemblyLoadContext context, AssemblyName asm)
        {
            var path = Path.Combine("lib", asm.Name, $"{string.Join('.', asm.Version.Major, asm.Version.Minor)}", $"{asm.Name}.dll");

            using var fs = new FileStream(path, FileMode.Open);
            using var ms = new MemoryStream();
            fs.CopyTo(ms);
            ms.Position = 0;

            return context.LoadFromStream(ms);
        }
    }

}
