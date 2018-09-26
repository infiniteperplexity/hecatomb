/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 9/25/2018
 * Time: 10:51 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace Hecatomb
{
	public abstract class Task : Component
	{
		public Creature Worker;
		public int Range;
		//public string Makes;
		public Task() : base()
		{
			
		}
	}
	
	public class DigTask : Task
	{
		public DigTask() : base()
		{
			
		}
	}
	
	public class BuildTask : Task
	{
		public BuildTask() : base()
		{
			
		}
	}
}
