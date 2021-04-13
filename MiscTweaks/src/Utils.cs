using System;
using System.Runtime.InteropServices;

namespace MiscTweaks
{
	static class Utils
	{
		[DllImport("user32.dll")]
		static extern bool SetWindowPos(IntPtr hwnd, int hWndInsertAfter, int x, int y, int cx, int cy, int wFlags);

		[DllImport("user32.dll")]
		static extern IntPtr GetActiveWindow();

		public static void setWindowPos(int x, int y) => SetWindowPos(GetActiveWindow(), 0, x, y, 0, 0, 0x0001);
	}
}