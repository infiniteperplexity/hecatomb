using System;
using System.Collections.Generic;
using System.Text;

namespace Hecatomb8
{
    using static HecatombAliases;
    class Zombie : Creature
    {
        public Species CorpseSpecies;
        public Zombie()
        {
            _name = "zombie";
            _symbol = 'z';
            _fg = "lime green";
            CorpseSpecies = Species.Human;
        }

        protected override char getSymbol()
        {
            if (CorpseSpecies == Species.Human)
            {
                return 'z';
            }
            else
            {
                return CorpseSpecies.Symbol;
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
    }
}
