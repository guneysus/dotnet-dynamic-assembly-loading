using HelloAlertPlugin;
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace DynamicDispatch
{
    class Program
    {
        static void Main(string[] args)
        {
            Type type = typeof(HelloAlertPlugin.HelloAlertPlugin);
            MethodInfo method = type.GetMethod("MessageBox");

            object[] parameters = new object[] {
                IntPtr.Zero, "Hello", "Hello World", 0
            };
            method.Invoke(null, parameters);

            //return;
            //MethodBody body = method.GetMethodBody();
            //var il = body.GetILAsByteArray();


            AssemblyBuilder a = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("foo"), AssemblyBuilderAccess.RunAndCollect);




            ModuleBuilder mod = a.DefineDynamicModule("foo");

            var modMain = mod.DefineGlobalMethod("Main",
    MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.Static, (Type?)null, (Type[])null);
            var ilModMain = modMain.GetILGenerator();

            ilModMain.Emit(OpCodes.Ldstr, "Merhaba");
            ilModMain.Emit(OpCodes.Call, typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }));
            ilModMain.Emit(OpCodes.Ret);


            mod.CreateGlobalFunctions();

            var tb = mod.DefineType("WinApi", TypeAttributes.Public | TypeAttributes.Class);
            tb.DefinePInvokeMethod(
     name: "MessageBox",
     dllName: "User32.dll",
     attributes: MethodAttributes.PinvokeImpl | MethodAttributes.Public | MethodAttributes.Static,
     callingConvention: CallingConventions.Standard,
     returnType: typeof(int),
     parameterTypes: new Type[] {
                            typeof(IntPtr),
                            typeof(string),
                            typeof(string),
                            typeof(int)
     },
     nativeCallConv: CallingConvention.StdCall,
     nativeCharSet: CharSet.Auto
     );

            var methodBuilderMain = tb.DefineMethod("Main",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.Static, (Type?)null, (Type[])null);
            var il = methodBuilderMain.GetILGenerator();
            var mi = typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) });

            il.Emit(OpCodes.Ldstr, "Merhaba");
            il.Emit(OpCodes.Call, mi);
            il.Emit(OpCodes.Ret);


            var winApi = tb.CreateType();

            var test = tb.Assembly.GetType("WinApi");
            var mbox = test.GetMethod("MessageBox");
            var main = test.GetMethod("Main");

            mbox.Invoke(null, parameters);
            main.Invoke(null, new object[] {});
            var l = AppDomain.CurrentDomain.GetAssemblies();


            // IntPtr hWnd, string lpText, string lpCaption, MB_TYPE uType
        }
    }
}
