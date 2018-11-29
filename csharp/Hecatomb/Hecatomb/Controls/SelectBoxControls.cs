﻿/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/8/2018
 * Time: 1:09 PM
 */
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using System.Collections.Generic;

namespace Hecatomb
{
	/// <summary>
	/// Description of SelectBoxContext.
	/// </summary>
	public interface ISelectsBox
	{
		int BoxWidth {get; set;}
		int BoxHeight {get; set;}
		void SelectBox(Coord c, List<Coord> squares);
		void BoxHover(Coord c, List<Coord> squares);
	}
		
	public class SelectBoxControls : CameraControls
	{
		ISelectsBox Selector;
		List<Coord> Squares;
		List<Particle> Highlights;
		
		public SelectBoxControls(ISelectsBox i) : base()
		{
			Selector = i;
			Squares = new List<Coord>();
			Highlights = new List<Particle>();
			KeyMap[Keys.Escape] = ()=>{
				Clean();
				Back();
			};
			MenuTop = new List<ColoredText>() {
     			"{orange}**Esc: Cancel.**",
      			"{yellow}Select a box with keys or mouse.",
      			" ",
     			"Move: NumPad/Arrows, ,/.: Up/Down.",
      			"(Control+Arrows for diagonal.)",
      			"Wait: NumPad 5 / Control+Space.",
      			" ",
      			"Click / Space: Select.",
      			"Enter: Toggle Pause."
			};
		}
		
		
		private void DrawBox(Coord c)
		{
			Clean();
			Highlights.Clear();
			Squares.Clear();
			for (int y=0; y<Selector.BoxHeight; y++)
			{
				for (int x=0; x<Selector.BoxWidth; x++)
				{
					Squares.Add(new Coord(c.X+x, c.Y+y, c.Z));
				}
			}
			foreach (Coord s in Squares)
			{
				Game.MainPanel.DirtifyTile(s);
				Highlight h = new Highlight("orange");
				h.Place(s.X, s.Y, s.Z);
				Highlights.Add(h);
			}
		}
		public override void HoverTile(Coord c)
		{
			//base.HoverTile(c);
			DrawBox(c);
			Selector.BoxHover(c, Squares);
			Game.MenuPanel.Dirty = true;
			Game.World.ShowTileDetails(c);
		}
		
		private void Clean()
		{
			foreach(Particle p in Highlights)
			{
				Coord s = new Coord(p.X, p.Y, p.Z);
				Game.MainPanel.DirtifyTile(s);
				p.Remove();
				
			}
		}
		
		public override void ClickTile(Coord c)
		{
			Selector.SelectBox(c, Squares);
			Clean();
			Game.MenuPanel.Dirty = true;
			Reset();
		}
		

	}
}
