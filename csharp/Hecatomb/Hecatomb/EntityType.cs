/*
 * Created by SharpDevelop.
 * User: m543015
 * Date: 9/18/2018
 * Time: 12:41 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Hecatomb
{
	public class EntityType
	{
		public static Dictionary<string, EntityType> Types = new Dictionary<string, EntityType>();
		public string Name;
		public Type[] Components;
		/// <summary>
		/// Description of EntityType.
		/// </summary>
		public EntityType(string s)
		{
			Name = s;
			Types[s] = this;
		}
			
		public void apply(TypedEntity e)
		{
			foreach (Type t in Components)
			{
				e.Components[t] = (Component) Activator.CreateInstance(t);
			}
		}
	}
}
