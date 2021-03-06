﻿/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 9/18/2018
 * Time: 12:41 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Hecatomb
{
	public class EntityType
	{
		public static Dictionary<string, EntityType> Types = new Dictionary<string, EntityType>();
		public string TypeName;
		public string Name;
		public string Species;
//		public string[] Components;
		public string FG;
		public string BG;
		public char Symbol;
        public bool Solid;
		public bool Plural;

		public Dictionary<string, string> Components;
		/// <summary>
		/// Description of EntityType.
		/// </summary>
		public EntityType(string s)
		{
			TypeName = s;
			Types[s] = this;
			Symbol = '@';
			FG = "White";
			Components = new Dictionary<string, string>();
		}
			
		public void Standardize(TypedEntity e)
		{
			e.TypeName = TypeName;
			e.Name = Name;
			/// oh god...the system is so fucked
			//if (e is Creature)
			//{
			//	Creature cr = (Creature)e;
			//	cr.Species = Species;
			//}
			e.FG = FG;
			e.Symbol = Symbol;
			e.BG = BG;
			e.Plural = Plural;
		}
		public void Typify(TypedEntity e)
		{
			e.TypeName = TypeName;
			e.Name = Name;
			if (e is Creature)
			{
				Creature cr = (Creature)e;
				cr.Species = Species;
			}
			e.FG = FG;
			e.BG = BG;
			e.Symbol = Symbol;
			e.Plural = Plural;
            if (e is Feature)
            {
                (e as Feature).Solid = Solid;
            }
			foreach (string t in Components.Keys)
			{
				Type T = Type.GetType("Hecatomb." + t);
                if (Type.GetType("Hecatomb." + t)==null)
                {
                    Debug.WriteLine($"Type {t} doesn't exist.");
                }
				Component c = (Component)Hecatomb.Entity.Spawn(Type.GetType("Hecatomb." + t));	
				c.AddToEntity(e);
				c.InterpretJSON(Components[t]);
			}
		}

        public void MockTypify(TypedEntity e)
        {
            e.TypeName = TypeName;
            e.Name = Name;
			if (e is Creature)
			{
				Creature cr = (Creature)e;
				cr.Species = Species;
			}
            e.FG = FG;
            e.BG = BG;
            e.Symbol = Symbol;
			e.Plural = Plural;
            if (e is Feature)
            {
                (e as Feature).Solid = Solid;
            }
            foreach (string t in Components.Keys)
            {
                Type T = Type.GetType("Hecatomb." + t);
                Component c = (Component)Entity.Mock(Type.GetType("Hecatomb." + t));
                // questionable...
                c.AddToMockEntity(e);
                c.InterpretJSON(Components[t]);
            }
        }


        public static void LoadEntities()
		{
			// dynamically load creature types from JSON
			string f, json;
			JObject obj;
			f = @"Content/Creatures.json";
			json = File.ReadAllText(f);
			obj = JObject.Parse(json);
			foreach (var t in obj["Types"])
			{
				
				EntityType et = new EntityType((string) t["Type"]);
				et.Name = (string) t["Name"];
				et.FG = (string) t["FG"];
				et.Symbol = (char) t["Symbol"];
				et.Species = (t["Species"] != null) ? (string)t["Species"] : (string)t["Type"];
				et.Plural = (t["Plural"] == null) ? false : (bool)t["Plural"];
				foreach (JProperty comp in (JToken) t["Components"])
				{
					string name = (string) comp.Name;
					et.Components[name] = comp.Value.ToString();
				}
			}
			f = @"Content/Features.json";
			json = File.ReadAllText(f);
			obj = JObject.Parse(json);
			foreach (var t in obj["Types"])
			{
				EntityType et = new EntityType((string)t["Type"]);
				et.Name = (string)t["Name"];
				et.FG = (string)t["FG"];
				et.Symbol = (char)t["Symbol"];
				et.Plural = (t["Plural"] == null) ? false : (bool)t["Plural"];
				if (t["Solid"] != null)
				{
					et.Solid = (bool)t["Solid"];
				}
				foreach (JProperty comp in (JToken)t["Components"])
				{
					string name = (string)comp.Name;
					et.Components[name] = comp.Value.ToString();
				}
			}
		}
	}
}