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
	public abstract class Structure : Component
	{
		public string MenuName;
		public string Name;
		public int Width;
		public int Height;
		public char[] Symbols;
		public string[] FGs;
		public string[] BGs;
		public Feature[] Features;
		
		public Structure()
		{
			Width = 3;
			Height = 3;
			// ugh.
			Features = new Feature[Width*Height];
		}
		public virtual void Standardize()
		{
			Type structureType = this.GetType();
			Structure structure = (Structure) Activator.CreateInstance(structureType);
		}
	}
}
