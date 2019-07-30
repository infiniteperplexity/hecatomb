/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/5/2018
 * Time: 2:26 PM
 */
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Hecatomb
{
    using static HecatombAliases;
    public class TurnHandler : StateHandler
    {
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

        public int Turn;
        public int Day;
        public int Hour;
        public int Minute;
        public char PhaseSymbol;
        public int LightLevel;
        public string MoonPhase;

        public bool PlayerActed;

		// already acted, but still have points to act again
		[JsonIgnore]
        public Queue<Actor> Deck;
        // have not yet acted and have points remaining
		[JsonIgnore]
        public Queue<Actor> Queue;
		
		public TurnHandler()
		{
			Turn = 0;
            Day = 0;
            Hour = StartHour;
            Minute = 0;
            PhaseSymbol = PhaseSymbols["Sunlight"];
            MoonPhase = "WaxingMoon";
            LightLevel = 255;
			Deck = new Queue<Actor>();
			Queue = new Queue<Actor>();
		}
		
		public void Try()
		{
			if (PlayerActed)
			{
				NextTurn();
			}
		}
        public void NextTurn()
        {
            Creature p = Game.World.Player;
            PlayerActed = false;
            Turn += 1;
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
            Game.World.Events.Publish(new TutorialEvent() { Action = "TurnBegin" });
            Game.World.Events.Publish(new TurnBeginEvent() { Turn = Turn });
            if (Minute == 0)
            {
                if (Hour == DawnHour)
                {
                    Game.StatusPanel.PushMessage("The sun is coming up.");
                    PhaseSymbol = PhaseSymbols["Twilight"];
                }
                else if (Hour == DuskHour)
                {
                    Game.StatusPanel.PushMessage("Night is falling.");
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
            //if (Turn % 5 == 0)
            //{
                if (Hour == DawnHour)
                {
                    LightLevel = (int) Math.Min(255, (Minute/60f) * (255 - Darkness) + Darkness + LightLevels[MoonPhase]);
                }
                else if (Hour == DuskHour)
                {
                    LightLevel = (int)Math.Min(255, ((60-Minute) / 60f) * (255 - Darkness) + Darkness + LightLevels[MoonPhase]);
                }
            //}
            Game.MainPanel.Dirty = true;
			Game.MenuPanel.Dirty = true;
			Game.StatusPanel.Dirty = true;
			Creature[] actors = Game.World.Creatures.ToArray();
			Queue.Clear();
			Deck.Clear();
			foreach (Entity e in Entities.Values)
			{
				if (e is Actor)
				{
                    
					Actor actor = (Actor) e;
                    if (actor.Asleep)
                    {
                        continue;
                    }
                    actor.Regain();
					Queue.Enqueue(actor);
				}
			}
			Queue.OrderBy(a=>a.CurrentPoints).ThenBy(a=>a.EID);
			ProcessActorQueue();
		}

        // let's try modifying this to use a loop
        public void ProcessActorQueue()
        {
            while (Queue.Count > 0)
            {
                Actor actor = Queue.Dequeue();
                if (actor.Entity == Player)
                {
                    HandleVisibility();
                    Game.MainPanel.Dirty = true;
                    Game.MenuPanel.Dirty = true;
                    Game.StatusPanel.Dirty = true;
                    PlayerActed = false;
                    return;
                }
                else
                {
                    if (actor.Spawned)
                    {
                        int checkPoints = actor.CurrentPoints;
                        if (checkPoints > 0)
                        {
                            Game.World.Events.Publish(new ActEvent() { Actor = actor, Entity = actor.Entity });
                            actor.Act();
                            if (actor.CurrentPoints == checkPoints)
                            {
                                throw new InvalidOperationException(String.Format("{0} somehow avoided using action points.", actor.Entity));
                            }
                        }
                        if (actor.CurrentPoints > 0)
                        {
                            Debug.Print("Replacing {0} on the queue.", actor.Entity.Entity.Describe());
                            actor.Acted = false;
                            Deck.Enqueue(actor);
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Removing despawned actor from queue");
                    }
                }
            }
            while (Deck.Count > 0)
            {
                Queue.Enqueue(Deck.Dequeue());
            }
            Queue.OrderBy(a => a.CurrentPoints).ThenBy(a => a.EID);
            if (Queue.Count > 0)
            {
                ProcessActorQueue();
            }
            else
            {
                Game.World.Events.Publish(new TurnEndEvent() { Turn = Turn });
                NextTurn();
            }
        }

        public void AfterPlayerActed()
        {
            Actor actor = Game.World.Player.GetComponent<Actor>();
            if (actor.CurrentPoints > 0)
            {
                Deck.Enqueue(actor);
            }
            // bunch of time and interface-handling stuff
            PlayerActed = true;
            ProcessActorQueue();
        }
		
		public Queue<int> QueueAsIDs(Queue<Actor> q)
		{
			List<Actor> list = q.ToList();
			Queue<int> qi = new Queue<int>();
			for (int i=0; i<list.Count; i++)
			{
				qi.Enqueue(list[i].EID);
			}
			return qi;
		}
		
		public Queue<Actor> QueueAsActors(Queue<int> q)
		{
			List<int> list = q.ToList();
			Queue<Actor> qa = new Queue<Actor>();
			for (int i=0; i<list.Count; i++)
			{
				qa.Enqueue((Actor) Entities[list[i]]);
			}
			return qa;
		}

        public static void HandleVisibility()
        {
            Game.World.ValidateLighting();
            if (!(Controls is CameraControls))
            {
                Game.Camera.Center(Player.X, Player.Y, Player.Z);
            }
            Game.Visible = Game.World.Player.GetComponent<Senses>().GetFOV();
            foreach (Creature c in GetState<TaskHandler>().Minions)
            {
                Senses s = c.GetComponent<Senses>();
                Game.Visible.UnionWith(s.GetFOV());
            }
            foreach (var t in Game.Visible)
            {
                Game.World.Explored.Add(t);
            }
        }
    }
}