/*
 * Created by SharpDevelop.
 * User: m543015
 * Date: 9/21/2018
 * Time: 11:33 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace Hecatomb
{
	/// <summary>
	/// Description of Movement.
	/// </summary>
	public class Movement : Component
	{
		public bool Walks;
		public bool Climbs;
		public bool Flies;
		public bool Swims;
		
		public Movement()
		{
			Required = new string[] {"Position"};
			Walks = true;
			Climbs = true;
			Flies = false;
			Swims = true;
		}
		
		public bool CanMove(int x1, int y1, int z1)
		{
			// there are situations where diagonal movement should be blocked...
			if (x1<0 || x1>=Constants.WIDTH || y1<0 || y1>=Constants.HEIGHT || z1<0 || z1>=Constants.DEPTH) {
				return false;
			}
			Terrain tile = Game.World.Tiles[x1,y1,z1];
			
			if (tile.Solid) {
				return false;
			}
			if (tile.Fallable && Flies==false) {
				return false;
			}
			return true;
		}
		
		public void StepTo(int x1, int y1, int z1)
		{
			Entity.x = x1;
			Entity.y = y1;
			Entity.z = z1;
		}
	}
}
