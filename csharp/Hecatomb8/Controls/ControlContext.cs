using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Hecatomb8
{
    using static HecatombAliases;
    // A ControlContext represents an input state or an interrelated cluster of input states for the game
    // The parent class will soon get set to abstract, but it will implement some default functionality that can be inherited
    public abstract class ControlContext
    {

        static KeyboardState OldKeyboard;
        static MouseState OldMouse;
        public static bool ControlDown;
        public static bool ShiftDown;
        static DateTime InputBegan;
        static ControlContext? LastInputCycleControls;
        static Coord? LastInputCycleCamera;
        
        


        protected Dictionary<Keys, Action> keyMap;
        public List<ColoredText> MenuTop;
        // throttle input between keys
        int Throttle = 200;
        // throttle input after switching contexts
        int StartThrottle = 750;
        protected bool HasKeyDefault = false;
        public bool AllowsUnpause = true;

        static ControlContext()
        {
            //Cursor = new Highlight("cyan");
            OldKeyboard = Keyboard.GetState();
            OldMouse = Mouse.GetState();
            InputBegan = DateTime.Now;
        }

        public ControlContext()
        {
            keyMap = new Dictionary<Keys, Action>();
            MenuTop = new List<ColoredText>();
        }

        public virtual void RefreshContent()
        {

        }

        public void HandleInput()
        {
            // avoid accidental desynching...although is this necessary because of the main Update loop?
            //if (!InterfaceState.ReadyForInput)
            //{
            //    return;
            //}
            var m = Mouse.GetState();
            var k = Keyboard.GetState();
            ControlDown = (k.IsKeyDown(Keys.LeftControl) || k.IsKeyDown(Keys.RightControl));
            ShiftDown = (k.IsKeyDown(Keys.LeftShift) || k.IsKeyDown(Keys.RightShift));
            DateTime now = DateTime.Now;
            double sinceInputBegan = now.Subtract(InputBegan).TotalMilliseconds;
            // this should be safe even if the world has not be created
            Coord c = new Coord(InterfaceState.Camera!.XOffset, InterfaceState.Camera!.YOffset, InterfaceState.Camera!.Z);
            if (!m.Equals(OldMouse))
            {
                HandleHover(m.X, m.Y);
            }
            // might handle these two cases separately
            else if (!c.Equals(LastInputCycleCamera))
            {
                CameraHover();
            }
            LastInputCycleCamera = c;
            int throttle = (LastInputCycleControls == this) ? Throttle : StartThrottle;
            if (IsKeyboardSubset(k) && sinceInputBegan < throttle && m.LeftButton == OldMouse.LeftButton && m.RightButton == OldMouse.RightButton)
            {
                if (!m.Equals(OldMouse))
                {
                    HandleHover(m.X, m.Y);
                }
                return;
            }
            LastInputCycleControls = this;
            OldMouse = m;
            Keys[] oldKeys = OldKeyboard.GetPressedKeys();
            OldKeyboard = k;

            InputBegan = now;
            Keys[] keys = k.GetPressedKeys();
            bool gotKey = false;
            // a splash screen escapes on any key...this may not be a good way to handle it
            if (HasKeyDefault && keys.Length > 0)
            {
                HandleKeyDefault();
                gotKey = true;
            }
            // prioritize newly pressed keys
            if (!gotKey)
            {
                foreach (Keys key in keys)
                {
                    if (keyMap.ContainsKey(key) && !Array.Exists(oldKeys, ky => ky == key))
                    {
                        HandleKeyDown(key);
                        gotKey = true;
                        break;
                    }
                }
            }
            // then check already-pressed keys
            if (!gotKey)
            {
                foreach (Keys key in keys)
                {
                    if (keyMap.ContainsKey(key))
                    {
                        HandleKeyDown(key);
                        gotKey = true;
                        break;
                    }
                }
            }

            if (!gotKey && m.LeftButton == ButtonState.Pressed)
            {
                HandleClick(m.X, m.Y);
            }
        }

        public static bool IsKeyboardSubset(KeyboardState k)
        {
            var oldKeys = OldKeyboard.GetPressedKeys();
            foreach (var key in k.GetPressedKeys())
            {
                if (Array.IndexOf(oldKeys, key) == -1)
                {
                    return false;
                }
            }
            return true;
        }
        public virtual void HandleKeyDown(Keys key)
        {
            keyMap[key]();
        }

        public virtual void HandleKeyDefault()
        {

        }

        public virtual void HandleClick(int x, int y)
        {
            var panel = InterfaceState.GetPanel(x, y);
            if (panel is null)
            {
                return;
            }
            if (panel is MainPanel)
            {
                int Size = InterfaceState.MainPanel.CharWidth;
                int Padding = InterfaceState.MainPanel.XPad;
                Camera Camera = InterfaceState.Camera!;
                if ((x - panel.X0 - Padding) / (Size + Padding) < Camera.Width)
                {
                    Coord tile = new Coord((x - panel.X0 - Padding) / (Size + Padding) + Camera.XOffset, (y - panel.Y0 - Padding) / (Size + Padding) + Camera.YOffset, Camera.Z);
                    ClickTile(tile);
                }
            }

        }

        public virtual void HandleHover(int x, int y)
        {
            var panel = InterfaceState.GetPanel(x, y);
            if (panel is MainPanel)
            {
                int Size = InterfaceState.MainPanel.CharWidth;
                int Padding = InterfaceState.MainPanel.XPad;
                Camera Camera = InterfaceState.Camera!;
                if ((x - panel.X0 - Padding) / (Size + Padding) < Camera.Width)
                {
                    Coord tile = new Coord((x - panel.X0 - Padding) / (Size + Padding) + Camera.XOffset, (y - panel.Y0 - Padding) / (Size + Padding) + Camera.YOffset, Camera.Z);
                    HoverTile(tile);
                }
            }
            else
            {
                Debug.WriteLine("here we are");
                InterfaceState.Cursor = null;
            }
        }

        public virtual void CameraHover()
        {
           
        }

        public virtual void ClickTile(Coord c)
        {
            if (GameState.World is null)
            {
                return;
            }
            var (x, y, z) = c;
            Creature? cr = Creatures.GetWithBoundsChecked(x, y, z);
            bool visible = InterfaceState.PlayerVisible.Contains(c);
            // this functionality should probably be defined in HecatombCommands
            if (cr != null && visible)
            {
                //ControlContext.Set(new MenuChoiceControls(cr));
                //ControlContext.Set(new MenuCameraControls(cr));
                //Game.Camera.CenterOnSelection();
                //return;
            }
            Feature? fr = GameState.World!.Features.GetWithBoundsChecked(x, y, z);
            //if (fr?.TryComponent<StructuralComponent>() != null)
            //{
            //    if (fr.GetComponent<StructuralComponent>().Structure.Placed)
            //    {
            //        var s = fr.GetComponent<StructuralComponent>().Structure.Unbox();
            //        ControlContext.Set(new MenuCameraControls(s));
            //        Game.Camera.CenterOnSelection();
            //        //ControlContext.Set(new MenuChoiceControls(fr.GetComponent<StructuralComponent>().Structure.Unbox()));
            //    }
            //    return;
            //}
            Debug.WriteLine($"Clicked {c.X} {c.Y} {c.Z}");
        }

        public virtual void HoverTile(Coord c)
        {
            if (GameState.World != null)
            {
                
                if (InterfaceState.Cursor != c)
                {
                    InterfaceState.Cursor = c;
                    InterfaceState.DirtifyMainPanel();
                }
                //Cursor.Place(c.X, c.Y, c.Z);
                //Game.MainPanel.DirtifyTile(c);
                //Game.World.ShowTileDetails(c);
            }
        }

    }
}