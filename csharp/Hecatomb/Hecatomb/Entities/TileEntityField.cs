using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Hecatomb
{
    using static HecatombAliases;

    public class TileEntityField<T> : EntityField<T> where T : TileEntity
    {
        [JsonIgnore]
        public string Name
        {
            get
            {
                return (Entity as TileEntity).Name;
            }
        }
        [JsonIgnore]
        public int X
        {
            get
            {
                return (Entity as TileEntity).X;
            }
        }
        [JsonIgnore]
        public int Y
        {
            get
            {
                return (Entity as TileEntity).Y;
            }
        }
        [JsonIgnore]
        public int Z
        {
            get
            {
                return (Entity as TileEntity).Z;
            }
        }
        [JsonIgnore]
        public bool Placed
        {
            get
            {
                return (Entity as TileEntity).Placed;
            }
        }

        public void Place(int x, int y, int z, bool fireEvent = true)
        {
            Entity.Place(x, y, z, fireEvent: fireEvent);
        }

        public void Remove()
        {
            Entity.Remove();
        }

        public string Describe(
            bool article = true,
            bool definite = false,
            bool capitalized = false)
        {
            return Entity.Describe(article: article, definite: definite, capitalized: capitalized);
        }

        public static implicit operator TileEntityField<T>(T t)
        {
            return new TileEntityField<T>() { EID = t.EID };
        }

        public static implicit operator TileEntityField<T>(int eid)
        {
            return new TileEntityField<T>() { EID = eid };
        }

        public void Deconstruct(out int X, out int Y, out int Z)
        {
            X = Entity.X;
            Y = Entity.Y;
            Z = Entity.Z;
        }
    }
}
