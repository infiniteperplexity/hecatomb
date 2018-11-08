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
			CurrentIndex = 0;
            Visible = true;
			Game.World.Events.Subscribe<GameEvent>(this, HandleEvent);
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
                        @"/: Toggle tutorial."
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
                    OnPlayerAction = (PlayerActionEvent p) =>
					{
						if (p.ActionType=="Move")
						{
							MoveCount+=1;
							Coord d = (Coord) p.Details;
							if (MoveCount>=5)
							{
								NextState();
							}
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
                        @"/: Toggle tutorial."
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
                        "- Other symbols (X, Y, Z) may be trees or plants.",
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
                    OnPlayerAction = (PlayerActionEvent p) =>
					{
						if (p.ActionType=="Move")
						{
							MoveCount+=1;
							Coord d = (Coord) p.Details;
							if (MoveCount>=10)
							{
								NextState();
							}
						}
					}
				},
				new TutorialState("Slopes")
				{
					
				},
				new TutorialState("CastSpell")
				{
					
				},
				new TutorialState("ChooseSpell")
				{
					
				},
				new TutorialState("TargetSpell")
				{
					
				},
				new TutorialState("Achievements")
				{
					
				},
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
			TutorialStates[CurrentIndex].HandleEvent(g);
			return g;
		}
		
		
		public class TutorialState
		{
			public string Name;
			public Action OnBegin;
			public Action<PlayerActionEvent> OnPlayerAction;
			public Action<ContextChangeEvent> OnContextChange;
			public List<string> ControlText;
			public List<string> InstructionsText;
			public TextColors ControlColors;
			public TextColors InstructionsColors;
			public TutorialState(string name)
			{
				Name = name;
				OnBegin = NonEvent;
				OnPlayerAction = NonEvent;
				OnContextChange = NonEvent;
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
			public GameEvent HandleEvent(GameEvent g)
			{
				if (g is PlayerActionEvent)
				{
					OnPlayerAction((PlayerActionEvent) g);
				}
				else if (g is ContextChangeEvent)
				{
					OnContextChange((ContextChangeEvent) g);
				}
				return g;
			}
			public void NonEvent() {}
			public void NonEvent(GameEvent g) {}
		}
		
	}
}
