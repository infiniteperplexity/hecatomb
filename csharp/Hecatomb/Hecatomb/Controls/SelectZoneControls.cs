/*
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
	/// Description of SelectSquareZoneContext.
	/// </summary>
	/// 
	public interface ISelectsZone
	{
		void SelectZone(List<Coord> squares);
		void TileHover(Coord c);
		void TileHover(Coord c, List<Coord> squares);
	}
	public class SelectZoneControls : NavigatorControls
	{
		ISelectsZone Selector;
		Coord FirstCorner;
		List<Coord> Squares;
		
		public SelectZoneControls(ISelectsZone i)
		{
			Selector = i;
			MenuText = new List<string>() {
     			"**Esc: Cancel.**",
      			"Select first corner with keys or mouse.",
      			" ",
     			"Move: NumPad/Arrows, ,/.: Up/Down.",
      			"(Control+Arrows for diagonal.)",
      			"Wait: NumPad 5 / Control+Space.",
      			" ",
      			"Click / Space: Select.",
      			"Enter: Toggle Pause."
			};
			TextColors = new Dictionary<Tuple<int, int>, string>() {
				{new Tuple<int, int>(0,0), "orange"},
				{new Tuple<int, int>(1,0), "yellow"}
			};
			Squares = new List<Coord>();
		}
		
		public override void HoverTile(Coord c)
		{
			if (FirstCorner.Equals(default(Coord)))
			{
				base.HoverTile(c);
				Selector.TileHover(c);
			}
			else
			{
				base.HoverTile(c);
				DrawSquareZone(c);
				Selector.TileHover(c, Squares);
			}
		}
		
		public void DrawSquareZone(Coord c)
		{
			foreach(Coord s in Squares)
			{
				Game.MainPanel.DirtifyTile(s);
				Particle p = Game.MainPanel.Particles[s.x, s.y, s.z];
				if (p!=null && p is Highlight)
				{
					p.Remove();
				}
			}
			Squares.Clear();
			int x0 = FirstCorner.x;
			int y0 = FirstCorner.y;
			int x1 = c.x;
			int y1 = c.y;
			int z = c.z;
			int swap;
			if (x0>x1) {swap = x0; x0 = x1; x1 = swap;}
			if (y0>y1) {swap = y0; y0 = y1; y1 = swap;}
			for (int x=x0; x<=x1; x++)
			{
				for (int y=y0; y<=y1; y++)
				{
					Coord s = new Coord(x, y, z);
					Squares.Add(s);
					Game.MainPanel.DirtifyTile(s);
					Highlight h = new Highlight("orange");
					h.Place(s.x, s.y, s.z);
				}
			}
			Selector.TileHover(c);
		}
		
		public override void ClickTile(Coord c)
		{
			if (FirstCorner.Equals(default(Coord)))
			{
				SelectFirstCorner(c);
			}
			else
			{
				SelectSecondCorner(c);
			}
		}
		public void SelectFirstCorner(Coord c)
		{
			Debug.WriteLine("flag 1");
			FirstCorner = c;
			MenuText[1] = "Select second corner with keys or mouse.";
			KeyMap[Keys.Escape] = () => {
				FirstCorner = default(Coord);
				MenuText[1] = "Select first corner with keys or mouse.";
			};
			Game.GraphicsDirty = true;
		}
		
		public void SelectSecondCorner(Coord c)
		{
			Selector.SelectZone(Squares);
			foreach(Coord s in Squares)
			{
				Particle p = Game.MainPanel.Particles[s.x, s.y, s.z];
				if (p!=null && p is Highlight)
				{
					p.Remove();
				}
			}
			Squares.Clear();
			Game.Controls.Reset();
		}
	}
}