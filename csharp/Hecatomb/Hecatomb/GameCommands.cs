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
using System.Collections.Generic;
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
		
		public void MoveCameraNorth()
		{
			moveCameraHorizontal(+0,-1);
		}
		public void MoveCameraSouth()
		{
			moveCameraHorizontal(+0,+1);
		}
		public void MoveCameraEast()
		{
			moveCameraHorizontal(+1,+0);
		}
		public void MoveCameraWest()
		{
			moveCameraHorizontal(-1,+0);
		}
		public void MoveCameraUp()
		{
			moveCameraVertical(+1);
		}
		public void MoveCameraDown()
		{
			moveCameraVertical(-1);
		}
		
		private void moveCameraVertical(int dz)
		{
			Game.Camera.z = Math.Max(Math.Min(Game.Camera.z+dz,Constants.DEPTH-2),1);
			Game.MainPanel.Dirty = true;
			Game.MenuPanel.Dirty = true;
			Game.StatusPanel.Dirty = true;
		}
		private void moveCameraHorizontal(int dx, int dy)
		{
			GameCamera c = Game.Camera;
			int xhalf = c.Width/2;
			int yhalf = c.Height/2;
			c.XOffset = Math.Min(Math.Max(0, c.XOffset+dx), Constants.WIDTH-c.Width);
			c.YOffset = Math.Min(Math.Max(0, c.YOffset+dy), Constants.HEIGHT-c.Height);
			Game.MainPanel.Dirty = true;
			Game.MenuPanel.Dirty = true;
			Game.StatusPanel.Dirty = true;
		}
		
		public void ChooseTask()
		{
			var choices = new List<IMenuListable>() {
				Game.World.Entities.Mock<DigTask>()
			};
			Game.Controls = new MenuChoiceControls("Choose a task:", choices);
			Game.MenuPanel.Dirty = true;
		}
		
		public void ChooseSpell()
		{
			
			var choices = new List<IMenuListable>() {};
			SpellCaster caster = Game.World.Player.GetComponent<SpellCaster>();
			foreach (Spell s in caster.GetSpells())
			{
				choices.Add(s);
			}
			Game.Controls = new MenuChoiceControls("Choose a task:", choices);
			Game.MenuPanel.Dirty = true;
		}
		
		public void AutoWait()
		{
			p.Acted = true;
		}
		
		public void SaveGameCommand()
		{
			Game.World.Stringify();
		}
		
		public void RestoreGameCommand()
		{
			string json = System.IO.File.ReadAllText(@"..\GameWorld.json");
			Game.World.Parse(json);
		}
	}
}