using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Newtonsoft.Json;

namespace Hecatomb8
{
    using static HecatombAliases;
    // A TileEntity is any entity, like a creature, item, or task, that resides in one tile.  Generally you can only have one entity of each type in a tile.
    public abstract class TileEntity : Entity
    {
        protected char _symbol = ' ';
        protected string? _fg;
        protected string? _bg;
        protected string? _name;
        // this needs to be accessible for serialization
        public Coord? _coord;
        [JsonIgnore] public string? Name { get => _name; }
        [JsonIgnore] public char Symbol { get => _symbol; }
        [JsonIgnore] public string? FG { get => _fg; }
        [JsonIgnore] public string? BG { get => _bg; }
        [JsonIgnore] public int? X { get => _coord?.X; }
        [JsonIgnore] public int? Y { get => _coord?.Y; }
        [JsonIgnore] public int? Z { get => _coord?.Z; }
        [JsonIgnore] public virtual bool Placed { get => !(_coord is null); }


        // do I need this in order to appease the warning
        public virtual void PlaceInValidEmptyTile(int x, int y, int z)
        {
            _coord = new Coord(x, y, z);
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

        public virtual string Describe(bool capitalized = false)
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

        public virtual void Destroy(string? cause = null)
        {
            Publish(new DestroyEvent() { Entity = this, Cause = cause });
            Despawn();

        }
    }

}
