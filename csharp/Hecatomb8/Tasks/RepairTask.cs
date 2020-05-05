using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hecatomb8
{
    using static HecatombAliases;
    public class RepairTask : Task, IMenuListable
    {
        public RepairTask() : base()
        {
            _name = "repair";
            Priority = 4;
            RequiresStructures = new List<Type> { typeof(Workshop) };
            _bg = "yellow";
        }

        protected override string getName()
        {
            if (!Placed)
            {
                if (Makes == typeof(Structure))
                {
                    return "repair or complete structure";
                }
                else if (Makes == typeof(Feature))
                {
                    return "repair or complete fixture";
                }
            }
            Feature? f = Features.GetWithBoundsChecked((int)X!, (int)Y!, (int)Z!);
            if (f is null)
            {
                return _name!;
            }
            else 
            return "repair " + f.Describe();
        }

        public override void Start()
        {
            // don't place a new feature
        }
        public override void Finish()
        {
            if (!Placed)
            {
                return;
            }
            Feature? f = Features.GetWithBoundsChecked((int)X!, (int)Y!, (int)Z!);
            if (f != null && f.HasComponent<Defender>())
            {
                f.GetComponent<Defender>().Wounds = 0;
            }
            Complete();
        }

        public override void ChooseFromMenu()
        {
            var c = new SelectTileControls(this);
            c.SelectedMenuCommand = "Jobs";
            c.InfoMiddle = new List<ColoredText>() { "{green}Repair or complete " + ((Makes == typeof(Structure)) ? "structure." : "fixture.") };
            InterfaceState.SetControls(c);
        }

        public override bool ValidTile(Coord c)
        {
            var (x, y, z) = c;
            // what about non-harvestable, i.e. owned features?
            // maybe make those the very lowest priority?
            // in order to avoid giving away unexplored terrain, always allow designation
            if (!Explored.Contains(c) && !HecatombOptions.Explored)
            {
                return false;
            }
            Feature? f = Features.GetWithBoundsChecked(x, y, z);
            if (f is null)
            {
                return false;
            }
            if (f is IncompleteFixture)
            {
                return true;
            }
            else if (f.HasComponent<Defender>() && f.GetComponent<Defender>().Wounds > 0)
            {
                Debug.WriteLine("Yep, totally damaged");
                return true;
            }
            // a structure that wasn't completely finished
            else if (f is StructuralFeature)
            {
                return true;
            }
            return false;
        }

        public override void TileHover(Coord c)
        {
            var co = InterfaceState.Controls;
            co.InfoMiddle.Clear();
            if (ValidTile(c))
            {
                Feature? f = Features.GetWithBoundsChecked(c.X, c.Y, c.Z);
                if (f != null)
                {
                    co.InfoMiddle = new List<ColoredText>() { "{green}" + String.Format("Repair or complete {3} at {0} {1} {2}", c.X, c.Y, c.Z, f.Describe()) };
                }
            }
            else
            {
                co.InfoMiddle = new List<ColoredText>() { "{orange}Nothing to repair or complete here." };
            }
        }

        public JsonArrayDictionary<Resource, int> GetRepairIngredients(Feature f)
        {
            // charge full cost for repairs for now
            return f.GetComponent<Fixture>().Ingredients;
        }
        // does this stuff ever get used?  some of the logic seems kind of nonsensical
        public override void SelectTile(Coord c)
        {
            if (Tasks.GetWithBoundsChecked(c.X, c.Y, c.Z) is null && ValidTile(c))
            {
                Feature? f = Features.GetWithBoundsChecked(c.X, c.Y, c.Z);
                if (f is null)
                {
                    return;
                }
                if (f is IncompleteFixture)
                {
                    var ifc = (IncompleteFixture)f;
                    Type makes = ifc.Makes!;

                    if (ifc.Structure != null)
                    {
                        // this doesn't quite work...there's no tracking of which structure it was attached to
                        ifc.Structure.UnboxBriefly()!.BuildInSquares();
                    }
                    else
                    {
                        Task task = Entity.Spawn<FurnishTask>();
                        task.Makes = makes;
                        task.PlaceInValidEmptyTile(c.X, c.Y, c.Z);
                    }
                    // what the heck is this for?
                    //string json = EntityType.Types[Makes].Components["Fixture"];
                    //JObject obj = JObject.Parse(json);

                }
                // not quite sure how to test this yet
                else if (f.HasComponent<Defender>() && f.GetComponent<Defender>().Wounds > 0)
                {
                    base.SelectTile(c); // place the task using default logic
                    Task? task = Tasks.GetWithBoundsChecked(c.X, c.Y, c.Z);
                    if (task != null && task is RepairTask)
                    {
                        task.Ingredients = GetRepairIngredients(f);
                    }
                }
                // a structure that wasn't completely finished
                else if (f is StructuralFeature)
                {
                    Structure? s = (f as StructuralFeature)!.Structure?.UnboxBriefly();
                    if (s != null)
                    {
                        s.BuildInSquares();
                    }
                }
            }
        }
    }
}

