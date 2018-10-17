/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/17/2018
 * Time: 12:33 PM
 */
using System;

namespace Hecatomb
{
	/// <summary>
	/// Description of Structure.
	/// </summary>
	public class Structure : Component
	{
		public Structure() : base()
		{
		}
		
		public virtual void Standardize()
		{
			Type structureType = this.GetType();
			Structure structure = (Structure) Activator.CreateInstance(structureType);
		}
	}
}
