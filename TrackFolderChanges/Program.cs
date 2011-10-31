using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Win32;

namespace TrackFolderChanges
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }


        public static void ReportError(IWin32Window owner, string text)
        {
            MessageBox.Show(owner, text, "Track folder changes", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void ReportError(IWin32Window owner, Exception exception)
        {
            ReportError(owner, exception.Message);
        }

        private const string RegKey = "HKEY_CURRENT_USER\\Software\\antiufo\\TrackFolderChanges";

        public static void WriteSetting(string name, object value)
        {
            Registry.SetValue(RegKey, name, value);
        }

        public static string ReadSetting(string name, string @default)
        {
            return (Registry.GetValue(RegKey, name, null) as string) ?? @default;
        }

    }
}
