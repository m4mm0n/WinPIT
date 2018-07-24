using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Engine.Injectors
{
    /// <summary>
    ///     InjectionsLoader - a better word is a Plugin-Loader...
    /// </summary>
    public class InjectionsLoader
    {
        private static readonly Logger log = new Logger(LoggerType.Console_File, "Injector.InjectionsLoader");

        public static List<IInjector> GetInjectors()
        {
            log.Log(LogType.Normal, "[+] Loading Injectors...");

            var _injectors = new List<IInjector>();

            if (Directory.Exists(Environment.CurrentDirectory + "\\Injectors"))
            {
                var injs = Directory.GetFiles(Environment.CurrentDirectory + "\\Injectors", "Injectors.*.dll");
                foreach (var inj in injs)
                    try
                    {
                        System.Reflection.Assembly.LoadFile(Path.GetFullPath(inj));
                    }
                    catch (BadImageFormatException bife)
                    {
                        log.Log(bife, "Failed to load injector: {0}", Path.GetFileName(inj));
                    }
                    catch (FileNotFoundException fnfe)
                    {
                        log.Log(fnfe, "Failed to load injector: {0}", Path.GetFileName(inj));
                    }
                    catch (FileLoadException fle)
                    {
                        log.Log(fle, "Failed to load injector: {0}", Path.GetFileName(inj));
                    }

                var iType = typeof(IInjector);
                var types = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .Where(p => iType.IsAssignableFrom(p) && p.IsClass)
                    .ToArray();
                foreach (var tt in types)
                    _injectors.Add((IInjector) Activator.CreateInstance(tt));

                log.Log(LogType.Success, "Successfully loaded and initiated {0} injectors!",
                    _injectors.Count.ToString());
            }
            else
            {
                log.Log(LogType.Failure, "Unable to locate injectors directory: {0}",
                    Marshal.GetLastWin32Error().ToString("X"));
            }

            return _injectors;
        }
    }
}