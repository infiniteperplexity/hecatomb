/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 10/18/2018
 * Time: 10:34 AM
 */
using System;

namespace Hecatomb
{
	/// <summary>
	/// Description of TypedEntityEvents.
	/// </summary>
	public abstract partial class PositionedEntity
	{
		public Action<PositionedEntity, int, int, int> AfterSelfPlace;
	}
}
