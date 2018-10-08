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
	/// Description of SelectBoxContext.
	/// </summary>
	public interface ISelectsBox
	{
		void SelectBox(List<Coord> squares);
		void TileHover(Coord c);
	}
		
	public class SelectBoxControls : NavigatorControls
	{
		public SelectBoxControls(ISelectsBox i) : base()
		{
		}
	}
}
