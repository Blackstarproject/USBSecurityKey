using System;
using System.Windows.Forms;

namespace UsbSecurityKey
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Standard Windows Forms application initialization.
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Instead of running a visible form with Application.Run(new Form1()),
            // this starts the application message loop but relies on our Form1's logic
            // to hide itself and manage its lifecycle through the NotifyIcon (system tray icon).
            Application.Run(new Form1());
        }
    }
}