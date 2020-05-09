using System;
using System.Collections.Generic;
using System.Text;

namespace Hecatomb8
{
    using static HecatombAliases;
    class Dryad : Creature
    {
        public Dryad()
        {
            _name = "dryad";
            _fg = "green";
            _symbol = 'n';
            Species = Species.Elemental;
            GetPrespawnComponent<Actor>().Team = Team.Hostile;
        }

        public override void Destroy(string? cause = null)
        {
            Publish(new AchievementEvent() { Action = "KilledDryad" });
            base.Destroy(cause);
        }

    }
}
