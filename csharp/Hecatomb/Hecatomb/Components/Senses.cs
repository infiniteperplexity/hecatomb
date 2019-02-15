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
using System.Linq;
using System.Diagnostics;

namespace Hecatomb
{
    using static HecatombAliases;

	public class Senses : Component
	{
		public int Range = 10;
		[JsonIgnore] public HashSet<Coord> Visible;
		[JsonIgnore] private int storedZ;

		public HashSet<Coord> GetFOV()
		{
			resetVisible();
            var (x, y, z) = Entity;
            storedZ = z;
			ShadowCaster.ShadowCaster.ComputeFieldOfViewWithShadowCasting(x, y, Range, cannotSeeThrough, addToVisible);
            foreach (Coord c in Visible.ToList())
            {        
                if (Terrains[c.X, c.Y, c.Z + 1].ZView==-1)
                {
                    Visible.Add(new Coord(c.X, c.Y, c.Z + 1));
                }
                if (Terrains[c.X, c.Y, c.Z].ZView == -1)
                {
                    Visible.Add(new Coord(c.X, c.Y, c.Z - 1));
                }
            }
			return Visible;
		}

        public bool CanSeeThrough(int x, int y)
        {
            return cannotSeeThrough(x, y);
        }

		private bool cannotSeeThrough(int x, int y)
        {
            return Game.World.GetTile(x, y, storedZ).Opaque;
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
