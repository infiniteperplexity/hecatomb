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
//https://codereview.stackexchange.com/questions/1002/mapping-expandoobject-to-another-object-type

namespace Hecatomb
{
	public class EntityType
	{
		public static Dictionary<string, EntityType> Types = new Dictionary<string, EntityType>();
		public string Name;
		public string[] Components;
		public string FG;
		public char Symbol;
		/// <summary>
		/// Description of EntityType.
		/// </summary>
		public EntityType(string s)
		{
			Name = s;
			Types[s] = this;
			Symbol = '@';
			FG = "White";
		}
			
		public void Typify(TypedEntity e)
		{
			foreach (string t in Components)
			{
				e.FG = FG;
				e.Symbol = Symbol;
				Component c = (Component) Activator.CreateInstance(Type.GetType("Hecatomb." + t));
				c.AddToEntity(e);
				
			}
		}
		
		
		public static void LoadEntities()
		{
			// dynamically load creature types from JSON
			string f = "Creatures.json";
			string json = File.ReadAllText(f);
			var obj = JObject.Parse(json);
			foreach (var t in obj["Types"])
			{
				EntityType et = new EntityType((string) t["Type"]);
				et.FG = (string) t["FG"];
				et.Symbol = (char) t["Symbol"];
				var Components = new List<string>();
				foreach (JProperty comp in (JToken) t["Components"])
				{
					string name = (string) comp.Name;
					Components.Add(name);
					// need to do something more with this, eventually
					Debug.WriteLine((JToken) comp.Value);
				}
				et.Components = Components.ToArray<string>();
			}
		
			// dynamically create a typed entity for each subclass of Task
			var tasks = typeof(Game).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Task))).ToList();
			foreach (var task in tasks)
			{
				EntityType t = new EntityType(task.Name);
				t.Components = new string[] {
					task.Name
				};
			}
		}
	}
}