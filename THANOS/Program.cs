using System;
using Engine.Extensions;
using Engine.ProcessCore;
using Engine.UWP;

namespace THANOS
{
    internal class Program
    {
        public static Core TargetProcess;
        public static bool Verbose;

        [STAThread]
        private static void Main(string[] args)
        {
            Verbose = false;

            if (args != null && args.Length > 0)
                if (args[0].StartsWith("verbose="))
                    Verbose = args[0].Replace("verbose=", "").ParseFromString();

            if (Helper.IsRunningElevated())
            {
                Tokenizer.Initiate();
                Tokenizer.SetProcessDebugToken((int) WinAPI.GetCurrentProcessId());
                Tokenizer.ImpersonateSystem();
                using (var frm = new frmProcesses())
                {
                    frm.ShowDialog();
                    if (TargetProcess != null)
                        using (var fi = new frmInjection(TargetProcess))
                        {
                            fi.ShowDialog();
                        }
                }
            }
        }
    }
}