using System;
using System.Collections.Generic;
using System.Text;

namespace Hecatomb8
{
    class Bandit : Creature
    {
        public Bandit()
        {
            _name = "bandit";
            _fg = "brown";
            _symbol = '@';
            AddComponent(new Inventory());
            GetPrespawnComponent<Actor>().Team = Team.Evil;
        }

        public static Bandit SpawnSiegeBandit()
        {
            var bandit = Entity.Spawn<Bandit>();
            bandit.GetPrespawnComponent<Actor>().Activities = new List<Activity>()
            {
                Activity.TargetPlayer,
                Activity.Alert,
                Activity.Seek,
                Activity.Vandalize,
                Activity.Seek,
                Activity.Wander
            };
            return bandit;
        }
    }
}
