using System;
using System.Collections.Generic;
using System.Text;

namespace Hecatomb8
{
    using static HecatombAliases;
    public class Spider : Creature
    {
        public Spider()
        {
            _name = "spider";
            _fg = "white";
            _symbol = 's';
            GetPrespawnComponent<Actor>().Activities = new List<Activity>() { Activity.Spider };
        }

        public static void _spin(Actor a, Creature cr)
        {
            if (GameState.World!.Random.Next(250) == 0)
            {
                var (x, y, z) = cr.GetValidCoordinate();
                if (Features.GetWithBoundsChecked(x, y, z) is null && Tasks.GetWithBoundsChecked(x, y, z) is null && Terrains.GetWithBoundsChecked(x, y, z) == Terrain.FloorTile)
                {
                    Entity.Spawn<SpiderWeb>().PlaceInValidEmptyTile(x, y, z);
                    a.Spend(16);
                }
            }
        }
    }

    public partial class Activity
    {
        public static readonly Activity Spin = new Activity(
            type: "Spin",
            act: Hecatomb8.Spider._spin
        );
        public static readonly Activity Spider = new Activity(
            type: "Spider",
            act: (a, cr) =>
            {
                if (!a.Acted)
                {
                    Alert.Act(a, cr);
                }
                if (!a.Acted)
                {
                    Seek.Act(a, cr);
                }
                if (!a.Acted)
                {
                    Spin.Act(a, cr);
                }
                if (!a.Acted)
                {
                    Wander.Act(a, cr);
                }
            }
        );
    }
}
