using System;
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
		ISelectsBox Selector;
		List<Coord> Squares;
		//List<Particle> Highlights;

		public SelectBoxControls(ISelectsBox i) : base()
		{
			AllowsUnpause = true;
			Selector = i;
			Squares = new List<Coord>();
			//Highlights = new List<Particle>();
			//KeyMap[Keys.Space] = SelectTile;
			//KeyMap[Keys.Escape] = () => {
			//	Clean();
			//	Back();
			//};
			MenuTop = new List<ColoredText>() {
				 "{orange}**Esc: Cancel.**",
				" ",
				"{yellow}Select an area with keys or mouse."
			};
		}


		private void DrawBox(Coord c)
		{
			//Clean();
			//Highlights.Clear();
			//Squares.Clear();
			for (int y = 0; y < Selector.BoxHeight; y++)
			{
				for (int x = 0; x < Selector.BoxWidth; x++)
				{
					Squares.Add(new Coord(c.X + x, c.Y + y, c.Z));
				}
			}
			foreach (Coord s in Squares)
			{
				//Game.MainPanel.DirtifyTile(s);
				//Highlight h = new Highlight(Selector.GetHighlightColor());
				//h.Place(s.X, s.Y, s.Z);
				//Highlights.Add(h);
			}
		}
		//public override void HoverTile(Coord c)
		//{
		//	//base.HoverTile(c);
		//	DrawBox(c);
		//	Selector.BoxHover(c, Squares);
		//	// this might get changed to a different panel
		//	InterfacePanel.DirtifySidePanels();
		//	Game.World.ShowTileDetails(c);
		//}

		//private void Clean()
		//{
		//	foreach (Particle p in Highlights)
		//	{
		//		Coord s = new Coord(p.X, p.Y, p.Z);
		//		Game.MainPanel.DirtifyTile(s);
		//		p.Remove();

		//	}
		//}

		//public override void ClickTile(Coord c)
		//{
		//	Selector.SelectBox(Squares);
		//	Clean();
		//	InterfacePanel.DirtifyUsualPanels();
		//	Reset();
		//}
	}
}
