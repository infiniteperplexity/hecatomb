using System;
using System.Collections.Generic;
using System.Text;

namespace Hecatomb8
{
    using static HecatombAliases;
    class Flower : Feature
    {
        public Resource? Dye;
        public Flower()
        {
            _name = "flower";
            _fg = "white";
            _symbol = '\u2698';
            Components.Add(new Harvestable());
        }

        protected override string? getName()
        {
            return Dye?.Name;
        }
        protected override string? getFG()
        {
            return Dye?.FG;
        }

        public static Flower Spawn(Resource r)
        {
            Flower f = Spawn<Flower>();
            f.Dye = r;
            f.GetComponent<Harvestable>().Yields = new JsonArrayDictionary<Resource, float>(){ [r] = 1};
            return f;
        }

        public static Flower Mock(Resource r)
        {
            Flower f = Mock<Flower>();
            f.Dye = r;
            return f;
        }
    }
}
