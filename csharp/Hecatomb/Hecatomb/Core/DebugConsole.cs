﻿/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/26/2018
 * Time: 11:43 AM
 */

//say which ingredients are getting gathered
//worker can still get distracted somehow
    // helps to add task to creature selection
//- dismantle / repairtask?
    // can we avoid proliferation here?  these are rare tasks and I don't want them to take up a lot of air.



using System;
using Microsoft.VisualBasic;

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
				TileEntity et = Entity.Spawn<Creature>(parsed[1]);
				int x = Int32.Parse(parsed[2]);
				int y = Int32.Parse(parsed[3]);
				int z = Int32.Parse(parsed[4]);
				et.Place(x, y, z);
				Game.MainPanel.Dirty = true;
			}
		}
	}
}
