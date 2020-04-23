using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Hecatomb8
{
	public interface IMenuListable
	{
		void ChooseFromMenu();
		ColoredText ListOnMenu();
	}

	public interface IChoiceMenu
	{
		void BuildMenu(MenuChoiceControls menu);
		void FinishMenu(MenuChoiceControls menu);
	}

	public class MenuChoiceControls : ControlContext
	{
		public string Header;
		public IChoiceMenu Chooser;
		public List<IMenuListable> Choices;
		public static List<Keys> Alphabet = new List<Keys> {
			Keys.A,
			Keys.B,
			Keys.C,
			Keys.D,
			Keys.E,
			Keys.F,
			Keys.G,
			Keys.H,
			Keys.I,
			Keys.J,
			Keys.K,
			Keys.L,
			Keys.M,
			Keys.N,
			Keys.O,
			Keys.P,
			Keys.Q,
			Keys.R,
			Keys.S,
			Keys.T,
			Keys.U,
			Keys.V,
			Keys.W,
			Keys.X,
			Keys.Y,
			Keys.Z
		};

		public static string alphabet = "abcdefghijklmnopqrstuvwxyz";

		public MenuChoiceControls(IChoiceMenu chooser) : base()
		{
			Header = "";
			Choices = new List<IMenuListable>();
			AllowsUnpause = false;
			Chooser = chooser;
			RefreshContent();
		}

		public override void RefreshContent()
		{
			Chooser.BuildMenu(this);
			var Commands = InterfaceState.Commands!;
			KeyMap[Keys.Space] = () =>
			{
				if (GameState.World != null)
				{
					Commands.Wait();
				}
			};
			KeyMap[Keys.Escape] = InterfaceState.ResetControls;
			MenuTop = new List<ColoredText>() {
				"{orange}**Esc: Cancel**.",
				" ",
				("{yellow}"+Header)
			};
			for (int i = 0; i < Choices.Count; i++)
			{
				KeyMap[Alphabet[i]] = Choices[i].ChooseFromMenu;
				ColoredText ct = new ColoredText(alphabet[i] + ") ") + Choices[i].ListOnMenu();
				MenuTop.Add(ct);
			}
			Chooser.FinishMenu(this);
		}


	}
}
