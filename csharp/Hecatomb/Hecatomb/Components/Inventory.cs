/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/23/2018
 * Time: 12:43 PM
 */
using System;
using System.Diagnostics;

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
					return (Item) Entities[ItemEID];
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
            AddListener<DespawnEvent>(OnItemDespawn);
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
