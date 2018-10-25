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
		public List<TutorialState> TutorialStates;
		
		public override void Activate()
		{
			MoveCount = 0;
			CurrentIndex = 0;
			Game.World.Events.Subscribe<GameEvent>(this, HandleEvent);
			base.Activate();
			TutorialStates = new List<TutorialState>
			{
				new TutorialState("Welcome")
				{
					OnPlayerAction = (PlayerActionEvent p) =>
					{
						if (p.ActionType=="Move")
						{
							MoveCount+=1;
							Coord d = (Coord) p.Details;
							Debug.Print("Player stepped to {0} {1} {2}", d.X, d.Y, d.Z);
							if (MoveCount>=5)
							{
								NextState();
							}
						}
					}
				},
				new TutorialState("Movement")
				{
					OnPlayerAction = (PlayerActionEvent p) =>
					{
						if (p.ActionType=="Move")
						{
							MoveCount+=1;
							Coord d = (Coord) p.Details;
							Debug.Print("Player stepped2 to {0} {1} {2}", d.X, d.Y, d.Z);
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
