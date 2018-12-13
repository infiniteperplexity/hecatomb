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
        public PositionedEntity Entity;
        public int X;
        public int Y;
        public int Z;
    }

    public class RemoveEvent : GameEvent
    {
        public PositionedEntity Entity;
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
        public Dictionary<string, int> Modifiers;
    }

    public class AchievementEvent : GameEvent
    {
        public string Action;
    }

    public class SensoryEvent : GameEvent
    {
        public int X;
        public int Y;
        public int Z;
        public ColoredText Sight;
        public ColoredText Sound;

        public override void Fire()
        {
            // a lot more conditionals than this...
            Game.StatusPanel.PushMessage(Sight);
        }
    }
}
