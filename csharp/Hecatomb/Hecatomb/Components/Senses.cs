﻿/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 9/21/2018
 * Time: 11:35 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;

namespace Hecatomb
{
	/// <summary>
	/// Description of Senses.
	/// </summary>
	public class Senses : Component
	{
		public int Range;
		[NonSerialized] public HashSet<Tuple<int,int,int>> Visible;
		[NonSerialized] private int storedZ;
		
		public Senses() : base()
		{
			Range = 16;
		}
		
		public HashSet<Tuple<int,int,int>> GetFOV()
		{
			resetVisible();
			int x = Entity.x;
			int y = Entity.y;
			storedZ = Entity.z;
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
			Visible = new HashSet<Tuple<int, int, int>>();
		}
		
		private void addToVisible(int x, int y)
		{
			Visible.Add(new Tuple<int, int, int>(x, y, storedZ));
		}
	}
}
