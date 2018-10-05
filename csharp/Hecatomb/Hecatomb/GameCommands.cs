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
		public void Wait()
		{
			Game.World.Player.Acted = true;
		}
		public void MoveNorthCommand()
		{
			moveHorizontalCommand(+0, -1);
		}
		public void MoveSouthCommand()
		{
			moveHorizontalCommand(+0, +1);
		}
		public void MoveEastCommand()
		{
			moveHorizontalCommand(+1, +0);
		}
		public void MoveWestCommand()
		{
			moveHorizontalCommand(-1, +0);
		}
		public void MoveUpCommand()
		{
			moveVerticalCommand(+1);
		}
		public void MoveDownCommand()
		{
			moveVerticalCommand(-1);
		}
		private void moveHorizontalCommand(int dx, int dy)
		{
			Player p = Game.World.Player;
			int x1 = p.x + dx;
			int y1 = p.y + dy;
			int z1 = p.z;
			var m = p.TryComponent<Movement>();
			if (m==null) {
				return;
			}
			if (!m.CanPass(x1, y1, z1)) {
				if (m.CanPass(p.x, p.y, z1+1)){
					m.StepTo(p.x, p.y, z1+1);
					p.Acted = true;
					return;
				} else if (m.CanPass(p.x, p.y, z1-1)){
					m.StepTo(p.x, p.y, z1-1);
					p.Acted = true;
					return;
				}
			    return;
			} else {
			    m.StepTo(x1, y1, z1);
			    p.Acted = true;
			    return;
			}
		}
			
		private void moveVerticalCommand(int dz)
		{
			Player p = Game.World.Player;
			int x1 = p.x;
			int y1 = p.y;
			int z1 = p.z + dz;
			var m = p.TryComponent<Movement>();
			if (m==null) {
				return;
			}
			if (!m.CanPass(x1, y1, z1)) {
			    return;
			} else {
			    m.StepTo(x1, y1, z1);
				p.Acted = true;
				return;
			}
		}
	}
}