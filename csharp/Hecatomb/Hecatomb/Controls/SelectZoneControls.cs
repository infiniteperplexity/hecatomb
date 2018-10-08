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
	}
	public class SelectZoneControls : NavigatorControls
	{
		ISelectsZone Selector;
		Coord FirstCorner;
		Coord SecondCorner;
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
		
		public void TileHover(Coord c)
		{
			if (FirstCorner.Equals(default(Coord)))
			{
				base.HoverTile(c);
				Selector.TileHover(c);
			}
			else
			{
				Selector.TileHover(c);
			}
			
			
		}
		
		public void SelectFirstCorner(Coord c)
		{
			FirstCorner = c;
			
		}
		
		public void SelectSecondCorner(Coord c)
		{
			Selector.SelectZone(Squares);
			Game.Controls.Reset();
		}
	}
}
//	public void SelectSquareZone(
//			Action<Coord> hover=null,
//			Action<Coord> click=null
//		)
//		{
//			hover = hover ?? Nothing;
//			click = click ?? Nothing;
//			var c = new NavigatorContext();
//			c.MenuText = new List<string>() {
//      			"**Esc: Cancel.**",
//      			"Select first corner with keys or mouse.",
//      			" ",
//      			"Move: NumPad/Arrows, ,/.: Up/Down.",
//      			"(Control+Arrows for diagonal.)",
//      			"Wait: NumPad 5 / Control+Space.",
//      			" ",
//      			"Click / Space: Select.",
//      			"Enter: Toggle Pause."
//			};
//			c.TextColors = new Dictionary<Tuple<int, int>, string>() {
//				{new Tuple<int, int>(0,0), "orange"},
//				{new Tuple<int, int>(1,0), "yellow"}
//			};
//			c.OnTileClick = SelectFirstSquare();
//			c.OnTileHover = hover;
//			Game.Controls = c;
//			Game.GraphicsDirty = true;
//		}
//		
//		private void SelectFirstSquare(){}
//		private void SelectSecondSquare(){}
//		private void DrawSquareBox() {}
//		
//		public void SelectBox()
//		{
//			
//		}
//		
//		public void SelectSquare()
//		{
//			
//		}