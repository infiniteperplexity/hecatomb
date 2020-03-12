using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public class CommandLogger : StateHandler
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
    }
}
