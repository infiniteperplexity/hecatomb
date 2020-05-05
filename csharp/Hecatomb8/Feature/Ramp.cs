using System;
using System.Collections.Generic;
using System.Text;

namespace Hecatomb8
{
    using static HecatombAliases;
    class Ramp : Feature
    {
        public Ramp()
        {
            _name = "ramp";
            _fg = "WALLFG";
            _symbol = '^';
            Components.Add(new Harvestable());
            Components.Add(new Fixture()
            {
                Ingredients = new JsonArrayDictionary<Resource, int>() { { Resource.Rock, 2 } },
                RequiresStructures = new Type[] { typeof(Workshop) }
            });
            AddListener<AfterPlaceEvent>(AfterPlace);
        }

        public GameEvent AfterPlace(GameEvent ge)
        {
            AfterPlaceEvent pe = (AfterPlaceEvent)ge;
            if (pe.Entity == this)
            {
                Despawn();
                Cover.ClearGroundCover(pe.X, pe.Y, pe.Z);
                Terrains.SetWithBoundsChecked(pe.X, pe.Y, pe.Z, Terrain.UpSlopeTile);
                if (Terrains.GetWithBoundsChecked(pe.X, pe.Y, pe.Z + 1) != Terrain.VoidTile)
                {
                    Terrains.SetWithBoundsChecked(pe.X, pe.Y, pe.Z + 1, Terrain.DownSlopeTile);
                }
            }
            return ge;
        }
    }
}
