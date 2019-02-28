﻿/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/25/2018
 * Time: 11:40 AM
 */
using System;
using System.Diagnostics;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Hecatomb
{
    using static HecatombAliases;
    /// <summary>
    /// Description of TutorialTracker.
    /// </summary>
    public class TutorialHandler : StateHandler
    {
        public int MoveCount;
        public int SlopeCount;
        public bool HasUnpaused;
        public int TurnsWaited;
        [JsonIgnore] public List<ColoredText> OffTutorialText;
        [JsonIgnore] public List<ColoredText> OffTutorialCamera;
        

        public int CurrentIndex;
        // the tutorial is always active but it's not always visible
        public bool Visible;
        [JsonIgnore] public List<TutorialState> TutorialStates;
        [JsonIgnore]
        public TutorialState Current
        {
            get
            {
                return TutorialStates[CurrentIndex];
            }
            set { }
        }

        public TutorialHandler()
        {
            MoveCount = 0;
            SlopeCount = 0;
            CurrentIndex = 0;
            Visible = !Options.NoTutorial;
            OffTutorialText = new List<ColoredText>() { "{orange}You have strayed from the tutorial.  Press Escape to get back on track or ? to hide tutorial messages." };
            OffTutorialCamera = new List<ColoredText>() { "{orange}You have strayed from the tutorial.  Press Tab to get back on track or ? to hide tutorial messages." };
            TutorialStates = new List<TutorialState>
            {
                new TutorialState("Welcome")
                {
                    ControlText = new List<ColoredText>()
                    {
                        "Esc: System view.",
                        " ",
                        "{cyan}Move: NumPad/Arrows.",
                        "{cyan}(Control+Arrows for diagonal.)",
                        " ",
                        @"\: Toggle tutorial."
                    },
                    InstructionsText = new List<ColoredText>()
                    {
                        "{yellow}Welcome to the Hecatomb in-game tutorial.  Follow the instructions on this panel to proceed through the tutorial, or press ? to turn off these messages and play without the tutorial.",
                        " ",
                        "You walk amongst the tombstones of a hillside graveyard, searching for the site upon which you will build your mighty fortress.",
                        " ",
                        "{lime green}This symbol is you: {magenta}@",
                        " ",
                        "{cyan}Try walking around using the numeric keypad.If your keyboard has no keypad, use the arrow keys.One turn will pass for each step you take."
                    },
                    HandleEvent = (TutorialEvent t) =>
                    {
                        if (t.Action=="Move")
                        {
                            MoveCount+=1;
                            if (MoveCount>=5)
                            {
                                NextState();
                            }
                        }
                        else if (t.Action=="ShowSpells")
                        {
                            GotoState("ChooseSpell");
                        }
                    }
                },
                new TutorialState("Movement")
                {
                    ControlText = new List<ColoredText>()
                    {
                        "Esc: System view.",
                        " ",
                        "{cyan}Move: NumPad/Arrows.",
                        "{cyan}(Control+Arrows for diagonal.)",
                        " ",
                        @"\: Toggle tutorial."
                    },
                    InstructionsText = new List<ColoredText>()
                    {
                        "You walk amongst the tombstones of a hillside graveyard, searching for the site upon which you will build your mighty fortress.",
                        " ",
                        @"{lime green}- Green areas with {GRASSFG}"" {lime green}are grass.",
                        " ",
                        "{lime green}- Dim green areas are also grass, one elevation level below you.",
                        " ",
                        "{lime green}- Gray areas with {WALLFG}# {lime green}are walls, but they may have walkable floors one level above you.",
                        " ",
                        "{lime green}- Other symbols (clubs, spades, flowers) may be trees or plants.",
                        " ",
                        "{lime green}- Letters such as s or b are wild animals, mostly harmless for now.",
                        " ",
                        "{cyan}Try walking around using the numeric keypad.  If your keyboard has no keypad, use the arrow keys.  One turn will pass for each step you take."
                    },
                    HandleEvent = (TutorialEvent t) =>
                    {
                        if (t.Action=="Move")
                        {
                            MoveCount+=1;
                            if (MoveCount>=10)
                            {
                                NextState();
                            }
                        }
                        else if (t.Action=="ShowSpells")
                        {
                            GotoState("ChooseSpell");
                        }
                    }
                },
                new TutorialState("Slopes")
                {
                    ControlText = new List<ColoredText>()
                    {
                        "Esc: System view.",
                        " ",
                        "{cyan}Move: NumPad/Arrows, ,/.: Up/Down.",
                        "{cyan}(Control+Arrows for diagonal.)",
                        " ",
                        @"\: Toggle tutorial."
                    },
                    InstructionsText = new List<ColoredText>()
                    {
                        "You scramble up and down the slopes for a better view of the area.",
                        " ",
                        "{lime green}- The {WALLFG}^ {lime green}and {WALLFG}v {lime green}symbols are slopes.",
                        " ",
                        "{lime green}- The game world is a 3D grid of tiles.  Slopes provide access to different elevation levels.",
                        " ",
                        "{lime green}- You can climb up or down a slope by standing on it and pressing . (<) or , (>).",
                        " ",
                        "{lime green}- If you try to walk sideways off a cliff or into a wall, you will automatically climb a slope instead if possible.",
                        " ",
                        "{lime green}- When you climb up or down, colors change with your relative elevation.",
                        " ",
                        "{cyan}Try climbing up and down a few slopes."
                    },
                    HandleEvent = (TutorialEvent t) =>
                    {
                        if (t.Action=="Climb")
                        {
                            {
                                SlopeCount+=1;
                            }
                            if (SlopeCount>=3)
                            {
                                NextState();
                            }
                        }
                        else if (t.Action=="ShowSpells")
                        {
                            GotoState("ChooseSpell");
                        }
                    }
                },
                new TutorialState("CastSpell")
                {
                    ControlText = new List<ColoredText>()
                    {
                        "Esc: System view.",
                        " ",
                        "Move: NumPad/Arrows, ,/.: Up/Down.",
                        "(Control+Arrows for diagonal.)",
                        " ",
                        "{cyan}Z: Cast spell.",
                        " ",
                        @"\: Toggle tutorial."
                    },
                    InstructionsText = new List<ColoredText>()
                    {
                        "Enough of this pointless wandering - it is time to summon an undead servant.",
                        " ",
                        "{lime green}Near where you started, there should be some cross-shaped symbols. These are tombstones.  If you want to know what a symbol represents, hover over it with the mouse and look at the bottom half of the right panel.",
                        " ",
                        "{cyan}Find a tombstone - you don't have to stand right next to it.  Then press Z to view a list of spells you can cast, and press A to choose 'raise zombie.'"
                    },
                    HandleEvent = (TutorialEvent t) =>
                    {
                        if (t.Action=="ShowSpells")
                        {
                            NextState();
                        }
                        else if (t.Action=="Cancel")
                        {
                            GotoState("CastSpell");
                        }
                    },
                },
                new TutorialState("ChooseSpell")
                {
                    RequiresDefaultControls = false,
                    ControlText = new List<ColoredText>()
                    {
                        "{orange}**Esc: Cancel**.",
                        "{yellow}Choose a spell:",
                        "{cyan}a) Raise Zombie"
                    },
                    InstructionsText = new List<ColoredText>()
                    {
                        "Enough of this pointless wandering - it is time to summon an undead servant.",
                        " ",
                        "{lime green}Near where you started, there should be some cross-shaped symbols. These are tombstones.  If you want to know what a symbol represents, hover over it with the mouse and look at the bottom half of the right panel.",
                        " ",
                        "{cyan}Find a tombstone - you don't have to stand right next to it.  Then press Z to view a list of spells you can cast, and press A to choose 'raise zombie.'"
                    },
                    HandleEvent = (TutorialEvent t) =>
                    {
                        if (t.Action =="ChooseRaiseZombie")
                        {
                            NextState();
                        }
                        else if (t.Action == "Cancel")
                        {
                            GotoState("CastSpell");
                        }
                    },
                },
                new TutorialState("TargetSpell")
                {
                    RequiresDefaultControls = false,
                    ControlText = new List<ColoredText>()
                    {
                        "{orange}**Esc: Cancel.**",
                        "{yellow}Select a square with keys or mouse.",
                        " ",
                        "Move: NumPad/Arrows, ,/.: Up/Down.",
                        "(Control+Arrows for diagonal.)",
                        " ",
                        "{cyan}Click / Space: Select.",
                    },
                    InstructionsText = new List<ColoredText>()
                    {
                        "{cyan}Select a tombstone, either by using the mouse, or by navigating with the direction keys and pressing space to select.  Make sure the tombstone is on the current elevation level.",
                        " ",
                        "{lime green}Notice that the bottom portion of this panel gives you information about the square you are hovering over - whether it's a valid target for your spell, what the terrain is like, and so on."
                    },
                    HandleEvent = (TutorialEvent t) =>
                    {
                        if (t.Action=="CastRaiseZombie")
                        {
                            NextState();
                        }
                        else if (t.Action=="Cancel")
                        {
                            GotoState("CastSpell");
                        }
                    },
                },
                new TutorialState("Achievements")
                {
                    ControlText = new List<ColoredText>()
                    {
                        "Esc: System view.",
                        " ",
                        "Move: NumPad/Arrows, ,/.: Up/Down.",
                        "(Control+Arrows for diagonal.)",
                        " ",
                        "Enter: Enable auto-pause.",
                        "+/-: Change speed.",
                        " ",
                        "Z: Cast spell.",
                        " ",
                        "PageUp/Down: Scroll messages.",
                        "{cyan}A: Achievements, {white}/: Toggle tutorial."
                    },
                    InstructionsText = new List<ColoredText>()
                    {
                        "Forbidden runes swirl around you as you call forth a corpse from its grave.",
                        " ",
                        "{lime green}You just earned an achievement, as noted on the message bar below the play area.  You can scroll messages up and down using the PageUp and PageDown keys (on a Mac, Fn+Arrows.)",
                        " ",
                        "{cyan}Press 'A' to view the achievements screen."
                    },
                    HandleEvent = (TutorialEvent t) =>
                    {
                        if (t.Action=="ShowAchievements")
                        {
                            NextState();
                        }
                        else if (t.Action=="ZombieEmerges")
                        {
                            GotoState("Unpausing");
                        }
                    },
                },
                new TutorialState("WaitForZombie")
                {
                    ControlText = new List<ColoredText>()
                    {
                        "Esc: System view.",
                        " ",
                        "Move: NumPad/Arrows, ,/.: Up/Down.",
                        "(Control+Arrows for diagonal.)",
                        "{cyan}Wait: NumPad 5 / Space.",
                        " ",
                        "Z: Cast spell.",
                        " ",
                        "PageUp/Down: Scroll messages.",
                        "A: Achievements, /: Toggle tutorial."
                    },
                    InstructionsText = new List<ColoredText>()
                    {
                        "You wait impatiently as your undead thrall claws its way out of its grave.",
                        " ",
                        "{lime green}The orange background around the tombstone indicates that there is a task assigned in that square.",
                        " ",
                        @"{cyan}Press 5 on the numeric keypad several times, to pass turns (""wait"") until your zombie emerges.  If you have no numeric keypad, press Space to wait."
                    },
                    HandleEvent = (TutorialEvent t) =>
                    {
                        if (t.Action=="ZombieEmerges")
                        {
                            NextState();
                        }
                    },
                },
                new TutorialState("Unpausing")
                {
                    ControlText = new List<ColoredText>()
                    {
                        "Esc: System view.",
                        " ",
                        "Move: NumPad/Arrows, ,/.: Up/Down.",
                        "(Control+Arrows for diagonal.)",
                        "{cyan}Wait: NumPad 5 / Space.",
                        " ",
                        "{cyan}Enter: Enable auto-pause.",
                        "+/-: Change speed.",
                        " ",
                        "Z: Cast spell.",
                        " ",
                        "PageUp/Down: Scroll messages.",
                        "A: Achievements, /: Toggle tutorial."
                    },
                    InstructionsText = new List<ColoredText>()
                    {
                        "Your minion bursts forth from the ground!",
                        " ",
                        "{lime green}The word 'Pause' above the right-hand side of the message bar.  The game is currently auto-paused - one turn will pass for each action you take.  If you turn auto-pause off, turns will pass in realtime even if you take no actions.  You can press + or - to make time pass faster or slower.",
                        " ",
                        "{cyan}Press Enter / Return to turn off auto-pause, then wait for several turns to pass.",
                        " ",
                        "{lime green}Your zombie will wander a short distance from you.  If it seems to disappear, it probably went up or down a slope."
                    },
                    HandleEvent = (TutorialEvent t) =>
                    {
                        if (t.Action=="TogglePause")
                        {
                            HasUnpaused = true;
                        }
                        else if (t.Action=="TurnBegin")
                        {
                            TurnsWaited+=1;
                        }
                        if (HasUnpaused && TurnsWaited>=15)
                        {
                            NextState();
                        }
                        else if (t.Action=="ShowJobs")
                        {
                            GotoState("ChooseJob");
                        }
                    },
                },
                new TutorialState("AssignJob")
                {
                    ControlText = new List<ColoredText>()
                    {
                        "Esc: System view.",
                        " ",
                        "Move: NumPad/Arrows, ,/.: Up/Down.",
                        "(Control+Arrows for diagonal.)",
                        "Wait: NumPad 5 / Space.",
                        " ",
                        "Enter: Enable auto-pause.",
                        "+/-: Change speed.",
                        " ",
                        "Z: Cast spell, {cyan}J: Assign job.",
                        " ",
                        "PageUp/Down: Scroll messages.",
                        "A: Achievements, /: Toggle tutorial."
                    },
                    InstructionsText = new List<ColoredText>()
                    {
                        "You close your eyes and concentrate, formulating a task for your unthinking slave.",
                        " ",
                        "{cyan}Press J to assign a job, and then press A to make your zombie dig.  You can assign a job from any distance.",
                        " ",
                        "{lime green}(Remember, if auto-pause is turned off, you can turn it back on by pressing Enter / Return.)"
                    },
                    HandleEvent = (TutorialEvent t) =>
                    {
                        if (t.Action == "ShowJobs")
                        {
                            NextState();
                        }
                    }
                },
                new TutorialState("ChooseJob")
                {
                    RequiresDefaultControls = false,
                    ControlText = new List<ColoredText>()
                    {
                        "{orange}**Esc: Cancel**.",
                        "{yellow}Choose a task:",
                        "{cyan}a) Dig or harvest"
                    },
                    InstructionsText = new List<ColoredText>()
                    {
                        "You close your eyes and concentrate, formulating a task for your unthinking slave.",
                        " ",
                        "{cyan}Press J to assign a job, and then press A to make your zombie dig.  You can assign a job from any distance.",
                        " ",
                        "{lime green}(Remember, if auto-pause is turned off, you can turn it back on by pressing Enter / Return.)"
                    },
                    HandleEvent = (TutorialEvent t) =>
                    {
                        if (t.Action =="ChooseDigTask")
                        {
                            NextState();
                        }
                        else if (t.Action == "Cancel")
                        {
                            GotoState("AssignJob");
                        }
                    },
                },
                new TutorialState("ChooseDigTiles")
                {
                    RequiresDefaultControls = false,
                    ControlText = new List<ColoredText>()
                    {
                        "{orange}**Esc: Cancel.**",
                        "{yellow}Select a square with keys or mouse.",
                        " ",
                        "Move: NumPad/Arrows, ,/.: Up/Down.",
                        "(Control+Arrows for diagonal.)",
                        " ",
                        "{cyan}Click / Space: Select.",
                    },
                    // there's a divergence between how the tutorial used to work and how it works now...
                    // now it's possible to accidentally harvest trees in this step
                    InstructionsText = new List<ColoredText>()
                    {
                        "{cyan}Using the mouse or keyboard, select two corners of a rectangular area for your zombie to dig.",
                        " ",
                        "{lime green}What 'dig' means is contextual, depending on the terrain you select:",
                        " ",
                        "{lime green}- Digging on the floor will make a pit.",
                        " ",
                        "{lime green}- Digging in a wall will make a tunnel.",
                        " ",
                        "{lime green}- Digging on a slope levels the slope.",
                        " ",
                        "{lime green}Look below this panel for a hint about what digging does in the highlighted square."
                    },
                    HandleEvent = (TutorialEvent t) =>
                    {
                        if (t.Action == "DesignateDigTask")
                        {
                            NextState();
                        }
                        else if (t.Action == "Cancel")
                        {
                            GotoState("AssignJob");
                        }
                    },
                },
                new TutorialState("WaitForDig")
                {
                    ControlText = new List<ColoredText>()
                    {
                        "Esc: System view.",
                        " ",
                        "Move: NumPad/Arrows, ,/.: Up/Down.",
                        "(Control+Arrows for diagonal.)",
                        "{cyan}Wait: NumPad 5 / Space.",
                        " ",
                        "Enter: Enable auto-pause.",
                        "+/-: Change speed.",
                        " ",
                        "Z: Cast spell, A: Assign job.",
                        " ",
                        "PageUp/Down: Scroll messages.",
                        "A: Achievements, /: Toggle tutorial."
                    },
                    InstructionsText = new List<ColoredText>()
                    {
                        "The zombie shuffles dutifully to complete its task.",
                        " ",
                        "{cyan}Now wait (pass turns) while your zombie digs.",
                        " ",
                        "{lime green}There is a chance that you will unlock one or more additional achievements, depending on where your zombie digs and what it finds.  Note that some types of soil or stone may be too hard to dig through, at least until your zombies equip better tools."
                    },
                    HandleEvent = (TutorialEvent t) =>
                    {
                        if (t.Action == "DigTaskComplete")
                        {
                            if (GetState<TaskHandler>().Minions.Count>1)
                            {
                                GotoState("CameraMode");
                            }
                            else
                            {
                                NextState();
                            }
                        }
                    },
                },
                new TutorialState("CastSpell2")
                {
                    ControlText = new List<ColoredText>()
                    {
                        "Esc: System view.",
                        " ",
                        "Move: NumPad/Arrows, ,/.: Up/Down.",
                        "(Control+Arrows for diagonal.)",
                        " ",
                        "{cyan}Z: Cast spell.",
                        " ",
                        @"\: Toggle tutorial."
                    },
                    InstructionsText = new List<ColoredText>()
                    {
                        "This decaying wretch is but the beginning - soon, you will command an undead horde.",
                        " ",
                        "{lime green}Every zombie under your control raises the cost of the 'raise zombie' spell in sanity points.  Your current sanity points are listed above the left-hand side of the message bar.",
                        " ",
                        "{cyan}Wait (pass turns) until you have 15 sanity points, then raise a second zombie and wait for it to emerge.  Press Z to cast a spell, press A to choose 'raise zombie', and then select a tombstone."
                    },
                    HandleEvent = (TutorialEvent t) =>
                    {
                        if (t.Action=="ShowSpells")
                        {
                            NextState();
                        }
                        else if (t.Action=="Cancel")
                        {
                            GotoState("CastSpell");
                        }
                    },
                },
                new TutorialState("ChooseSpell2")
                {
                    RequiresDefaultControls = false,
                    ControlText = new List<ColoredText>()
                    {
                        "{orange}**Esc: Cancel**.",
                        "{yellow}Choose a spell:",
                        "a) Raise Zombie"
                    },
                    InstructionsText = new List<ColoredText>()
                    {
                        "This decaying wretch is but the beginning - soon, you will command an undead horde.",
                        " ",
                        "{lime green}Every zombie under your control raises the cost of the 'raise zombie' spell in sanity points.  Your current sanity points are listed above the left-hand side of the message bar.",
                        " ",
                        "{cyan}Wait (pass turns) until you have 15 sanity points, then raise a second zombie and wait for it to emerge.  Press Z to cast a spell, press A to choose 'raise zombie', and then select a tombstone."
                    },
                    HandleEvent = (TutorialEvent t) =>
                    {
                        if (t.Action =="ChooseRaiseZombie")
                        {
                            NextState();
                        }
                        else if (t.Action == "Cancel")
                        {
                            GotoState("CastSpell2");
                        }
                    },
                },
                new TutorialState("TargetSpell2")
                {
                    RequiresDefaultControls = false,
                    ControlText = new List<ColoredText>()
                    {
                        "{orange}**Esc: Cancel.**",
                        "{yellow}Select a square with keys or mouse.",
                        " ",
                        "Move: NumPad/Arrows, ,/.: Up/Down.",
                        "(Control+Arrows for diagonal.)",
                        " ",
                        "{cyan}Click / Space: Select.",
                    },
                    InstructionsText = new List<ColoredText>()
                    {
                        "This decaying wretch is but the beginning - soon, you will command an undead horde.",
                        " ",
                        "{lime green}Every zombie under your control raises the cost of the 'raise zombie' spell in sanity points.  Your current sanity points are listed above the left-hand side of the message bar.",
                        " ",
                        "{cyan}Wait (pass turns) until you have 15 sanity points, then raise a second zombie and wait for it to emerge.  Press Z to cast a spell, press A to choose 'raise zombie', and then select a tombstone."
                    },
                    HandleEvent = (TutorialEvent t) =>
                    {
                        if (t.Action=="CastRaiseZombie")
                        {
                            NextState();
                        }
                        else if (t.Action=="Cancel")
                        {
                            GotoState("CastSpell2");
                        }
                    },
                },

                new TutorialState("WaitForZombie2")
                {
                    ControlText = new List<ColoredText>()
                    {
                        "Esc: System view.",
                        " ",
                        "Move: NumPad/Arrows, ,/.: Up/Down.",
                        "(Control+Arrows for diagonal.)",
                        "{cyan}Wait: NumPad 5 / Space.",
                        " ",
                        "Enter: Enable auto-pause.",
                        "+/-: Change speed.",
                        " ",
                        "Z: Cast spell, A: Assign job.",
                        " ",
                        "PageUp/Down: Scroll messages.",
                        "A: Achievements, /: Toggle tutorial."
                    },
                    InstructionsText = new List<ColoredText>()
                    {
                        "This decaying wretch is but the beginning - soon, you will command an undead horde.",
                        " ",
                        "{lime green}Every zombie under your control raises the cost of the 'raise zombie' spell in sanity points.  Your current sanity points are listed above the left-hand side of the message bar.",
                        " ",
                        "{cyan}Wait (pass turns) until you your zombie emerges."
                    },
                    HandleEvent = (TutorialEvent t) =>
                    {
                        if (t.Action=="ZombieEmerges")
                        {
                            // in the unusual circumstance that there are no rocks available, skip to the end of the tutorial
                            if (Game.World.Player.GetComponent<Movement>().CanFindResources(new Dictionary<string, int>() {{"Rock", 1}}))
                            {
                                NextState();
                            }
                            else
                            {
                                GotoState("EndOfTutorial");
                            }
                        }
                    },
                },
                new TutorialState("CameraMode")
                {

                    ControlText = new List<ColoredText>()
                    {
                        "Esc: System view.",
                        "{cyan}Avatar mode (Tab: Navigation mode)",
                        " ",
                        "Move: NumPad/Arrows, ,/.: Up/Down.",
                        "(Control+Arrows for diagonal.)",
                        "Wait: NumPad 5 / Space.",
                        " ",
                        "Enter: Enable auto-pause.",
                        "+/-: Change speed.",
                        " ",
                        "Z: Cast spell, J: Assign job.",
                        " ",
                        "PageUp/Down: Scroll messages.",
                        "A: Achievements, /: Toggle tutorial."
                    },
                    InstructionsText = new List<ColoredText>()
                    {
                        "These mindless servants shall be your hands, eyes, and ears.",
                        " ",
                        "{lime green}Once you have several zombies, there is less need for your necromancer to walk around.  You may wish to spend most of your time in 'Navigation Mode', moving the viewing window independently while your necromancer meditates on a throne or conducts research in a laboratory.",
                        " ",
                        "{cyan}Press Tab to enter Navigation Mode.",
                    },
                    HandleEvent = (TutorialEvent t) =>
                    {
                        if (t.Action=="CameraMode")
                        {
                            NextState();
                        }
                    },
                },
                new TutorialState("MoveCamera")
                {
                    RequiresDefaultControls = false,
                    ControlText = new List<ColoredText>()
                    {
                        "Esc: System view.",
                        "{cyan}Navigation mode (Tab: Avatar mode)",
                        " ",
                        "{cyan}Move: NumPad/Arrows, ,/.: Up/Down",
                        "{cyan}(Control+Arrows for diagonal.)",
                        "{cyan}Wait: NumPad 5 / Control+Space.",
                        " ",
                        "Enter: Enable auto-pause.",
                        "+/-: Change speed.",
                        " ",
                        "Z: Cast spell, J: Assign job.",
                        " ",
                        "PageUp/Down: Scroll messages.",
                        "A: Achievements, /: Toggle tutorial."
                    },
                    InstructionsText = new List<ColoredText>()
                    {
                        "Now you are in navigation mode.",
                        " ",
                        "{lime green}Move the screen around using the keypad or arrows.  Hold Shift to move multiple spaces at a time.  Also try pressing . or , to move the view up or down a level.  To wait in Navigation Mode, press 5 on the keypad, or Control+Space.",
                        " ",
                        "{cyan}Press Tab to return to 'Avatar Mode' and recenter the screen, putting you in direct control of the necromancer."
                    },
                    HandleEvent = (TutorialEvent t) =>
                    {
                        if (t.Action=="MainMode")
                        {
                            NextState();
                        }
                    },

                },
                new TutorialState("AssignJob2")
                {
                    ControlText = new List<ColoredText>()
                    {
                        "Esc: System view.",
                        " ",
                        "Move: NumPad/Arrows, ,/.: Up/Down.",
                        "(Control+Arrows for diagonal.)",
                        "Wait: NumPad 5 / Space.",
                        " ",
                        "Enter: Enable auto-pause.",
                        "+/-: Change speed.",
                        " ",
                        "Z: Cast spell, {cyan}J: Assign job.",
                        " ",
                        "PageUp/Down: Scroll messages.",
                        "A: Achievements, /: Toggle tutorial."
                    },
                    InstructionsText = new List<ColoredText>()
                    {
                        "The stones your zombies hew from the hills can be shaped into walls and pillars.",
                        " ",
                        "{cyan}Press J to assign a job, and then press B to make your zombie build.",
                    },
                    HandleEvent = (TutorialEvent t) =>
                    {
                        if (t.Action == "ShowJobs")
                        {
                            NextState();
                        }
                    }
                },
                new TutorialState("ChooseJob2")
                {
                    RequiresDefaultControls = false,
                    ControlText = new List<ColoredText>()
                    {
                        "{orange}**Esc: Cancel**.",
                        "{yellow}Choose a task:",
                        "a) dig or harvest",
                        "{cyan}b) build walls or floors ($: 1 rock)"
                    },
                    InstructionsText = new List<ColoredText>()
                    {
                        "The stones your zombies hew from the hills can be shaped into walls and pillars.",
                        " ",
                        "{cyan}Press J to assign a job, and then press B to make your zombie build.",
                    },
                    HandleEvent = (TutorialEvent t) =>
                    {
                        if (t.Action =="ChooseBuildTask")
                        {
                            NextState();
                        }
                        else if (t.Action == "Cancel")
                        {
                            GotoState("AssignJob2");
                        }
                    },
                },
                new TutorialState("ChooseBuildTiles")
                {
                    RequiresDefaultControls = false,
                    ControlText = new List<ColoredText>()
                    {
                        "{orange}**Esc: Cancel.**",
                        "{yellow}Select a square with keys or mouse.",
                        " ",
                        "Move: NumPad/Arrows, ,/.: Up/Down.",
                        "(Control+Arrows for diagonal.)",
                        " ",
                        "{cyan}Click / Space: Select.",
                    },
                    InstructionsText = new List<ColoredText>()
                    {
                        "{cyan}Using the mouse or keyboard, select two corners of a rectangular area for your zombie to build.",
                        " ",
                        "{lime green}What 'build' means is contextual, depending on the terrain you select:",
                        " ",
                        "{lime green}- Building on a floor or upward slope will make a wall.",
                        " ",
                        "{lime green}- Building on a downward slope or empty space will make a floor.",
                        " ",
                        "{lime green}Look below this panel for a hint about what building does in the highlighted square."
                    },
                    HandleEvent = (TutorialEvent t) =>
                    {
                        if (t.Action == "DesignateBuildTask")
                        {
                            NextState();
                        }
                        else if (t.Action == "Cancel")
                        {
                            GotoState("AssignJob2");
                        }
                    },
                },
                new TutorialState("WaitForBuild")
                {
                    ControlText = new List<ColoredText>()
                    {
                        "Esc: System view.",
                        " ",
                        "Move: NumPad/Arrows, ,/.: Up/Down.",
                        "(Control+Arrows for diagonal.)",
                        "{cyan}Wait: NumPad 5 / Space.",
                        " ",
                        "Enter: Enable auto-pause.",
                        "+/-: Change speed.",
                        " ",
                        "Z: Cast spell, A: Assign job.",
                        " ",
                        "PageUp/Down: Scroll messages.",
                        "A: Achievements, /: Toggle tutorial."
                    },
                    InstructionsText = new List<ColoredText>()
                    {
                        "{lime green}Building walls or floors requires rocks as ingredients.  Notice that your zombies claim rocks and fetch them; the claimed rocks are highlighted.",
                        " ",
                        "{cyan}Wait for your zombies to build."
                    },
                    HandleEvent = (TutorialEvent t) =>
                    {
                        if (t.Action == "BuildTaskComplete")
                        {
                            NextState();
                        }
                    },
                },
                // Maybe just call it quits at this point
				new TutorialState("EndOfTutorial")
                {
                    ControlText = new List<ColoredText>()
                    {
                        "Esc: System view.",
                        "Navigation mode (Tab: Avatar mode)",
                        " ",
                        "Move: NumPad/Arrows, ,/.: Up/Down",
                        "(Control+Arrows for diagonal.)",
                        "Wait: NumPad 5 / Control+Space.",
                        " ",
                        "Enter: Enable auto-pause.",
                        "+/-: Change speed.",
                        " ",
                        "Z: Cast spell, J: Assign job.",
                        " ",
                        "PageUp/Down: Scroll messages.",
                        "A: Achievements, {cyan}/: Toggle tutorial."
                    },
                    InstructionsText = new List<ColoredText>()
                    {
                        // rewrite
                        "Cruel laughter wells in your throat.  Your fortress will cast a shadow of menace over all the land.  The undead under your command will become a legion, a multitude, an army.  And then all who have wronged you will pay!",
                        " ",
                        "{lime green}Congratulations, you finished the in-game tutorial.  Experiment with different tasks and commands.  See if you can unlock all the achievements in the demo.",
                        " ",
                        "{cyan}Press ? to dismiss these messages."
                    },
                    HandleEvent = (TutorialEvent t) =>
                    {
                        if (t.Action=="HideTutorial")
                        {
                            NextState();
                        }
                    },
                }
            };
        }
		
		public override void Activate()
		{
            Game.World.Events.Subscribe<TutorialEvent>(this, HandleEvent);
			//Game.World.Events.Subscribe<GameEvent>(this, HandleEvent);
			base.Activate();	
		}
		
		public void NextState()
		{
			CurrentIndex+=1;
			TutorialStates[CurrentIndex].Begin();
		}
		
		public void GotoState(string s)
		{
			int i=0;
			foreach(var state in TutorialStates)
			{
				if (state.Name==s)
				{
					CurrentIndex = i;
					TutorialStates[CurrentIndex].Begin();
				}
				i+=1;
			}
		}
		
		public GameEvent HandleEvent(GameEvent g)
		{
            if (!Game.World.StateHandlers.ContainsKey("TutorialHandler"))
            {
                Activate();
            }
            TutorialStates[CurrentIndex].HandleEvent((TutorialEvent) g);
			return g;
		}
		
		
		public class TutorialState
		{
			public string Name;
			public Action OnBegin;
            public Action<TutorialEvent> HandleEvent;
			public List<ColoredText> ControlText;
			public List<ColoredText> InstructionsText;
            public bool RequiresDefaultControls;
            public TutorialState(string name)
			{
				Name = name;
                OnBegin = NonEvent;
				HandleEvent = NonEvent;
				ControlText = new List<ColoredText>();
				InstructionsText = new List<ColoredText>();
                RequiresDefaultControls = true;
			}
			
			public void Begin()
			{
				// probably deal with text stuff here?
				Game.MenuPanel.Dirty = true;
				OnBegin();
			}
            public void NonEvent() { }
			public void NonEvent(TutorialEvent t) {}
		}
		
	}
}