/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/24/2018
 * Time: 10:09 AM
 */
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace Hecatomb
{
	/// <summary>
	/// Description of StateTrackers.
	/// </summary>
	public class StateHandler: Entity
	{
		public void Activate()
		{
			if (OldGame.World.StateHandlers.ContainsKey(GetType().Name))
			{
				OldGame.World.StateHandlers[GetType().Name].Despawn();
			}
			OldGame.World.StateHandlers[GetType().Name] = this;
		}
	}
}
