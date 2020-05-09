using System;
using System.Collections.Generic;
using System.Text;

namespace Hecatomb8
{
    class HecatombOptions
    {
        public static bool Visible;
        public static bool Explored;
        public static bool ZeroSeed;
        public static bool NoErrorLog;
        public static bool ShowPathfindingNotes;
        public static bool NoBuildWarnings;
        public static bool NoStartupScreen;
        public static bool NoTutorial;
        public static bool NoIngredients;
        public static int WorkBonus;
        public static bool Invincible;
        public static bool ShowDebugSpells;
        public static bool ShowAllSpells;
        public static bool IgnoreHardness;
        public static bool NoSpiders;
        public static bool NoManaCost;
        public static bool NoisyErrors;
        public static bool NoSieges;
        public static bool NoGhouls;
        public static bool NoDryads;


        static HecatombOptions()
        {
            Visible = false;
            Visible = true;
            Explored = false;
            Explored = true;
            ZeroSeed = false;
            
            NoErrorLog = false;
            //NoErrorLog = true;
            NoBuildWarnings = false;
            //NoBuildWarnings = true;
            NoStartupScreen = false;
            //NoStartupScreen = true;
            NoTutorial = false;
            //NoTutorial = true;
            NoIngredients = false;
            //NoIngredients = true;
            WorkBonus = 0;
            //WorkBonus = 2;
            IgnoreHardness = false;
            //IgnoreHardness = true;
            ShowDebugSpells = false;
            //ShowDebugSpells = true;
            ShowAllSpells = false;
            //ShowAllSpells = true;
            NoSpiders = false;
            //NoSpiders = true;
            NoManaCost = false;
            //NoManaCost = true;
            Invincible = false;
            //Invincible = true;
            NoisyErrors = false;
            //NoisyErrors = true;
            NoSieges = false;
            //NoSieges = true;
            NoGhouls = false;
            //NoGhouls = true;
            NoDryads = false;
            //NoDryads = true;
        }
    }
}
