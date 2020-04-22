using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Hecatomb8
{
    // A TileEntity is any entity, like a creature, item, or task, that resides in one tile.  Generally you can only have one entity of each type in a tile.
    public abstract class TileEntity : Entity
    {
        protected char _symbol = ' ';
        protected string? _fg;
        protected string? _bg;
        protected string _name = "";
        public string Name { get => _name; }
        protected (int x, int y, int z)? _coord;
        public char Symbol { get => _symbol; }
        public string? FG { get => _fg; }
        public string? BG { get => _bg; }
        public int? X { get => _coord?.x; }
        public int? Y { get => _coord?.y; }
        public int? Z { get => _coord?.z; }

        public virtual bool Placed { get => !(_coord is null); }


        // do I need this in order to appease the warning
        public virtual void PlaceInValidEmptyTile(int x, int y, int z)
        {
            _coord = (x, y, z);
        }

        public virtual void Remove()
        {
            _coord = null;
        }


        public void Deconstruct(out int? x, out int? y, out int? z)
        {
            x = X;
            y = Y;
            z = Z;
        }

        public virtual string Describe()
        {
            return this.GetType().Name;
        }

        public override void Despawn()
        {
            if (Placed)
            {
                Remove();
            }
            base.Despawn();
        }
    }

}
