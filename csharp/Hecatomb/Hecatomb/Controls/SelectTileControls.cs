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
	}
		
	public class SelectTileControls : NavigatorControls
	{
		public SelectTileControls(ISelectsTile i) : base()
		{
		}
	}
}
