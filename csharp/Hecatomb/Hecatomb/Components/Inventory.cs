/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/23/2018
 * Time: 12:43 PM
 */
using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace Hecatomb
{
	public class Inventory : Component
	{
        public TileEntityField<Item> Item = new TileEntityField<Item>();
		
        public Inventory()
        {
            AddListener<DespawnEvent>(OnDespawn);
            AddListener<DestroyEvent>(OnDestroy);
        }

        public GameEvent OnDespawn(GameEvent ge)
        {
            DespawnEvent de = (DespawnEvent)ge;
            if (de.Entity==this && Item!=null)
            {
                Item.Despawn();
            }
            return ge;
        }

        public GameEvent OnDestroy(GameEvent ge)
        {
            DestroyEvent de = (DestroyEvent)ge;
            if (de.Entity == this.Entity.Unbox())
            {
                Drop();
            }
            return ge;
        }
        public void Drop()
        {
            var (x, y, z) = Entity;
            if (Item != null)
            {
                if (Item.Unbox().Resource == "Gold")
                {
                    Game.World.Events.Publish(new AchievementEvent() { Action = "FoundGold" });
                }
                Item.Place(x, y, z);
                Item = null;
            }
        }
    }
}
