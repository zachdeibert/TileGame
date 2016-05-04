using System;
using System.Collections;
using System.Drawing;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TileGame {
	public static class Screenshots {
		public static IntPtr OurHandle;

		[DllImport("user32.dll", CharSet=CharSet.Auto, SetLastError=true)]
		private static extern bool EnumWindows(WindowEnumerator lpEnumFunc, ArrayList lParam);

		[DllImport("user32.dll")]
		private static extern bool IsWindowVisible(IntPtr hWnd);

		[DllImport("user32.dll")]
		private static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, uint nFlags);

		[DllImport("user32.dll")]
		private static unsafe extern bool GetWindowRect(IntPtr hWnd, [In, Out] RECT *lpRect);

		public static ArrayList OpenWindows {
			get {
				ArrayList handles = new ArrayList();
				EnumWindows(WindowEnumeratorImpl, handles);
				return handles;
			}
		}

		private static bool WindowEnumeratorImpl(IntPtr handleWindow, ArrayList handles) {
			if ( handleWindow != OurHandle && IsWindowVisible(handleWindow) ) {
				handles.Add(handleWindow);
			}
			return true;
		}

		private static unsafe void PaintWindow(IntPtr hWnd, Bitmap bmp, Rectangle screen) {
			RECT rect;
			GetWindowRect(hWnd, &rect);
			int width = rect.Right - rect.Left;
			int height = rect.Top - rect.Bottom;
			if ( width < 1 || width > screen.Width ) {
				width = screen.Width;
			}
			if ( height < 1 || height > screen.Height ) {
				height = screen.Height;
			}
			Bitmap win = new Bitmap(width, height);
			Graphics winG = Graphics.FromImage(win);
			Rectangle bounds = new Rectangle(rect.Left, rect.Top, width, height);
			winG.DrawImage(bmp, new Rectangle(0, 0, width, height), bounds, GraphicsUnit.Pixel);
			IntPtr hdc = winG.GetHdc();
			PrintWindow(hWnd, hdc, 0);
			winG.ReleaseHdc();
			winG.Dispose();
			Graphics g = Graphics.FromImage(bmp);
			g.DrawImage(win, bounds);
			win.Dispose();
			g.Dispose();
		}

		public static Bitmap CaptureScreen() {
			Rectangle screen = Screen.PrimaryScreen.Bounds;
			Bitmap bmp = new Bitmap(screen.Width, screen.Height);
			/*
			Taskbar.Show();
			PaintWindow(new IntPtr(Taskbar.Handle), bmp, screen);
			PaintWindow(new IntPtr(Taskbar.StartBtnHandle), bmp, screen);
			Taskbar.Hide();
			*/
			object[] windows = OpenWindows.ToArray();
			for ( int i = windows.Length - 1; i >= 0; --i ) {
				PaintWindow((IntPtr) windows[i], bmp, screen);
			}
			return bmp;
		}

		private delegate bool WindowEnumerator(IntPtr handleWindow, ArrayList handles);
	}
}

