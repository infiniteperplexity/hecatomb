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
	/// Description of Entity.
	/// </summary>
	public class Entity
	{
		public string Type;
		public string Name;
		public static int MaxEID = -1;
		public int EID;
		public Dictionary<string, Component> Components;

		// shortcuts for common components
		public int? x
		{
			get {
				if (Components.ContainsKey("Position")) {
					return ((PositionComponent) Components["Position"]).x;
				} else {
					return null;
				}
			}
			set {
				if (Components.ContainsKey("Position")) {
					Components["Position"].x = value;
				}
			}
		}
		public int? y
		{
			get {
				if (Components.ContainsKey("Position")) {
					return Components["Position"].y;
				} else {
					return null;
				}
			}
			set {
				if (Components.ContainsKey("Position")) {
					Components["Position"].y = value;
				}
			}
		}
		public int? z
		{
			get {
				if (Components.ContainsKey("Position")) {
					return Components["Position"].z;
				} else {
					return null;
				}
			}
			set {
				if (Components.ContainsKey("Position")) {
					Components["Position"].z = value;
				}
			}
		}
		
		public Entity(string t)
		{
			EID = Entity.MaxEID + 1;
			Type = t;
			Components = new Dictionary<string, Component>();
			// look up our TypeObject to find out what all we're supposed to include
			
		}
		
	}
}
