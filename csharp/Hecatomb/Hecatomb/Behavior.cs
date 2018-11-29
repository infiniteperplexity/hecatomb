/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/29/2018
 * Time: 1:16 PM
 */
using System;

namespace Hecatomb
{
	/// <summary>
	/// Description of Behavior.
	/// </summary>
	public class Behavior : FlyWeight<Behavior>
	{
		public Behavior(string type, string name) : base(type)
		{
		}
		
		
	}
}
