using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Hecatomb8
{
    using static HecatombAliases;
    public class ChirurgyTask : Task, ISelectsTile, IMenuListable
    {
        public ChirurgyTask() : base()
        {
            _name = "mend zombie at chirurgeon";
            RequiresStructures = new List<Type>() { typeof(Chirurgeon) };
            _bg  = "pink";
            Ingredients = new JsonArrayDictionary<Resource, int>() { { Resource.Flesh, 1 }, { Resource.Bone, 1 } };
            WorkSameTile = true;
        }

        public override void ChooseFromMenu()
        {
            var c = new SelectTileControls(this);
            c.SelectedMenuCommand = "Jobs";
            c.InfoMiddle = new List<ColoredText>() { "{green}Mend zombie at chirurgeon." };
            InterfaceState.SetControls(c);
        }

        public override void TileHover(Coord c)
        {
            var co = InterfaceState.Controls;
            co.InfoMiddle.Clear();
            var cr = Creatures.GetWithBoundsChecked(c.X, c.Y, c.Z);
            if (cr != null && cr.HasComponent<Minion>())
            {
                co.InfoMiddle = new List<ColoredText>() { "{green}" + String.Format("Mend wounds of zombie at chirurgeon.") };
            }
            else
            {
                co.InfoMiddle = new List<ColoredText>() { "{orange} No minion to heal." };
            }
        }

        public override bool ValidTile(Coord c)
        {
            var cr = Creatures.GetWithBoundsChecked(c.X, c.Y, c.Z);
            if (cr != null && cr.HasComponent<Minion>())
            {
                return true;
            }
            return false;
        }

        public override void SelectTile(Coord c)
        {
            CommandLogger.LogCommand(command: "MendingTask", x: c.X, y: c.Y, z: c.Z);
            var cr = Creatures.GetWithBoundsChecked(c.X, c.Y, c.Z);
            if (cr is null)
            {
                return;
            }
            if (ValidTile(c))
            {
                var structures = Structure.ListStructures();
                Chirurgeon? ch = null;
                foreach (var st in structures)
                {
                    if (st is Chirurgeon)
                    {
                        ch = (Chirurgeon)st;
                    }
                }
                if (ch != null && ch.Placed)
                {
                    var (x, y, z) = ch.GetValidCoordinate();
                    var t = Tasks.GetWithBoundsChecked(x, y, z);
                    if (t == null)
                    {
                        ChirurgyTask mending = Entity.Spawn<ChirurgyTask>();
                        mending.PlaceInValidEmptyTile(x, y, z);
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
            if (!cr.HasComponent<Defender>())
            {
                return false;
            }
            Defender d = cr.GetComponent<Defender>();
            if (d.Wounds > 0)
            {
                return true;
            }
            return false;
        }

        public override void Finish()
        {
            if (Worker?.UnboxBriefly() is null)
            {
                return;
            }
            var cr = Worker.UnboxBriefly()!;
            Defender d =cr.GetComponent<Defender>();
            d.Wounds = 0;
            if (cr is Zombie)
            {
                (cr as Zombie)!.Decay = (cr as Zombie)!.MaxDecay;
            }
            base.Finish();
        }

        public override void Start()
        {
            // do nothing special
        }
    }
}
