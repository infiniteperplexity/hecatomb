using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Hecatomb
{
    using static HecatombAliases;

    public class GameEvent
    {

        public virtual void Fire()
        {

        }
    }

    public class PlaceEvent : GameEvent
    {
        public TileEntity Entity;
        public int X;
        public int Y;
        public int Z;
    }

    public class RemoveEvent : GameEvent
    {
        public TileEntity Entity;
        public int X;
        public int Y;
        public int Z;
    }

    public class SpawnEvent : GameEvent
    {
        public Entity Entity;
    }

    public class DespawnEvent : GameEvent
    {
        public Entity Entity;
    }

    public class TurnBeginEvent : GameEvent
    {
        public int Turn;
    }

    public class TurnEndEvent : GameEvent
    {
        public int Turn;
    }

    public class PlayerActionEvent : GameEvent
    {
        public string ActionType;
        public Dictionary<string, object> Details;
    }

    public class ContextChangeEvent : GameEvent
    {
        public string Note;
        public ControlContext OldContext;
        public ControlContext NewContext;
    }

    public class TutorialEvent : GameEvent
    {
        public string Action;
    }

    public class AttackEvent : GameEvent
    {
        public Attacker Attacker;
        public Defender Defender;
        public int Roll;
        public int EvasionModifier;
        public int DamageModifier;
        public int AccuracyModifier;
        public int ToughnessModifier;
        public int ArmorModifier;
    }

    public class AchievementEvent : GameEvent
    {
        public string Action;
    }

    public class PathChangeEvent : GameEvent
    {

    }

    public class SensoryEvent : GameEvent
    {
        public int X;
        public int Y;
        public int Z;
        public ColoredText Sight;
        public ColoredText Sound;
        public int SoundRange;

        public SensoryEvent()
        {
            X = -1;
            Y = -1;
            Z = -1;
        }
        public SensoryEvent(ColoredText sight, int x, int y, int z)
        {
            Sight = sight;
            X = x;
            Y = y;
            Z = z;
        }
        public override void Fire()
        {
            if (X == -1 && Y == -1 && Z == -1)
            {
                Debug.WriteLine(Sight);
                Debug.WriteLine(Sound);
                throw new InvalidOperationException("We shouldn't have sensory events without locations.");
            }
            Coord c = new Coord(X, Y, Z);
            // should make this an actual event
            if (OldGame.Visible.Contains(c))
            {
                OldGame.InfoPanel.PushMessage(Sight);
            }
            else
            {
                if (Sound != null)
                {
                    bool audible = false;
                    if (Tiles.QuickDistance(Player.X, Player.Y, Player.Z, X, Y, Z) < SoundRange)
                    {
                        audible = true;
                    }
                    foreach (Creature cr in GetState<TaskHandler>().Minions)
                    {
                        if (Tiles.QuickDistance(cr.X, cr.Y, cr.Z, X, Y, Z) < SoundRange)
                        {
                            audible = true;
                        }
                    }
                    if (audible)
                    {
                        OldGame.InfoPanel.PushMessage(Sound);
                    }
                }
            }
        }
    }

    public class ActEvent : GameEvent
    {
        public Actor Actor;
        public Entity Entity;
        public string Step;
    }

    public class DestroyEvent : GameEvent
    {
        public Entity Entity;
        public string Cause;
    }

    public class DigEvent : GameEvent
    {
        public int X;
        public int Y;
        public int Z;
        public string EventType;
    }

}
