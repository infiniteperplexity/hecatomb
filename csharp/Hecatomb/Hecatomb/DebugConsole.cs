/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/26/2018
 * Time: 11:43 AM
 */
using System;
using Microsoft.VisualBasic;
using System.Diagnostics;

namespace Hecatomb
{
	/// <summary>
	/// Description of DebugConsole.
	/// </summary>
	public static class DebugConsole
	{
		public static void ShowConsole()
		{
			string s = Microsoft.VisualBasic.Interaction.InputBox(@"Enter Command:","Hecatomb Console","");
			RunConsoleCommand(s);
		}
		
		public static void RunConsoleCommand(string s)
		{
			string[] parsed = s.Split(' ');
			if (parsed[0]=="spawn")
			{
				TypedEntity et = Game.World.Entities.Spawn<Creature>(parsed[1]);
				int x = Int32.Parse(parsed[2]);
				int y = Int32.Parse(parsed[3]);
				int z = Int32.Parse(parsed[4]);
				et.Place(x, y, z);
				Game.MainPanel.Dirty = true;
			}
		}
	}
}
