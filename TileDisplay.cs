using System;
using System.Drawing;
using System.Threading;
using System.Windows;
using System.Windows.Forms;

namespace TileGame {
	public class TileDisplay : Control {
		private static readonly int TargetTiles = 50;
		private static int xTiles = 0;
		private static int yTiles = 0;
		public Image Image;
		private TileMap Map;
		private Randomizer Random;
		private Thread UpdateThread;
		private Thread WinThread;

		public static int XTiles {
			get {
				if ( xTiles == 0 ) {
					CalculateTiles();
				}
				return xTiles;
			}
		}

		public static int YTiles {
			get {
				if ( yTiles == 0 ) {
					CalculateTiles();
				}
				return yTiles;
			}
		}

		private static void CalculateTiles() {
			Rectangle screen = Screen.PrimaryScreen.Bounds;
			double ratio = screen.Width / (double) screen.Height;
			for ( int i = 1; i < 64; ++i ) {
				double res = ratio * i;
				double rnd = Math.Round(res);
				if ( Math.Abs(res - rnd) < 0.000001 ) {
					xTiles = (int) rnd;
					yTiles = (int) Math.Round(res / ratio);
					break;
				}
			}
			if ( xTiles == 0 || yTiles == 0 ) {
				xTiles = 10;
				yTiles = 5;
			} else {
				int tiles = xTiles * yTiles;
				int factor = 1;
				while ( tiles < TargetTiles ) {
					++factor;
					tiles = xTiles * yTiles * factor * factor;
				}
				int lower = xTiles * (yTiles * factor * factor - 2 * factor + 1);
				int upper = tiles;
				if ( Math.Abs(TargetTiles - lower) < Math.Abs(TargetTiles - upper) ) {
					--factor;
				}
				xTiles *= factor;
				yTiles *= factor;
			}
		}

		void UpdateThreadImpl() {
			Image last = Image;
			while ( true ) {
				Thread.Yield();
				Image next = Screenshots.CaptureScreen();
				Image = next;
				Invalidate();
				last.Dispose();
			}
		}

		void WinThreadImpl() {
			Thread.Sleep(10000);
			while ( true ) {
				Thread.Yield();
				for ( int x = 0; x < XTiles; ++x ) {
					for ( int y = 0; y < YTiles; ++y ) {
						if ( (x != 0 || y != 0) && Map.GetTile(x, y) != TileMap.ToCoord(x, y) ) {
							goto done;
						}
					}
				}
				Thread.Sleep(1000);
				((DisplayWindow) Parent).AllowClose = true;
				((DisplayWindow) Parent).ClosingForm(this, null);
				done:
				Thread.Sleep(100);
			}
		}

		void PaintBackground(object sender, PaintEventArgs e) {
			Rectangle screen = Bounds;
			Graphics g = e.Graphics;
			float dx = screen.Width / (float) XTiles;
			float dy = screen.Height / (float) YTiles;
			for ( int x = 0; x < XTiles; ++x ) {
				for ( int y = 0; y < YTiles; ++y ) {
					int val = Map.GetTile(x, y);
					if ( val > 0 ) {
						int tx;
						int ty;
						TileMap.FromCoord(val, out tx, out ty);
						g.DrawImage(Image, new RectangleF(x * dx, y * dy, dx, dy), new RectangleF(tx * dx, ty * dy, dx, dy), GraphicsUnit.Pixel);
					} else {
						g.FillRectangle(new SolidBrush(Color.Black), new RectangleF(x * dx, y * dy, dx, dy));
					}
				}
			}
			int fromCoord;
			int toCoord;
			int animFrame = Map.GetAnimation(out fromCoord, out toCoord);
			if ( animFrame > 0 ) {
				int fromX;
				int fromY;
				int toX;
				int toY;
				TileMap.FromCoord(fromCoord, out fromX, out fromY);
				TileMap.FromCoord(toCoord, out toX, out toY);
				g.FillRectangle(new SolidBrush(Color.Black), new RectangleF(fromX * dx, fromY * dy, dx, dy));
				g.FillRectangle(new SolidBrush(Color.Black), new RectangleF(toX * dx, toY * dy, dx, dy));
				float anim = animFrame / (float) TileMap.AnimationFrames;
				float x = fromX + ((toX - fromX) * anim);
				float y = fromY + ((toY - fromY) * anim);
				int tx;
				int ty;
				TileMap.FromCoord(Map.GetTile(toX, toY), out tx, out ty);
				g.DrawImage(Image, new RectangleF(x * dx, y * dy, dx, dy), new RectangleF(tx * dx, ty * dy, dx, dy), GraphicsUnit.Pixel);
				Invalidate();
			} else if ( Random.Randomize(Map) ) {
				Invalidate();
			}
		}

		void PaintBorders(object sender, PaintEventArgs e) {
			Rectangle screen = Bounds;
			Graphics g = e.Graphics;
			for ( int x = 0; x <= XTiles; ++x ) {
				g.DrawLine(new Pen(Color.FromArgb(0x3F000000), 4), x * screen.Width / XTiles, 0, x * screen.Width / XTiles, screen.Height);
			}
			float dx = screen.Width / (float) XTiles;
			for ( int x = 0; x <= XTiles; ++x ) {
				for ( int y = 0; y <= YTiles; ++y ) {
					g.DrawLine(new Pen(Color.FromArgb(0x3F000000), 4), x * dx + 2, y * screen.Height / YTiles, x * dx + dx - 2, y * screen.Height / YTiles);
				}
			}
		}

		void TrySlide(object sender, EventArgs e) {
			Rectangle screen = Bounds;
			float dx = screen.Width / (float) XTiles;
			float dy = screen.Height / (float) YTiles;
			int mx = MousePosition.X;
			int my = MousePosition.Y;
			int tx = (int) (mx / dx);
			int ty = (int) (my / dy);
			if ( Map.Slide(tx, ty) ) {
				Invalidate();
			}
		}

		public TileDisplay() {
			DoubleBuffered = true;
			Dock = DockStyle.Fill;
			Image = Screenshots.CaptureScreen();
			Click += TrySlide;
			Paint += PaintBackground;
			Paint += PaintBorders;
			Map = new TileMap();
			Random = new Randomizer();
			UpdateThread = new Thread(UpdateThreadImpl);
			UpdateThread.Start();
			WinThread = new Thread(WinThreadImpl);
			WinThread.Start();
		}

		~TileDisplay() {
			UpdateThread.Abort();
			WinThread.Abort();
		}
	}
}

