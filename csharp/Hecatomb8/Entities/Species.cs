using System;
using System.Collections.Generic;
using System.Text;

namespace Hecatomb8
{
    class Species : FlyWeight<Species>
    {
        public readonly string Name;
        public readonly char Symbol;
        public readonly string FG;
        public readonly string BG;

        public Species(
           string type,
           string name = "",
           string fg = "white",
           string bg = "black",
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
            bg: "black"
        );
    }
}


   
