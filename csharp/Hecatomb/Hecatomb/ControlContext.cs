/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/5/2018
 * Time: 12:11 PM
 */
using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Hecatomb
{
	/// <summary>
	/// Description of ControlContext.
	/// </summary>
	public abstract class ControlContext
	{
		static KeyboardState OldKeyboard;
        static MouseState OldMouse;
        static DateTime InputBegan;
        public const int Throttle = 250;
        public Dictionary <Keys, Action> KeyMap;
        public List<string> MenuText;
        public Dictionary<Tuple<int, int>, string> TextColors;
           
        public void Nothing(int x, int y) {}
        
        public void HandleClick(int x, int y)
        {
        	if (x>Game.MainPanel.Size && y>Game.MainPanel.Size && x<Game.MenuPanel.X0-Game.MainPanel.Size && y<Game.StatusPanel.Y0-Game.MainPanel.Size)
        	{
         		int Size = Game.MainPanel.Size;
	        	int Padding = Game.MainPanel.Padding;
	        	GameCamera Camera = Game.Camera;
	        	Coord tile = new Coord(x/(Size+Padding)-1+Camera.XOffset,y/(Size+Padding)-1+Camera.YOffset,Camera.z);
	        	ClickTile(tile);
        	}
        	else if (x>=Game.MenuPanel.X0) 
        	{
        		MenuClick(x, y);
        	}
        	else if (y>=Game.StatusPanel.Y0)
        	{
        		StatusClick(x, y);
        	}
        	else
        	{
        		Nothing(x, y);
        	}
        }
        
        public virtual void ClickTile(Coord c)
        {
        	Debug.Print("Clicked on tile at {0} {1}", c.x, c.y);
        	Nothing(c.x, c.y);
        }
        
        public virtual void MenuClick(int x, int y)
        {
        	Debug.Print("Clicked on the menu at {0} {1}", x, y);
        }
        
        public virtual void StatusClick(int x, int y)
        {
        	Debug.Print("Clicked on the status bar at {0} {1}", x, y);
        }
        
        static ControlContext()
        {
        	OldKeyboard = Keyboard.GetState();
        	OldMouse = Mouse.GetState();
        	InputBegan = DateTime.Now;
        }
		public ControlContext()
		{
			KeyMap = new Dictionary<Keys, Action>();
			MenuText = new List<string>();
			TextColors = new Dictionary<Tuple<int, int>, string>();
		}
		
		public void HandleInput()
		{
			if (!Game.game.IsActive)
			{
				return;
			}
        	var m = Mouse.GetState();
        	var k = Keyboard.GetState();
        	DateTime now = DateTime.Now;
        	double sinceInputBegan = now.Subtract(InputBegan).TotalMilliseconds;
        	if (k.Equals(OldKeyboard) && m.Equals(OldMouse) && sinceInputBegan<Throttle) {
        		return;
        	}
        	OldMouse = m;
        	OldKeyboard = k;
        	InputBegan = now;
        	foreach (Keys key in KeyMap.Keys)
        	{
        		if (k.IsKeyDown(key))
        		{
        			KeyMap[key]();
        			return;
        		}
        	}
        	if(m.LeftButton == ButtonState.Pressed)
        	{
        		HandleClick(m.X, m.Y);
        		return;
        	}
		}
	}
	
	class DefaultControlContext : ControlContext
	{
		public DefaultControlContext() : base()
		{
			var Commands = Game.Commands;
			KeyMap[Keys.Up] = Commands.MoveNorthCommand;
			KeyMap[Keys.Down] = Commands.MoveSouthCommand;
			KeyMap[Keys.Left] = Commands.MoveWestCommand;
			KeyMap[Keys.Right] = Commands.MoveEastCommand;
			KeyMap[Keys.OemComma] = Commands.MoveUpCommand;
			KeyMap[Keys.OemPeriod] = Commands.MoveDownCommand;
			KeyMap[Keys.Space] = Commands.Wait;
			
			MenuText = new List<string>() {
				"Esc: System view.",
				"Avatar mode (Tab: Navigation mode)",
				" ",
			    "Move: NumPad/Arrows, ,/.: Up/Down.",
			    "(Control+Arrows for diagonal.)",
			    "Wait: NumPad 5 / Space.",
			    " ",
			    "Enter: Enable auto-pause.",
			    "+/-: Change speed.",
			    " ",
			    "Z: Cast spell, J: Assign job.",
			    "M: Minions, S: Structures, U: Summary.",
			    "G: Pick Up, D: Drop.",
			    "I: Inventory, E: Equip/Unequip.",
			    " ",
			    "PageUp/Down: Scroll messages.",
			    "A: Achievements, /: Toggle tutorial."
			};
			TextColors = new Dictionary<Tuple<int, int>, string>() {
				{new Tuple<int, int>(1,0), "yellow"}
			};
		}
		
		public override void ClickTile(Coord c)
		{
			if (Game.World.Tasks[c.x, c.y, c.z]==null) 
			{
				TaskEntity task = new TaskEntity("DigTask");
				task.Place(c.x, c.y, c.z);
			}
		}
	}
}