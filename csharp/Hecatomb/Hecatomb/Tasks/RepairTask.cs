
/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/18/2018
 * Time: 12:50 PM
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hecatomb
{
    public class RepairTask : Task, IMenuListable
    {
        public RepairTask() : base()
        {
            MenuName = "repair or complete fixture";
            Priority = 4;
            PrereqStructures = new List<string> { "Workshop" };
        }

        public override void Start()
        {
            // don't place a new feature
        }
        public override void Finish()
        {
            Feature f = Game.World.Features[X, Y, Z];
            if (f != null && f.TryComponent<Defender>() != null)
            {
                f.GetComponent<Defender>().Wounds = 0;
            }
            Complete();
        }

        public override void ChooseFromMenu()
        {
            Game.Controls.Set(new SelectTileControls(this));
        }

        public override bool ValidTile(Coord c)
        {
            var (x, y, z) = c;
            // what about non-harvestable, i.e. owned features?
            // maybe make those the very lowest priority?
            // in order to avoid giving away unexplored terrain, always allow designation
            if (!Game.World.Explored.Contains(c) && !Game.Options.Explored)
            {
                return false;
            }
            Feature f = Game.World.Features[x, y, z];
            if (f == null)
            {
                return false;
            }
            if (f.TryComponent<IncompleteFixtureComponent>() != null)
            {
                return true;
            }
            else if (f.TryComponent<Defender>() != null && f.GetComponent<Defender>().Wounds > 0)
            {
                Debug.WriteLine("Yep, totally tamaged");
                return true;
            }
            // a structure that wasn't completely finished
            else if (f.TryComponent<StructuralComponent>() != null)
            {
                return true;
            }
            return false;
        }

        public override void TileHover(Coord c)
        {
            var co = Game.Controls;
            co.MenuMiddle.Clear();
            if (ValidTile(c))
            {
                Feature f = Game.World.Features[c];
                co.MenuMiddle = new List<ColoredText>() { "{green}" + String.Format("Repair or complete {3} at {0} {1} {2}", c.X, c.Y, c.Z, f.Describe()) };
            }
            else
            {
                co.MenuMiddle = new List<ColoredText>() { "{orange} Nothing to repair or complete here." };
            }
        }

        public Dictionary<string, int> GetRepairIngredients(Feature f)
        {
            // charge full cost for repairs for now
            return f.GetComponent<Fixture>().Ingredients;
        }
        public override void SelectTile(Coord c)
        {
            if (Game.World.Tasks[c.X, c.Y, c.Z] == null && ValidTile(c))
            {
                Feature f = Game.World.Features[c.X, c.Y, c.Z];
                if (f.TryComponent<IncompleteFixtureComponent>() != null)
                {
                    var ifc = f.GetComponent<IncompleteFixtureComponent>();
                    string makes = ifc.Makes;
                    
                    if (ifc.Structure!=null)
                    {
                        // this doesn't quite work...there's no tracking of which structure it was attached to
                        ifc.Structure.Unbox().BuildInSquares();
                    }
                    else
                    {
                        Task task = Entity.Spawn<FurnishTask>();
                        task.Makes = makes;
                        task.Place(c.X, c.Y, c.Z);
                    }
                    // what the heck is this for?
                    //string json = EntityType.Types[Makes].Components["Fixture"];
                    //JObject obj = JObject.Parse(json);

                }
                // not quite sure how to test this yet
                else if (f.TryComponent<Defender>() != null && f.GetComponent<Defender>().Wounds > 0)
                {
                    base.SelectTile(c);
                    Debug.WriteLine("tried to place a repair task");
                    Task task = Game.World.Tasks[c.X, c.Y, c.Z];
                    task.Ingredients = GetRepairIngredients(f);
                }
                // a structure that wasn't completely finished
                else if (f.TryComponent<StructuralComponent>() != null)
                {
                    Debug.WriteLine("flag 2");
                    Structure s = f.GetComponent<StructuralComponent>().Structure;
                    s.BuildInSquares();
                }
            }
        }
    }
}

