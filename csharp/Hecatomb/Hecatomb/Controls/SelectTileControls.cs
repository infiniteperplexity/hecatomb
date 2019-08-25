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
	/// Description of SelectTileContext.
	/// </summary>
	/// 
	public interface ISelectsTile
	{
		void SelectTile(Coord c);
		void TileHover(Coord c);
        string GetHighlightColor();
    }
		
	public class SelectTileControls : CameraControls
	{
		ISelectsTile Selector;
		
		public SelectTileControls(ISelectsTile i) : base()
		{
			Selector = i;
			KeyMap[Keys.Escape] = Back;
            KeyMap[Keys.Space] = SelectTile;
			MenuTop = new List<ColoredText>() {
     			"{orange}**Esc: Cancel.**",
      			"{yellow}Select a tile with keys or mouse.",
      			" ",
     			"Move: NumPad/Arrows, ,/.: Up/Down.",
      			"(Control+Arrows for diagonal.)",
      			"Wait: NumPad 5 / Control+Space.",
      			" ",
      			"Click / Space: Select.",
      			"Enter: Toggle Pause."
			};
		}
		
		public override void HoverTile(Coord c)
		{
			base.HoverTile(c);
			Selector.TileHover(c);
            InterfacePanel.DirtifySidePanels();
        }
		
		public override void ClickTile(Coord c)
		{
            Selector.SelectTile(c);
			Reset();
		}
	}
}
