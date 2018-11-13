/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/25/2018
 * Time: 11:40 AM
 */
using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace Hecatomb
{
    /// <summary>
    /// Description of TutorialTracker.
    /// </summary>
    public class TutorialTracker : StateTracker
    {
        public int MoveCount;
        public int SlopeCount;
        public bool HasUnpaused;
        public int TurnsWaited;

        public int CurrentIndex;
        // the tutorial is always active but it's not always visible
        public bool Visible;
        public List<TutorialState> TutorialStates;
        public TutorialState Current
        {
            get
            {
                return TutorialStates[CurrentIndex];
            }
            set { }
        }
        protected TextColors StrayColorsMiddle = new TextColors("orange");
		
		public override void Activate()
		{
			MoveCount = 0;
            SlopeCount = 0;
			CurrentIndex = 0;
            Visible = true;
            Game.World.Events.Subscribe<TutorialEvent>(this, HandleEvent);
			//Game.World.Events.Subscribe<GameEvent>(this, HandleEvent);
			base.Activate();
            TutorialStates = new List<TutorialState>
            {
                new TutorialState("Welcome")
                {
                    ControlText = new List<string>()
                    {
                        "Esc: System view.",
                        " ",
                        "Move: NumPad/Arrows.",
                        "(Control+Arrows for diagonal.)",
                        " ",
                        @"\: Toggle tutorial."
                    },
                    ControlColors = new TextColors(2,"cyan"),
                    InstructionsText = new List<string>()
                    {
                        "Welcome to the Hecatomb in-game tutorial.  Follow the instructions on this panel to proceed through the tutorial, or press ? to turn off these messages and play without the tutorial.",
                        " ",
                        "You walk amongst the tombstones of a hillside graveyard, searching for the site upon which you will build your mighty fortress.",
                        " ",
                        "This symbol is you: @",
                        " ",
                        "Try walking around using the numeric keypad.If your keyboard has no keypad, use the arrow keys.One turn will pass for each step you take."
                    },
                    InstructionsColors = new TextColors(0, "yellow", 2, "white", 4, "lime green", 4, 4, "magenta", 6, "cyan"),
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
                    ControlText = new List<string>()
                    {
                        "Esc: System view.",
                        " ",
                        "Move: NumPad/Arrows.",
                        "(Control+Arrows for diagonal.)",
                        " ",
                        @"\: Toggle tutorial."
                    },
                    ControlColors = new TextColors(2,"cyan"),
                    InstructionsText = new List<string>()
                    {
                        "You walk amongst the tombstones of a hillside graveyard, searching for the site upon which you will build your mighty fortress.",
                        " ",
                        @"- Green areas with "" are grass.",
                        " ",
                        "- Dim green areas are also grass, one elevation level below you.",
                        " ",
                        "- Gray areas with # are walls, but they may have walkable floors one level above you.",
                        " ",
                        "- Other symbols (clubs, spades, flowers) may be trees or plants.",
                        " ",
                        "- Letters such as s or b are wild animals, mostly harmless for now.",
                        " ",
                        "Try walking around using the numeric keypad.  If your keyboard has no keypad, use the arrow keys.  One turn will pass for each step you take."
                    },
                    InstructionsColors = new TextColors(
                        2, "lime green",2,4,"GRASSFG",2,5,"lime green",
                        4, "lime green",    
                        6, "lime green", 6,4,"WALLFG",6,5,"lime green",
                        8, "lime green",
                        10, "lime green",
                        12, "cyan"
                    ),
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
                    ControlText = new List<string>()
                    {
                        "Esc: System view.",
                        " ",
                        "Move: NumPad/Arrows, ,/.: Up/Down.",
                        "(Control+Arrows for diagonal.)",
                        " ",
                        @"\: Toggle tutorial."
                    },
                    ControlColors = new TextColors(2,2,"cyan"),
                    InstructionsText = new List<string>()
                    {
                        "You scramble up and down the slopes for a better view of the area.",
                        " ",
                        "- The ^ and v symbols are slopes.",
                        " ",
                        "- The game world is a 3D grid of tiles.  Slopes provide access to different elevation levels.",
                        " ",
                        "- You can climb up or down a slope by standing on it and pressing . (<) or , (>).",
                        " ",
                        "- If you try to walk sideways off a cliff or into a wall, you will automatically climb a slope instead if possible.",
                        " ",
                        "- When you climb up or down, colors change with your relative elevation.",
                        " ",
                        "Try climbing up and down a few slopes."
                    },
                    InstructionsColors = new TextColors(
                        2, "lime green",2,2,"WALLFG",2,3,"lime green",2,4,"WALLFG",2,5,"lime green",
                        4, "lime green",
                        6, "lime green",
                        8, "lime green",
                        10, "lime green",
                        12, "cyan"
                    ),
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
                    ControlText = new List<string>()
                    {
                        "Esc: System view.",
                        " ",
                        "Move: NumPad/Arrows, ,/.: Up/Down.",
                        "(Control+Arrows for diagonal.)",
                        " ",
                        "Z: Cast spell.",
                        " ",
                        @"\: Toggle tutorial."
                    },
                    ControlColors = new TextColors(5,"cyan"),
                    InstructionsText = new List<string>()
                    {
                        "Enough of this pointless wandering - it is time to summon an undead servant.",
                        " ",
                        "Near where you started, there should be some cross-shaped symbols. These are tombstones.  If you want to know what a symbol represents, hover over it with the mouse and look at the bottom half of the right panel.",
                        " ",
                        "Find a tombstone - you don't have to stand right next to it.  Then press Z to view a list of spells you can cast, and press A to choose 'raise zombie.'"
                    },
                    InstructionsColors = new TextColors(
                        2, "lime green",
                        4, "cyan"
                    ),
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
                    ControlText = new List<string>()
                    {
                        "**Esc: Cancel**.",
                        "Choose a spell:",
                        "a) Raise Zombie"
                    },
                    ControlColors = new TextColors(0, "orange", 1, "yellow", 2, "cyan"),
                    InstructionsText = new List<string>()
                    {
                        "Enough of this pointless wandering - it is time to summon an undead servant.",
                        " ",
                        "Near where you started, there should be some cross-shaped symbols. These are tombstones.  If you want to know what a symbol represents, hover over it with the mouse and look at the bottom half of the right panel.",
                        " ",
                        "Find a tombstone - you don't have to stand right next to it.  Then press Z to view a list of spells you can cast, and press A to choose 'raise zombie.'"
                    },
                    InstructionsColors = new TextColors(
                        2, "lime green",
                        4, "cyan"
                    ),                 
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
                    ControlText = new List<string>()
                    {
                        "**Esc: Cancel.**",
                        "Select a square with keys or mouse.",
                        " ",
                        "Move: NumPad/Arrows, ,/.: Up/Down.",
                        "(Control+Arrows for diagonal.)",
                        " ",
                        "Click / Space: Select.",
                    },
                    ControlColors = new TextColors(0, "orange", 1, "yellow", 6, "cyan"),
                    InstructionsText = new List<string>()
                    {
                        "Select a tombstone, either by using the mouse, or by navigating with the direction keys and pressing space to select.  Make sure the tombstone is on the current elevation level.",
                        " ",
                        "Notice that the bottom portion of this panel gives you information about the square you are hovering over - whether it's a valid target for your spell, what the terrain is like, and so on."
                    },
                    InstructionsColors = new TextColors(
                        0, "cyan",
                        2, "lime green"
                    ),
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
                    ControlText = new List<string>()
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
                        "A: Achievements, /: Toggle tutorial."
                    },
                    ControlColors = new TextColors(11, "cyan", 11, 2, "white"),
                    InstructionsText = new List<string>()
                    {
                        "Forbidden runes swirl around you as you call forth a corpse from its grave.",
                        " ",
                        "You just earned an achievement, as noted on the message bar below the play area.  You can scroll messages up and down using the PageUp and PageDown keys (on a Mac, Fn+Arrows.)",
                        " ",
                        "Press 'A' to view the achievements screen."                    },
                    InstructionsColors = new TextColors(
                        2, "lime green",
                        4, "cyan"
                    ),
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
                    ControlText = new List<string>()
                    {
                        "Esc: System view.",
                        " ",
                        "Move: NumPad/Arrows, ,/.: Up/Down.",
                        "(Control+Arrows for diagonal.)",
                        "Wait: NumPad 5 / Space.",
                        " ",
                        "Z: Cast spell.",
                        " ",
                        "PageUp/Down: Scroll messages.",
                        "A: Achievements, /: Toggle tutorial."
                    },
                    ControlColors = new TextColors(4, "cyan"),
                    InstructionsText = new List<string>()
                    {
                        "You wait impatiently as your undead thrall claws its way out of its grave.",
                        " ",
                        "The orange background around the tombstone indicates that there is a task assigned in that square.",
                        " ",
                        @"Press 5 on the numeric keypad several times, to pass turns (""wait"") until your zombie emerges.  If you have no numeric keypad, press Space to wait."
                    },
                    InstructionsColors = new TextColors(
                        2, "lime green",
                        4, "cyan"
                    ),
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
                    ControlText = new List<string>()
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
                        "Z: Cast spell.",
                        " ",
                        "PageUp/Down: Scroll messages.",
                        "A: Achievements, /: Toggle tutorial."
                    },
                    ControlColors = new TextColors(6, "cyan", 7, "cyan"),
                    InstructionsText = new List<string>()
                    {
                        "Your minion bursts forth from the ground!",
                        " ",
                        "The word 'Pause' above the right-hand side of the message bar.  The game is currently auto-paused - one turn will pass for each action you take.  If you turn auto-pause off, turns will pass in realtime even if you take no actions.  You can press + or - to make time pass faster or slower.",
                        " ",
                        "Press Enter / Return to turn off auto-pause, then wait for several turns to pass.",
                        " ",
                        "Your zombie will wander a short distance from you.  If it seems to disappear, it probably went up or down a slope."                    },
                    InstructionsColors = new TextColors(
                        2, "lime green",
                        4, "cyan",
                        6, "lime green"
                    ),
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
                    ControlText = new List<string>()
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
                        "Z: Cast spell, J: Assign job.",
                        " ",
                        "PageUp/Down: Scroll messages.",
                        "A: Achievements, /: Toggle tutorial."
                    },
                    ControlColors = new TextColors(9,3, "cyan"),
                    InstructionsText = new List<string>()
                    {
                        "You close your eyes and concentrate, formulating a task for your unthinking slave.",
                        " ",
                        "Press J to assign a job, and then press A to make your zombie dig.  You can assign a job from any distance.",
                        " ",
                        "(Remember, if auto-pause is turned off, you can turn it back on by pressing Enter / Return.)"
                    },
                    InstructionsColors = new TextColors(
                        2, "lime green",
                        4, "cyan",
                        6, "lime green"
                    ),
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
                    ControlText = new List<string>()
                    {
                        "**Esc: Cancel**.",
                        "Choose a task:",
                        "a) Dig or harvest"
                    },
                    ControlColors = new TextColors(0, "orange", 1, "yellow", 2, "cyan"),
                    InstructionsText = new List<string>()
                    {
                        "You close your eyes and concentrate, formulating a task for your unthinking slave.",
                        " ",
                        "Press J to assign a job, and then press A to make your zombie dig.  You can assign a job from any distance.",
                        " ",
                        "(Remember, if auto-pause is turned off, you can turn it back on by pressing Enter / Return.)"
                    },
                    InstructionsColors = new TextColors(
                        2, "lime green",
                        4, "cyan",
                        6, "lime green"
                    ),
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
                    ControlText = new List<string>()
                    {
                        "**Esc: Cancel.**",
                        "Select a square with keys or mouse.",
                        " ",
                        "Move: NumPad/Arrows, ,/.: Up/Down.",
                        "(Control+Arrows for diagonal.)",
                        " ",
                        "Click / Space: Select.",
                    },
                    ControlColors = new TextColors(0, "orange", 1, "yellow", 6, "cyan"),
                    // there's a divergence between how the tutorial used to work and how it works now...
                    // now it's possible to accidentally harvest trees in this step
                    InstructionsText = new List<string>()
                    {
                        "Using the mouse or keyboard, select two corners of a rectangular area for your zombie to dig.",
                        " ",
                        "What 'dig' means is contextual, depending on the terrain you select:",
                        " ",
                        "- Digging on the floor will make a pit.",
                        " ",
                        "- Digging in a wall will make a tunnel.",
                        " ",
                        "- Digging on a slope levels the slope.",
                        " ",
                        "Look below this panel for a hint about what digging does in the highlighted square."
                    },
                    InstructionsColors = new TextColors(
                        0, "cyan",
                        2, "lime green",
                        4, "lime green",
                        6, "lime green",
                        8, "lime green",
                        10, "lime green"
                    ),
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
                    ControlText = new List<string>()
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
                        "Z: Cast spell, A: Assign job.",
                        " ",
                        "PageUp/Down: Scroll messages.",
                        "A: Achievements, /: Toggle tutorial."
                    },
                    ControlColors = new TextColors(4, "cyan"),
                    InstructionsText = new List<string>()
                    {
                        "The zombie shuffles dutifully to complete its task.",
                        " ",
                        "Now wait (pass turns) while your zombie digs.",
                        " ",
                        "There is a chance that you will unlock one or more additional achievements, depending on where your zombie digs and what it finds.  Note that some types of soil or stone may be too hard to dig through, at least until your zombies equip better tools."
                    },
                    InstructionsColors = new TextColors(
                        2, "lime green",
                        4, "cyan",
                        6, "lime green"
                    ),
                    HandleEvent = (TutorialEvent t) =>
                    {
                        if (t.Action == "DigTaskComplete")
                        {
                            NextState();
                        }
                    },
                },
                new TutorialState("CastSpell2")
                {
                    ControlText = new List<string>()
                    {
                        "Esc: System view.",
                        " ",
                        "Move: NumPad/Arrows, ,/.: Up/Down.",
                        "(Control+Arrows for diagonal.)",
                        " ",
                        "Z: Cast spell.",
                        " ",
                        @"\: Toggle tutorial."
                    },
                    ControlColors = new TextColors(5,"cyan"),
                    InstructionsText = new List<string>()
                    {
                        "This decaying wretch is but the beginning - soon, you will command an undead horde.",
                        " ",
                        "Every zombie under your control raises the cost of the 'raise zombie' spell in sanity points.  Your current sanity points are listed above the left-hand side of the message bar.",
                        " ",
                        "Wait (pass turns) until you have 15 sanity points, then raise a second zombie and wait for it to emerge.  Press Z to cast a spell, press A to choose 'raise zombie', and then select a tombstone."                    },
                    InstructionsColors = new TextColors(
                        2, "lime green",
                        4, "cyan"
                    ),
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
                    ControlText = new List<string>()
                    {
                        "**Esc: Cancel**.",
                        "Choose a spell:",
                        "a) Raise Zombie"
                    },
                    ControlColors = new TextColors(0, "orange", 1, "yellow", 2, "cyan"),
                    InstructionsText = new List<string>()
                    {
                        "This decaying wretch is but the beginning - soon, you will command an undead horde.",
                        " ",
                        "Every zombie under your control raises the cost of the 'raise zombie' spell in sanity points.  Your current sanity points are listed above the left-hand side of the message bar.",
                        " ",
                        "Wait (pass turns) until you have 15 sanity points, then raise a second zombie and wait for it to emerge.  Press Z to cast a spell, press A to choose 'raise zombie', and then select a tombstone."                    },
                    InstructionsColors = new TextColors(
                        2, "lime green",
                        4, "cyan"
                    ),
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
                    ControlText = new List<string>()
                    {
                        "**Esc: Cancel.**",
                        "Select a square with keys or mouse.",
                        " ",
                        "Move: NumPad/Arrows, ,/.: Up/Down.",
                        "(Control+Arrows for diagonal.)",
                        " ",
                        "Click / Space: Select.",
                    },
                    ControlColors = new TextColors(0, "orange", 1, "yellow", 6, "cyan"),
                    InstructionsText = new List<string>()
                    {
                        "This decaying wretch is but the beginning - soon, you will command an undead horde.",
                        " ",
                        "Every zombie under your control raises the cost of the 'raise zombie' spell in sanity points.  Your current sanity points are listed above the left-hand side of the message bar.",
                        " ",
                        "Wait (pass turns) until you have 15 sanity points, then raise a second zombie and wait for it to emerge.  Press Z to cast a spell, press A to choose 'raise zombie', and then select a tombstone."                    },
                    InstructionsColors = new TextColors(
                        2, "lime green",
                        4, "cyan"
                    ),
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
                    ControlText = new List<string>()
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
                        "Z: Cast spell, A: Assign job.",
                        " ",
                        "PageUp/Down: Scroll messages.",
                        "A: Achievements, /: Toggle tutorial."
                    },
                    ControlColors = new TextColors(4, "cyan"),
                    InstructionsText = new List<string>()
                    {
                        "This decaying wretch is but the beginning - soon, you will command an undead horde.",
                        " ",
                        "Every zombie under your control raises the cost of the 'raise zombie' spell in sanity points.  Your current sanity points are listed above the left-hand side of the message bar.",
                        " ",
                        "Wait (pass turns) until you your zombie emerges."
                    },
                    InstructionsColors = new TextColors(
                        2, "lime green",
                        4, "cyan"
                    ),
                    HandleEvent = (TutorialEvent t) =>
                    {
                        if (t.Action=="ZombieEmerges")
                        {
                            NextState();
                        }
                    },
                },
                new TutorialState("CameraMode")
				{
                    ControlText = new List<string>()
                    {
                        "Esc: System view.",
                        "Avatar mode (Tab: Navigation mode)",
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
                    ControlColors = new TextColors(1, "cyan"),
                    InstructionsText = new List<string>()
                    {
                        "These mindless servants shall be your hands, eyes, and ears.",
                        " ",
                        "Once you have several zombies, there is less need for your necromancer to walk around.  You may wish to spend most of your time in 'Navigation Mode', moving the viewing window independently while your necromancer meditates on a throne or conducts research in a laboratory.",
                        " ",
                        "Press Tab to enter Navigation Mode.",
                    },
                    InstructionsColors = new TextColors(
                        2, "lime green",
                        4, "cyan"
                    ),
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
                    ControlText = new List<string>()
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
                        "A: Achievements, /: Toggle tutorial."
                    },
                    ControlColors = new TextColors(1, "cyan", 3, "cyan", 4, "cyan", 5, "cyan"),
                    InstructionsText = new List<string>()
                    {
                        "Now you are in navigation mode.",
                        " ",
                        "Move the screen around using the keypad or arrows.  Hold Shift to move multiple spaces at a time.  Also try pressing . or , to move the view up or down a level.  To wait in Navigation Mode, press 5 on the keypad, or Control+Space.",
                        " ",
                        "Press Tab to return to 'Avatar Mode' and recenter the screen, putting you in direct control of the necromancer."
                    },
                    InstructionsColors = new TextColors(
                        2, "lime green",
                        4, "cyan"
                    ),
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
                    ControlText = new List<string>()
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
                        "Z: Cast spell, J: Assign job.",
                        " ",
                        "PageUp/Down: Scroll messages.",
                        "A: Achievements, /: Toggle tutorial."
                    },
                    ControlColors = new TextColors(9,3, "cyan"),
                    InstructionsText = new List<string>()
                    {
                        "You close your eyes and concentrate, formulating a task for your unthinking slave.",
                        " ",
                        "Press J to assign a job, and then press A to make your zombie dig.  You can assign a job from any distance.",
                        " ",
                        "(Remember, if auto-pause is turned off, you can turn it back on by pressing Enter / Return.)"
                    },
                    InstructionsColors = new TextColors(
                        2, "lime green",
                        4, "cyan",
                        6, "lime green"
                    ),
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
                    ControlText = new List<string>()
                    {
                        "**Esc: Cancel**.",
                        "Choose a task:",
                        "a) dig or harvest",
                        "b) build walls or floors"
                    },
                    ControlColors = new TextColors(0, "orange", 1, "yellow", 3, "cyan"),
                    InstructionsText = new List<string>()
                    {
                        "The stones your zombies hew from the hills can be shaped into walls and pillars.",
                        " ",
                        "Press J to assign a job, and then press B to make your zombie build.",
                    },
                    InstructionsColors = new TextColors(
                        2, "cyan"
                    ),
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
                    ControlText = new List<string>()
                    {
                        "**Esc: Cancel.**",
                        "Select a square with keys or mouse.",
                        " ",
                        "Move: NumPad/Arrows, ,/.: Up/Down.",
                        "(Control+Arrows for diagonal.)",
                        " ",
                        "Click / Space: Select.",
                    },
                    ControlColors = new TextColors(0, "orange", 1, "yellow", 6, "cyan"),
                    InstructionsText = new List<string>()
                    {
                        "Using the mouse or keyboard, select two corners of a rectangular area for your zombie to build.",
                        " ",
                        "What 'build' means is contextual, depending on the terrain you select:",
                        " ",
                        "- Building on a floor or upward slope will make a wall.",
                        " ",
                        "- Building on a downward slope or empty space will make a floor.",
                        " ",
                        "Look below this panel for a hint about what building does in the highlighted square."
                    },
                    InstructionsColors = new TextColors(
                        0, "cyan",
                        2, "lime green",
                        4, "lime green",
                        6, "lime green",
                        8, "lime green"
                    ),
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
                    ControlText = new List<string>()
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
                        "Z: Cast spell, A: Assign job.",
                        " ",
                        "PageUp/Down: Scroll messages.",
                        "A: Achievements, /: Toggle tutorial."
                    },
                    ControlColors = new TextColors(4, "cyan"),
                    InstructionsText = new List<string>()
                    {
                        "Building walls or floors requires rocks as ingredients.  Notice that your zombies claim rocks and fetch them; the claimed rocks are highlighted.",
                        " ",
                        "Wait for your zombies to build."
                    },
                    InstructionsColors = new TextColors(
                        0, "lime green",
                        2, "cyan"
                    ),
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
                    ControlText = new List<string>()
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
                        "A: Achievements, /: Toggle tutorial."
                    },
                    ControlColors = new TextColors(1, "cyan", 3, "cyan", 4, "cyan", 5, "cyan"),
                    InstructionsText = new List<string>()
                    {
                        // rewrite
                        "Cruel laughter wells in your throat.  Your fortress will cast a shadow of menace over all the land.  The undead under your command will become a legion, a multitude, an army.  And then all who have wronged you will pay!",
                        " ",
                        "Congratulations, you finished the in-game tutorial.  Experiment with different tasks and commands.  See if you can unlock all the achievements in the demo.",
                        " ",
                        "Press ? to dismiss these messages."                    },
                    InstructionsColors = new TextColors(
                        2, "lime green",
                        4, "cyan"
                    ),
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
			TutorialStates[CurrentIndex].HandleEvent((TutorialEvent) g);
			return g;
		}
		
		
		public class TutorialState
		{
			public string Name;
			public Action OnBegin;
            public Action<TutorialEvent> HandleEvent;
			public List<string> ControlText;
			public List<string> InstructionsText;
			public TextColors ControlColors;
			public TextColors InstructionsColors;
			public TutorialState(string name)
			{
				Name = name;
                OnBegin = NonEvent;
				HandleEvent = NonEvent;
				ControlText = new List<string>();
				InstructionsText = new List<string>();;
				ControlColors = TextColors.NoColors;
				InstructionsColors = TextColors.NoColors;
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
