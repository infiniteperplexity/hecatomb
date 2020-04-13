using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;

namespace Hecatomb
{

    public class GameCommand
    {
        public string Command;
        public int N;
        // we need some way to describe trade tasks?  Oh...we could choose those generated on the list
        public int X;
        public int Y;
        public int Z;
        public string Makes;
        public List<Coord> Squares = new List<Coord>();
        //public long Calls;
    }
    public class CommandLogger : StateHandler, IChoiceMenu
    {
        public List<GameCommand> LoggedCommands;
        [JsonIgnore] public Queue<GameCommand> CommandQueue;

        public void SubmitCommand(GameCommand g)
        {
            var commands = OldGame.Commands;
            var s = OldGame.World.Player.GetComponent<SpellCaster>();
            var t = OldGame.World.GetState<TaskHandler>();
            var c = new Coord(g.X, g.Y, g.Z);
            if (g.Command == "Wait")
            {
                commands.Wait();
            }
            else if (g.Command == "MoveHorizontal")
            {
                commands.moveHorizontalCommand(g.X, g.Y);
            }
            else if (g.Command == "MoveVertical")
            {
                commands.moveVerticalCommand(g.Z);
            }
            else if (g.Command == "RaiseZombie")
            {
                (s.GetSpell("RaiseZombieSpell") as RaiseZombieSpell).SelectTile(c);
            }
            else if (g.Command == "SacrificeMinion")
            {
                (s.GetSpell("SacrificeMinionSpell") as SacrificeMinionSpell).SelectTile(c);
            }
            else if (g.Command == "SiphonFlesh")
            {
                (s.GetSpell("SiphonFleshSpell") as SiphonFleshSpell).SelectTile(c);
            }
            else if (g.Command == "CondenseEctoplasm")
            {
                (s.GetSpell("CondenseEctoplasmSpell") as CondenseEctoplasmSpell).ChooseFromMenu();
            }
            else if (g.Command == "ShadowHop")
            {
                (s.GetSpell("ShadowHopSpell") as ShadowHopSpell).ChooseFromMenu();
            }
            else if (g.Command == "BuildTask")
            {
                t.GetTask("BuildTask").SelectZone(g.Squares);
            }
            else if (g.Command == "ButcherTask")
            {
                t.GetTask("ButcherTask").SelectZone(g.Squares);
            }
            else if (g.Command == "ClaimTask")
            {
                t.GetTask("ClaimTask").SelectZone(g.Squares);
            }
            else if (g.Command == "ConstructTask")
            {
                var task = t.GetTask("ConstructTask");
                task.Makes = g.Makes;
                task.SelectBox(g.Squares);
            }
            else if (g.Command == "DigTask")
            {
                t.GetTask("DigTask").SelectZone(g.Squares);
            }
            else if (g.Command == "ForbidTask")
            {
                t.GetTask("ForbidTask").SelectZone(g.Squares);
            }
            else if (g.Command == "UndesignateTask")
            {
                t.GetTask("UndesignateTask").SelectZone(g.Squares);
            }
            else if (g.Command == "FurnishTask")
            {
                var task = t.GetTask("FurnishTask");
                task.Makes = g.Makes;
                task.SelectTile(c);
            }
            else if (g.Command == "MendingTask")
            {
                t.GetTask("MendingTask").SelectTile(c);
            }
            else if (g.Command == "MurderTask")
            {
                t.GetTask("MurderTask").SelectTile(c);
            }
            else if (g.Command == "PatrolTask")
            {
                t.GetTask("PatrolTask").SelectTile(c);
            }
            else if (g.Command == "RallyTask")
            {
                t.GetTask("RallyTask").SelectTile(c);
            }
            else if (g.Command == "ResearchTask")
            {
                var task = (ResearchTask) t.GetTask("ResearchTask");
                task.Makes = g.Makes;
                task.Structure = OldGame.World.Features[c].GetComponent<StructuralComponent>().Structure;
                task.ChooseFromMenu();
            }
            else if (g.Command == "TradeTask")
            {
                var market = (BlackMarket) OldGame.World.Features[c].GetComponent<StructuralComponent>().Structure;
                var task = market.AvailableTrades[g.N];
                task.ChooseFromMenu();
            }
            else if (g.Command == "DyeTask")
            {
                var task = (DyeTask) t.GetTask("DyeTask");
                task.Makes = g.Makes;
                task.Background = (g.N == 1) ? true : false;
                task.SelectZone(g.Squares);
            }
            else if (g.Command == "FarmingTask")
            {
                var task = t.GetTask("FarmingTask");
                task.Makes = g.Makes;
                task.SelectTile(c);
            }
            else
            {
                Debug.WriteLine("Apparently I forgot to code the " + g.Command + " command?");
            }
        }
        public CommandLogger()
        {
            LoggedCommands = new List<GameCommand>();
            CommandQueue = new Queue<GameCommand>();
        }

        public static void LogCommand(string command = "NoCommand", int n = -1, string makes = null, int x = -1, int y = -1, int z = -1, List<Coord> squares = null)
        {
            int turn = OldGame.World.GetState<TurnHandler>().Turn;
            //Game.World.Random.Poll();
            if (OldGame.ReconstructMode)
            {
                //return;
                //Debug.WriteLine("Explicit command interrupted reconstruction");
                //Game.ReconstructMode = false;
            }
            var c = new GameCommand()
            {
                Command = command,
                N = n,
                Makes = makes,
                X = x,
                Y = y,
                Z = z,
                Squares = (squares == null) ? new List<Coord>() : new List<Coord>(squares)
                //,
                //Calls = Game.World.Random.Calls
            };
            OldGame.World.GetState<CommandLogger>().LoggedCommands.Add(c);
        }

        public static string DumpLog()
        {
            return JsonConvert.SerializeObject(OldGame.World.GetState<CommandLogger>().LoggedCommands);
        }

        public static void RebuildFromFile()
        {

        }

        public void BuildMenu(MenuChoiceControls menu)
        {
            var path = (System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
            menu.Header = "Choose a crash report:";
            menu.Choices = new List<IMenuListable>();
            System.IO.Directory.CreateDirectory(path + @"\logs");
            string[] filePaths = Directory.GetFiles(path + @"\logs", "*.txt");
            foreach (string paths in filePaths)
            {
                string[] split = paths.Split('\\');
                string fname = split[split.Length - 1];
                split = fname.Split('.');
                fname = split[0];
                menu.Choices.Add(new CrashReportFile(fname.Replace("HecatombCrashReport","")));
            }
        }

        public void FinishMenu(MenuChoiceControls menu)
        {
            menu.KeyMap[Microsoft.Xna.Framework.Input.Keys.Escape] = OldGame.Controls.Back;
        }

        public void StepForward()
        {
            if (CommandQueue.Count == 0)
            {
                OldGame.ReconstructMode = false;
                ControlContext.Reset();
                return;
            }
            ControlContext.Reset();
            GameCommand gc = CommandQueue.Dequeue();
           // Debug.WriteLine("Submitting a " + gc.Command + " command.");
            SubmitCommand(gc);
            //if (gc.Calls != Game.World.Random.Calls)
            //{
             //   Debug.WriteLine($"New calls {Game.World.Random.Calls}, old calls {gc.Calls}");
            //    Debug.WriteLine("Command was " + gc.Command);
            //    Debug.WriteLine("Turn was " + Game.World.Turns.Turn);
            //}
            //Debug.WriteLine(CommandQueue.Count + " commands left in the queue.");
            if (CommandQueue.Count == 0)
            {
                OldGame.ReconstructMode = false;
                //ControlContext.Reset();
            }
            ControlContext.Reset();
        }

        public void AsyncSteps(int n, int delay = 25)
        {
            StepForward();
            int steps = 1;
            Thread thread = new Thread(() =>
            {
                while(steps < n)
                {
                    Thread.Sleep(delay);
                    if (OldGame.World.Turns.PlayerActed)
                    {
                        try
                        {
                            StepForward();
                            steps += 1;
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine($"We caught {e}");
                        }
                    }
                }
            });
            thread.Start();
        }
    }

    class CrashReportFile : IMenuListable
    {
        public string Name;
        public int Seed;
        public static List<GameCommand> LoggedCommands;

        public CrashReportFile(string name)
        {
            Name = name;
        }

        public ColoredText ListOnMenu()
        {
            return Name;
        }

        public void ChooseFromMenu()
        {
            CheckBuildDate();
        }

        public void CheckBuildDate()
        {
            var path = (System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
            System.IO.Directory.CreateDirectory(path + @"\logs");
            System.IO.StreamReader file = new System.IO.StreamReader(path + @"\logs\HecatombCrashReport" + Name + ".txt");
            string line = file.ReadLine();
            MatchCollection col = Regex.Matches(line, "\\\"(.*?)\\\"");
            if (col.Count >= 2)
            {
                var str = col[1].ToString();
                Seed = Int32.Parse(str.Substring(1,str.Length-2));
                Debug.WriteLine("seed was " + Seed);
            }
            int maxTries = 5000;
            int tries = 0;
            while (line != "Logged Commands:")
            {
                
                line = file.ReadLine();
                tries += 1;
                if (tries > maxTries)
                {
                    throw new Exception("Invalid crash report");
                }
            }
            string json = file.ReadLine();
            while ((line = file.ReadLine()) != null)
            {
                json += line;
            }
            LoggedCommands = JsonConvert.DeserializeObject<List<GameCommand>>(json);
            Debug.WriteLine("there were " + LoggedCommands.Count + " logged commands");
            if (col.Count == 0 || col[0].ToString() != '"' + OldGame.BuildDate.ToString() + '"')
            {
                ControlContext.Set(new ConfirmationControls(
                    "Warning: This crash report was created under a different build of Hecatomb and reconstructing it may cause unexpected results.  Really reconstruct the game?"
                , ReconstructGame));
            }
            else
            {
                ReconstructGame();
            }
        }

        public void ReconstructGame()
        {
            OldGame.GameName = Name;
            OldGame.SplashPanel.Splash(new List<ColoredText>()
            {
                $"Reconstructing {Name}..."
            }, frozen: true);
            Debug.WriteLine("reconstructing the game");
            ControlContext.Set(new FrozenControls());
            OldGame.ReconstructMode = true;
            OldGame.FixedSeed = Seed;

            OldGame.game.StartGame();
        }
    }
}
