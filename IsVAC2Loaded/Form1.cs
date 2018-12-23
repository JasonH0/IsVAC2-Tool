using System;
using System.Windows.Forms;
using System.Threading;
using System.Text;
using System.Diagnostics;

using MemTools;

namespace IsVAC2Loaded
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            label1.Text = "";
            label2.Text = "";

            System.Diagnostics.Process.EnterDebugMode();

            try
            {
                Globals.ProcessID = Memory.GetProcessIdByProcessName("steam.exe");
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            if (Globals.ProcessID == 0)
            {
                MessageBox.Show("Failed to find Steam process..");
                System.Diagnostics.Process.LeaveDebugMode();
                return;
            }

            Process proc = Process.GetProcessById(Globals.ProcessID);
            ProcessModuleCollection mods = proc.Modules;
            bool found = false;
            foreach (ProcessModule mod in mods)
            {
                if (mod.ModuleName.Contains(".tmp"))
                {
                    found = true;
                    label2.Text = "Module name: " + mod.ModuleName + " | Module size: " + mod.ModuleMemorySize;
                }
            }

            System.Diagnostics.Process.LeaveDebugMode();

            if (found)
            {
                label1.ForeColor = System.Drawing.Color.Green;
                label1.Text = "Yes!";
            }
            else
            {
                label1.ForeColor = System.Drawing.Color.Red;
                label1.Text = "No.";
            }
        }
    }
}
