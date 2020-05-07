using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Hecatomb8
{
    public delegate void Remains(Creature cr, string? cause);
    public class Species : FlyWeight<Species>
    {
        [JsonIgnore] public readonly string? Name;
        [JsonIgnore] public readonly char? Symbol;
        [JsonIgnore] public readonly string? FG;
        [JsonIgnore] public readonly string? BG;
        [JsonIgnore] public readonly Remains Remains;

        public static void LeaveCorpse(Creature cr, string? cause)
        {
            var (x, y, z) = cr.GetPlacedCoordinate();
            Corpse.SpawnNewCorpse(cr.Species).PlaceInValidEmptyTile(x, y, z);
        }

        public static void NoCorpse(Creature cr, string? cause)
        {
            // do nothing
        }

        public static void UndeadRemains(Creature cr, string? cause)
        {
            var (x, y, z) = cr.GetPlacedCoordinate();
            if (GameState.World!.Random.Next(2) == 0)
            {
                Item.SpawnNewResource(Resource.Bone, 1).PlaceInValidEmptyTile(x, y, z);
            }
            else
            {
                Item.SpawnNewResource(Resource.Flesh, 1).PlaceInValidEmptyTile(x, y, z);
            }
        }

        public Species(
           string type,
           string name = "",
           string fg = "white",
           string bg = "",
           char symbol = ' ',
           Remains? remains = null
        ) : base(type)
        {
            Name = name;
            FG = fg;
            BG = bg;
            Symbol = symbol;
            Remains = remains ?? LeaveCorpse;
        }

        public static readonly Species NoSpecies = new Species(
            type: "NoSpecies",
            name: "no species",
            symbol: 'X'
        );

        public static readonly Species Human = new Species(
            type: "Human",
            name: "human",
            fg: "white",
            symbol: '@'
        );

        public static readonly Species Undead = new Species(
            type: "Undead",
            name: "undead",
            fg: "brown",
            symbol: 'z',
            remains: UndeadRemains
        );

        public static readonly Species Elemental = new Species(
            type: "Elemental",
            name: "elemental",
            fg: "green",
            symbol: 'e',
            remains: NoCorpse
        );

        public static readonly Species Spider = new Species(
            type: "Spider",
            name: "spider",
            fg: "white",
            symbol: 's'
        );
    }
}


   
