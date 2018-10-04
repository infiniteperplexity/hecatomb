/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 9/18/2018
 * Time: 11:39 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Diagnostics;
using System.Reflection;

namespace Hecatomb
{
	class NoSaveAttribute : Attribute
	{
		
	}
	/// <summary>
	/// Description of Component.
	/// </summary>
	/// 
	
	public abstract class Component : GameEntity
	{
		public TypedEntity Entity;
		[NonSerialized] public string[] Required;
		
		public Component() : base()
		{
			Required = new string[0];
		}
		
		public void AddToEntity(TypedEntity e)
		{
			// if it's a plain old Component subclass, use its own type as the key
			if (this.GetType().BaseType==typeof(Component))
			{
				e.Components[this.GetType()] = this;
			} else {
				// if it's a subclass of a Component subclass (e.g. Task), use the base type as the key
				e.Components[this.GetType().BaseType] = this;
			}
			
			Entity = e;
		}
		
//		public virtual GameEvent OnEntityEvent(GameEvent ge)
//		{
//			return ge;
//		}
				
		public void OnAddToEntity()
		{
		}
		public void RemoveFromEntity()
		{
			// if it's a plain old Component subclass, use its own type as the key
			if (this.GetType().BaseType==typeof(Component))
			{
				Entity.Components.Remove(this.GetType());
			} else {
				// if it's a subclass of a Component subclass (e.g. Task), use the base type as the key
				Entity.Components.Remove(this.GetType().BaseType);
			}
			Entity = null;
		}
		
		public override string Stringify()
		{
			string s = "{";
			string f, v;
			foreach(var field in this.GetType().GetFields())
			{
				if (field.MemberType==MemberTypes.Field && !field.IsStatic && !field.IsNotSerialized)
				{
					if (field.FieldType==typeof(System.Boolean))
					{
						v = ((bool) field.GetValue(this)==true) ? "true" : "false";
					}
					else if(field.FieldType==typeof(System.String))
					{
						v = "\"" + (string) field.GetValue(this) + "\"";
					}
					else if (field.FieldType==typeof(System.Int32))
					{
						v = ((int) field.GetValue(this)).ToString();;
					}
					else if (field.FieldType.IsSubclassOf(typeof(Hecatomb.GameEntity)))
					{
						GameEntity ge = (GameEntity) field.GetValue(this);
						v = ge.StringifyReference();
					}
					else
					{
						v = default(string);
					}
					f = "\"" + field.Name + "\": ";
					Debug.WriteLine(f+v);
				}
			}
			s+="}";
			return s;
		}
	}
}
