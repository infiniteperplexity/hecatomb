using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Linq;

// the TurnHandler keeps track of 
namespace Hecatomb8
{
    using static HecatombAliases;
    class TurnHandler : StateHandler
    {
        public int Turn;


        public static int StartHour = 8;
        //public static int StartHour = 16;
        public static int DawnHour = 6;
        public static int DuskHour = 17;
        public static int LunarDays = 12;
        public static int WaxingMoon = 2;
        public static int FullMoon = 5;
        public static int WaningMoon = 8;
        public static int NewMoon = 11;
        public static int Darkness = 64;
        public static Dictionary<string, int> LightLevels = new Dictionary<string, int>()
        {
            ["WaningMoon"] = 32,
            ["WaxingMoon"] = 32,
            ["FullMoon"] = 64,
            ["NewMoon"] = 0
        };


        public static Dictionary<string, char> PhaseSymbols = new Dictionary<string, char>()
        {
            ["Sunlight"] = '\u2600',
            ["Twilight"] = '\u25D2',
            ["WaningMoon"] = '\u2600',
            ["WaxingMoon"] = '\u263D',
            ["FullMoon"] = '\u2600',
            ["NewMoon"] = '\u25CF'
        };

        public int Day;
        public int Hour;
        public int Minute;
        public char PhaseSymbol;
        public int LightLevel;
        public string MoonPhase;


        public Queue<int> ActorDeck;
        // have not yet acted and have points remaining
        public Queue<int> ActorQueue;

        public TurnHandler()
        {
            Turn = 0;
            Day = 0;
            Hour = StartHour;
            Minute = 0;
            PhaseSymbol = PhaseSymbols["Sunlight"];
            MoonPhase = "WaxingMoon";
            LightLevel = 255;
            ActorDeck = new Queue<int>();
            ActorQueue = new Queue<int>();
        }

        public void NextTurn()
        {
            Turn += 1;
            //Debug.WriteLine($"Turn: {Turn}");

            foreach (var e in Entities.Values)
            {
                if (e is TileEntity)
                {
                    var te = (TileEntity)e;
                    if (te is Creature || te is Feature)
                    {
                        if (!te.Placed)
                        {
                            Debug.WriteLine("Hovering in the void, you see: " + te);
                        }
                    }
                }
            }
            Minute += 1;
            if (Minute >= 60)
            {
                Minute = 0;
                Hour += 1;
                if (Hour >= 24)
                {
                    Hour = 0;
                    Day += 1;
                }
            }
            GameState.World!.Events.Publish(new TutorialEvent() { Action = "TurnBegin" });
            GameState.World!.Events.Publish(new TurnBeginEvent() { Turn = Turn });
            if (Minute == 0)
            {
                if (Hour == DawnHour)
                {
                    PushMessage("{yellow}The sun is coming up.");
                    PhaseSymbol = PhaseSymbols["Twilight"];
                }
                else if (Hour == DuskHour)
                {
                    PushMessage("{yellow}Night is falling.");
                    PhaseSymbol = PhaseSymbols["Twilight"];
                }
                else if (Hour == DawnHour + 1)
                {
                    PhaseSymbol = PhaseSymbols["Sunlight"];
                    LightLevel = 255;
                }
                else if (Hour == DuskHour + 1)
                {
                    int day = Day % LunarDays;
                    if (day <= WaxingMoon || day > NewMoon)
                    {
                        MoonPhase = "WaxingMoon";
                    }
                    else if (day <= FullMoon)
                    {
                        MoonPhase = "FullMoon";
                    }
                    else if (day <= WaningMoon)
                    {
                        MoonPhase = "WaningMoon";
                    }
                    else
                    {
                        MoonPhase = "NewMoon";
                    }
                    PhaseSymbol = PhaseSymbols[MoonPhase];
                    LightLevel = LightLevels[MoonPhase] + Darkness;
                }
            }
            if (Hour == DawnHour)
            {
                LightLevel = (int)Math.Min(255, (Minute / 60f) * (255 - Darkness) + Darkness + LightLevels[MoonPhase]);
            }
            else if (Hour == DuskHour)
            {
                LightLevel = (int)Math.Min(255, ((60 - Minute) / 60f) * (255 - Darkness) + Darkness + LightLevels[MoonPhase]);
            }
            InterfaceState.DirtifyMainPanel();
            InterfaceState.DirtifyTextPanels();

            ActorDeck.Clear();
            ActorQueue.Clear();
            foreach (var e in Entities.Values)
            {
                if (e is Actor)
                {
                    (e as Actor)!.Regain();
                    ActorQueue.Enqueue((int)e.EID!);
                }
            }
            ProcessActorQueue();
        }

        public void ProcessActorQueue()
        {
            while (ActorQueue.Count > 0)
            {
                Actor actor = (Actor) Entities[ActorQueue.Dequeue()];
                if (actor.Entity?.UnboxBriefly() == Player)
                {
                    InterfaceState.PlayerIsReady();
                    return;
                }
                else
                {
                    if (actor.Spawned)
                    {
                        actor.Act();
                        //var cr = actor.Entity.UnboxBriefly()!;
                        //Debug.WriteLine($"Let's pretend like this {cr.Describe()} just did something.");
                    }
                    else
                    {
                        Debug.WriteLine("Removing despawned actor from queue");
                    }
                }
            }
            while (ActorDeck.Count > 0)
            {
                int eid = ActorDeck.Dequeue();
                if (Entities.ContainsKey(eid))
                {
                    ActorQueue.Enqueue(eid);
                }
            }
            ActorQueue.OrderBy(eid => (Entities[eid] as Actor)!.CurrentPoints).ThenBy(eid => eid);
            if (ActorQueue.Count > 0)
            {
                ProcessActorQueue();
            }
            else
            {
                //Game.World.Events.Publish(new TurnEndEvent() { Turn = Turn });
                NextTurn();
            }
        }

        public void AfterPlayerActed()
        {
            Time.LastUpdate = DateTime.Now;
            Actor actor = Player.GetComponent<Actor>();
            if (actor.CurrentPoints > 0)
            {
                ActorDeck.Enqueue((int)actor.EID!);
            }
            // bunch of time and interface-handling stuff
            ProcessActorQueue();
        }
    }
}