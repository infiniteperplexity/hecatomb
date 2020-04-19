using System;
using System.Collections;
using System.Collections.Generic;

namespace Hecatomb8
{ 
	// Sparse three-dimensional arrays the size of the world
	public class SparseArray3D<T> : IEnumerable<T> where T: class
	{


		private Dictionary<int, T> dict;
		public readonly int _x;
		public readonly int _y;
		public readonly int _z;

		public SparseArray3D(int x, int y, int z)
		{
			_x = x;
			_y = y;
			_z = z;
			dict = new Dictionary<int, T>();
		}

		public T? GetWithBoundsChecked(int x, int y, int z)
		{
			if (x < 0 || x >= _x || y < 0 || y >= _y || z < 0 || z >= _z)
			{
				throw new IndexOutOfRangeException(String.Format("{0} {1} {2}", x, y, z));
			}
			else
			{
				int n = z * _x * _y + x * _y + y;
				if (dict.ContainsKey(n))
				{
					return dict[n];
				}
				else
				{
					return null;
				}
			}
		}
		public void SetWithBoundsChecked(int x, int y, int z, T? t)
		{
			if (x < 0 || x >= _x || y < 0 || y >= _y || z < 0 || z >= _z)
			{
				throw new IndexOutOfRangeException(String.Format("{0} {1} {2}", x, y, z));
			}
			else
			{
				int n = z * _x * _y + x * _y + y;
				if (t == null)
				{
					if (dict.ContainsKey(n))
					{
						dict.Remove(n);
					}
				}
				else
				{
					dict[n] = t;
				}
			}
		}

		// an experiment with named tuples
		//public T? this[(int x, int y, int z) c]
		//{
		//	get
		//	{
		//		return this[c.x, c.y, c.z];
		//	}
		//	set
		//	{
		//		this[c.x, c.y, c.z] = value;
		//	}
		//}

		//public T? this[Coord c]
		//{
		//	get
		//	{
		//		return this[c.X, c.Y, c.Z];
		//	}
		//	set
		//	{
		//		this[c.X, c.Y, c.Z] = value;
		//	}
		//}

		// I could maybe lay off the Constrained<int>s here, since you're going to have to constrain them later anyway
		public bool ContainsKey(int x, int y, int z)
		{
			if (x < 0 || x >= _x || y < 0 || y >= _y || z < 0 || z >= _z)
			{
				throw new IndexOutOfRangeException(String.Format("{0} {1} {2}", x, y, z));
			}
			else
			{
				return dict.ContainsKey(z * _x * _y + x * _y + y);
			}
		}

		public IEnumerator<T> GetEnumerator()
		{
			return dict.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		public void Clear()
		{
			dict.Clear();
		}
	}
}