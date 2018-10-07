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
        static Particle Cursor = new CursorParticle();
        public const int Throttle = 125;
        public Dictionary <Keys, Action> KeyMap;
        public List<string> MenuText;
        public Dictionary<Tuple<int, int>, string> TextColors;
        public Action<Coord> OnTileClick;
        public Action<Coord> OnTileHover;
        public Action<int, int> OnMenuHover;
        public Action<int, int> OnMenuClick;
        public Action<int, int> OnStatusClick;
        public Action<int, int> OnStatusHover;
           
        public void Nothing(int x, int y) {}
        
        public void HandleClick(int x, int y)
        {
        	if (x>Game.MainPanel.Size && y>Game.MainPanel.Size && x<Game.MenuPanel.X0-Game.MainPanel.Size && y<Game.StatusPanel.Y0-Game.MainPanel.Size)
        	{
         		int Size = Game.MainPanel.Size;
	        	int Padding = Game.MainPanel.Padding;
	        	GameCamera Camera = Game.Camera;
	        	Coord tile = new Coord((x-Padding)/(Size+Padding)-1+Camera.XOffset,(y-Padding)/(Size+Padding)-1+Camera.YOffset,Camera.z);
	        	OnTileClick(tile);
        	}
        	else if (x>=Game.MenuPanel.X0) 
        	{
        		OnMenuClick(x, y);
        	}
        	else if (y>=Game.StatusPanel.Y0)
        	{
        		OnStatusClick(x, y);
        	}
        	else
        	{
        		Nothing(x, y);
        	}
        }
        
        public void HandleHover(int x, int y)
        {
        	if (Cursor.x>-1)
        	{
        		Game.MainPanel.DirtifyTile(Cursor.x, Cursor.y, Cursor.z);
        		Cursor.Remove();
        	}
        	if (x>Game.MainPanel.Size && y>Game.MainPanel.Size && x<Game.MenuPanel.X0-Game.MainPanel.Size && y<Game.StatusPanel.Y0-Game.MainPanel.Size)
        	{
         		int Size = Game.MainPanel.Size;
	        	int Padding = Game.MainPanel.Padding;
	        	GameCamera Camera = Game.Camera;
	        	Coord tile = new Coord((x-Padding)/(Size+Padding)-1+Camera.XOffset,(y-Padding)/(Size+Padding)-1+Camera.YOffset,Camera.z);
	        	OnTileHover(tile);
        	} else if (x>=Game.MenuPanel.X0)
        	{
        		OnMenuHover(x, y);
        	}
        	else if (y>=Game.StatusPanel.Y0)
        	{
        		OnStatusHover(x, y);
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
        
         public virtual void HoverTile(Coord c)
        {
        	Cursor.Place(c.x, c.y, c.z);
        	Game.MainPanel.DirtifyTile(c);
        }
        
        public virtual void MenuClick(int x, int y)
        {
        	Debug.Print("Clicked on the menu at {0} {1}", x, y);
        }
        
        public virtual void StatusClick(int x, int y)
        {
        	Debug.Print("Clicked on the status bar at {0} {1}", x, y);
        }
        
        public virtual void MenuHover(int x, int y)
        {
        	Nothing(x, y);
        }
        
        public virtual void StatusHover(int x, int y)
        {
        	Nothing(x, y);
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
			OnTileClick = ClickTile;
			OnTileHover = HoverTile;
			OnMenuClick = MenuClick;
			OnMenuHover = MenuHover;
			OnStatusClick = StatusClick;
			OnStatusHover = StatusHover;
		}
		
		public virtual void HandleKeyDown(Keys key)
		{
			KeyMap[key]();		
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
        	if (!m.Equals(OldMouse))
        	{
        		HandleHover(m.X, m.Y);
        	}
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
        			HandleKeyDown(key);
        			return;
        		}
        	}
        	
        	if(m.LeftButton == ButtonState.Pressed)
        	{
        		HandleClick(m.X, m.Y);
        		return;
        	}
		}
		
		public void SelectSquareZone()
		{
			
		}
		
		public void SelectBox()
		{
			
		}
		
		public void SelectSquare()
		{
			
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
				Game.GraphicsDirty = true;
			}
		}
	}
	
	public class NavigatorContext : ControlContext
	{
		static int Z;
		static int XOffset;
		static int YOffset;
		
		public override void HandleKeyDown(Keys key)
		{
			base.HandleKeyDown(key);
			GameCamera c = Game.Camera;
			Z = c.z;
			XOffset = c.XOffset;
			YOffset = c.YOffset;
		}
		
		
		public NavigatorContext() : base()
		{
			var Commands = Game.Commands;
			KeyMap[Keys.Up] = Commands.MoveCameraNorth;
			KeyMap[Keys.Down] = Commands.MoveCameraSouth;
			KeyMap[Keys.Left] = Commands.MoveCameraWest;
			KeyMap[Keys.Right] = Commands.MoveCameraEast;
			KeyMap[Keys.OemComma] = Commands.MoveCameraUp;
			KeyMap[Keys.OemPeriod] = Commands.MoveCameraDown;
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
	}
	
	public class MenuChoiceContext : ControlContext
	{
		public static List<Keys> Alphabet = new List<Keys> {
			Keys.A,
			Keys.B,
			Keys.C,
			Keys.D,
			Keys.E,
			Keys.F,
			Keys.G,
			Keys.H,
			Keys.I,
			Keys.J,
			Keys.K,
			Keys.L,
			Keys.M,
			Keys.N,
			Keys.O,
			Keys.P,
			Keys.Q,
			Keys.R,
			Keys.S,
			Keys.T,
			Keys.U,
			Keys.V,
			Keys.W,
			Keys.X,
			Keys.Y,
			Keys.Z
		};
		
		static string alphabet = "abcdefghijklmnopqrstuvwxyz";
		public List<string> Choices;
		
		public MenuChoiceContext(List<Action> choices): base()
		{
			var Commands = Game.Commands;
			KeyMap[Keys.Space] = Commands.Wait;
			MenuText = new List<string>() {
				"Esc: System view.",
				"Choose a thing:"
		    };
			for (int i=0; i<choices.Count; i++)
			{
				KeyMap[Alphabet[i]] = choices[i];
				MenuText.Add(alphabet[i] + ") how do we figure this out?");
			}
		}
	}
}