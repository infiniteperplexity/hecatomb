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
        public Coord? _coord;
        [JsonIgnore]protected bool Plural = false;

        protected virtual char getSymbol() => _symbol;
        protected virtual string? getFG() => _fg;
        protected virtual string? getBG() => _bg;
        protected virtual string? getName() => _name;
        // this needs to be accessible for serialization
        
        [JsonIgnore] public string? Name { get => getName(); }
        [JsonIgnore] public char Symbol { get => getSymbol(); }
        [JsonIgnore] public string? FG { get => getFG(); }
        [JsonIgnore] public string? BG { get => getBG(); }
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

        public Coord GetVerifiedCoord()
        {
            return new Coord((int)X!, (int)Y!, (int)Z!);
        }

        public virtual string Describe(
            bool article = true,
            bool definite = false,
            bool capitalized = false
        )
        {
            string name = Name ?? "nameless";
            bool vowel = false;
            if (name == null)
            {
                return "";
            }
            if ("aeiou".Contains(char.ToLower(name[0]).ToString()))
            {
                vowel = true;
            }
            if (article || definite)
            {
                if (definite)
                {
                    name = "the " + name;
                }
                else if (!Plural)
                {
                    if (vowel)
                    {
                        name = "an " + name;
                    }
                    else
                    {
                        name = "a " + name;
                    }
                }
            }
            if (capitalized)
            {
                name = char.ToUpper(name[0]) + name.Substring(1);
            }
            return name;
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
