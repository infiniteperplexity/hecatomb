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
        public List<Coord> Squares;
    }
    public class CommandLogger : StateHandler, IChoiceMenu
    {
        public List<GameCommand> LoggedCommands;

        //[JsonIgnore]
        public Dictionary<string, Object> CommandIndex = new Dictionary<string, Object>
        {
            { "MoveHorizontal", null }, // x y
            { "MoveVertical", null }, // z
            { "CondenseEctoplasm", null },
            { "RaiseZombie", null }, // x y z
            { "ShadowHop", null },
            { "SiphonFlesh", null }, // x y z
            { "BuildTask", null}, // squares,
            { "ButcherTask", null}, // squares,
            { "ClaimTask", null}, // squares,
            { "ConstructTask", null}, // squares !!!als oneed to specify what structure!,
            { "DigTask", null}, // squares,
            { "ForbidTask", null}, // squares,
            { "FurnishTask", null}, // x y z and makes,
            { "MendingTask", null}, // x y z,
            { "MurderTask", null}, // x y z,
            { "PatrolTask", null}, // x y z,
            { "RallyTask", null}, // x y z,
            { "UndesignateTask", null}, // squares,
            { "ResearchTask", null}, // x y z makes,
            { "TradeTask", null} // x y z complicated stuff...,


        };

        public void SubmitCommand(GameCommand g)
        {

        }
        public CommandLogger()
        {
            LoggedCommands = new List<GameCommand>();
        }

        public static void LogCommand(string command = "NoCommand", int n = -1, string makes = null, int x = -1, int y = -1, int z = -1, List<Coord> squares = null)
        {
            Game.World.GetState<CommandLogger>().LoggedCommands.Add(new GameCommand()
            {
                Command = command,
                N = n,
                Makes = makes,
                X = x,
                Y = y,
                Z = z,
                Squares = squares
            });
        }

        public static string DumpLog()
        {
            return JsonConvert.SerializeObject(Game.World.GetState<CommandLogger>().LoggedCommands);
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
            menu.KeyMap[Microsoft.Xna.Framework.Input.Keys.Escape] = Game.Controls.Back;
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
            if (col.Count == 0 || col[0].ToString() != '"' + Game.BuildDate.ToString() + '"')
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
            Game.GameName = Name;
            Game.SplashPanel.Splash(new List<ColoredText>()
            {
                $"Reconstructing {Name}..."
            }, frozen: true);
            Debug.WriteLine("reconstructing the game");
            ControlContext.Set(new FrozenControls());
            Game.ReconstructMode = true;
            Game.FixedSeed = Seed;

            Game.game.StartGame();
        }
    }
}
