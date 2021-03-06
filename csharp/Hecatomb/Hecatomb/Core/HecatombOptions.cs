﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hecatomb
{
    public class HecatombOptions
    {
        public bool NoErrorLog;
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
        public bool NoLairs;
        public bool NoCaverns;
        public bool IgnoreHardness;
        public bool Invincible;
        public bool ReseedRandom;
        public bool FullScreen;
        public bool ShowDebugSpells;
        public bool HaulTaskClaims;
        public bool ReconstructGames;
        public List<(string, int)> FreeStuff;

        public HecatombOptions()
        {
            NoErrorLog = false;
            //NoErrorLog = true;
            ReconstructGames = false;
            //ReconstructGames = true;
            FullScreen = false;
            //FullScreen = true;
            Explored = false;
            //Explored = true;
            Visible = false;
            //Visible = true;
            NoIngredients = false;
            //NoIngredients = true;
            NoTutorial = false;
            //NoTutorial = true;
            NoStartupScreen = false;
            //NoStartupScreen = true;
            NoHumanAttacks = false;
            //NoHumanAttacks = true;
            NoNatureAttacks = false;
            //NoNatureAttacks = true;
            AllSpells = false;
            //AllSpells = true;
            ShowDebugSpells = false;
            //ShowDebugSpells = true;
            NoManaCost = false;
            //NoManaCost = true;
            WorkBonus = 0;
            //WorkBonus = 5;
            NoSpiders = false;
            //NoSpiders = true;
            NoGhouls = false;
            //NoGhouls = true;
            NoBatCaves = false;
            NoBatCaves = true;
            NoLairs = false;
            NoLairs = true;
            NoCaverns = false;
            //NoCaverns = true;
            IgnoreHardness = false;
            //IgnoreHardness = true;
            Invincible = false;
            //Invincible = true;
            ReseedRandom = false;
            //ReseedRandom = true;
            HaulTaskClaims = false;
            //HaulTaskClaims = true;
            FreeStuff = new List<(string, int)>();
            //FreeStuff.Add(("TradeGoods", 4));
            //FreeStuff.Add(("Wood", 4));
            //FreeStuff.Add(("Rock", 4));
            //FreeStuff.Add(("Coal", 4));
            //FreeStuff.Add(("Flint", 4));
        }
    }
    
}
