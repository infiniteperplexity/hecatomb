using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Hecatomb8
{
    // A ControlContext represents an input state or an interrelated cluster of input states for the game
    // The parent class will soon get set to abstract, but it will implement some default functionality that can be inherited
    class ControlContext
    {
        Dictionary<Keys, Action> keyMap;

        public ControlContext()
        {
            var commands = InterfaceState.Commands;
            keyMap = new Dictionary<Keys, Action>()
            {
                [Keys.W] = commands!.MoveNorthCommand,
                [Keys.A] = commands!.MoveWestCommand,
                [Keys.S] = commands!.MoveSouthCommand,
                [Keys.D] = commands!.MoveEastCommand,
                [Keys.Up] = commands!.MoveNorthCommand,
                [Keys.Left] = commands!.MoveWestCommand,
                [Keys.Down] = commands!.MoveSouthCommand,
                [Keys.Right] = commands!.MoveEastCommand
            };
        }
        public void HandleInput()
        {
            // avoid accidental desynching
            if (!InterfaceState.ReadyForInput)
            {
                return;
            }
            var m = Mouse.GetState();
            var k = Keyboard.GetState();
            var keys = k.GetPressedKeys();
            if (keys.Length>0)
            {
                foreach (var key in keys)
                {
                    if (keyMap.ContainsKey(key))
                    {
                        
                        keyMap[key]();
                        break;
                    }
                }
            }
        }


        public void PlayerHasActed()
        {
            PlayerIsReady();
        }

        public void PlayerIsReady()
        {
            var p = GameState.World!.Player;
            InterfaceState.Camera!.Center((int)p.X!, (int)p.Y!, (int)p.Z!);
            InterfaceState.ReadyForInput = true;
        }
    }
}


//public ControlContext()
//{
//	Throttle = 200;
//	if (OldControls == null)
//	{
//		OldControls = this;
//	}
//	KeyMap = new Dictionary<Keys, Action>();
//	MenuTop = new List<ColoredText>();
//	MenuMiddle = new List<ColoredText>();
//	MenuBottom = new List<ColoredText>();
//	OnTileClick = ClickTile;
//	OnTileHover = HoverTile;
//	OnMenuClick = MenuClick;
//	OnMenuHover = MenuHover;
//	OnStatusClick = StatusClick;
//	OnStatusHover = StatusHover;
//	MenuCommands = new List<(string, string)>();
//	MenuSelectable = true;
//	MenuCommands.Add(("Tutorial", "?) Tutorial"));
//	MenuCommands.Add(("Spells", "Z) Spells"));
//	MenuCommands.Add(("Jobs", "J) Jobs"));
//	MenuCommands.Add(("Log", "L) Log"));
//	MenuCommands.Add(("Research", "R) Research"));
//	MenuCommands.Add(("Achievements", "V) Achievements"));
//}

//public virtual void HandleKeyDown(Keys key)
//{
//	KeyMap[key]();
//}
//public void HandleInput()
//{
//	if (!Game.game.IsActive)
//	{
//		return;
//	}
//	var m = Mouse.GetState();
//	var k = Keyboard.GetState();
//	ControlDown = (k.IsKeyDown(Keys.LeftControl) || k.IsKeyDown(Keys.RightControl));
//	ShiftDown = (k.IsKeyDown(Keys.LeftShift) || k.IsKeyDown(Keys.RightShift));
//	DateTime now = DateTime.Now;
//	double sinceInputBegan = now.Subtract(InputBegan).TotalMilliseconds;
//	Coord c = new Coord(Game.Camera.XOffset, Game.Camera.YOffset, Game.Camera.Z);
//	if (!m.Equals(OldMouse))
//	{
//		HandleHover(m.X, m.Y);
//	}
//	// might handle these two cases separately
//	else if (!c.Equals(OldCamera))
//	{
//		CameraHover();
//	}
//	OldCamera = c;
//	//if (!Redrawn)
//	int throttle = (OldControls == this) ? Throttle : StartThrottle;
//	// we need to make it so unpressing one key does not trigger this
//	if (IsKeyboardSubset(k) && sinceInputBegan < throttle && m.LeftButton == OldMouse.LeftButton && m.RightButton == OldMouse.RightButton)
//	//if (k.Equals(OldKeyboard) && sinceInputBegan<throttle && m.LeftButton==OldMouse.LeftButton && m.RightButton==OldMouse.RightButton)
//	{
//		if (!m.Equals(OldMouse))
//		{
//			HandleHover(m.X, m.Y);
//		}
//		return;
//	}
//	Redrawn = false;

//	OldControls = this;
//	OldMouse = m;
//	Keys[] oldKeys = OldKeyboard.GetPressedKeys();
//	OldKeyboard = k;
//	InputBegan = now;
//	Keys[] keys = k.GetPressedKeys();
//	bool gotKey = false;
//	// a splash screen escapes on any key...this may not be a good way to handle it
//	if (UseKeyFallback && keys.Length > 0)
//	{
//		HandleKeyFallback();
//		gotKey = true;
//	}
//	// prioritize newly pressed keys
//	if (!gotKey)
//	{
//		foreach (Keys key in keys)
//		{
//			if (KeyMap.ContainsKey(key) && !Array.Exists(oldKeys, ky => ky == key))
//			{
//				HandleKeyDown(key);
//				gotKey = true;
//				break;
//			}
//		}
//	}
//	// then check already-pressed keys
//	if (!gotKey)
//	{
//		foreach (Keys key in keys)
//		{
//			if (KeyMap.ContainsKey(key))
//			{
//				HandleKeyDown(key);
//				gotKey = true;
//				break;
//			}
//		}
//	}

//	if (!gotKey && m.LeftButton == ButtonState.Pressed)
//	{
//		HandleClick(m.X, m.Y);
//	}
//}