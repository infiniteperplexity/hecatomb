﻿/*
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
		
        public void Drop()
        {
            var (x, y, z) = Entity;
            Item.Place(x, y, z);
            Item = null;
            Debug.WriteLine(Item.EID);
            Debug.WriteLine("Item has been dropped");
        }

		//public Inventory() : base()
		//{
  //          AddListener<DespawnEvent>(OnItemDespawn);
		//}
        //public GameEvent OnItemDespawn(GameEvent ge)
        //{
        //    DespawnEvent de = (DespawnEvent)ge;
        //    if (de.Entity == Item)
        //    {
        //        Item = null;
        //    }
        //    return ge;
        //}
    }
}
