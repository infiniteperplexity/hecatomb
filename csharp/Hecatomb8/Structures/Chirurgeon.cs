
using System;
using System.Collections.Generic;

namespace Hecatomb8
{
    using static Resource;
    public class Chirurgeon : Structure
    {
        public Chirurgeon() : base()
        {
            Width = 2;
            Height = 2;
            Symbols = new char[]
            {
                '\u2625','%',
                '.','\u2694'
            };
            FGs = new string[]
            {
                "white", "pink",
                "FLOORFG", "#444499"
            };
            _bg = "#777799";
            BGs = new string[]
            {
                "WALLBG","FLOORBG",
                "FLOORBG","FLOORBG"
            };
            Ingredients = new Dictionary<Resource, int>[]
            {
                new Dictionary<Resource, int>() {{Gold, 1}}, new Dictionary<Resource, int>() {{Bone, 2}},
                new Dictionary<Resource, int>() {{Flesh, 2}}, new Dictionary<Resource, int>() {{Ectoplasm, 1}}
            };
            _name = "chirurgeon";
            UseHint = "(repair injured minions)";
            RequiresStructures = new[] { typeof(Slaughterhouse) };
            RequiresResearch = new[] { Research.Chirurgy };
        }
    }
}
