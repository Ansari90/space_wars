using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Server
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (Form1 frm = new Form1())
            {
                frm.Show();
                frm.HostGame();
                Application.Run(frm);
            }
        }
    }
}
