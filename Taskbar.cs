using System;
using System.Runtime;
using System.Runtime.InteropServices;

namespace TileGame {
	public static class Taskbar {
		[DllImport("user32.dll")]
		private static extern int FindWindow(string className, string windowText);

		[DllImport("user32.dll")]
		private static extern int ShowWindow(int hwnd, int command);

		[DllImport("user32.dll")]
		private static extern int FindWindowEx(int parentHandle, int childAfter, string className, int windowTitle);

		[DllImport("user32.dll")]
		private static extern int GetDesktopWindow();

		public static int Handle {
			get {
				return FindWindow("Shell_TrayWnd", "");
			}
		}

		public static int StartBtnHandle {
			get {
				return FindWindowEx(GetDesktopWindow(), 0, "button", 0);
			}
		}

		public static void Show() {
			ShowWindow(Handle, 1);
			ShowWindow(StartBtnHandle, 1);
		}

		public static void Hide() {
			ShowWindow(Handle, 0);
			ShowWindow(StartBtnHandle, 0);
		}
	}
}

