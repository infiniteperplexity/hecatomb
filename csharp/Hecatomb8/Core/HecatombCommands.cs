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
            GameState.World!.GetState<TurnHandler>().AfterPlayerActed();
        }

        public void Wait()
        {
            CommandLogger.LogCommand(command: "Wait");
            Player.GetComponent<Actor>().Wait();
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
            CommandLogger.LogCommand(command: "MoveHorizontal", x: dx, y: dy);
            Creature p = GameState.World!.Player!;
            var m = p.GetComponent<Movement>();
            if (p is null || !p.Placed)
            {
                return;
            }
            // LogSomeKindOfCommand
            var (x, y, z) = ((int)p.X!, (int)p.Y!, (int)p.Z!);
            // so will blocking other kinds of illegal movement
            Coord[] moves = new Coord[]
            {
                new Coord(x + dx, y + dy, z),
                new Coord(x, y, z+1),
                new Coord(x, y, z-1)
            };
            //foreach (Coord c in moves)
            //{
            //    if (m.CanPassBounded(c.X, c.Y, c.Z))
            //    {
            //        Publish(new TutorialEvent() { Action = (c.Z == p.Z) ? "Move" : "Climb" });
            //        m.StepToValidEmptyTile(c.X, c.Y, c.Z);
            //        Act();
            //        return;
            //    }
            //}

            foreach (Coord c in moves)
            {
                if (m.CanPassBounded(c.X, c.Y, c.Z))
                {
                    Publish(new TutorialEvent() { Action = (c.Z == p.Z) ? "Move" : "Climb" });
                    m.StepToValidEmptyTile(c.X, c.Y, c.Z);
                    Act();
                    return;
                }
                Creature? cr = Creatures.GetWithBoundsChecked(c.X, c.Y, c.Z);
                if (cr != null && p.GetComponent<Actor>().IsFriendly(cr))
                {
                    Publish(new TutorialEvent() { Action = (c.Z == p.Z) ? "Move" : "Climb" });
                    m.Displace(cr);
                    Act();
                    return;
                }
                else if (cr != null && p.GetComponent<Actor>().IsHostile(cr))
                {
                    p.GetComponent<Attacker>().Attack(cr);
                    Act();
                    return;
                }
            }
        }

        public void moveVerticalCommand(int dz)
        {
            CommandLogger.LogCommand(command: "MoveVertical", z: dz);
            Creature p = Player;
            var (x1, y1, z1) = p.GetValidCoordinate();
            var m = p.GetComponent<Movement>();
            if (!m.CanPassBounded(x1, y1, z1))
            {
                Creature? cr = Creatures.GetWithBoundsChecked(x1, y1, z1);
                if (cr != null && p.GetComponent<Actor>().IsFriendly(cr))
                {
                    Publish(new TutorialEvent() { Action = "Climb" });
                    m.Displace(cr);
                    Act();
                }
                else if (cr != null && p.GetComponent<Actor>().IsHostile(cr))
                {
                    p.GetComponent<Attacker>().Attack(cr);
                    Act();
                }

                return;

            }
            else
            {
                Publish(new TutorialEvent() { Action = "Climb" });
                m.StepToValidEmptyTile(x1, y1, z1 + dz);
                Act();
                return;
            }
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
            InterfaceState.CenterCursor();
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
            InterfaceState.CenterCursor();
            InterfaceState.DirtifyMainPanel();
            InterfaceState.DirtifyTextPanels();
        }

        public void ToggleMovingCamera()
        {
            if (!InterfaceState.MovingCamera)
            {
                Publish(new TutorialEvent() { Action = "CameraMode" });
            }
            else
            {
                Publish(new TutorialEvent() { Action = "MainMode" });
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
            Publish(new TutorialEvent() { Action = "ShowJobs" });
            var tasks = GameState.World!.GetState<TaskHandler>();
            //tasks.PurgeCache();
            InterfaceState.SetControls(new InfoDisplayControls(tasks));
            //Game.Controls.MenuSelectable = false;
            InterfaceState.Controls.SelectedMenuCommand = "Jobs";
            InterfaceState.DirtifyTextPanels();
        }

        public void ChooseSpell()
        {
            Publish(new TutorialEvent() { Action = "ShowSpells" });
            var spells = Player.GetComponent<SpellCaster>();
            InterfaceState.SetControls(new InfoDisplayControls(spells));
            //Game.Controls.MenuSelectable = false;
            InterfaceState.Controls.SelectedMenuCommand = "Spells";
            InterfaceState.DirtifyTextPanels();
        }

        public void SystemMenuCommand()
        {
            //Time.Frozen = true;
            var commands = new List<(Keys, ColoredText, Action)>() {
                (Keys.Escape, "Cancel.", InterfaceState.ResetControls),
                (Keys.S, "Save game.", GameManager.SaveGameCheckFileName),
                (Keys.A, "Save as...", GameManager.SaveGameAs),
                //(Keys.R, "Restore game.", GameManager.RestoreGameWithConfirmation),
                (Keys.R, "Restore game.", GameManager.RestoreGame),
                (Keys.N, "New game.", GameManager.StartGameWithConfirmation),
                (Keys.Q, "Quit.", GameManager.BackToTitleWithConfirmation)
            };
            //if (Options.ReconstructGames)
            //{
            //    commands.Add((Keys.C, "Reconstruct game from log.", ReconstructGameCommand));
            //}
            InterfaceState.SetControls(new StaticMenuControls(" ", commands));
        }

        public void ShowStructures()
        {
            var structures = Structure.ListStructures();
            if (structures.Count > 0)
            {
                InterfaceState.SetControls(new InfoDisplayControls(structures[0]));
                InterfaceState.Camera!.CenterOnSelection();
            }
        }

        public void ShowMinions()
        {
            var minions = GetState<TaskHandler>().GetMinions();
            if (minions.Count > 0)
            {
                InterfaceState.SetControls(new InfoDisplayControls((Creature)minions[0]));
                InterfaceState.Camera!.CenterOnSelection();
            }
            InterfaceState.SetControls(new InfoDisplayControls(Player));
        }

        public void ShowAchievements()
        {
            Publish(new TutorialEvent() { Action = "ShowAchievements" });
            var controls = new InfoDisplayControls(GetState<AchievementHandler>());
            controls.MenuCommandsSelectable = true;
            controls.SelectedMenuCommand = "Achievements";
            InterfaceState.SetControls(controls);
            InterfaceState.DirtifyTextPanels();
        }

        public void ShowResearch()
        {
            var controls = new InfoDisplayControls(GetState<ResearchHandler>());
            controls.MenuCommandsSelectable = true;
            controls.SelectedMenuCommand = "Research";
            InterfaceState.SetControls(controls);
            InterfaceState.DirtifyTextPanels();
        }


        public void ShowLog()
        {
            GameState.World!.Events.Publish(new TutorialEvent() { Action = "ShowLog" });
            InterfaceState.SetControls(new InfoDisplayControls(GetState<GameLog>()));
            InterfaceState.Controls.MenuCommandsSelectable = true;
            InterfaceState.Controls.SelectedMenuCommand = "Log";
            GetState<GameLog>().MarkAsRead();
            InterfaceState.DirtifyTextPanels();
        }

        public void ToggleTutorial()
        {
            var tutorial = GetState<TutorialHandler>();
            if (tutorial.Visible)
            {
                Publish(new TutorialEvent() { Action = "HideTutorial" });
            }
            tutorial.Visible = !tutorial.Visible;
            InterfaceState.DirtifyTextPanels();
        }

        public void TogglePause()
        {
            Publish(new TutorialEvent() { Action = "TogglePause" });
            Time.AutoPausing = !Time.AutoPausing;
            InterfaceState.DirtifyTextPanels();
        }

        public void ScrollUpCommand()
        {
            GetState<GameLog>().ScrollUp();
            InterfaceState.DirtifyTextPanels();
        }

        public void ScrollDownCommand()
        {
            GetState<GameLog>().ScrollDown();
            InterfaceState.DirtifyTextPanels();
        }

        public void SpeedUp()
        {
            Time.SpeedUp();
        }

        public void SlowDown()
        {
            Time.SlowDown();
        }

    }
}