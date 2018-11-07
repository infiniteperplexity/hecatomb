/*
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
//		public string[] Components;
		public string FG;
		public string BG;
		public char Symbol;
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
			
		public void Standardize(PositionedEntity e)
		{
			e.TypeName = TypeName;
			e.Name = Name;
			e.FG = FG;
			e.Symbol = Symbol;
			e.BG = BG;
		}
		public void Typify(PositionedEntity e)
		{
			e.TypeName = TypeName;
			e.Name = Name;
			e.FG = FG;
			e.BG = BG;
			e.Symbol = Symbol;
			foreach (string t in Components.Keys)
			{
				Type T = Type.GetType("Hecatomb." + t);
				Component c = (Component) Game.World.Entities.Spawn(Type.GetType("Hecatomb." + t));	
				c.AddToEntity(e);
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
				EntityType et = new EntityType((string) t["Type"]);
				et.Name = (string) t["Name"];
				et.FG = (string) t["FG"];
				et.Symbol = (char) t["Symbol"];
				foreach (JProperty comp in (JToken) t["Components"])
				{
					string name = (string) comp.Name;
					et.Components[name] = comp.Value.ToString();
				}
			}
			f = @"Content/Items.json";
			json = File.ReadAllText(f);
			obj = JObject.Parse(json);
			foreach (var t in obj["Types"])
			{
				EntityType et = new EntityType((string) t["Type"]);
				et.Name = (string) t["Name"];
				et.FG = (string) t["FG"];
				et.Symbol = (char) t["Symbol"];
				foreach (JProperty comp in (JToken) t["Components"])
				{
					string name = (string) comp.Name;
					et.Components[name] = comp.Value.ToString();
				}
			}
			// dynamically create a typed entity for each subclass of Task
			var tasks = typeof(Game).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Task))).ToList();
			foreach (var task in tasks)
			{
				EntityType t = new EntityType(task.Name);
				t.Name = task.Name;
				// do I need "mock" here to get the correct name?
				t.Components = new Dictionary<string, string>() {{task.Name, "{}"}};
			}
			// dynamically create a typed entity for each subclass of Structure
			var structures = typeof(Game).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Structure))).ToList();
			foreach (var structure in structures)
			{
				EntityType t = new EntityType(structure.Name);
				t.Components = new Dictionary<string, string>() {{structure.Name, "{}"}};
				EntityType tf = new EntityType(structure.Name + "Feature");
				tf.Name = "need to figure this out.";
				tf.Components = new Dictionary<string, string>();
			}
		}
	}
}