﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Hecatomb
{
    using static HecatombAliases;
    public class Item : TileEntity
    {
        public bool Owned;
        public int Quantity;
        public int Claimed;
        public string Resource;
        public int MaxStack;

        public int Unclaimed
        {
            get
            {
                return Quantity - Claimed;
            }
        }

        public Item() : base()
        {
            Quantity = 1;
            Claimed = 0;
            MaxStack = 1;
        }

        public Item(string s) : base()
        {
            Resource = s;
            // get stacksize frmo resource
        }

        public Item(string s, int n) : this(s)
        {
            Resource = s;
            Quantity = n;
            // g
        }

        public override void Place(int x1, int y1, int z1, bool fireEvent = true)
        {
            Item e = Game.World.Items[x1, y1, z1];
            if (e == null)
            {
                Game.World.Items[x1, y1, z1] = this;
                base.Place(x1, y1, z1, fireEvent);
            }
            else if (e.Resource==Resource)
            {
                e.AddQuantity(Quantity);
                Despawn();
            }
            else
            {
                Tumble();
            }
        }
        public override void Remove()
        {
            int x0 = X;
            int y0 = Y;
            int z0 = Z;
            base.Remove();
            Game.World.Items[x0, y0, z0] = null;
        }

        public Item Take(int n)
        {
            Quantity -= n;
            Item item = Spawn<Item>();
            item.Resource = Resource;
            item.Quantity = n;
            if (Quantity<=0)
            {
                Despawn();
            }
            return item;
        }


        public bool IsStored()
        {
            Feature f = Features[X, Y, Z];
            StructuralComponent sc = f?.TryComponent<StructuralComponent>();
            if (sc != null)
            {
                if (sc.Structure.Unbox().Stores.Contains(Resource))
                {
                    return true;
                }
            }
            return false;
        }

        public void AddQuantity(int n)
        {

        }

        public void Tumble()
        {

        }
    }
}
