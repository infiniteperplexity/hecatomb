/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/18/2018
 * Time: 1:03 PM
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Hecatomb
{
    using static HecatombAliases;
    /// <summary>
    /// Stupid name, this actually means declare ownership
    /// </summary>
    public class MendingTask : Task, ISelectsTile, IMenuListable
    {
        public MendingTask() : base()
        {
            MenuName = "mend zombie at chirurgeon";
            PrereqStructures = new List<string>() { "Chirurgeon" };
            BG = "pink";
            Ingredients = new Dictionary<string, int>() { { "Flesh", 1 }, { "Bone", 1 } };
            WorkRange = 0;
        }

        public override void ChooseFromMenu()
        {
            var c = new SelectTileControls(this);
            c.SelectedMenuCommand = "Jobs";
            c.MenuCommandsSelectable = false;
            ControlContext.Set(c);
        }

        public override void TileHover(Coord c)
        {
            var co = Game.Controls;
            co.MenuMiddle.Clear();
            var cr = Creatures[c];
            if (cr != null && cr.TryComponent<Minion>() != null)
            {
                co.MenuMiddle = new List<ColoredText>() { "{green}" + String.Format("Mend wounds of zombie at chirurgeon.") };
            }
            else
            {
                co.MenuMiddle = new List<ColoredText>() { "{orange} No minion to heal." };
            }
        }

        public override bool ValidTile(Coord c)
        {
            var cr = Creatures[c];
            if (cr != null && cr.TryComponent<Minion>() != null)
            {
                return true;
            }
            return false;
        }
       
        public override void SelectTile(Coord c)
        {
            CommandLogger.LogCommand(command: "MendingTask", x: c.X, y: c.Y, z: c.Z);
            var cr = Creatures[c];
            if (ValidTile(c))
            {
                var structures = Structure.ListStructures();
                Chirurgeon ch = null;
                foreach (var st in structures)
                {
                    if (st is Chirurgeon)
                    {
                        ch = (Chirurgeon) st;
                    }
                }
                if (ch != null)
                {
                    var (x, y, z) = ch;
                    var t = Tasks[x, y, z];
                    if (t == null)
                    {
                        MendingTask mending = Entity.Spawn<MendingTask>();
                        mending.Place(x, y, z);
                        if (mending.CanAssign(cr))
                        {
                            mending.AssignTo(cr);
                        }
                    }        
                }
            }
        }

        public override bool CanAssign(Creature cr)
        {
            Defender d = cr.TryComponent<Defender>();
            if (d != null && d.Wounds > 0)
            {
                return true;
            }
            return false;
        }

        public override void Finish()
        {
            Defender d = Worker.Unbox().TryComponent<Defender>();
            if (d != null)
            {
                d.Wounds = 0;
            }
            base.Finish();
        }

        public override void Start()
        {
            // do nothing special
        }
    }
}
