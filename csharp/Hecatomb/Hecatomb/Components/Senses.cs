/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 9/21/2018
 * Time: 11:35 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;

namespace Hecatomb
{
	/// <summary>
	/// Description of Senses.
	/// </summary>
	public class Senses : Component
	{
		public int Range;
		[JsonIgnore] public HashSet<Coord> Visible;
		[JsonIgnore] private int storedZ;
		
		public Senses() : base()
		{
			Range = 10;
		}
		
		public HashSet<Coord> GetFOV()
		{
			resetVisible();
			int x = Entity.X;
			int y = Entity.Y;
			storedZ = Entity.Z;
			ShadowCaster.ShadowCaster.ComputeFieldOfViewWithShadowCasting(x, y, Range, cannotSeeThrough, addToVisible);
			return Visible;
		}
		
		public bool CanSeeThrough(int x, int y) {
			return !cannotSeeThrough(x, y);
		}
		
		private bool cannotSeeThrough(int x, int y) {
			return Game.World.Tiles[x, y, storedZ].Opaque;
		}
		private void resetVisible()
		{
			Visible = new HashSet<Coord>();
		}
		
		private void addToVisible(int x, int y)
		{
			Visible.Add(new Coord(x, y, storedZ));
		}
	}
}
