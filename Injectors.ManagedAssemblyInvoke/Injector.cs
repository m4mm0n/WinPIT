using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Engine;
using Engine.Extensions;
using Engine.Injectors;
using Engine.ProcessCore;
using Module = Engine.ProcessCore.Module;

namespace Injectors.ManagedAssemblyInvoke
{
    public class Injector : IInjector
    {
        private readonly Logger log = new Logger(LoggerType.Console_File, "Injector.MAI");

        public string SelfFileName => Path.GetFileName(Assembly.GetExecutingAssembly().Location);

        public string UniqueId => "Injectors.MAI-" + QuickExt.GetHash(
                                      Encoding.UTF8.GetBytes(UniqueName +
                                                             Marshal.GetTypeLibGuidForAssembly(
                                                                 Assembly.GetExecutingAssembly())), HashType.MD5);

        public string UniqueName => "Managed Injection by Assembly Instance Invoke";

        public string About => "API: Managed System" + Environment.NewLine +
                               "DLL: mscorlib.dll" + Environment.NewLine + Environment.NewLine +
                               "Stealth: None" + Environment.NewLine +
                               "Kernel/System/Normal Access: Normal" + Environment.NewLine +
                               "Original Author: mammon";

        public Module InjectedModule { get; set; }
        public IntPtr Inject(Core targetProcess, string filePath)
        {
            return IntPtr.Zero;
        }
    }
}
