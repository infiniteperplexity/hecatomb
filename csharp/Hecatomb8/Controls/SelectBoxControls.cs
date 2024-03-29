﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using System.Collections.Generic;

namespace Hecatomb8
{
	public interface ISelectsBox
	{
		int BoxWidth { get; set; }
		int BoxHeight { get; set; }
		void SelectBox(List<Coord> squares);
		void BoxHover(Coord c, List<Coord> squares);
		string GetHighlightColor();
	}

	public class SelectBoxControls : AbstractCameraControls
	{
		public string Header; // undecided whether to use
		ISelectsBox Selector;
		List<Coord> Squares;
		List<Particle> Highlights;

		public SelectBoxControls(ISelectsBox i) : base()
		{
			AllowsUnpause = true;
			Selector = i;
			Squares = new List<Coord>();
			Highlights = new List<Particle>();
			KeyMap[Keys.Space] = SelectTile;
			KeyMap[Keys.Escape] = () =>
			{
				Clean();
				InterfaceState.RewindControls();
			};
			InfoTop = new List<ColoredText>() {
				 "{orange}**Esc: Cancel.**",
				" ",
				"{yellow}Select an area with keys or mouse."
			};
		}


		private void DrawBox(Coord c)
		{
			Clean();
			Highlights.Clear();
			Squares.Clear();
			for (int y = 0; y < Selector.BoxHeight; y++)
			{
				for (int x = 0; x < Selector.BoxWidth; x++)
				{
					Squares.Add(new Coord(c.X + x, c.Y + y, c.Z));
				}
			}
			foreach (Coord s in Squares)
			{
				InterfaceState.DirtifyTile(s);
				Highlight h = new Highlight(Selector.GetHighlightColor());
				h.Place(s.X, s.Y, s.Z);
				Highlights.Add(h);
			}
		}

		public override void HoverTile(Coord c)
		{
			base.HoverTile(c);
			DrawBox(c);
			Selector.BoxHover(c, Squares);
			// this might get changed to a different panel
			InterfaceState.DirtifyTextPanels();
			
		}

		private void Clean()
		{
			foreach (Particle p in Highlights)
			{
				p.Remove();
			}
			InterfaceState.DirtifyMainPanel();
			InterfaceState.DirtifyTextPanels();
		}

		public override void ClickTile(Coord c)
		{
			Selector.SelectBox(Squares);
			Clean();
			InterfaceState.ResetControls();
		}
	}
}
