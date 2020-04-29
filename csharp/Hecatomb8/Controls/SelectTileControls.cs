using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using System.Collections.Generic;

namespace Hecatomb8
{
	public interface ISelectsTile
	{
		void SelectTile(Coord c);
		void TileHover(Coord c);
		string GetHighlightColor();
	}

	public class SelectTileControls : AbstractCameraControls
	{
		ISelectsTile Selector;

		public SelectTileControls(ISelectsTile i) : base()
		{
			AllowsUnpause = false;
			Selector = i;
			KeyMap[Keys.Escape] = InterfaceState.RewindControls;
			KeyMap[Keys.Space] = SelectTile;
			InfoTop = new List<ColoredText>() {
				 "{orange}**Esc: Cancel.**",
				" ",
				  "{yellow}Select a tile with keys or mouse."
			};
		}

		public override void HoverTile(Coord c)
		{
			base.HoverTile(c);
			Selector.TileHover(c);
			InterfaceState.DirtifyTextPanels();
		}

		public override void ClickTile(Coord c)
		{
			Selector.SelectTile(c);
			InterfaceState.ResetControls();
		}
	}
}
