/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/25/2018
 * Time: 11:40 AM
 */
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
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
        public int NonTutorialTurns;
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

        public List<ColoredText> GetText()
        {
            var list = new List<ColoredText>();
            if (Current.Name == "Log")
            {
                list = list.Concat(Game.Controls.MenuTop).ToList();
                list.Add(" ");
                list.Add("{yellow}Tutorial: Casting spells / creating zombies.");
                list.Add(" ");
                list.Add("{cyan}Press escape to return to the main view.");
                list[0] = "{cyan}Esc) Go back.";
            }
            else if (!Current.RequiresDefaultControls || Game.Controls is DefaultControls)
            {
                list = Current.ControlText.ToList();
                list = list.Concat(Current.InstructionsText).ToList();
                if (Current.ShowTimeAndSanity > -1)
                {
                    var ins = Game.Time.GetTimeText();
                    ins.Add(" ");
                    ins.Add(Game.World.Player.GetComponent<SpellCaster>().GetSanityText());
                    list.InsertRange(Current.ShowTimeAndSanity, ins);
                }
            }
            else if (Game.Controls == Game.CameraControls)
            {
                list = list.Concat(Game.Controls.MenuTop).ToList();
                list.Add(" ");
                list = list.Concat(OffTutorialCamera).ToList();
                //if (Current.ShowTimeAndSanity > -1)
                //{
                //    var ins = Game.Time.GetTimeText();
                //    ins.Add(" ");
                //    ins.Add(Game.World.Player.GetComponent<SpellCaster>().GetSanityText());
                //    list.InsertRange(Current.ShowTimeAndSanity, ins);
                //}
            }
            else if (Game.Controls is ExamineTileControls)
            {
                list = list.Concat(Game.Controls.MenuTop).ToList();
            }
            else
            {
                Debug.WriteLine(Game.Controls);
                list = list.Concat(Game.Controls.MenuTop).ToList();
                list.Add(" ");
                list = list.Concat(OffTutorialText).ToList();
            }
            return list;
        }

  

        public TutorialHandler()
        {
            MoveCount = 0;
            SlopeCount = 0;
            CurrentIndex = 0;
            Visible = !Options.NoTutorial;
            AddListener<TutorialEvent>(HandleEvent);
            OffTutorialText = new List<ColoredText>() { "{orange}You have strayed from the tutorial.  Press Escape to get back on track or ? to hide tutorial messages." };
            OffTutorialCamera = new List<ColoredText>() { "{orange}You have strayed from the tutorial.  Press Tab to get back on track or ? to hide tutorial messages." };
            TutorialStates = new List<TutorialState>
            {
                new TutorialState("Welcome")
                {
                    InstructionsText = new List<ColoredText>()
                    {
                        "{yellow}Welcome to the Hecatomb in-game tutorial.  Follow the instructions below to proceed through the tutorial, or press ? to turn off these messages and play without the tutorial.",
                        " ",
                        "You walk amongst the tombstones of a hillside graveyard, searching for the site upon which you will build your mighty fortress.",
                        " ",
                        "{lime green}The magenta 'at' symbol is you: {magenta}@",
                        " ",
                        "{cyan}Try walking around using the numeric keypad, WASD/QEXC, or the arrow keys.  One turn will pass for each step you take."
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
                    InstructionsText = new List<ColoredText>()
                    {
                        "{yellow}Tutorial: Movement and vision.",
                        " ",
                        "You walk amongst the tombstones of a hillside graveyard, searching for the site upon which you will build your mighty fortress.",
                        " ",
                        @"{lime green}Look at all those colored Unicode symbols!  What does it all mean?  {GRASSFG}Olive green {lime green}terrain is grass, level with your field of vision.  Dark olive green areas are also grass, one elevation level below you.  Gray areas with {WALLFG}# {lime green}are walls, but they may have walkable floors one level above you.",
                        " ",
                        "{cyan}Try walking around using the numeric keypad, WASD/QEXC, or the arrow keys.  One turn will pass for each step you take."
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
                    InstructionsText = new List<ColoredText>()
                    {
                        "{yellow}Tutorial: Movement and vision.",
                        " ",
                        "You scramble up and down the slopes for a better view of the area.",
                        " ",
                        "{lime green}The {WALLFG}^ {lime green}and {WALLFG}v {lime green}symbols are slopes.  Slopes provide access to different elevation levels in this 3D world.  You can climb up or down a slope by standing on it and pressing . (<) or , (>).  If you try to walk sideways off a cliff or into a wall, you will automatically climb a slope instead if possible.  When you climb up or down, colors change with your relative elevation.",
                        " ",
                        "{cyan}Try climbing up and down a few slopes.  Hover the mouse over a tile for a description of what's there."
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
                    MenuCommands = new List<ColoredText>()
                    {
                        "{yellow}?) Tutorial",
                        "{cyan}Z) Spells",
                        "{gray}J) Jobs",
                        "{gray}L) Log",
                        "{gray}R) Research",
                        "{gray}V) Achievements"
                    },
                    InstructionsText = new List<ColoredText>()
                    {
                        " ",
                        "{yellow}Tutorial: Casting spells / creating zombies.",
                        " ",
                        "Enough of this pointless wandering - it is time to summon an undead servant.",
                        " ",
                        "{cyan}Near where you started, there should be some cross-shaped symbols. These are tombstones.  Find a tombstone - you don't have to stand right next to it.  Then press Z to view a list of spells you can cast, and press A to choose 'raise zombie.'"
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
                    MenuCommands = new List<ColoredText>()
                    {
                        "{cyan}?) Tutorial",
                        "{yellow}Z) Spells",
                        "{gray}J) Jobs",
                        "{gray}L) Log",
                        "{gray}R) Research",
                        "{gray}V) Achievements"
                    },
                    // oh all of a sudden we *do* need this...
                    ControlText = new List<ColoredText>()
                    {
                        "{orange}**Esc: Cancel**.",
                        " ",
                        "{yellow}Choose a spell:",
                        "{cyan}a) Raise Zombie",
                        " "
                    },
                    InstructionsText = new List<ColoredText>()
                    {
                        "{yellow}Tutorial: Casting spells / creating zombies.",
                        " ",
                        "Enough of this pointless wandering - it is time to summon an undead servant.",
                        " ",
                        "{cyan}Near where you started, there should be some cross-shaped symbols. These are tombstones.  Find a tombstone - you don't have to stand right next to it.  Then press Z to view a list of spells you can cast, and press A to choose 'raise zombie.'"
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
                    MenuCommands = new List<ColoredText>()
                    {
                        "{cyan}?) Tutorial",
                        "{yellow}Z) Spells",
                        "{gray}J) Jobs",
                        "{gray}L) Log",
                        "{gray}R) Research",
                        "{gray}V) Achievements"
                    },
                    ControlText = new List<ColoredText>()
                    {
                        "{orange}**Esc: Cancel.**",
                        " ",
                        "{yellow}Select a square with keys or mouse."
                    },
                    InstructionsText = new List<ColoredText>()
                    {
                        "{yellow}Tutorial: Casting spells / creating zombies.",
                        " ",
                        "{cyan}Select a tombstone, either by using the mouse, or by navigating with the direction keys and clicking or pressing space to select.  Make sure the tombstone is on the current elevation level.",
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
                // let's skip over this step for now...it's a little hairy
                new TutorialState("Achievements")
                {
                    MenuCommands = new List<ColoredText>()
                    {
                        "{cyan}?) Tutorial",
                        "Z) Spells",
                        "{gray}J) Jobs",
                        "{cyan}L) Log",
                        "{gray}R) Research",
                        "{cyan}V) Achievements"
                    },
                    InstructionsText = new List<ColoredText>()
                    {
                        "{yellow}Tutorial: Casting spells / creating zombies.",
                        " ",
                        "Forbidden runes swirl around you as you call forth a corpse from its grave.",
                        " ",
                        "{lime green}You just earned an achievement, which has been noted in your message log and on the achievements list.",
                        " ",
                        "{cyan}Press 'L' to view the message log."
                    },
                    HandleEvent = (TutorialEvent t) =>
                    {
                        if (t.Action=="ShowLog")
                        {
                            NextState();
                        }
                        else if (t.Action=="ZombieEmerges")
                        {
                            GotoState("Unpausing");
                        }
                    },
                },
                new TutorialState("Log")
                {
                    RequiresDefaultControls = false,
                    MenuCommands = new List<ColoredText>()
                    {
                        "{cyan}?) Tutorial",
                        "Z) Spells",
                        "{gray}J) Jobs",
                        "{yellow}L) Log",
                        "{gray}R) Research",
                        "{cyan}V) Achievements"
                    },
                    InstructionsText = new List<ColoredText>()
                    {
                        // needs special handling
                    },
                    HandleEvent = (TutorialEvent t) =>
                    {
                        if (t.Action=="Cancel")
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
                    MenuCommands = new List<ColoredText>()
                    {
                        "{cyan}?) Tutorial",
                        "Z) Spells",
                        "{gray}J) Jobs",
                        "L) Log",
                        "{gray}R) Research",
                        "V) Achievements"
                    },
                    InstructionsText = new List<ColoredText>()
                    {
                        "{yellow}Tutorial: Casting spells / creating zombies.",
                        " ",
                        "You wait impatiently as your undead thrall claws its way out of its grave.",
                        " ",
                        "{lime green}The orange background around the tombstone indicates that there is a task assigned in that square.",
                        " ",
                        @"{cyan}Press keypad 5 or space bar several times, to pass turns (""wait"") until your zombie emerges."
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
                    ShowTimeAndSanity = 4,
                    MenuCommands = new List<ColoredText>()
                    {
                        "{cyan}?) Tutorial",
                        "Z) Spells",
                        "{gray}J) Jobs",
                        "L) Log",
                        "{gray}R) Research",
                        "V) Achievements"
                    },
                    InstructionsText = new List<ColoredText>()
                    {
                        "{yellow}Tutorial: Pausing / Speed.",
                        " ",
                        " ",
                        "Your minion bursts forth from the ground!",
                        " ",
                        "{lime green}Notice the word 'Paused' near the top left corner of the screen.  The game is currently auto-paused - one turn will pass for each action you take.  If you turn auto-pause off, turns will pass in realtime even if you take no actions.  You can press + or - to make time pass faster or slower.",
                        " ",
                        "{cyan}Press Enter / Return to turn off auto-pause, then wait for several turns to pass.  Press '+' or '-' to adjust the speed at which time passes when the game is unpaused.",
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
                        if (t.Action == "DigTaskComplete")
                        {
                            if (GetState<TaskHandler>().Minions.Count>1)
                            {
                                GotoState("CameraMode");
                            }
                            else
                            {
                                GotoState("CastSpell2");
                            }
                        }
                        else if (HasUnpaused && TurnsWaited>=15)
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
                    ShowTimeAndSanity = 4,
                    InstructionsText = new List<ColoredText>()
                    {
                        "{yellow}Tutorial: Assigning tasks.",
                        " ",
                        " ",
                        "You close your eyes and concentrate, formulating a task for your unthinking slave.",
                        " ",
                        "{cyan}Press J to assign a job, and then press A to make your zombie dig.  You can assign a job from any distance.",
                        " ",
                        "{lime green}(Remember, if auto-pause is turned off, you can turn it back on by pressing Enter / Return.)"
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
                                GotoState("CastSpell2");
                            }
                        }
                        else if (t.Action == "ShowJobs")
                        {
                            NextState();
                        }
                    }
                },
                new TutorialState("ChooseJob")
                {
                    RequiresDefaultControls = false,
                    MenuCommands = new List<ColoredText>()
                    {
                        "{cyan}?) Tutorial",
                        "Z) Spells",
                        "{yellow}J) Jobs",
                        "L) Log",
                        "{gray}R) Research",
                        "V) Achievements"
                    },
                    ControlText = new List<ColoredText>()
                    {
                        "{orange}**Esc: Cancel**.",
                        " ",
                        "{yellow}Choose a task:",
                        "{cyan}a) Dig or harvest",
                        " "
                    },
                    InstructionsText = new List<ColoredText>()
                    {
                        "{yellow}Tutorial: Assigning tasks.",
                        " ",
                        "You close your eyes and concentrate, formulating a task for your unthinking slave.",
                        " ",
                        "{cyan}Press J to assign a job, and then press A to make your zombie dig.  You can assign a job from any distance.",
                        " ",
                        "{lime green}(Remember, if auto-pause is turned off, you can turn it back on by pressing Enter / Return.)"
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
                                GotoState("CastSpell2");
                            }
                        }
                        else if (t.Action =="ChooseDigTask")
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
                    MenuCommands = new List<ColoredText>()
                    {
                        "{cyan}?) Tutorial",
                        "Z) Spells",
                        "{yellow}J) Jobs",
                        "L) Log",
                        "{gray}R) Research",
                        "V) Achievements"
                    },
                    ControlText = new List<ColoredText>()
                    {
                        "{orange}**Esc: Cancel.**",
                        " ",
                        "{yellow}Select a square with keys or mouse.",
                        " "
                    },
                    // there's a divergence between how the tutorial used to work and how it works now...
                    // now it's possible to accidentally harvest trees in this step
                    InstructionsText = new List<ColoredText>()
                    {
                        "{yellow}Tutorial: Assigning tasks.",
                        " ",
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
                        if (t.Action == "DigTaskComplete")
                        {
                            if (GetState<TaskHandler>().Minions.Count>1)
                            {
                                GotoState("CameraMode");
                            }
                            else
                            {
                                GotoState("CastSpell2");
                            }
                        }
                        else if (t.Action == "DesignateDigTask")
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
                    ShowTimeAndSanity = 4,
                    MenuCommands = new List<ColoredText>()
                    {
                        "{cyan}?) Tutorial",
                        "Z) Spells",
                        "J) Jobs",
                        "L) Log",
                        "{gray}R) Research",
                        "V) Achievements"
                    },
                    InstructionsText = new List<ColoredText>()
                    {
                        "{yellow}Tutorial: Assigning tasks.",
                        " ",
                        " ",
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
                    ShowTimeAndSanity = 4,
                    MenuCommands = new List<ColoredText>()
                    {
                        "{cyan}?) Tutorial",
                        "{cyan}Z) Spells",
                        "J) Jobs",
                        "L) Log",
                        "{gray}R) Research",
                        "V) Achievements"
                    },
                    InstructionsText = new List<ColoredText>()
                    {
                        "{yellow}Tutorial: Raising multiple zombies.",
                        " ",
                        " ",
                        "This decaying wretch is but the beginning - soon, you will command an undead horde.",
                        " ",
                        "{lime green}Every zombie under your control raises the cost of the 'raise zombie' spell in sanity points.  Your current sanity points are listed near the top of this panel.",
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
                            // was this the problem right here?
                            GotoState("CastSpell2");
                        }
                    },
                },
                new TutorialState("ChooseSpell2")
                {
                    RequiresDefaultControls = false,
                    MenuCommands = new List<ColoredText>()
                    {
                        "{cyan}?) Tutorial",
                        "{yellow}Z) Spells",
                        "J) Jobs",
                        "L) Log",
                        "{gray}R) Research",
                        "V) Achievements"
                    },
                    ControlText = new List<ColoredText>()
                    {
                        "{orange}**Esc: Cancel**.",
                        " ",
                        "{yellow}Choose a spell:",
                        "a) Raise Zombie",
                        " "
                    },
                    InstructionsText = new List<ColoredText>()
                    {
                        "{yellow}Tutorial: Raising multiple zombies.",
                        " ",
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
                    MenuCommands = new List<ColoredText>()
                    {
                        "{cyan}?) Tutorial",
                        "{yellow}Z) Spells",
                        "J) Jobs",
                        "L) Log",
                        "{gray}R) Research",
                        "V) Achievements"
                    },
                    ControlText = new List<ColoredText>()
                    {
                        "{orange}**Esc: Cancel.**",
                        " ",
                        "{yellow}Select a square with keys or mouse.",
                        " "
                    },
                    InstructionsText = new List<ColoredText>()
                    {
                        "{yellow}Tutorial: Raising multiple zombies.",
                        " ",
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
                    ShowTimeAndSanity = 4,
                    MenuCommands = new List<ColoredText>()
                    {
                        "{cyan}?) Tutorial",
                        "Z) Spells",
                        "J) Jobs",
                        "L) Log",
                        "{gray}R) Research",
                        "V) Achievements"
                    },
                    InstructionsText = new List<ColoredText>()
                    {
                        "{yellow}Tutorial: Raising multiple zombies.",
                        " ",
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
                            //in the unusual circumstance that there are no rocks available, skip to the end of the tutorial
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
                    ShowTimeAndSanity = 6,
                    MenuCommands = new List<ColoredText>()
                    {
                        "{cyan}?) Tutorial",
                        "Z) Spells",
                        "J) Jobs",
                        "L) Log",
                        "{gray}R) Research",
                        "V) Achievements"
                    },
                    ControlText = new List<ColoredText>()
                    {
                        "Esc: Game menu.",
                        " ",
                        "{cyan}Avatar (Tab: Navigate)",
                        " ",
                    },
                    InstructionsText = new List<ColoredText>()
                    {
                        "{yellow}Tutorial: Navigation mode.",
                        " ",
                        " ",
                        "These mindless servants shall be your hands, eyes, and ears.",
                        " ",
                        "{lime green}Once you have several zombies, there is less need for your necromancer to walk around.  You may wish to spend most of your time in 'Navigation Mode', moving the viewing window independently while your necromancer meditates on a throne or conducts research in a laboratory.",
                        " ",
                        "{cyan}Press Tab to enter navigation mode.",
                    },
                    HandleEvent = (TutorialEvent t) =>
                    {
                        if (t.Action=="BuildTaskComplete")
                        {
                            GotoState("EndOfTutorial");
                        }
                        else if (t.Action=="CameraMode")
                        {
                            NextState();
                        }
                    },
                },
                new TutorialState("MoveCamera")
                {
                    RequiresDefaultControls = false,
                    ShowTimeAndSanity = 6,
                    MenuCommands = new List<ColoredText>()
                    {
                        "{cyan}?) Tutorial",
                        "Z) Spells",
                        "J) Jobs",
                        "L) Log",
                        "{gray}R) Research",
                        "V) Achievements"
                    },
                    ControlText = new List<ColoredText>()
                    {
                        "Esc: Game menu.",
                        " ",
                        "{cyan}Navigate (Tab: Avatar)",
                        " ",
                    },
                    InstructionsText = new List<ColoredText>()
                    {
                        "{yellow}Tutorial: Navigation mode.",
                        " ",
                        " ",
                        "Now you are in navigation mode.",
                        " ",
                        "{lime green}Move the screen around using the keypad, WASD/QEXC, or arrows.  Hold Shift to move multiple spaces at a time.  Also try pressing . or , to move the view up or down a level.  To wait in Navigation Mode, press 5 on the keypad, or Control+Space.",
                        " ",
                        "{cyan}Press Tab to return to 'Avatar Mode' and recenter the screen, putting you in direct control of the necromancer."
                    },
                    HandleEvent = (TutorialEvent t) =>
                    {
                        if (t.Action=="BuildTaskComplete")
                        {
                            GotoState("EndOfTutorial");
                        }
                        else if (t.Action=="MainMode")
                        {
                            NextState();
                        }
                    },

                },
                new TutorialState("AssignJob2")
                {
                    ShowTimeAndSanity = 6,
                    ControlText = new List<ColoredText>()
                    {
                        "Esc: Game menu.",
                        " ",
                        "{yellow}Avatar (Tab: Navigate)",
                        " ",
                    },
                    MenuCommands = new List<ColoredText>()
                    {
                        "{cyan}?) Tutorial",
                        "Z) Spells",
                        "{cyan}J) Jobs",
                        "L) Log",
                        "{gray}R) Research",
                        "V) Achievements"
                    },
                    InstructionsText = new List<ColoredText>()
                    {
                        "{yellow}Tutorial: Assigning tasks with ingredients.",
                        " ",
                        " ",
                        "The stones your zombies hew from the hills can be shaped into walls and pillars.",
                        " ",
                        "{cyan}Press J to assign a job, and then press B to make your zombie build.  Your zombies need one rock for each wall, so don't assign too many.",
                    },
                    HandleEvent = (TutorialEvent t) =>
                    {
                        if (t.Action=="AnyBuildComplete")
                        {
                            GotoState("EndOfTutorial");
                        }
                        else if (t.Action == "ShowJobs")
                        {
                            NextState();
                        }
                    }
                },
                new TutorialState("ChooseJob2")
                {
                    RequiresDefaultControls = false,
                    MenuCommands = new List<ColoredText>()
                    {
                        "{cyan}?) Tutorial",
                        "Z) Spells",
                        "{yellow}J) Jobs",
                        "L) Log",
                        "{gray}R) Research",
                        "V) Achievements"
                    },
                    ControlText = new List<ColoredText>()
                    {
                        "{orange}**Esc: Cancel**.",
                        " ",
                        "{yellow}Choose a task:",
                        "a) dig or harvest",
                        "{cyan}b) build walls or floors ($: 1 rock)",
                        " "
                    },
                    InstructionsText = new List<ColoredText>()
                    {
                        "{yellow}Tutorial: Assigning tasks with ingredients.",
                        " ",
                        " ",
                        "The stones your zombies hew from the hills can be shaped into walls and pillars.",
                        " ",
                        "{cyan}Press J to assign a job, and then press B to make your zombie build.  Your zombies need one rock for each wall, so don't assign too many.",
                    },
                    HandleEvent = (TutorialEvent t) =>
                    {
                        if (t.Action=="BuildTaskComplete")
                        {
                            GotoState("EndOfTutorial");
                        }
                        else if (t.Action =="ChooseBuildTask")
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
                    MenuCommands = new List<ColoredText>()
                    {
                        "{cyan}?) Tutorial",
                        "Z) Spells",
                        "{yellow}J) Jobs",
                        "L) Log",
                        "{gray}R) Research",
                        "V) Achievements"
                    },
                    ControlText = new List<ColoredText>()
                    {
                        "{orange}**Esc: Cancel.**",
                        " ",
                        "{yellow}Select a square with keys or mouse.",
                        " "
                    },
                    InstructionsText = new List<ColoredText>()
                    {
                        "{yellow}Tutorial: Assigning tasks with ingredients.",
                        " ",
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
                        if (t.Action=="BuildTaskComplete")
                        {
                            GotoState("EndOfTutorial");
                        }
                        else if (t.Action == "DesignateBuildTask")
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
                    ShowTimeAndSanity = 6,
                    MenuCommands = new List<ColoredText>()
                    {
                        "{cyan}?) Tutorial",
                        "Z) Spells",
                        "J) Jobs",
                        "L) Log",
                        "{gray}R) Research",
                        "V) Achievements"
                    },
                    ControlText = new List<ColoredText>()
                    {
                        "Esc: Game menu.",
                        " ",
                        "{yellow}Avatar (Tab: Navigate)",
                        " ",
                    },
                    InstructionsText = new List<ColoredText>()
                    {
                        "{yellow}Tutorial: Assigning tasks with ingredients.",
                        " ",
                        " ",
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
                // This may need to be split into two steps, one of which has all the different commands listed
				new TutorialState("EndOfTutorial")
                {
                    ShowTimeAndSanity = 6,
                    MenuCommands = new List<ColoredText>()
                    {
                        "{cyan}?) Tutorial",
                        "Z) Spells",
                        "J) Jobs",
                        "L) Log",
                        "R) Research",
                        "V) Achievements"
                    },
                    ControlText = new List<ColoredText>()
                    {
                        "Esc: Game menu.",
                        " ",
                        "{yellow}Avatar (Tab: Navigate)",
                        " "
                    },
                    InstructionsText = new List<ColoredText>()
                    {
                        "{yellow}End of tutorial.",
                        " ",
                        " ",
                        "Cruel laughter wells in your throat.  Your fortress will cast a shadow of menace over all the land.  The undead under your command will become a legion, a multitude, an army.  And then all who have wronged you will pay!",
                        " ",
                        "{lime green}Congratulations, you finished the in-game tutorial.  Experiment with different tasks and commands.  Your next step might be to construct buildings, like a Guard Post and a Workshop.  Then research Flint Tools so you can dig through harder stone.",
                        " ",
                        "{cyan}Press ? to dismiss these messages. If you return to the tutorial later, you will see a list of commands."
                    },
                    HandleEvent = (TutorialEvent t) =>
                    {
                        Debug.WriteLine("Checking end of tutorial");
                        if (t.Action=="HideTutorial")
                        {
                            NextState();
                        }
                    },
                },
                new TutorialState("CommandsReference")
                {
                    ShowTimeAndSanity = 6,
                    MenuCommands = new List<ColoredText>()
                    {
                        "{cyan}?) Tutorial",
                        "Z) Spells",
                        "J) Jobs",
                        "L) Log",
                        "R) Research",
                        "V) Achievements"
                    },
                    ControlText = new List<ColoredText>()
                    {
                        "Esc: Game menu.",
                        " ",
                        "{yellow}Avatar (Tab: Navigate)",
                        " "
                    },
                    InstructionsText = new List<ColoredText>()
                    {
                        "{yellow}Tutorial: Commands.",
                        " ",
                        " ",
                        "{cyan}WASD/QEXC/arrows/keypad to walk or move cursor.",
                        " ",
                        "{cyan}Keypad 5/Space to wait (Ctrl+Space in navigation mode.)",
                        " ",
                        "{cyan}Space to select with cursor.",
                        " ",
                        "{cyan}M to view minions, U to view structures.",

                        " ",
                        "{cyan}Enter/Return to toggle auto-pause.",
                        " ",
                        "{cyan}+ or - to adjust speed."
                    },
                    HandleEvent = (TutorialEvent t) =>
                    {
                    },
                }
            };
            if (Options.NoTutorial)
            {
                GotoState("CommandsReference");
            }
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
            if (g is TurnBeginEvent && !Visible)
            {
                NonTutorialTurns += 1;
                if (NonTutorialTurns > 50 && Current.Name != "CommandsReference")
                {
                    GotoState("CommandsReference");
                }
            }
            if (g is TutorialEvent)
            {
                TutorialEvent t = (TutorialEvent)g;
                if (t.Action == "AnyBuildComplete" || (t.Action == "ZombieEmerges" && GetState<TaskHandler>().Minions.Count > 2))
                {
                    if (Visible && Current.Name != "CommandsReference")
                    {
                        GotoState("EndOfTutorial");
                    }
                    else
                    {
                        GotoState("CommandsReference");
                    }
                }
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
            public int ShowTimeAndSanity;
            public List<ColoredText> MenuCommands;
            
            public TutorialState(string name)
			{
				Name = name;
                OnBegin = NonEvent;
				HandleEvent = NonEvent;
				ControlText = new List<ColoredText>() { "Esc: Game menu.", " "};
				InstructionsText = new List<ColoredText>();
                RequiresDefaultControls = true;
                ShowTimeAndSanity = -1;
                MenuCommands = new List<ColoredText>()
                {
                    "{cyan}?) Tutorial",
                    "{gray}Z) Spells",
                    "{gray}J) Jobs",
                    "{gray}L) Log",
                    "{gray}R) Research",
                    "{gray}V) Achievements"
                };
			}
			
			public void Begin()
			{
                // probably deal with text stuff here?
                InterfacePanel.DirtifySidePanels();
                OnBegin();
			}
            public void NonEvent() { }
			public void NonEvent(TutorialEvent t) {}
		}
		
	}
}
