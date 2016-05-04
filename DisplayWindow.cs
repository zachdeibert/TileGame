using System;
using System.Drawing;
using System.Windows.Forms;

namespace TileGame {
	public class DisplayWindow : Form {
		public bool AllowClose;
		string AuthCode;

		void KeyPressed(object sender, KeyPressEventArgs e) {
			if ( e.KeyChar == 'q' ) {
#if DEBUG
				AllowClose = true;
				Application.Exit();
#endif
			} else if ( e.KeyChar >= '0' && e.KeyChar <= '9' ) {
				AuthCode = string.Concat(AuthCode, new string(e.KeyChar, 1));
				if ( AuthCode.Length == 6 ) {
					if ( Authentication.IsValid(Entry.Secret, AuthCode) ) {
						AllowClose = true;
						Application.Exit();
					} else {
						Console.WriteLine("Invalid auth code {0}", AuthCode);
						AuthCode = "";
					}
				}
			}
		}

		public void ClosingForm(object sender, FormClosingEventArgs e) {
			if ( AllowClose ) {
				Taskbar.Show();
				Environment.Exit(0);
			} else {
				e.Cancel = true;
			}
		}

		public DisplayWindow() {
			AuthCode = "";
			AllowClose = false;
			AllowDrop = false;
			AllowTransparency = true;
			Bounds = Screen.PrimaryScreen.Bounds;
			FormBorderStyle = FormBorderStyle.None;
			Name = "Tile Game";
			SetStyle(ControlStyles.FixedHeight, true);
			SetStyle(ControlStyles.FixedWidth, true);
			SetStyle(ControlStyles.Opaque, true);
			Text = "Tile Game";
			TopMost = true;
			FormClosing += ClosingForm;
			KeyPress += KeyPressed;
			WindowState = FormWindowState.Normal;
			Taskbar.Hide();
			Screenshots.OurHandle = Handle;
			TileDisplay display = new TileDisplay();
			display.KeyPress += KeyPressed;
			display.Parent = this;
		}
	}
}

