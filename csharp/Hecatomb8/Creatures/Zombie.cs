using System;
using System.Collections.Generic;
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
            Species = Species.Zombie;
            CorpseSpecies = Species.Human;
            LeavesCorpse = false;
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
            if (CorpseSpecies == Species.Human)
            {
                return _name;
            }
            else
            {
                return $"{ CorpseSpecies.Name} {_name}";
            }
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

        public override void Destroy(string? cause = null)
        {
            if (LeavesCorpse && Placed && Spawned)
            {
                var (x, y, z) = GetValidCoordinate();
                if (GameState.World!.Random.Next(2)==0)
                {
                    Item.SpawnNewResource(Resource.Bone, 1).PlaceInValidEmptyTile(x, y, z);
                }
                else
                {
                    Item.SpawnNewResource(Resource.Flesh, 1).PlaceInValidEmptyTile(x, y, z);
                }
                
            }
            base.Destroy(cause);
        }
    }
}
