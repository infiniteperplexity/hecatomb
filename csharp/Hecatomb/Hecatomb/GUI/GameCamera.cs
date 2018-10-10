/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 9/19/2018
 * Time: 2:38 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace Hecatomb
{
	/// <summary>
	/// Description of Camera.
	/// </summary>
	public class GameCamera
	{
		public int Height = 25;
		public int Width = 25;
		public int XOffset = 0;
		public int YOffset = 0;
		public int z = 0;
		
		public GameCamera()
		{
			
		}
		
		public void Center(int x, int y, int _z)
		{
			int xhalf = Width/2;
			int yhalf = Height/2;
			XOffset = Math.Min(Math.Max(0, x-xhalf), Constants.WIDTH-Width);
			YOffset = Math.Min(Math.Max(0, y-yhalf), Constants.HEIGHT-Height);
			z = _z;
		}
	}
}
