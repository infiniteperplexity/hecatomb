using System;
using System.Collections.Generic;
using System.Text;

namespace Hecatomb8
{
    class Item : TileEntity
    {
        public int Claimed;
        public override void PlaceInValidEmptyTile(int x, int y, int z)
        {
            if (GameState.World!.Items.GetWithBoundsChecked(x, y, z) != null)
            {
                throw new InvalidOperationException($"Can't place {Describe()} at {x} {y} {z} because {GameState.World!.Items.GetWithBoundsChecked(x, y, z)!.Describe()} is already there.");
            }
            var (_x, _y, _z) = this;
            if (Placed)
            {
                GameState.World!.Items.SetWithBoundsChecked((int)_x!, (int)_y!, (int)_z!, null);
            }
            GameState.World!.Items.SetWithBoundsChecked(x, y, z, this);
            base.PlaceInValidEmptyTile(x, y, z);
        }
    }
}
