/*
 * Created by SharpDevelop.
 * User: Glenn
 * Date: 10/29/2018
 * Time: 10:10 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace Hecatomb
{
	/// <summary>
	/// Description of Combat.
	/// </summary>
	public class Combat : Component
	{
		public Combat() : base()
		{
		}
		public void Attack(TypedEntity t)
		{
			Game.World.Events.Publish(new AttackEvent() {});
		}
	}
}
