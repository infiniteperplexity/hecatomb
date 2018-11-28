/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/17/2018
 * Time: 12:33 PM
 */
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

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
		
		public Structure()
		{
			Width = 3;
            Height = 3;
			Features = new Feature[Width*Height];
            Ingredients = new Dictionary<string, int>[Width*Height];
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
                return new List<IMenuListable>();
            }
            set { }
        }

        [JsonIgnore]
        public List<bool> ValidChoices
        {
            get
            {
                return new List<bool>();
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
