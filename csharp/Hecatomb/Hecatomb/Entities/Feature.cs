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

    public class Feature : TypedEntity
    {

        public bool Solid;
        public bool Owned;

        public override void Place(int x1, int y1, int z1, bool fireEvent = true)
        {
            Feature e = Features[x1, y1, z1];
            if (e == null)
            {
                Features[x1, y1, z1] = this;
                base.Place(x1, y1, z1, fireEvent);
            }
            else
            {
                throw new InvalidOperationException(String.Format(
                    "Cannot place {0} at {1} {2} {3} because {4} is already there.", TypeName, x1, y1, z1, e.TypeName
                ));
            }
        }
        public override void Remove()
        {
            int x0 = X;
            int y0 = Y;
            int z0 = Z;
            base.Remove();
            Features[x0, y0, z0] = null;
        }

        public override string GetCalculatedBG()
        {
            if (ControlContext.Selection is Structure)
            {
                var str = TryComponent<StructuralComponent>();
                if (str != null && str.Structure.Unbox() == ControlContext.Selection)
                {
                    return "lime green";
                }
            }
            return base.GetCalculatedBG();
        }


        public override string GetDisplayName()
        {
            var defend = TryComponent<Defender>();
            int wounds = (defend == null) ? 0 : defend.Wounds;
            if (wounds >= 6)
            {
                return ("severely damaged " + base.GetDisplayName());
            }
            else if (wounds >= 4)
            {
                return ("damaged " + base.GetDisplayName());
            }
            else if (wounds >= 2)
            {
                return ("slightly damaged " + base.GetDisplayName());
            }
            return base.GetDisplayName();
        }

        // are these good defaults?
        public static Coord? FindPlace(int x, int y, int z, int max = 5, int min = 0, bool groundLevel = true, int expand = 0)
        {
            return Tiles.NearbyTile(x, y, z, max: max, min: min, groundLevel: groundLevel, expand: expand, valid: (fx, fy, fz) => { return 
                (Features[fx, fy, fz] == null)
                && (!Covers[fx, fy, fz].Liquid)
                && (Terrains[fx, fy, fz] == Terrain.FloorTile)
                ; });
        }
    }
}
