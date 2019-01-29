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
		
        public void Drop()
        {
            var (x, y, z) = Entity;
            Debug.WriteLine("about to drop");
            Item.Place(x, y, z);
            Debug.WriteLine("dropped");
            Item = null;
            Debug.WriteLine("clearing inventory");
        }
    }
}
