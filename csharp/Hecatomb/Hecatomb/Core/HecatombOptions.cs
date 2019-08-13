using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hecatomb
{
    public class HecatombOptions
    {
        public bool Explored;
        public bool Visible;
        public bool NoIngredients;
        public bool NoTutorial;
        public bool NoStartupScreen;
        public bool NoHumanAttacks;
        public bool NoNatureAttacks;
        public int WorkBonus;
        public bool AllSpells;
        public bool NoManaCost;
        public bool NoSpiders;
        public bool NoGhouls;
        public bool NoBatCaves;
        public bool NoDwarfLairs;
        public bool NoCaverns;
        public bool IgnoreHardness;
        public bool Invincible;
        public List<(string, int)> FreeStuff;

        public HecatombOptions()
        {
            Explored = false;
            //Explored = true;
            Visible = false;
            //Visible = true;
            NoIngredients = false;
            NoIngredients = true;
            NoTutorial = false;
            NoTutorial = true;
            NoStartupScreen = false;
            NoStartupScreen = true;
            NoHumanAttacks = false;
            //NoHumanAttacks = true;
            NoNatureAttacks = false;
            //NoNatureAttacks = true;
            AllSpells = false;
            AllSpells = true;
            NoManaCost = false;
            //NoManaCost = true;
            WorkBonus = 0;
            WorkBonus = 5;
            NoSpiders = false;
            //NoSpiders = true;
            NoGhouls = false;
            //NoGhouls = true;
            NoBatCaves = false;
            //NoBatCaves = true;
            NoDwarfLairs = false;
            //NoDwarfLairs = true;
            NoCaverns = false;
            //NoCaverns = true;
            IgnoreHardness = false;
            //IgnoreHardness = true;
            Invincible = false;
            //Invincible = true;
            FreeStuff = new List<(string, int)>();
            //FreeStuff.Add(("TradeGoods", 4));
            //FreeStuff.Add(("Wood", 4));
            //FreeStuff.Add(("Rock", 4));
            //FreeStuff.Add(("Coal", 4));
            //FreeStuff.Add(("Flint", 4));
        }
    }
    
}
