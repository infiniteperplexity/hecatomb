/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 9/25/2018
 * Time: 12:06 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Diagnostics;

namespace Hecatomb
{
	/// <summary>
	/// Description of Minion.
	/// </summary>
	public class Minion : Component
	{
	
		
		public TaskEntity Task;
		
		public Minion(): base()
		{
			Required = new string[] {"Actor"};
		}
	}
}
