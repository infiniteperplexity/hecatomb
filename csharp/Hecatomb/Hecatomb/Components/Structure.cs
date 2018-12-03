/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/17/2018
 * Time: 12:33 PM
 */
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Hecatomb
{
	/// <summary>
	/// Description of Structure.
	/// </summary>
	public abstract class Structure : Component, IChoiceMenu
	{
		public string MenuName;
		public string Name;
		public int Width;
		public int Height;
		public char[] Symbols;
		public string[] FGs;
		public string[] BGs;
        public string BG;
		public Feature[] Features;
        public Dictionary<string, int>[] Ingredients;
        public string[] Researches;
        public string Researching;
        public int ResearchTurns;
		
		public Structure()
		{
			Width = 3;
            Height = 3;
			Features = new Feature[Width*Height];
            Ingredients = new Dictionary<string, int>[Width*Height];
            AddListener<TurnBeginEvent>(OnTurnBegin);
		}
		public virtual void Standardize()
		{
			Type structureType = this.GetType();
			Structure structure = (Structure) Activator.CreateInstance(structureType);
		}

        public Dictionary<string, int> GetIngredients()
        {
            var ingredients = new Dictionary<string, int>();
            foreach (var resources in Ingredients)
            {
                if (resources!=null)
                {
                    foreach (string resource in resources.Keys)
                    {
                        if (!ingredients.ContainsKey(resource))
                        {
                            ingredients[resource] = 0;
                        }
                        ingredients[resource] += resources[resource];
                    }
                }
            }
            return ingredients;
        }

        public GameEvent OnTurnBegin(GameEvent ge)
        {
            if (Researching != null)
            {
                ResearchTurns -= 1;
                if (ResearchTurns == 0)
                {
                    var researched = Game.World.GetTracker<ResearchTracker>().Researched;
                    if (!researched.Contains(Researching))
                    {
                        researched.Add(Researching);
                    }
                    Researching = null;
                }
            }
            return ge;
        }

        public virtual string MenuHeader
        {
            get
            {
                return String.Format("{3} at {0} {1} {2}", Entity.X, Entity.Y, Entity.Z, Name);
            }
            set { }
        }

        public virtual List<IMenuListable> MenuChoices
        {
            get
            {
                var list = new List<IMenuListable>();
                foreach (string s in Researches)
                {
                    list.Add(new ResearchMenuListing(Research.Types[s], this));
                }
                return list;
            }
            set { }
        }

        public void Highlight(string s)
        {
            foreach (Feature fr in Features)
            {
                fr.Highlight = s;
            }
        }

        public void Highlight()
        {
            Highlight("lime green");
        }
        public void Unhighlight()
        {
            foreach (Feature fr in Features)
            {
                fr.Highlight = null;
            }
        }

	}
}
