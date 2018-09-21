﻿/*
 * Created by SharpDevelop.
 * User: m543015
 * Date: 9/18/2018
 * Time: 12:41 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hecatomb
{
	public class EntityType
	{
		public static Dictionary<string, EntityType> Types = new Dictionary<string, EntityType>();
		public string Name;
		public string[] Components;
		/// <summary>
		/// Description of EntityType.
		/// </summary>
//		public EntityType(string s)
//		{
//			Name = s;
//			Types[s] = this;
//		}
		
		public EntityType(string s)
		{
//			string f = s + ".json";
//			string json = File.ReadAllText(f);
//			var jobj = JObject.Parse(json);
			Name = s;
//			var comps = jobj["Components"].Children();
			
//			List<string> l = comps.Select(c=>(string)c).ToList();
			
			Types[s] = this;
		}
			
		public void Typify(TypedEntity e)
		{
			foreach (string t in Components)
			{
				Component c = (Component) Activator.CreateInstance(Type.GetType("Hecatomb." + t));
				c.AddToEntity(e);
			}
		}
		
		
		public static void LoadEntities()
		{
			// Player
			EntityType Player = new EntityType("Player");
			Player.Components = new string[] {
				"Position",
				"Senses",
				"Movement"
			};
			
			EntityType Zombie = new EntityType("Zombie");
			Zombie.Components = new string[] {
				"Position",
				"Senses",
				"Movement"
			};
		}
	}
}