using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hecatomb8
{
    using static HecatombAliases;

    public partial class Activity
    {
        public static readonly Activity BesiegePlayer = new Activity(
            type: "BesiegePlayer",
            act: SiegeHandler._besiegePlayer
        );
        public static readonly Activity Frustrated = new Activity(
            type: "Frustrated",
            act: SiegeHandler._frustrated
        );
        public static readonly Activity Vandalize = new Activity(
            type: "Vandalize",
            act: Actor._vandalize
        );
    }

    /*
     * TargetPlayer, Alert, Seek, Frustrated, Vandalize, Seek, Wander
     */

    public class SiegeHandler : StateHandler
    {
        public int PastSieges; // how many sieges have there 
        public Coord? EntryTile;
        public int TurnsSince;
        public int Frustration;
        public List<int> SiegeCreatures;
        [JsonIgnore] public int FrustrationLimit = 25;
        public bool FrustrationAnnounced;

        public SiegeHandler()
        {
            SiegeCreatures = new List<int>();
            AddListener<TurnBeginEvent>(OnTurnBegin);
            AddListener<AttackEvent>(OnAttack);
        }

        public GameEvent OnAttack(GameEvent ge)
        {
            AttackEvent ae = (AttackEvent)ge;
            Attacker att = ae.Attacker;
            ComposedEntity? cr = att.Entity?.UnboxBriefly();
            if (cr is Creature && cr.Spawned && SiegeCreatures.Contains((int)cr.EID!))
            {
                var defender = ae.Defender.Entity?.UnboxBriefly();
                if (defender is Door)
                {
                    Debug.WriteLine($"attacking a door, frustration {Frustration}");
                    Frustration += 1;
                }
            }
            return ge;
        }

        // like targetPlayer, but checks frustration first and also targets doors as a fallback
        public static void _besiegePlayer(Actor actor, Creature cr)
        {
            // this logic makes it only
            if (actor.Target is null)
            {
                var siege = GetState<SiegeHandler>();
                if (siege.Frustration > siege.FrustrationLimit)
                {
                    return;
                }
                Movement m = cr.GetComponent<Movement>();
                // avoid pathfinding if the distance is long; just head generally there
                if (Tiles.Distance(cr, Player) > 30 || m.CanReachBounded(Player))
                {
                    actor.Target = Player.GetHandle<TileEntity>(actor.OnDespawn);
                }
                else
                {
                    List<Feature> doors = Features.Where<Feature>((f) => (f is Door)).ToList();
                    doors = doors.OrderBy((f) => Tiles.Distance((int)f.X!, (int)f.Y!, (int)f.Z!, (int)Player.X!, (int)Player.Y!, (int)Player.Z!)).ToList();
                    foreach (var f in doors)
                    {
                        if (m.CanReachBounded(f, useLast: false))
                        {
                            actor.Target = f.GetHandle<TileEntity>(actor.OnDespawn);
                            return;
                        }
                    }
                }
            }
        }

        public static void _frustrated(Actor actor, Creature cr)
        {
            var siege = GetState<SiegeHandler>();
            if (siege.Frustration <= siege.FrustrationLimit || siege.EntryTile is null || !cr.Spawned || !cr.Placed)
            {
                return;
            }
            var target = actor.Target?.UnboxBriefly();
            if (target is Creature && target.Placed && target.Spawned)
            {
                return;
            }
            // this will only happen after we try to seek and attack, right?
            actor.Target = null;
            if (!siege.FrustrationAnnounced)
            {
                InterfaceState.Splash(
                    new List<ColoredText> { "Unable to penetrate your defenses, the bandits grow frustrated and break off the siege." },
                    sleep: 2500,
                    callback: InterfaceState.ResetControls
                );
                siege.FrustrationAnnounced = true;
            }
            var (x, y, z) = (Coord)siege.EntryTile!;
            var (X, Y, Z) = cr.GetPlacedCoordinate();
            // if you're right near where you entered the map, despawn
            if (Tiles.Distance(X, Y, Z, x, y, z) <= 1)
            {
                Debug.WriteLine("Screw you guys, I'm going home.");
                actor.Spend();
                cr.Despawn();
            }
            else
            {
                actor.WalkToward(x, y, z);
            }
        }

        //}

        public GameEvent OnDespawn(GameEvent ge)
        {
            DespawnEvent de = (DespawnEvent)ge;
            if (de.Entity is Creature && de.Entity.Spawned && SiegeCreatures.Contains((int)de.Entity.EID!))
            {
                SiegeCreatures.Remove((int)de.Entity.EID!);
            }
            return ge;
        }

        public GameEvent OnTurnBegin(GameEvent ge)
        {
            TurnsSince += 1;
            //if (Options.NoHumanAttacks)
            //{
            //    return ge;
            //}
            if (TurnsSince > 1500 && GameState.World!.Random.Next(100) == 0)
            {
                BanditAttack();
            }
            return ge;
        }

        public void BanditAttack(bool debugCloser = false)
        {
            var world = GameState.World!;
            FrustrationAnnounced = false;
            Frustration = 0;
            TurnsSince = 0;
            SiegeCreatures.Clear();
            bool xwall = (world.Random.Next(2) == 0);
            bool zero = (world.Random.Next(2) == 0);
            string dir = "";
            if (xwall)
            {
                dir = (zero) ? "western" : "eastern";
            }
            else
            {
                dir = (zero) ? "northern" : "southern";
            }
            InterfaceState.Splash(new List<ColoredText> {
                   "A gang of bandits has been spotted near the " + dir + " border of your domain.",
                   " ",
                    "They must be coming to loot your supplies.  You should either hide behind sturdy doors, or kill them and take their ill-gotten loot for your own."
                },
                sleep: 2500,
                callback: InterfaceState.ResetControls,
                logText: "{red}A gang of bandits approaches from the " + dir + " border!"
            );
            int x0, y0;
            if (xwall)
            {
                x0 = (zero) ? 1 : world.Width - 2;
                y0 = world.Random.Next(world.Height - 2) + 1;
                if (debugCloser)
                {
                    x0 = (zero) ? 75 : 180;
                    y0 = world.Random.Next(world.Height - 75) + 75;
                }
            }
            else
            {
                y0 = (zero) ? 1 : world.Height - 2;
                x0 = world.Random.Next(world.Width - 2) + 1;
                if (debugCloser)
                {
                    y0 = (zero) ? 75 : 180;
                    x0 = world.Random.Next(world.Width - 75) + 75;
                }
            }
            // for repeatable testing
            //x0 = 12;
            //y0 = 12;

            for (int i = 0; i < PastSieges + 2; i++)
            {
                //string creature = (i % 3 == 2) ? "WolfHound" : "HumanBandit";
                Coord? cc = Creature.FindPlace(x0, y0, 0);
                if (cc != null)
                {
                    Coord c = (Coord)cc;
                    var bandit = Bandit.SpawnSiegeBandit();
                    //var bandit = Entity.Spawn<Bandit>();
                    bandit.PlaceInValidEmptyTile(c.X, c.Y, c.Z);
                    SiegeCreatures.Add((int)bandit.EID!);
                    Activity.BesiegePlayer.Act(bandit.GetComponent<Actor>(), bandit);
                    // do they occasionally get placed one step underground?
                    Debug.WriteLine($"{bandit.Describe()} placed at {bandit.X} {bandit.Y}");
                    if (i == 0)
                    {
                        // setting it here shoudl make sure it's walkable
                        EntryTile = c;
                        Item loot = Item.SpawnNewResource(Resource.Gold, 1);
                        bandit.GetComponent<Inventory>().GrantItem(loot);
                    }
                }

            }
            PastSieges += 1;
        }
    }
}