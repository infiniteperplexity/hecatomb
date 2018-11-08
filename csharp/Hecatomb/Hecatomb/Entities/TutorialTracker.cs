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
				//new TutorialState("Achievements")
				//{
					
				//},
				new TutorialState("WaitForZombie")
				{
					
				},
				new TutorialState("Unpausing")
				{
					
				},
				new TutorialState("AssignJob")
				{
					
				},
				new TutorialState("ChooseJob")
				{
					
				},
				new TutorialState("ChooseDigTiles")
				{
					
				},
				new TutorialState("WaitForDig")
				{
					
				},
				new TutorialState("RaiseSecondZombie")
				{
					
				},
				new TutorialState("WaitForSecondZombie")
				{
					
				},
				new TutorialState("CameraMode")
				{
					
				},
				new TutorialState("MoveCamera")
				{
					
				},
				new TutorialState("WaitForZombie")
				{
					
				},
				new TutorialState("EndOfTutorial")
				{
					
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
