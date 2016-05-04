using System;

namespace TileGame {
	public class TileMap {
		public static readonly int AnimationFrames = 2;
		private int[,] Data;
		private int Animating;
		private int AnimationStage;

		public static int ToCoord(int x, int y) {
			return ((x & 0xFFFF) << 16) | (y & 0xFFFF);
		}

		public static void FromCoord(int coord, out int x, out int y) {
			x = (coord >> 16);
			y = (coord & 0xFFFF);
		}

		public int GetAnimation(out int fromCoord, out int toCoord) {
			fromCoord = 0;
			toCoord = 0;
			int stage = AnimationStage;
			if ( stage > 0 ) {
				toCoord = Animating;
				int x;
				int y;
				FromCoord(Animating, out x, out y);
				if ( GetTile(x + 1, y) == -1 ) {
					fromCoord = ToCoord(x + 1, y);
				} else if ( GetTile(x - 1, y) == -1 ) {
					fromCoord = ToCoord(x - 1, y);
				} else if ( GetTile(x, y + 1) == -1 ) {
					fromCoord = ToCoord(x, y + 1);
				} else {
					fromCoord = ToCoord(x, y - 1);
				}
				if ( ++AnimationStage > AnimationFrames ) {
					AnimationStage = 0;
				}
			}
			return stage;
		}

		public bool Slide(int x, int y) {
			if ( AnimationStage == 0 ) {
				int data = GetTile(x, y);
				if ( GetTile(x + 1, y) == -1 ) {
					SetTile(x + 1, y, data);
					Animating = ToCoord(x + 1, y);
				} else if ( GetTile(x - 1, y) == -1 ) {
					SetTile(x - 1, y, data);
					Animating = ToCoord(x - 1, y);
				} else if ( GetTile(x, y + 1) == -1 ) {
					SetTile(x, y + 1, data);
					Animating = ToCoord(x, y + 1);
				} else if ( GetTile(x, y - 1) == -1 ) {
					SetTile(x, y - 1, data);
					Animating = ToCoord(x, y - 1);
				} else {
					return false;
				}
				SetTile(x, y, -1);
				AnimationStage = 1;
				return true;
			}
			return false;
		}

		public int SetTile(int x, int y, int coord) {
			return Data[x, y] = coord;
		}

		public int GetTile(int x, int y) {
			if ( x < 0 || y < 0 || x >= TileDisplay.XTiles || y >= TileDisplay.YTiles ) {
				return -2;
			} else {
				return Data[x, y];
			}
		}

		public TileMap() {
			Data = new int[TileDisplay.XTiles, TileDisplay.YTiles];
			for ( int x = 0; x < TileDisplay.XTiles; ++x ) {
				for ( int y = 0; y < TileDisplay.YTiles; ++y ) {
					SetTile(x, y, ToCoord(x, y));
				}
			}
			SetTile(0, 0, -1);
			AnimationStage = 0;
		}
	}
}

