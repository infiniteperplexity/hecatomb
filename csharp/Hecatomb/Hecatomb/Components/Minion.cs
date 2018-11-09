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
using Newtonsoft.Json;

namespace Hecatomb
{
	/// <summary>
	/// Description of Minion.
	/// </summary>
	public class Minion : Component
	{
	
		public int TaskEID;
		[JsonIgnore] public TaskEntity Task
		{
			get
			{
				if (TaskEID==-1)
				{
					return null;
				}
				else
				{
					return (TaskEntity) Game.World.Entities.Spawned[TaskEID];
				}
			}
			private set
			{
				if (value==null)
				{
					TaskEID = -1;
				}
				else
				{
					TaskEID = value.EID;
				}
			}
		}
		
		public Minion(): base()
		{
			TaskEID = -1;
			Required = new string[] {"Actor"};
		}
		
		// called by Task.AssignEntity
		public void _AssignTask(TaskEntity t)
		{
			Task = t;
		}
		
		public void _Unassign()
		{
			Task = null;
		}
	}
}
