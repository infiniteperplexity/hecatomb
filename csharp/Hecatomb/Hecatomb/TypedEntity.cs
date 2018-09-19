/*
 * Created by SharpDevelop.
 * User: m543015
 * Date: 9/18/2018
 * Time: 11:35 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;

namespace Hecatomb
{
	/// <summary>
	/// Description of TypedEntity.
	/// </summary>
	public class TypedEntity
	{
		public EntityType EntityType;
		public string Name;
		// might remove this...but for testing...
		public char Symbol;
		public string FG;
		public string BG;
		public static int MaxEID = -1;
		public int EID;
		public Dictionary<Type, Component> Components;
		
		// Position shortcuts
		public int? x {
			get {
				if (Components.ContainsKey(typeof(Position))) {
					return GetComponent<Position>().x;
				}
				return null;
			}
			set {
				if (Components.ContainsKey(typeof(Position))) {
					GetComponent<Position>().x = value;
				}
			}
		}
		public int? y {
			get {
				if (Components.ContainsKey(typeof(Position))) {
					return GetComponent<Position>().y;
				}
				return null;
			}
			set {
				if (Components.ContainsKey(typeof(Position))) {
					GetComponent<Position>().y = value;
				}
			}
		}
		public int? z {
			get {
				if (Components.ContainsKey(typeof(Position))) {
					return GetComponent<Position>().z;
				}
				return null;
			}
			set {
				if (Components.ContainsKey(typeof(Position))) {
					GetComponent<Position>().z = value;
				}
			}
		}

		
		public TypedEntity(EntityType t)
		{
			EID = TypedEntity.MaxEID + 1;
			Symbol = '@';
			FG = "white";
			EntityType = t;
			Components = new Dictionary<Type, Component>();
//			System.Diagnostics.Debug.WriteLine(t.GetType());
			if (EntityType!=null) {
				EntityType.apply(this);
			}
			// look up our TypeObject to find out what all we're supposed to include
			
		}
		
		public T GetComponent<T>() where T : Component
		{
			Type t = typeof(T);
			if (Components.ContainsKey(t)) {
				return (T) Components[t];
			} else {
				return default(T);
			}
		}
		
	}
}
