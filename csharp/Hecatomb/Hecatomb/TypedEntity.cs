﻿/*
 * Created by SharpDevelop.
 * User: m543015
 * Date: 9/18/2018
 * Time: 11:35 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace Hecatomb
{
	/// <summary>
	/// Description of TypedEntity.
	/// </summary>
	public class TypedEntity
	{
		public string EType;
		public string Name;
		// might remove this...but for testing...
		public char Symbol;
		public string FG;
		public string BG;
		public static int MaxEID = -1;
		public int EID;
		public Dictionary<Type, Component> Components;
		
		// Position shortcuts
		public int x {
			get { return GetComponent<Position>().x; }
			private set {}
		}
		public int y {
			get { return GetComponent<Position>().y; }
			private set {}
		}
		public int z {
			get { return GetComponent<Position>().z; }
			private set {}
		}
		
		public void Place(int x, int y, int z) {
			GetComponent<Position>().Place(x,y,z);
		}

		
		public TypedEntity(string t)
		{
			EID = TypedEntity.MaxEID + 1;
			Symbol = '@';
			FG = "white";
			EType = t;
			Components = new Dictionary<Type, Component>();
			EntityType et = EntityType.Types[t];
			if (et!=null) {
				et.Typify(this);
			}		
		}
		
		public T GetComponent<T>() where T : Component
		{
			Type t = typeof(T);
			if (Components.ContainsKey(t)) {
				return (T) Components[t];
			} else {
				throw new InvalidOperationException();
			}
		}
		
		
		public T TryComponent<T>() where T : Component
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