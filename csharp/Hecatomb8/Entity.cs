using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Hecatomb8
{
    
    class Entity
    {
        static int MaxEID = -1;
        public int EID;
        public string ClassName;
        //public bool Exists;

        protected Entity()
        {
            ClassName = this.GetType().Name;
            EID = -1;
        }

        public static T Spawn<T>() where T: Entity, new()
        {
            T t = new T();
            t.EID = MaxEID + 1;
            MaxEID = t.EID;
            GameState.Entities[t.EID] = t;
            //t.Exists = true;
            return t;
        }
    }

    class TileEntity : Entity
    {
        char _symbol = '@';
        (int x, int y, int z)? _coord;
        public char Symbol { get =>_symbol; }
        public int? X { get => _coord?.x; }
        public int? Y { get => _coord?.y; }
        public int? Z { get => _coord?.z; }

        public void Place(int x, int y, int z)
        {
            
            _coord = (x, y, z);
        }

        public bool Placed() => !(_coord is null);


        public void Deconstruct(out int? x, out int? y, out int? z)
        {
            x = X;
            y = Y;
            z = Z;
        }
    }
}
