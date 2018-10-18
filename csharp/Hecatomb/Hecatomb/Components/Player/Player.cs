/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 9/26/2018
 * Time: 10:39 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;

namespace Hecatomb
{
	/// <summary>
	/// Description of Player.
	/// </summary>
	public class Player : Creature
	{
		[JsonProperty] private List<int> MinionEIDs;
		
		public bool Acted;
		public Player() : base()
		{
			Acted = false;
		}
		
		public void Initialize()
		{
			Component c;
			c = (Component) Game.World.Entities.Spawn(typeof(Minions));
			c.AddToEntity(this);
			c = (Component) Game.World.Entities.Spawn(typeof(TaskMaster));
			c.AddToEntity(this);
		}
		
		public void Act()
		{
			Acted = true;
			Game.Time.Acted();
		}
		public void Ready()
		{
			HandleVisibility();
			Game.Camera.Center(X, Y, Z);
			Game.MainPanel.Dirty = true;
			Game.MenuPanel.Dirty = true;
			Game.StatusPanel.Dirty = true;
			Acted = false;
		}
		public override GameEvent OnSelfSpawn(GameEvent g)
		{
			Game.World.Events.Subscribe<PlaceEvent>(this, OnPlace);
			return g;
		}
		public GameEvent OnPlace(GameEvent g)
		{
			PlaceEvent p = (PlaceEvent) g;
			if (p.Entity.Equals(this))
			{
				Debug.WriteLine(String.Format("{0} placed at {1} {2} {3}", Describe(), p.X, p.Y, p.Z));
			}
			return p;
		}
		
		public void HandleVisibility()
		{
			Game.Camera.Center(X, Y, Z);
			Game.Visible = GetComponent<Senses>().GetFOV();
			foreach(Creature c in GetComponent<Minions>())
			{
				Senses s = c.GetComponent<Senses>();
				Game.Visible.UnionWith(s.GetFOV());
			}
			foreach (var t in Game.Visible)
			{
				Game.World.Explored.Add(t);
			}
		}
	}
}
