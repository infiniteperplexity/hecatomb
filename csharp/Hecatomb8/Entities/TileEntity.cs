using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Hecatomb8
{
    abstract class TileEntity : Entity
    {
        char _symbol = '@';
        protected (int x, int y, int z)? _coord;
        public char Symbol { get => _symbol; }
        public int? X { get => _coord?.x; }
        public int? Y { get => _coord?.y; }
        public int? Z { get => _coord?.z; }

        public virtual bool Placed { get => !(_coord is null); }


        // do I need this in order to appease the warning
        public virtual void PlaceInEmptyTile(Constrained<int> x, Constrained<int> y, Constrained<int> z)
        {
            _coord = (x.Unbox(), y.Unbox(), z.Unbox());
        }


        public void Deconstruct(out int? x, out int? y, out int? z)
        {
            x = X;
            y = Y;
            z = Z;
        }
    }

}
