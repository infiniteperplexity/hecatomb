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
        string GetHighlightColor();
	}
	public class SelectZoneControls : AbstractCameraControls
	{
		ISelectsZone Selector;
		public Coord FirstCorner;
		List<Coord> Squares;
		List<Particle> Highlights;
		
		public SelectZoneControls(ISelectsZone i)
		{
			AlwaysPaused = true;
			Selector = i;
            KeyMap[Keys.Space] = SelectTile;
			KeyMap[Keys.Escape] = ()=>{
				Clean();
				Back();
			};
			Squares = new List<Coord>();
			Highlights = new List<Particle>();
			MenuTop = new List<ColoredText>() {
     			"{orange}**Esc: Cancel.**",
                " ",
      			"{yellow}Select first corner with keys or mouse."
			};
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
			foreach(Particle p in Highlights)
			{
				Coord s = new Coord(p.X, p.Y, p.Z);
				Game.MainPanel.DirtifyTile(s);
				p.Remove();
				
			}
			Highlights.Clear();
			Squares.Clear();
			int x0 = FirstCorner.X;
			int y0 = FirstCorner.Y;
			int x1 = c.X;
			int y1 = c.Y;
			int z = c.Z;
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
					Highlight h = new Highlight(Selector.GetHighlightColor());
					h.Place(s.X, s.Y, s.Z);
					Highlights.Add(h);
				}
			}
		}
		
		public override void ClickTile(Coord c)
		{
			if (FirstCorner.Equals(default(Coord)))
			{
				SelectFirstCorner(c);
                DrawSquareZone(c);
            }
			else
			{
				SelectSecondCorner(c);
			}
		}
		public void SelectFirstCorner(Coord c)
		{
			FirstCorner = c;
			MenuTop[2] = "{yellow}Select second corner with keys or mouse.";
            //			KeyMap[Keys.Escape] = BackToFirstSquare;
            InterfacePanel.DirtifyUsualPanels();
        }
		
		private void BackToFirstSquare()
		{
			// not currently used
			FirstCorner = default(Coord);
			MenuTop[2] = "{yellow}Select first corner with keys or mouse.";
			Clean();
			Highlights.Clear();
            InterfacePanel.DirtifyUsualPanels();
            KeyMap[Keys.Escape] = Back;
		}
		
		private void Clean()
		{
			foreach (Particle p in Highlights)
			{
				p.Remove();
			}
            InterfacePanel.DirtifyMainPanel();
        }
		public void SelectSecondCorner(Coord c)
		{
			Selector.SelectZone(Squares);
			Clean();
			Highlights.Clear();
			Squares.Clear();
            ControlContext.Reset();
		}
	}
}