using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.ProcessCore;
using Engine.UWP;

namespace THANOS
{
    class Program
    {
        static void Main(string[] args)
        {
            //Tokenizer.Initiate();
            if (Helper.IsRunningElevated())
            {
                using (frmProcesses frm = new frmProcesses())
                {
                    frm.ShowDialog();
                }
            }

            Console.ReadKey();
        }
    }
}
