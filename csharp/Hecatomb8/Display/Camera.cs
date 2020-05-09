using System;

namespace Hecatomb8
{
	/// <summary>
	/// Description of Camera.
	/// </summary>
	public class Camera
	{
		public int Height;
		public int Width;
		public int XOffset = 0;
		public int YOffset = 0;
		public int Z = 0;

		public Camera(int width, int height)
		{
			Height = height;
			Width = width;
		}

		public void Center(int x, int y, int _z)
		{
			int xhalf = Width / 2;
			int yhalf = Height / 2;
			XOffset = Math.Min(Math.Max(0, x - xhalf), GameState.World!.Width - Width);
			YOffset = Math.Min(Math.Max(0, y - yhalf), GameState.World!.Height - Height);
			Z = _z;
		}

		public void CenterOnSelection()
		{
			var selected = InterfaceState.Controls.SelectedEntity;
			if (selected != null && selected!.Placed)
			{
				var (x, y, z) = selected!.GetPlacedCoordinate();
				InterfaceState.Camera!.Center(x, y, z);
				InterfaceState.Cursor = new Coord(x, y, z);
			}
		}
	}
}
