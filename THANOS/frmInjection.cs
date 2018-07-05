using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Engine.ProcessCore;

namespace THANOS
{
    public partial class frmInjection : MetroFramework.Forms.MetroForm
    {
        private Core proc;

        public frmInjection(Core core)
        {
            proc = core;
            InitializeComponent();
        }

        private void frmInjection_Load(object sender, EventArgs e)
        {
            //nothin yet :P
            
        }
    }
}
