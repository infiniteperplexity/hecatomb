/*
 * Created by SharpDevelop.
 * User: m543015
 * Date: 9/21/2018
 * Time: 10:48 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
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

namespace Hecatomb
{
    using static HecatombAliases;

    public class HecatombCommmands
    {

        public void Act()
        {
            Turns.PlayerActed = true;
            Time.Acted();
        }

        public void Wait()
        {
            CommandLogger.LogCommand(command: "Wait");
            Player.GetComponent<Actor>().Wait();
            Act();
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
            Creature p = Player;
            int x1 = p.X + dx;
            int y1 = p.Y + dy;
            int z1 = p.Z;
            var m = p.TryComponent<Movement>();
            if (m == null)
            {
                return;
            }
            Coord[] moves = new Coord[]
            {
                new Coord(x1, y1, z1),
                new Coord(p.X, p.Y, z1+1),
                new Coord(p.X, p.Y, z1-1)
            };
            foreach (Coord c in moves)
            {
                Dictionary<string, object> details = new Dictionary<string, object>();
                if (m.CanPass(c.X, c.Y, c.Z))
                {
                    OldGame.World.Events.Publish(new TutorialEvent() { Action = (c.Z == z1) ? "Move" : "Climb" });
                    m.StepTo(c.X, c.Y, c.Z);
                    Act();
                    return;
                }
                Creature cr = OldGame.World.Creatures[c.X, c.Y, c.Z];
                if (cr != null && p.GetComponent<Actor>().IsFriendly(cr))
                {
                    OldGame.World.Events.Publish(new TutorialEvent() { Action = (c.Z == z1) ? "Move" : "Climb" });
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
            Creature p = OldGame.World.Player;
            int x1 = p.X;
            int y1 = p.Y;
            int z1 = p.Z + dz;
            var m = p.TryComponent<Movement>();
            if (m == null)
            {
                return;
            }
            if (!m.CanPass(x1, y1, z1))
            {
                Creature cr = OldGame.World.Creatures[x1, y1, z1];
                if (cr != null && p.GetComponent<Actor>().IsFriendly(cr))
                {
                    OldGame.World.Events.Publish(new TutorialEvent() { Action = "Climb" });
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
                OldGame.World.Events.Publish(new TutorialEvent() { Action = "Climb" });
                m.StepTo(x1, y1, z1);
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
            OldGame.Camera.Z = Math.Max(Math.Min(OldGame.Camera.Z + dz, OldGame.World.Depth - 2), 1);
            ControlContext.CenterCursor();
            InterfacePanel.DirtifyUsualPanels();
        }
        private void moveCameraHorizontal(int dx, int dy)
        {
            if (ControlContext.ShiftDown)
            {
                dx *= 10;
                dy *= 10;
            }
            Camera c = OldGame.Camera;
            int xhalf = c.Width / 2;
            int yhalf = c.Height / 2;
            c.XOffset = Math.Min(Math.Max(1, c.XOffset + dx), OldGame.World.Width - c.Width - 1);
            c.YOffset = Math.Min(Math.Max(1, c.YOffset + dy), OldGame.World.Height - c.Height - 1);
            ControlContext.CenterCursor();
            InterfacePanel.DirtifyUsualPanels();
        }

        public void ChooseTask()
        {
            OldGame.World.Events.Publish(new TutorialEvent() { Action = "ShowJobs" });
            var tasks = GetState<TaskHandler>();
            tasks.PurgeCache();
            OldGame.Controls = new MenuChoiceControls(tasks);
            OldGame.Controls.MenuSelectable = false;
            OldGame.Controls.SelectedMenuCommand = "Jobs";
            InterfacePanel.DirtifySidePanels();
        }

        public void ChooseSpell()
        {
            OldGame.World.Events.Publish(new TutorialEvent() { Action = "ShowSpells" });
            OldGame.Controls = new MenuChoiceControls(OldGame.World.Player.GetComponent<SpellCaster>());
            OldGame.Controls.MenuSelectable = false;
            OldGame.Controls.SelectedMenuCommand = "Spells";
            InterfacePanel.DirtifySidePanels();
        }

        public void ShowAchievements()
        {
            OldGame.World.Events.Publish(new TutorialEvent() { Action = "ShowAchievements" });
            OldGame.Controls = new ListViewControls(OldGame.World.GetState<AchievementHandler>());
            OldGame.Controls.MenuSelectable = false;
            OldGame.Controls.SelectedMenuCommand = "Achievements";
            InterfacePanel.DirtifySidePanels();
        }

        public void ShowResearch()
        {
            OldGame.Controls = new ListViewControls(OldGame.World.GetState<ResearchHandler>());
            OldGame.Controls.MenuSelectable = false;
            OldGame.Controls.SelectedMenuCommand = "Research";
            //Game.Controls.LinkedCommand = Game.MenuPanel.GetCommand("R: Research");
            InterfacePanel.DirtifySidePanels();
        }


        public void ShowLog()
        {
            OldGame.World.Events.Publish(new TutorialEvent() { Action = "ShowLog" });
            OldGame.Controls = new MessageLogControls();
            OldGame.Controls.MenuSelectable = false;
            OldGame.Controls.SelectedMenuCommand = "Log";
            OldGame.World.GetState<MessageHandler>().Unread = false;
            OldGame.World.GetState<MessageHandler>().UnreadColor = "white";
            ControlContext.LogMode = true;
            InterfacePanel.DirtifySidePanels();
        }

        public void AutoWait()
        {
            CommandLogger.LogCommand(command: "Wait");
            OldGame.World.Player.GetComponent<Actor>().Wait();
            Act();
        }

        public void SaveGameCommandCheckFileName()
        {
            var path = (System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
            System.IO.Directory.CreateDirectory(path + @"\saves");
            if (File.Exists(path + @"\saves\" + OldGame.GameName + ".json"))
            {
                ControlContext.Set(new ConfirmationControls("Really overwrite " + OldGame.GameName + ".json?", SaveGameCommand));
            }
            else
            {
                SaveGameCommand();
            }
        }
        public void SaveGameCommand()
        {
            OldGame.SplashPanel.Splash(new List<ColoredText>()
            {
                $"Saving {OldGame.GameName}..."
            }, frozen: true);
            Debug.WriteLine("saving the game");
            Thread thread = new Thread(SaveGameProcess);
            thread.Start();
        }

        public void SaveGameProcess()
        {
            try
            {
                OldGame.World.Stringify();
                ControlContext.Reset();
            }
            catch (Exception e)
            {
                OldGame.HandleException(e);
            }
        }

        public void RestoreGameProcess()
        {
            try
            {
                //string json = System.IO.File.ReadAllText(@"..\" + Game.GameName + ".json");
                if (OldGame.World == null)
                {
                    OldGame.World = new World(256, 256, 64);
                }
                //Game.World.Parse(json);
                var path = (System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
                System.IO.Directory.CreateDirectory(path + @"\saves");
                OldGame.World.Parse(path + @"\saves\" + OldGame.GameName + ".json");
                // we need some kind of failure handling...
                OldGame.MainPanel.IntroState = false;
                ControlContext.Reset();
            }
            catch (Exception e)
            {
                OldGame.HandleException(e);
            }
        }

        public void RestoreGameCommand()
        {
            //throw new Exception("test exception");
            // I guess maybe a Save File should have an object representation?
            ControlContext.Set(new MenuChoiceControls(new SaveGameChooser()));
        }

        public void SystemMenuCommand()
        {
            Time.Frozen = true;
            var commands = new List<(Keys, ColoredText, Action)>() {
                (Keys.Escape, "Cancel.", ControlContext.Reset),
                (Keys.S, "Save game.", SaveGameCommandCheckFileName),
                (Keys.A, "Save as...", SaveGameAsCommand),
                (Keys.R, "Restore game.", OldGame.game.RestoreGameWithConfirmation),
                //(Keys.D, "Delete game.", Game.game.StartGame),
                (Keys.N, "New game.", OldGame.game.StartGameWithConfirmation),
                (Keys.Q, "Quit.", OldGame.game.BackToTitleWithConfirmation)
            };
            if (Options.ReconstructGames)
            {
                commands.Add((Keys.C, "Reconstruct game from log.", ReconstructGameCommand));
            }
            ControlContext.Set(new StaticMenuControls(" ", commands));
        }

        public void ReconstructGameCommand()
        {
            
            ControlContext.Set(new MenuChoiceControls(new CommandLogger()));
        }

        public void SaveGameAsCommand()
        {
            Action<string> saveGameAs = (string name) =>
            {
                //could check legality of name here?
                OldGame.GameName = name;
                SaveGameCommandCheckFileName();
            };
            ControlContext.Set(new TextEntryControls("Type a name for your saved game.", saveGameAs));
            (Controls as TextEntryControls).CurrentText = OldGame.GameName;
        }

        public void TogglePause()
        {        
            OldGame.World.Events.Publish(new TutorialEvent() { Action = "TogglePause" });
            OldGame.Time.PausedAfterLoad = false;
            OldGame.Time.AutoPausing = !OldGame.Time.AutoPausing;
            InterfacePanel.DirtifySidePanels();
        }

        public void ToggleMovingCamera()
        {
            if (!ControlContext.MovingCamera)
            {
                OldGame.World.Events.Publish(new TutorialEvent() { Action = "CameraMode" });
            }
            else
            {
                OldGame.World.Events.Publish(new TutorialEvent() { Action = "MainMode" });
            }
            ControlContext.MovingCamera = !ControlContext.MovingCamera;
            if (ControlContext.MovingCamera)
            {
                OldGame.Controls = OldGame.CameraControls;
            }
            else
            {
                OldGame.Controls = OldGame.DefaultControls;
            }
            Creature p = OldGame.World.Player;
            OldGame.Camera.Center(p.X, p.Y, p.Z);
            InterfacePanel.DirtifyUsualPanels();
        }

        public void ShowConsole()
        {
            //DebugConsole.ShowConsole();
        }

        public void HoverCamera()
        {
            OldGame.Controls.CameraHover();
        }

        public void ToggleTutorial()
        {
            var tutorial = OldGame.World.GetState<TutorialHandler>();
            if (tutorial.Visible)
            {
                OldGame.World.Events.Publish(new TutorialEvent() { Action = "HideTutorial" });
            }
            tutorial.Visible = !tutorial.Visible;
            InterfacePanel.DirtifySidePanels();
        }

        public void ScrollUpCommand()
        {
            OldGame.InfoPanel.ScrollUp();
            InterfacePanel.DirtifySidePanels();
        }

        public void ScrollDownCommand()
        {
            OldGame.InfoPanel.ScrollDown();
            InterfacePanel.DirtifySidePanels();
        }

        public void SpeedUp()
        {
            Time.SpeedUp();
        }

        public void SlowDown()
        {
            Time.SlowDown();
        }

        public void ShowStructures()
        {
            var structures = Structure.ListStructures();
            if (structures.Count > 0)
            {
                ControlContext.Set(new MenuCameraControls(structures[0]));
                OldGame.Camera.CenterOnSelection();
                //ControlContext.Set(new MenuChoiceControls(structures[0]));
            }
        }

        public void ShowMinions()
        {
            var minions = GetState<TaskHandler>().Minions;
            if (minions.Count > 0)
            {
                ControlContext.Set(new MenuCameraControls((Creature)minions[0]));
                OldGame.Camera.CenterOnSelection();
                //ControlContext.Set(new MenuChoiceControls((Creature)minions[0]));
            }
        }

        public void PlayerDies()
        {
            // I don't really like doing this but it's just for debugging so it's probably okay
            Options.NoStartupScreen = false;
            if (OldGame.Options.Invincible)
            {
                OldGame.SplashPanel.Splash(new List<ColoredText> {
                    "The spark of life fades from your fallen corpse and your soul slips away to darker realms.",
                    " ",
                    "Your depraved ambition has come to an end...or it would have, if you weren't invincible for debugging purposes."
                });
                OldGame.World.Player.GetComponent<Defender>().Wounds = 0;
            }
            else
            {
                OldGame.SplashPanel.Splash(new List<ColoredText> {
                    "The spark of life fades from your fallen corpse and your soul slips away to darker realms.",
                    " ",
                    "Your depraved ambition has come to an end."
                }, callback: OldGame.game.BackToTitle);
            }
            
        }
    }
}