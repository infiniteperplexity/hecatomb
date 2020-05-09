using System;
using System.Collections.Generic;
using System.Text;

namespace Hecatomb8
{
    public class Union<T1, T2>
    {
        Object Item;
        private Union(Object item)
        {
            Item = item;
        }
        public bool Is<T>() where T : T1, T2
        {
            return (Item is T);
        }
        public T As<T>() where T: T1, T2
        {
            return (T)Item;
        }
        public static implicit operator Union<T1, T2>(T1 item)
        {
            return new Union<T1, T2>(item!);
        }
        public static implicit operator Union<T1, T2>(T2 item)
        {
            return new Union<T1, T2>(item!);
        }
        public override bool Equals(Object? obj)
        {
            if (Item is T1 && obj is T1)
            {
                return object.Equals(Item, obj);
            }
            else if (Item is T2 && obj is T2)
            {
                return object.Equals(Item, obj);
            }
            return false;
        }
        public static bool operator ==(Union<T1, T2> u1, Union<T1, T2> u2)
        {
            return object.ReferenceEquals(u1.Item, u2.Item);
        }
        public static bool operator !=(Union<T1, T2> u1, Union<T1, T2> u2)
        {
            return !object.ReferenceEquals(u1.Item, u2.Item);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode() + Item.GetHashCode();
        }
    }
}
