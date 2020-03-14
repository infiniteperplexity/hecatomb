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
                    Game.World.Events.Publish(new TutorialEvent() { Action = (c.Z == z1) ? "Move" : "Climb" });
                    m.StepTo(c.X, c.Y, c.Z);
                    Act();
                    return;
                }
                Creature cr = Game.World.Creatures[c.X, c.Y, c.Z];
                if (cr != null && p.GetComponent<Actor>().IsFriendly(cr))
                {
                    Game.World.Events.Publish(new TutorialEvent() { Action = (c.Z == z1) ? "Move" : "Climb" });
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
            Creature p = Game.World.Player;
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
                Creature cr = Game.World.Creatures[x1, y1, z1];
                if (cr != null && p.GetComponent<Actor>().IsFriendly(cr))
                {
                    Game.World.Events.Publish(new TutorialEvent() { Action = "Climb" });
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
                Game.World.Events.Publish(new TutorialEvent() { Action = "Climb" });
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
            Game.Camera.Z = Math.Max(Math.Min(Game.Camera.Z + dz, Game.World.Depth - 2), 1);
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
            Camera c = Game.Camera;
            int xhalf = c.Width / 2;
            int yhalf = c.Height / 2;
            c.XOffset = Math.Min(Math.Max(1, c.XOffset + dx), Game.World.Width - c.Width - 1);
            c.YOffset = Math.Min(Math.Max(1, c.YOffset + dy), Game.World.Height - c.Height - 1);
            ControlContext.CenterCursor();
            InterfacePanel.DirtifyUsualPanels();
        }

        public void ChooseTask()
        {
            Game.World.Events.Publish(new TutorialEvent() { Action = "ShowJobs" });
            var tasks = GetState<TaskHandler>();
            tasks.PurgeCache();
            Game.Controls = new MenuChoiceControls(tasks);
            Game.Controls.MenuSelectable = false;
            Game.Controls.SelectedMenuCommand = "Jobs";
            InterfacePanel.DirtifySidePanels();
        }

        public void ChooseSpell()
        {
            Game.World.Events.Publish(new TutorialEvent() { Action = "ShowSpells" });
            Game.Controls = new MenuChoiceControls(Game.World.Player.GetComponent<SpellCaster>());
            Game.Controls.MenuSelectable = false;
            Game.Controls.SelectedMenuCommand = "Spells";
            InterfacePanel.DirtifySidePanels();
        }

        public void ShowAchievements()
        {
            Game.World.Events.Publish(new TutorialEvent() { Action = "ShowAchievements" });
            Game.Controls = new ListViewControls(Game.World.GetState<AchievementHandler>());
            Game.Controls.MenuSelectable = false;
            Game.Controls.SelectedMenuCommand = "Achievements";
            InterfacePanel.DirtifySidePanels();
        }

        public void ShowResearch()
        {
            Game.Controls = new ListViewControls(Game.World.GetState<ResearchHandler>());
            Game.Controls.MenuSelectable = false;
            Game.Controls.SelectedMenuCommand = "Research";
            //Game.Controls.LinkedCommand = Game.MenuPanel.GetCommand("R: Research");
            InterfacePanel.DirtifySidePanels();
        }


        public void ShowLog()
        {
            Game.World.Events.Publish(new TutorialEvent() { Action = "ShowLog" });
            Game.Controls = new MessageLogControls();
            Game.Controls.MenuSelectable = false;
            Game.Controls.SelectedMenuCommand = "Log";
            Game.World.GetState<MessageHandler>().Unread = false;
            Game.World.GetState<MessageHandler>().UnreadColor = "white";
            ControlContext.LogMode = true;
            InterfacePanel.DirtifySidePanels();
        }

        public void AutoWait()
        {
            CommandLogger.LogCommand(command: "Wait");
            Game.World.Player.GetComponent<Actor>().Wait();
            Act();
        }

        public void SaveGameCommandCheckFileName()
        {
            var path = (System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
            System.IO.Directory.CreateDirectory(path + @"\saves");
            if (File.Exists(path + @"\saves\" + Game.GameName + ".json"))
            {
                ControlContext.Set(new ConfirmationControls("Really overwrite " + Game.GameName + ".json?", SaveGameCommand));
            }
            else
            {
                SaveGameCommand();
            }
        }
        public void SaveGameCommand()
        {
            Game.SplashPanel.Splash(new List<ColoredText>()
            {
                $"Saving {Game.GameName}..."
            }, frozen: true);
            Debug.WriteLine("saving the game");
            Thread thread = new Thread(SaveGameProcess);
            thread.Start();
        }

        public void SaveGameProcess()
        {
            try
            {
                Game.World.Stringify();
                ControlContext.Reset();
            }
            catch (Exception e)
            {
                Game.HandleException(e);
            }
        }

        public void RestoreGameProcess()
        {
            try
            {
                //string json = System.IO.File.ReadAllText(@"..\" + Game.GameName + ".json");
                if (Game.World == null)
                {
                    Game.World = new World(256, 256, 64);
                }
                //Game.World.Parse(json);
                var path = (System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
                System.IO.Directory.CreateDirectory(path + @"\saves");
                Game.World.Parse(path + @"\saves\" + Game.GameName + ".json");
                // we need some kind of failure handling...
                Game.MainPanel.IntroState = false;
                ControlContext.Reset();
            }
            catch (Exception e)
            {
                Game.HandleException(e);
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
                (Keys.R, "Restore game.", Game.game.RestoreGameWithConfirmation),
                //(Keys.D, "Delete game.", Game.game.StartGame),
                (Keys.N, "New game.", Game.game.StartGameWithConfirmation),
                (Keys.Q, "Quit.", Game.game.BackToTitleWithConfirmation)
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
                Game.GameName = name;
                SaveGameCommandCheckFileName();
            };
            ControlContext.Set(new TextEntryControls("Type a name for your saved game.", saveGameAs));
            (Controls as TextEntryControls).CurrentText = Game.GameName;
        }

        public void TogglePause()
        {        
            Game.World.Events.Publish(new TutorialEvent() { Action = "TogglePause" });
            Game.Time.PausedAfterLoad = false;
            Game.Time.AutoPausing = !Game.Time.AutoPausing;
            InterfacePanel.DirtifySidePanels();
        }

        public void ToggleMovingCamera()
        {
            if (!ControlContext.MovingCamera)
            {
                Game.World.Events.Publish(new TutorialEvent() { Action = "CameraMode" });
            }
            else
            {
                Game.World.Events.Publish(new TutorialEvent() { Action = "MainMode" });
            }
            ControlContext.MovingCamera = !ControlContext.MovingCamera;
            if (ControlContext.MovingCamera)
            {
                Game.Controls = Game.CameraControls;
            }
            else
            {
                Game.Controls = Game.DefaultControls;
            }
            Creature p = Game.World.Player;
            Game.Camera.Center(p.X, p.Y, p.Z);
            InterfacePanel.DirtifyUsualPanels();
        }

        public void ShowConsole()
        {
            DebugConsole.ShowConsole();
        }

        public void HoverCamera()
        {
            Game.Controls.CameraHover();
        }

        public void ToggleTutorial()
        {
            var tutorial = Game.World.GetState<TutorialHandler>();
            if (tutorial.Visible)
            {
                Game.World.Events.Publish(new TutorialEvent() { Action = "HideTutorial" });
            }
            tutorial.Visible = !tutorial.Visible;
            InterfacePanel.DirtifySidePanels();
        }

        public void ScrollUpCommand()
        {
            Game.InfoPanel.ScrollUp();
            InterfacePanel.DirtifySidePanels();
        }

        public void ScrollDownCommand()
        {
            Game.InfoPanel.ScrollDown();
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
                Game.Camera.CenterOnSelection();
                //ControlContext.Set(new MenuChoiceControls(structures[0]));
            }
        }

        public void ShowMinions()
        {
            var minions = GetState<TaskHandler>().Minions;
            if (minions.Count > 0)
            {
                ControlContext.Set(new MenuCameraControls((Creature)minions[0]));
                Game.Camera.CenterOnSelection();
                //ControlContext.Set(new MenuChoiceControls((Creature)minions[0]));
            }
        }

        public void PlayerDies()
        {
            // I don't really like doing this but it's just for debugging so it's probably okay
            Options.NoStartupScreen = false;
            if (Game.Options.Invincible)
            {
                Game.SplashPanel.Splash(new List<ColoredText> {
                    "The spark of life fades from your fallen corpse and your soul slips away to darker realms.",
                    " ",
                    "Your depraved ambition has come to an end...or it would have, if you weren't invincible for debugging purposes."
                });
                Game.World.Player.GetComponent<Defender>().Wounds = 0;
            }
            else
            {
                Game.SplashPanel.Splash(new List<ColoredText> {
                    "The spark of life fades from your fallen corpse and your soul slips away to darker realms.",
                    " ",
                    "Your depraved ambition has come to an end."
                }, callback: Game.game.BackToTitle);
            }
            
        }
    }
}