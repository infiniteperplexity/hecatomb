/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/8/2018
 * Time: 1:08 PM
 */
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using System.Collections.Generic;

namespace Hecatomb
{
	/// <summary>
	/// Description of MenuChoiceContext.
	/// </summary>
	/// 
	public interface IMenuListable
	{
		void ChooseFromMenu();
		string ListOnMenu();
	}
		
	public class MenuChoiceControls : ControlContext
	{
		public static List<Keys> Alphabet = new List<Keys> {
			Keys.A,
			Keys.B,
			Keys.C,
			Keys.D,
			Keys.E,
			Keys.F,
			Keys.G,
			Keys.H,
			Keys.I,
			Keys.J,
			Keys.K,
			Keys.L,
			Keys.M,
			Keys.N,
			Keys.O,
			Keys.P,
			Keys.Q,
			Keys.R,
			Keys.S,
			Keys.T,
			Keys.U,
			Keys.V,
			Keys.W,
			Keys.X,
			Keys.Y,
			Keys.Z
		};
		
		static string alphabet = "abcdefghijklmnopqrstuvwxyz";
		public List<string> Choices;
		
		public MenuChoiceControls(string header, List<IMenuListable> choices): base()
		{
			var Commands = Game.Commands;
			KeyMap[Keys.Space] = Commands.Wait;
			KeyMap[Keys.Escape] = Reset;
			MenuTop = new List<string>() {
				"**Esc: Cancel**.",
				header
		    };
			TopColors = new Dictionary<Tuple<int, int>, string>() {
				{new Tuple<int, int>(0, 0), "orange"},
				{new Tuple<int, int>(1, 0), "yellow"}
			};
			for (int i=0; i<choices.Count; i++)
			{
				KeyMap[Alphabet[i]] = choices[i].ChooseFromMenu;
				MenuTop.Add(alphabet[i] + ") "+choices[i].ListOnMenu());
			}
		}
	}
}
