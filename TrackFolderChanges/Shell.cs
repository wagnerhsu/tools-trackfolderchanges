using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;

namespace Antiufo
{
	public static class Shell
	{
		public static void StartWebPage(string Url)
		{
			try
			{
				Process.Start(Url);
			}
			catch
			{
				Process.Start(Convert.ToString(Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\App Paths\\IEXPLORE.EXE", string.Empty, "C:\\Program Files\\Internet Explorer\\IEXPLORE.EXE")), Url);
			}
		}
		
		public static void StartFolder(string path)
		{
			Process.Start(path);
		}

		public static void StartFile(string Path)
		{
			Process.Start(Path);
		}


		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);	
		
		[DllImport("Shell32", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
		private static extern int SHILCreateFromPath([MarshalAs(UnmanagedType.VBByRefStr)] ref string pszPath, ref IntPtr ppidl, ref int rgflnOut);
	
		[DllImport("Shell32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
		private static extern void ILFree(IntPtr ppidl);
	
		[DllImport("Shell32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
		private static extern IntPtr SHOpenFolderAndSelectItems(IntPtr pidlFolder, uint cidl, IntPtr apidl, int dwFlags);
	
		public static void OpenFolderAndSelectItem(string file)
		{
			try
			{
				int rgflnOut = 0;
				IntPtr ppidl = IntPtr.Zero;
				int result = SHILCreateFromPath(ref file, ref ppidl, ref rgflnOut);
				if (ppidl == (IntPtr)0)
				{
					throw new Exception();
				}
				IntPtr e = SHOpenFolderAndSelectItems(ppidl, 0u, IntPtr.Zero, 0);
				ILFree(ppidl);
			}
			catch
			{
				Process.Start("explorer.exe", "/select, \"" + file + "\"");
			}
		}


	}
}
