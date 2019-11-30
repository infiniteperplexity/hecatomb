/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/8/2018
 * Time: 1:08 PM
 */
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using System.Collections.Generic;

namespace Hecatomb
{
	/// <summary>
	/// Description of NavigationControlContext.
	/// </summary>
	public class CameraControls : ControlContext
	{
		static int Z;
		static int XOffset;
		static int YOffset;
		
		public override void HandleKeyDown(Keys key)
		{
			base.HandleKeyDown(key);
			Camera c = Game.Camera;
			Z = c.Z;
			XOffset = c.XOffset;
			YOffset = c.YOffset;
		}
		
		
		public CameraControls() : base()
		{
			var Commands = Game.Commands;
            KeyMap[Keys.Escape] = Commands.SystemMenuCommand;
            KeyMap[Keys.Up] = Commands.MoveCameraNorth;
			KeyMap[Keys.Down] = Commands.MoveCameraSouth;
			KeyMap[Keys.Left] = Commands.MoveCameraWest;
			KeyMap[Keys.Right] = Commands.MoveCameraEast;
            KeyMap[Keys.W] = Commands.MoveCameraNorth;
            KeyMap[Keys.S] = Commands.MoveCameraSouth;
            KeyMap[Keys.A] = Commands.MoveCameraWest;
            KeyMap[Keys.D] = Commands.MoveCameraEast;
            KeyMap[Keys.Q] = Commands.MoveCameraNorthWest;
            KeyMap[Keys.E] = Commands.MoveCameraNorthEast;
            KeyMap[Keys.X] = Commands.MoveCameraSouthWest;
            KeyMap[Keys.C] = Commands.MoveCameraSouthEast;
            KeyMap[Keys.U] = Commands.ShowStructures;
            KeyMap[Keys.M] = Commands.ShowMinions;

            KeyMap[Keys.OemComma] = Commands.MoveCameraUp;
			KeyMap[Keys.OemPeriod] = Commands.MoveCameraDown;
			KeyMap[Keys.Space] = Commands.Wait;
            KeyMap[Keys.OemMinus] = Commands.SlowDown;
            KeyMap[Keys.OemPlus] = Commands.SpeedUp;

            // skip for subclasses
            if (GetType()==typeof(CameraControls))
			{
				KeyMap[Keys.Tab] = Commands.ToggleMovingCamera;
                KeyMap[Keys.J] = Commands.ChooseTask;
                KeyMap[Keys.Z] = Commands.ChooseSpell;
                RefreshContent();
            }
		}

        public override void RefreshContent()
        {
            // this stupid code means I should probably subclass an abstract class instead
            if (GetType() == typeof(CameraControls))
            {
                MenuTop = new List<ColoredText>() {
                    "Esc: Game menu.",
                    " ",
                    "{yellow}Navigate (Tab: Avatar)",
                    " "
                };
                if (Game.World != null && Game.World.Player != null)
                {
                    var p = Game.World.Player;
                    var time = Game.Time.GetTimeText();
                    MenuTop.Add(time[0]);
                    MenuTop.Add(time[1]);
                    MenuTop.Add(" ");
                    MenuTop.Add(p.GetComponent<SpellCaster>().GetSanityText());
                    if (Game.World.GetState<TaskHandler>().Minions.Count > 0)
                    {
                        MenuTop.Add(" ");
                        MenuTop.Add("Minions:");
                        var types = new Dictionary<string, int>();
                        foreach (var minion in Game.World.GetState<TaskHandler>().Minions)
                        {
                            Creature c = (Creature)minion;
                            if (!types.ContainsKey(c.TypeName))
                            {
                                types[c.TypeName] = 1;
                            }
                            else
                            {
                                types[c.TypeName] += 1;
                            }
                        }
                        foreach (var type in types.Keys)
                        {
                            var mock = Entity.Mock<Creature>(type);
                            // might need better handling for when we have multiple zombie types that still share a TypeName?
                            MenuTop.Add("{" + mock.FG + "}" + type + ": " + types[type]);
                        }
                    }

                    var stored = new List<Dictionary<string, int>>();
                    var structures = Structure.ListStructures();
                    foreach (Structure s in structures)
                    {
                        stored.Add(s.GetStored());
                    }
                    var total = Item.CombinedResources(stored);
                    if (total.Count > 0)
                    {
                        MenuTop.Add(" ");
                        MenuTop.Add("Stored resources:");
                        foreach (var res in total.Keys)
                        {
                            var r = Resource.Types[res];
                            MenuTop.Add("{" + r.ListColor + "} - " + Resource.Format((res, total[res])));
                        }
                    }
                }
            }
        }

        public override void CameraHover()
        {
            if (Cursor.X > -1)
            {
                Coord tile = new Coord(Cursor.X, Cursor.Y, Game.Camera.Z);
                OnTileHover(tile);
            }
        }
    }
}
