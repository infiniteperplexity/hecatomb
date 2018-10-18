/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/17/2018
 * Time: 1:57 PM
 */
using System;
using System.Collections.Generic;

namespace Hecatomb
{

	public class TaskMaster : Component, IChoiceMenu
	{
		public string MenuHeader
		{
			get
			{
				return "Choose a task:";
			}
			set {}
		}
		public List<IMenuListable> MenuChoices
		{
			get
			{
				List<IMenuListable> tasks = new List<IMenuListable>();
				foreach (string t in Tasks)
				{
					tasks.Add(GetTask(t));
				}
				return tasks;
			}
			set {}
		}
		public string[] Tasks;
		public TaskMaster()
		{
			Tasks = new [] {"DigTask", "BuildTask", "ConstructTask", "FurnishTask", "UndesignateTask"};
		}
		
		public Task GetTask(Type t)
		{
			return (Task) Activator.CreateInstance(t);
		}
		
		public Task GetTask(String s)
		{
			Type t = Type.GetType("Hecatomb."+s);
			return (Task) Activator.CreateInstance(t);
		}
	}
}
