using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Engine.ProcessCore;
using Engine.UWP;

namespace THANOS
{
    class Program
    {
        public static Core TargetProcess;

        static void Main(string[] args)
        {
            if (Helper.IsRunningElevated())
            {
                Tokenizer.Initiate();
                Tokenizer.SetProcessDebugToken((int)WinAPI.GetCurrentProcessId());
                Tokenizer.ImpersonateSystem();
                //Tokenizer.ImpersonateTrustedInstaller();
                using (frmProcesses frm = new frmProcesses())
                {
                    frm.ShowDialog();
                    if (TargetProcess != null)
                    {
                        using (frmInjection fi = new frmInjection(TargetProcess))
                        {
                            fi.ShowDialog();
                        }
                    }
                }
            }

            Console.ReadKey();
        }
    }
}
