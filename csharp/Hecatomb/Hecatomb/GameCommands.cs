/*
 * Created by SharpDevelop.
 * User: m543015
 * Date: 9/21/2018
 * Time: 10:48 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Diagnostics;

namespace Hecatomb
{
	/// <summary>
	/// Description of Commands.
	/// </summary>
	public class GameCommands
	{
		public bool Wait()
		{
			return true;
		}
		public bool MoveNorthCommand()
		{
			return moveHorizontalCommand(+0, -1);
		}
		public bool MoveSouthCommand()
		{
			return moveHorizontalCommand(+0, +1);
		}
		public bool MoveEastCommand()
		{
			return moveHorizontalCommand(+1, +0);
		}
		public bool MoveWestCommand()
		{
			return moveHorizontalCommand(-1, +0);
		}
		public bool MoveUpCommand()
		{
			return moveVerticalCommand(+1);
		}
		public bool MoveDownCommand()
		{
			return moveVerticalCommand(-1);
		}
		private bool moveHorizontalCommand(int dx, int dy)
		{
			Player p = Game.World.Player;
			int x1 = p.x + dx;
			int y1 = p.y + dy;
			int z1 = p.z;
			var m = p.TryComponent<Movement>();
			if (m==null) {
				return false;
			}
			if (!m.CanPass(x1, y1, z1)) {
				if (m.CanPass(p.x, p.y, z1+1)){
					m.StepTo(p.x, p.y, z1+1);
					return true;
				} else if (m.CanPass(p.x, p.y, z1-1)){
					m.StepTo(p.x, p.y, z1-1);
					return true;
				}
			    return false;
			} else {
			    m.StepTo(x1, y1, z1);
				return true;
			}
		}
			
		private bool moveVerticalCommand(int dz)
		{
			Player p = Game.World.Player;
			int x1 = p.x;
			int y1 = p.y;
			int z1 = p.z + dz;
			var m = p.TryComponent<Movement>();
			if (m==null) {
				return false;
			}
			if (!m.CanPass(x1, y1, z1)) {
			    return false;
			} else {
			    m.StepTo(x1, y1, z1);
				return true;
			}
		}
	}
}