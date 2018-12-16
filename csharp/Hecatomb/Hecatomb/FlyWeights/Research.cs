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

    public class ResearchMenuListing : IMenuListable
    {
        Research MyResearch;
        Structure MyStructure;
        public ResearchMenuListing(Research r, Structure s)
        {
            MyResearch = r;
            MyStructure = s;
        }

        public ColoredText ListOnMenu()
        {

            //you'd want to check for a path between the structure and the ingredients
            if (MyResearch.Ingredients.Count == 0)
            {
                return MyResearch.Name;
            }
            else
            {
                bool available = false;
                if (Game.World.Player.GetComponent<Movement>().CanFindResources(MyResearch.Ingredients))
                {
                    available = true;
                }
                return (((available) ? "{white}" : "{gray}") + MyResearch.Name + " ($: " + Resource.Format(MyResearch.Ingredients) + ")");
            }
        }

        public void ChooseFromMenu()
        {
            // maybe this object could just be research tasks?
            if (Game.World.Player.GetComponent<Movement>().CanFindResources(MyResearch.Ingredients))
            {
                int x = MyStructure.X;
                int y = MyStructure.Y;
                int z = MyStructure.Z;
                ResearchTask research = new ResearchTask() { Makes = MyResearch.TypeName, Labor = MyResearch.Turns };
                // actually need to create a task for this...
                //MyStructure.Researching = MyResearch.TypeName;
                //MyStructure.ResearchTurns = MyResearch.Turns;
            }
            Game.MenuPanel.Dirty = true;
        }
    }

    public class Research : FlyWeight<Research>
    {
        public string Name;
        public Dictionary<string, int> Ingredients;
        public int Turns;

        public Research(string s) : base(s)
        {

        }

        public bool Researched
        {
            get
            {
                return Game.World.GetState<ResearchHandler>().Researched.Contains(Name);
            }
            set
            {
                if (value == false)
                {
                    Game.World.GetState<ResearchHandler>().Researched.Remove(Name);
                }
                else if (value == true)
                {
                    Game.World.GetState<ResearchHandler>().Researched.Add(Name);
                }
            }
        }



        public static Research FlintTools = new Research("FlintTools")
        {
            Name = "flint tools",
            Turns = 25,
            Ingredients = new Dictionary<string, int>() { { "Flint", 2 }, { "Wood", 2 } }
        };

        public static Research SpearTrap = new Research("SpearTrap")
        {
            Name = "spear trap",
            Turns = 25,
            Ingredients = new Dictionary<string, int>() { { "Flint", 1 }, { "Wood", 1 } }
        };


    }
}
