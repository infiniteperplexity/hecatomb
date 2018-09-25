/*
 * Created by SharpDevelop.
 * User: m543015
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
		private Entity _master;
		public Entity Master {
			get
			{
				return _master;
			}
			set
			{
				Master master;
				if (_master!=null)
				{
					master = _master.GetComponent<Hecatomb.Master>();
					master.Minions.Remove(Entity);
				}
				master = value.GetComponent<Hecatomb.Master>();
				_master = value;
				if (!master.Minions.Contains(Entity)) 
				{
					master.Minions.Add(Entity);
				}
			}
		}
		
		private Entity _task;
		public Entity Task {
			get {
				return _task;
			}
			set
			{
				if (value.TryComponent<Task>()==null)
				{
					throw new InvalidOperationException();
				} else {
					_task = value;
				}
			}
		}
		public Minion(): base()
		{
			Required = new string[] {"Actor"};
		}
	}
}
