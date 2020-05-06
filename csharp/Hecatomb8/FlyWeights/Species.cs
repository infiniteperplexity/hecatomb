using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Hecatomb8
{
    public class Species : FlyWeight<Species>
    {
        [JsonIgnore] public readonly string? Name;
        [JsonIgnore] public readonly char? Symbol;
        [JsonIgnore] public readonly string? FG;
        [JsonIgnore] public readonly string? BG;

        public Species(
           string type,
           string name = "",
           string fg = "white",
           string bg = "",
           char symbol = ' '
        ) : base(type)
        {
            Name = name;
            FG = fg;
            BG = bg;
            Symbol = symbol;
        }

        public static readonly Species NoSpecies = new Species(
            type: "NoSpecies"
        );

        public static readonly Species Human = new Species(
            type: "Human",
            name: "human",
            fg: "white",
            symbol: '@'
        );

        public static readonly Species Zombie = new Species(
            type: "Zombie",
            name: "zombie",
            fg: "lime green",
            symbol: 'z'
        );

        public static readonly Species Spider = new Species(
            type: "Spider",
            name: "spider",
            fg: "white",
            symbol: 's'
        );
    }
}


   
