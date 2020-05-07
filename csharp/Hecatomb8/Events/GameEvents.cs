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

    public class GameEvent
    {
        public virtual void Fire()
        {

        }
    }

    public class AfterPlaceEvent : GameEvent
    {
        public TileEntity? Entity;
        public int X;
        public int Y;
        public int Z;
    }

    public class BeforePlaceEvent : GameEvent
    {
        public TileEntity? Entity;
        public int X;
        public int Y;
        public int Z;
    }

    public class StepEvent : GameEvent
    {
        public Creature? Entity;
        public int X;
        public int Y;
        public int Z;
    }

    public class RemoveEvent : GameEvent
    {
        public TileEntity? Entity;
        public int X;
        public int Y;
        public int Z;
    }

    public class SpawnEvent : GameEvent
    {
        public Entity? Entity;
    }

    public class DespawnEvent : GameEvent
    {
        public Entity? Entity;
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
        public string? ActionType;
        public Dictionary<string, object>? Details;
    }

    public class ContextChangeEvent : GameEvent
    {
        public string? Note;
        public ControlContext? OldContext;
        public ControlContext? NewContext;
    }

    public class TutorialEvent : GameEvent
    {
        public string? Action;
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
        public string? Action;
    }

    public class PathChangeEvent : GameEvent
    {

    }

    public class ActEvent : GameEvent
    {
        public Actor? Actor;
        public Entity? Entity;
        public string? Step;
    }

    public class DestroyEvent : GameEvent
    {
        public Entity? Entity;
        public string? Cause;
    }

    public class DigEvent : GameEvent
    {
        public int X;
        public int Y;
        public int Z;
        public string? EventType;
    }

}
