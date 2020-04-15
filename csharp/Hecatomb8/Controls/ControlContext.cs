using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Hecatomb8
{
    class ControlContext
    {
        Dictionary<Keys, Action> keyMap;

        public ControlContext()
        {
            keyMap = new Dictionary<Keys, Action>()
            {
                [Keys.W] = () => MovePlayer(0, -1, 0),
                [Keys.A] = () => MovePlayer(-1, 0, 0),
                [Keys.S] = () => MovePlayer(0, +1, 0),
                [Keys.D] = () => MovePlayer(+1, 0, 0)
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

        public void MovePlayer(int dx, int dy, int dz)
        {
            var p = GameState.Player;
            if (p is null || !p.Placed)
            {
                return;
            }
            // LogSomeKindOfCommand
            var (x, y, z) = ((int)p.X!, (int)p.Y!, (int)p.Z!);
            // blocking movement off the map will be handled some time in the future
            // so will blocking other kinds of illegal movement
            p.PlaceInEmptyTile(new Constrained<int>((int)p.X! + dx), new Constrained<int>((int)p.Y! + dy), new Constrained<int>((int)p.Z! + dz));
            PlayerActed();
        }

        public void PlayerActed()
        {
            PlayerReady();
        }

        public void PlayerReady()
        {
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