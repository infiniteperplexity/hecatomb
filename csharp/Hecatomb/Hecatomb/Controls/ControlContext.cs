﻿/*
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
        static ControlContext OldControls;
        static DateTime InputBegan;
        public static Highlight Cursor = new Highlight("cyan");
        public static bool ControlDown = false;
        public static bool ShiftDown = false;
        public static bool MovingCamera;
        public const int Throttle = 200;
        public const int StartThrottle = 750;
        public static bool Redrawn = false;
        public Dictionary <Keys, Action> KeyMap;
        public List<ColoredText> MenuTop;
        public List<ColoredText> MenuMiddle;
        public List<ColoredText> MenuBottom;
        public Action<Coord> OnTileClick;
        public Action<Coord> OnTileHover;
        public Action<int, int> OnMenuHover;
        public Action<int, int> OnMenuClick;
        public Action<int, int> OnStatusClick;
        public Action<int, int> OnStatusHover;
        public ImageOverride ImageOverride;
        public bool UseKeyFallback;
        
        public void Set(ControlContext c)
        {
            Game.Controls.CleanUp();
        	Game.LastControls = Game.Controls;
        	Game.Controls = c;
        	Game.MenuPanel.Dirty = true;
        	Game.MainPanel.Dirty = true;
            Game.StatusPanel.Dirty = true;
        }

        public void Reset()
        {
            var old = Game.Controls;
            Game.Controls.CleanUp();
            Game.Controls = (MovingCamera) ? Game.CameraControls : Game.DefaultControls;
            if (Game.World != null)
            {
                Game.World.Events.Publish(new ContextChangeEvent() { Note = "Reset", OldContext = old, NewContext = Game.Controls });
                Game.World.Events.Publish(new TutorialEvent() { Action = "Cancel" });
            }
            Game.LastControls = Game.Controls;	
        	Game.MenuPanel.Dirty = true;
        	Game.MainPanel.Dirty = true;
            Game.StatusPanel.Dirty = true;
            Game.SplashPanel.Active = false;
            Game.Time.Frozen = false;
        }
        
        public virtual void Back()
        {
            var old = Game.Controls;
            Game.Controls = Game.LastControls;
            if (Game.World!=null)
            {
                Game.World.Events.Publish(new ContextChangeEvent() { Note = "Back", OldContext = old, NewContext = Game.Controls });
                Game.World.Events.Publish(new TutorialEvent() { Action = "Cancel" });
            }  
            Game.LastControls = (MovingCamera) ? Game.CameraControls : Game.DefaultControls;          
            Game.MenuPanel.Dirty = true;
        	Game.MainPanel.Dirty = true;
            //Game.ForegroundPanel.Active = false;
        }
        
        
        public void Nothing(int x, int y) {}
        
        public void Nothing(Coord c) {}
        
        public virtual void HandleClick(int x, int y)
        {
        	if (x>Game.MainPanel.Size && y>Game.MainPanel.Size && x<Game.MenuPanel.X0-Game.MainPanel.Size && y<Game.StatusPanel.Y0-Game.MainPanel.Size)
        	{
         		int Size = Game.MainPanel.Size;
	        	int Padding = Game.MainPanel.Padding;
	        	Camera Camera = Game.Camera;
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
        

        public virtual void CameraHover()
        {
            Coord tile = new Coord(Cursor.X, Cursor.Y, Game.Camera.Z);
            OnTileHover(tile);
        }
        public virtual void HandleHover(int x, int y)
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
	        	Camera Camera = Game.Camera;
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
            if (Game.World==null)
            {
                return;
            }
            var (x, y, z) = c;
            Creature cr = Game.World.Creatures[x, y, z];
            bool visible = Game.Visible.Contains(c);
            if (cr != null && visible)
            {
                Game.Controls.Set(new MenuChoiceControls(cr));
            }
            Feature fr = Game.World.Features[x, y, z];
            if (fr?.TryComponent<StructuralComponent>() != null)
            {
                Game.Controls.Set(new MenuChoiceControls(fr.GetComponent<StructuralComponent>().Structure.Unbox()));
            }
        }

        public virtual void HoverTile(Coord c)
        {
            if (Game.World != null)
            {
                Cursor.Place(c.X, c.Y, c.Z);
                Game.MainPanel.DirtifyTile(c);
                Game.World.ShowTileDetails(c);
            }
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
            if (OldControls==null)
            {
                OldControls = this;
            }
			KeyMap = new Dictionary<Keys, Action>();
			MenuTop = new List<ColoredText>();
			MenuMiddle = new List<ColoredText>();
			MenuBottom = new List<ColoredText>();
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
            ControlDown = (k.IsKeyDown(Keys.LeftControl) || k.IsKeyDown(Keys.RightControl));
            ShiftDown = (k.IsKeyDown(Keys.LeftShift) || k.IsKeyDown(Keys.RightShift));
            DateTime now = DateTime.Now;
        	double sinceInputBegan = now.Subtract(InputBegan).TotalMilliseconds;
        	Coord c = new Coord(Game.Camera.XOffset, Game.Camera.YOffset, Game.Camera.Z);
        	if (!m.Equals(OldMouse))
        	{
        		HandleHover(m.X, m.Y);
        	}
            // might handle these two cases separately
            else if (!c.Equals(OldCamera))
            {
                CameraHover();
            }
        	OldCamera = c;
            //if (!Redrawn)
            int throttle = (OldControls==this) ? Throttle : StartThrottle;
            if (k.Equals(OldKeyboard) && sinceInputBegan<throttle && m.LeftButton==OldMouse.LeftButton && m.RightButton==OldMouse.RightButton)
            {
                if (!m.Equals(OldMouse))
	        	{
	        		HandleHover(m.X, m.Y);
	        	}
        		return;
        	}
            Redrawn = false;

            OldControls = this;
        	OldMouse = m;
        	OldKeyboard = k;
        	InputBegan = now;
        	Keys[] keys = k.GetPressedKeys();
        	bool gotKey = false;
        	foreach (Keys key in keys)
        	{
                if (UseKeyFallback)
                {
                    HandleKeyFallback();
                    gotKey = true;
                    break;
                }
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

        public virtual void HandleKeyFallback()
        {

        }

        public virtual void RefreshContent()
        {

        }

        public virtual void CleanUp()
        {

        }

        public static void CenterCursor()
        {
            Cursor.Place(Game.Camera.XOffset + 13, Game.Camera.YOffset + 13, Game.Camera.Z);

        }

        public virtual void SelectTile()
        {
            Camera Camera = Game.Camera;
            Coord tile = new Coord(Cursor.X, Cursor.Y, Camera.Z);   
            ClickTile(tile);
        }
    }	
}