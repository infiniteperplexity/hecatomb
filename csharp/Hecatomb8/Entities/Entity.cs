using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Hecatomb8
{
    
    public class Entity
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
            GameState.World!.Entities[t.EID] = t;
            //t.Exists = true;
            return t;
        }
    }
}
