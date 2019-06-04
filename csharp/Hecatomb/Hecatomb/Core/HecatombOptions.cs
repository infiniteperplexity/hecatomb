﻿using System;
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
        public bool HumanAttacks;
        public int WorkBonus;
        public bool AllSpells;
        public bool NoManaCost;
        public bool NoSpiders;
        public bool NoGhouls;
        public bool NoBatCaves;
        public bool NoDwarfLairs;
        public bool NoCaverns;
        public bool IgnoreHardness;

        public HecatombOptions()
        {
            Explored = false;
            Explored = true;
            Visible = false;
            Visible = true;
            NoIngredients = false;
            NoIngredients = true;
            NoTutorial = false;
            NoTutorial = true;
            NoStartupScreen = false;
            NoStartupScreen = true;
            HumanAttacks = false;
            HumanAttacks = true;
            AllSpells = false;
            AllSpells = true;
            NoManaCost = false;
            NoManaCost = true;
            WorkBonus = 0;
            WorkBonus = 5;
            NoSpiders = false;
            //NoSpiders = true;
            NoGhouls = false;
            NoGhouls = true;
            NoBatCaves = false;
            //NoBatCaves = true;
            NoDwarfLairs = false;
            NoDwarfLairs = true;
            NoCaverns = false;
            //NoCaverns = true;
            IgnoreHardness = false;
            IgnoreHardness = true;
        }
    }
    
}
