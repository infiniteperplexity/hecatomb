/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/23/2018
 * Time: 12:43 PM
 */
using System;

namespace Hecatomb
{
	/// <summary>
	/// Description of Inventory.
	/// </summary>
	public class Inventory : Component
	{
		int ItemEID;
		public Item Item
		{
			get
			{
				if (ItemEID==-1)
				{
					return null;
				}
				else
				{
					return (Item) Game.World.Entities.Spawned[ItemEID];
				}
			}
			set
			{
				if (value==null)
				{
					ItemEID = -1;
				}
				else
				{
					ItemEID = value.EID;
				}
			}
		}
		public Inventory() : base()
		{
			ItemEID = -1;
		}
        public GameEvent OnSelfSpawn(GameEvent ge)
        {
            Game.World.Events.Subscribe<DespawnEvent>(this, OnItemDespawn);
            return ge;
        }
        public GameEvent OnItemDespawn(GameEvent ge)
        {
            DespawnEvent de = (DespawnEvent)ge;
            if (de.Entity == Item)
            {
                Item = null;
            }
            return ge;
        }
    }
}
