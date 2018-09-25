/*
 * Created by SharpDevelop.
 * User: m543015
 * Date: 9/25/2018
 * Time: 12:50 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;

namespace Hecatomb
{
	/// <summary>
	/// Description of Master.
	/// </summary>
	public class Master : Component
	{
		public List<Entity> Minions;
		public Master() : base()
		{
			Minions = new List<Entity>();
		}
		
		
		
	}
}
