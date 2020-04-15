using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;

namespace Hecatomb8
{
	public class Constrained<T> where T: struct
	{
		T item;
		public Constrained(T t)
		{
			item = t;
		}
		public T Unbox()
		{
			return item;
		}
		public bool Constrain(Func<T, bool> f)
		{
			if (f(item))
			{
				return true;
			}
			else
			{
				throw new Exception($"Value {item} did not meet required constraint.");
			}
		}
	}
}
