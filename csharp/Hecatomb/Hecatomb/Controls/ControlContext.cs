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
        static Coord OldCamera;
        static DateTime InputBegan;
        static Highlight Cursor = new Highlight("cyan");
        public static bool MovingCamera;
        public const int Throttle = 200;
        public static bool Redrawn = false;
        public Dictionary <Keys, Action> KeyMap;
        public List<string> MenuTop;
        public TextColors TopColors;
        public List<string> MenuMiddle;
        public TextColors MiddleColors;
        public List<string> MenuBottom;
        public TextColors BottomColors;
        public Action<Coord> OnTileClick;
        public Action<Coord> OnTileHover;
        public Action<int, int> OnMenuHover;
        public Action<int, int> OnMenuClick;
        public Action<int, int> OnStatusClick;
        public Action<int, int> OnStatusHover;
        public static TextColors ValidColor = new TextColors(0,"lime green");
        public static TextColors InvalidColor = new TextColors(0,"orange");
           
        
        public void Set(ControlContext c)
        {
        	Game.LastControls = Game.Controls;
        	Game.Controls = c;
        	Game.MenuPanel.Dirty = true;
        	Game.MainPanel.Dirty = true;
            Game.StatusPanel.Dirty = true;
        }
        
        public void Reset()
        {
        	Game.Controls = (MovingCamera) ? Game.CameraControls : Game.DefaultControls;
        	Game.LastControls = Game.Controls;	
        	Game.MenuPanel.Dirty = true;
        	Game.MainPanel.Dirty = true;
            Game.StatusPanel.Dirty = true;
        }
        
        public virtual void Back()
        {
        	Game.Controls = Game.LastControls;
        	Game.LastControls = (MovingCamera) ? Game.CameraControls : Game.DefaultControls;
        	Game.MenuPanel.Dirty = true;
        	Game.MainPanel.Dirty = true;
        }
        
        
        public void Nothing(int x, int y) {}
        
        public void Nothing(Coord c) {}
        
        public void HandleClick(int x, int y)
        {
        	if (x>Game.MainPanel.Size && y>Game.MainPanel.Size && x<Game.MenuPanel.X0-Game.MainPanel.Size && y<Game.StatusPanel.Y0-Game.MainPanel.Size)
        	{
         		int Size = Game.MainPanel.Size;
	        	int Padding = Game.MainPanel.Padding;
	        	GameCamera Camera = Game.Camera;
	        	Coord tile = new Coord((x-Padding)/(Size+Padding)-1+Camera.XOffset,(y-Padding)/(Size+Padding)-1+Camera.YOffset,Camera.Z);
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
        	if (Cursor.X>-1)
        	{
        		Game.MainPanel.DirtifyTile(Cursor.X, Cursor.Y, Cursor.Z);
        		Cursor.Remove();
        	}
        	if (x>Game.MainPanel.Size && y>Game.MainPanel.Size && x<Game.MenuPanel.X0-Game.MainPanel.Size && y<Game.StatusPanel.Y0-Game.MainPanel.Size)
        	{
         		int Size = Game.MainPanel.Size;
	        	int Padding = Game.MainPanel.Padding;
	        	GameCamera Camera = Game.Camera;
	        	Coord tile = new Coord((x-Padding)/(Size+Padding)-1+Camera.XOffset,(y-Padding)/(Size+Padding)-1+Camera.YOffset,Camera.Z);
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
        	Debug.Print("Clicked on tile at {0} {1}", c.X, c.Y);
        	Nothing(c.X, c.Y);
        }
        
         public virtual void HoverTile(Coord c)
        {
        	Cursor.Place(c.X, c.Y, c.Z);
        	Game.MainPanel.DirtifyTile(c);
        	Game.World.ShowTileDetails(c);
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
        	OldCamera = new Coord(-1, -1, -1);
        	InputBegan = DateTime.Now;
        }
		public ControlContext()
		{
			KeyMap = new Dictionary<Keys, Action>();
			MenuTop = new List<string>();
			TopColors = new TextColors();
			MenuMiddle = new List<string>();
			MiddleColors = new TextColors();
			MenuBottom = new List<string>();
			BottomColors = new TextColors();
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
        	Coord c = new Coord(Game.Camera.XOffset, Game.Camera.YOffset, Game.Camera.Z);
        	if (!m.Equals(OldMouse) || !c.Equals(OldCamera))
        	{
        		HandleHover(m.X, m.Y);
        	}
        	OldCamera = c;
            //if (!Redrawn)
            if (k.Equals(OldKeyboard) && m.Equals(OldMouse) && sinceInputBegan<Throttle)
            {
                if (!m.Equals(OldMouse))
	        	{
	        		HandleHover(m.X, m.Y);
	        	}
        		return;
        	}
            Redrawn = false;

        	OldMouse = m;
        	OldKeyboard = k;
        	InputBegan = now;
        	Keys[] keys = k.GetPressedKeys();
        	bool gotKey = false;
        	foreach (Keys key in keys)
        	{
        		if (KeyMap.ContainsKey(key))
        		{
        			HandleKeyDown(key);
        			gotKey = true;
        			break;
        		}
        	}
        	if(!gotKey && m.LeftButton == ButtonState.Pressed)
        	{
        		HandleClick(m.X, m.Y);
        	}
		}
	}	
}