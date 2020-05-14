using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace Hecatomb8
{
    using static HecatombAliases;
    class Zombie : Creature
    {
        public Species CorpseSpecies;
        [JsonIgnore] public int MaxDecay;
        public int Decay;

        public Zombie()
        {
            _name = "zombie";
            _symbol = 'z';
            _fg = "lime green";
            Species = Species.Undead;
            CorpseSpecies = Species.Human;
            MaxDecay = 3000;
            Decay = MaxDecay;
            AddListener<TurnBeginEvent>(OnTurnBegin);
        }

        protected override string? getFG()
        {
            var fg = base.getFG();
            if (fg != _fg)
            {
                return fg;
            }
            double decay = (double)Decay / (double)MaxDecay;
            if (decay >= 0.75)
            {
                return _fg;
            }
            else if (decay >= 0.5)
            {
                return "olive";
            }
            else if (decay >= 0.25)
            {
                return "brown";
            }
            else
            {
                return "purple";
            }
        }
        public GameEvent OnTurnBegin(GameEvent ge)
        {
            Decay -= 1;
            if (Decay <= 0)
            {
                Destroy(cause: "Decay");
            }
            return ge;
        }

        protected override char getSymbol()
        {
            if (CorpseSpecies == Species.Human)
            {
                return 'z';
            }
            else
            {
                return CorpseSpecies.Symbol ?? ' ';
            }
        }

        protected override string? getName()
        {
            var name = _name;
            if (CorpseSpecies != Species.Human)
            {
                name = $"{ CorpseSpecies.Name} {name}";
            }
            if (!HasComponent<Defender>())
            {
                return _name;
            }
            int wounds = GetComponent<Defender>().Wounds;
            double rotten = (double)Decay / (double)MaxDecay;
            if (wounds >= 6)
            {
                return ("severely wounded " + name);
            }
            else if (rotten < 0.25)
            {
                return ("severely rotted " + name);
            }
            else if (wounds >= 4)
            {
                return ("wounded " + name);
            }
            else if (rotten < 0.5)
            {
                return ("rotted " + name);
            }
            else if (wounds >= 2)
            {
                return ("slightly wounded " + name);
            }
            else if (rotten < 0.75)
            {
                return ("slightly rotted " + name);
            }
            return name;
        }

        public static Zombie SpawnZombieMinion()
        {
            var zombie = Spawn<Zombie>();
            GetState<TaskHandler>().AddMinion(zombie);
            zombie.GetComponent<Actor>().Team = Team.Friendly;
            zombie.GetComponent<Actor>().Activities = new List<Activity>()
            {
                Activity.Alert,
                Activity.Seek,
                Activity.Minion,
                Activity.Wander
            };
            return zombie;
        }

        public override void Despawn()
        {
            base.Despawn();
        }
    }
}
