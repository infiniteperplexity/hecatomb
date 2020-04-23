using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Threading;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Hecatomb8
{
    using static HecatombAliases;

    public class HecatombCommands
    {

        public void Act()
        {
            //Turns.PlayerActed = true;
            //Time.Acted();
            GameState.World!.GetState<TurnHandler>().AfterPlayerActed();
        }

        public void Wait()
        {
            //CommandLogger.LogCommand(command: "Wait");
            //Player.GetComponent<Actor>().Wait();
            Act();
        }

        public void AutoWait()
        {
            Wait();
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
        public void MoveNorthEastCommand()
        {
            moveHorizontalCommand(+1, -1);
        }
        public void MoveNorthWestCommand()
        {
            moveHorizontalCommand(-1, -1);
        }
        public void MoveSouthEastCommand()
        {
            moveHorizontalCommand(+1, +1);
        }
        public void MoveSouthWestCommand()
        {
            moveHorizontalCommand(-1, +1);
        }
        public void moveHorizontalCommand(int dx, int dy)
        {
            //CommandLogger.LogCommand(command: "MoveHorizontal", x: dx, y: dy);
            Creature p = GameState.World!.Player!;
            var m = p.GetComponent<Movement>();
            //var m = p.TryComponent<Movement>();
            //if (m == var p = GameState.World!.Player;
            if (p is null || !p.Placed)
            {
                return;
            }
            // LogSomeKindOfCommand
            var (x, y, z) = ((int)p.X!, (int)p.Y!, (int)p.Z!);
            // blocking movement off the map will be handled some time in the future
            // so will blocking other kinds of illegal movement
            Coord[] moves = new Coord[]
            {
                new Coord(x + dx, y + dy, z),
                new Coord(x, y, z+1),
                new Coord(x, y, z-1)
            };
            foreach (Coord c in moves)
            {
                if (m.CanPassBounded(c.X, c.Y, c.Z))
                {
                    p.PlaceInValidEmptyTile(c.X, c.Y, c.Z);
                    Act();
                    return;
                }
            }
            
            //{
            //    return;
            //}
            //Coord[] moves = new Coord[]
            //{
            //    new Coord(x1, y1, z1),
            //    new Coord(p.X, p.Y, z1+1),
            //    new Coord(p.X, p.Y, z1-1)
            //};
            //foreach (Coord c in moves)
            //{
            //    Dictionary<string, object> details = new Dictionary<string, object>();
            //    if (m.CanPass(c.X, c.Y, c.Z))
            //    {
            //        Game.World.Events.Publish(new TutorialEvent() { Action = (c.Z == z1) ? "Move" : "Climb" });
            //        m.StepTo(c.X, c.Y, c.Z);
            //        Act();
            //        return;
            //    }
            //    Creature cr = Game.World.Creatures[c.X, c.Y, c.Z];
            //    if (cr != null && p.GetComponent<Actor>().IsFriendly(cr))
            //    {
            //        Game.World.Events.Publish(new TutorialEvent() { Action = (c.Z == z1) ? "Move" : "Climb" });
            //        m.Displace(cr);
            //        Act();
            //        return;
            //    }
            //    else if (cr != null && p.GetComponent<Actor>().IsHostile(cr))
            //    {
            //        p.GetComponent<Attacker>().Attack(cr);
            //        Act();
            //        return;
            //    }
            //}
        }

        public void moveVerticalCommand(int dz)
        {
            //CommandLogger.LogCommand(command: "MoveVertical", z: dz);
            //Creature p = Game.World!.Player!;
            //int x1 = p.X;
            //int y1 = p.Y;
            //int z1 = p.Z + dz;
            //var m = p.TryComponent<Movement>();
            //if (m == null)
            //{
            //    return;
            //}
            //if (!m.CanPass(x1, y1, z1))
            //{
            //    Creature cr = Game.World.Creatures[x1, y1, z1];
            //    if (cr != null && p.GetComponent<Actor>().IsFriendly(cr))
            //    {
            //        Game.World.Events.Publish(new TutorialEvent() { Action = "Climb" });
            //        m.Displace(cr);
            //        Act();
            //    }
            //    else if (cr != null && p.GetComponent<Actor>().IsHostile(cr))
            //    {
            //        p.GetComponent<Attacker>().Attack(cr);
            //        Act();
            //    }

            //    return;

            //}
            //else
            //{
            //    Game.World.Events.Publish(new TutorialEvent() { Action = "Climb" });
            //    m.StepTo(x1, y1, z1);
            //    Act();
            //    return;
            //}
        }

        public void MoveCameraNorth()
        {
            moveCameraHorizontal(+0, -1);
        }
        public void MoveCameraSouth()
        {
            moveCameraHorizontal(+0, +1);
        }
        public void MoveCameraEast()
        {
            moveCameraHorizontal(+1, +0);
        }
        public void MoveCameraWest()
        {
            moveCameraHorizontal(-1, +0);
        }
        public void MoveCameraUp()
        {
            moveCameraVertical(+1);
        }
        public void MoveCameraDown()
        {
            moveCameraVertical(-1);
        }
        public void MoveCameraNorthWest()
        {
            moveCameraHorizontal(-1, -1);
        }
        public void MoveCameraNorthEast()
        {
            moveCameraHorizontal(+1, -1);
        }
        public void MoveCameraSouthEast()
        {
            moveCameraHorizontal(+1, +1);
        }
        public void MoveCameraSouthWest()
        {
            moveCameraHorizontal(-1, +1);
        }


        private void moveCameraVertical(int dz)
        {
            if (ControlContext.ShiftDown)
            {
                dz *= 10;
            }
            var camera = InterfaceState.Camera!;
            camera.Z = Math.Max(Math.Min(camera.Z + dz, GameState.World!.Depth - 2), 1);
            //ControlContext.CenterCursor();
            InterfaceState.DirtifyMainPanel();
            InterfaceState.DirtifyTextPanels();
        }
        private void moveCameraHorizontal(int dx, int dy)
        {
            if (ControlContext.ShiftDown)
            {
                dx *= 10;
                dy *= 10;
            }
            Camera c = InterfaceState.Camera!;
            int xhalf = c.Width / 2;
            int yhalf = c.Height / 2;
            c.XOffset = Math.Min(Math.Max(1, c.XOffset + dx), GameState.World!.Width - c.Width - 1);
            c.YOffset = Math.Min(Math.Max(1, c.YOffset + dy), GameState.World!.Height - c.Height - 1);
            //ControlContext.CenterCursor();
            InterfaceState.DirtifyMainPanel();
            InterfaceState.DirtifyTextPanels();
        }

        public void ToggleMovingCamera()
        {
            if (!InterfaceState.MovingCamera)
            {
                //Game.World.Events.Publish(new TutorialEvent() { Action = "CameraMode" });
            }
            else
            {
                //Game.World.Events.Publish(new TutorialEvent() { Action = "MainMode" });
            }
            InterfaceState.MovingCamera = !InterfaceState.MovingCamera;
            if (InterfaceState.MovingCamera)
            {
                InterfaceState.SetControls(InterfaceState.CameraControls!);
            }
            else
            {
                InterfaceState.SetControls(InterfaceState.DefaultControls!);
            }
            Creature p = GameState.World!.Player!;
            InterfaceState.Camera!.Center((int)p.X!, (int)p.Y!, (int)p.Z!);
            InterfaceState.DirtifyMainPanel();
            InterfaceState.DirtifyTextPanels();
        }


        public void ChooseTask()
        {
            //Game.World.Events.Publish(new TutorialEvent() { Action = "ShowJobs" });
            var tasks = GameState.World!.GetState<TaskHandler>();
            //tasks.PurgeCache();
            InterfaceState.SetControls(new MenuChoiceControls(tasks));
            //Game.Controls.MenuSelectable = false;
            //Game.Controls.SelectedMenuCommand = "Jobs";
            InterfaceState.DirtifyTextPanels();
        }

        public void ChooseSpell()
        {
            //Game.World.Events.Publish(new TutorialEvent() { Action = "ShowSpells" });
            var spells = Player.GetComponent<SpellCaster>();
            InterfaceState.SetControls(new MenuChoiceControls(spells));
            //Game.Controls.MenuSelectable = false;
            //Game.Controls.SelectedMenuCommand = "Spells";
            InterfaceState.DirtifyTextPanels();
        }
    }
}