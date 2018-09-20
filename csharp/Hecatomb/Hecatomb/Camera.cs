/*
 * Created by SharpDevelop.
 * User: m543015
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
	public class Camera
	{
		public int Height = 49;
		public int Width = 49;
		public int XOffset = 0;
		public int YOffset = 0;
		public int z = 0;
		
		public Camera()
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
