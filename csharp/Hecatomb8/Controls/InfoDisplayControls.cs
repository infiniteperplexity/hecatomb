﻿using System;
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

	public interface IDisplayInfo
	{
		void BuildInfoDisplay(InfoDisplayControls menu);
		void FinishInfoDisplay(InfoDisplayControls menu);
	}

	// sometimes we do it without menu choices, even
	public class InfoDisplayControls : AbstractCameraControls
	{
		public string Header;
		public IDisplayInfo Chooser;
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

		public InfoDisplayControls(IDisplayInfo chooser) : base()
		{
			Header = "";
			Choices = new List<IMenuListable>();
			AllowsUnpause = false;
			Chooser = chooser;
			KeyMap.Remove(Keys.W);
			KeyMap.Remove(Keys.A);
			KeyMap.Remove(Keys.S);
			KeyMap.Remove(Keys.D);
			KeyMap.Remove(Keys.Q);
			KeyMap.Remove(Keys.X);
			KeyMap.Remove(Keys.E);
			KeyMap.Remove(Keys.C);
			RefreshContent();
		}

		public override void RefreshContent()
		{
			Chooser.BuildInfoDisplay(this);
			var Commands = InterfaceState.Commands!;
			KeyMap[Keys.Space] = SelectOrWait;
			KeyMap[Keys.NumPad5] = Commands.Wait;
			KeyMap[Keys.Escape] = InterfaceState.ResetControls;
			InfoTop = new List<ColoredText>() {
				"{orange}**Esc: Cancel**.",
				" ",
				("{yellow}"+Header)
			};
			if (Choices.Count == 0)
			{
				KeyMap[Keys.W] = Commands.MoveCameraNorth;
				KeyMap[Keys.S] = Commands.MoveCameraSouth;
				KeyMap[Keys.A] = Commands.MoveCameraWest;
				KeyMap[Keys.D] = Commands.MoveCameraEast;
				KeyMap[Keys.Q] = Commands.MoveCameraNorthWest;
				KeyMap[Keys.E] = Commands.MoveCameraNorthEast;
				KeyMap[Keys.X] = Commands.MoveCameraSouthWest;
				KeyMap[Keys.C] = Commands.MoveCameraSouthEast;
			}
			for (int i = 0; i < Choices.Count; i++)
			{
				KeyMap[Alphabet[i]] = Choices[i].ChooseFromMenu;
				ColoredText ct = new ColoredText(alphabet[i] + ") ") + Choices[i].ListOnMenu();
				InfoTop.Add(ct);
			}

			Chooser.FinishInfoDisplay(this);
		}
	}
}
