using System;
using System.Collections;
using System.Collections.Generic;

namespace Hecatomb8
{
	// Coord represents a 3-dimensional coordinate.  This won't live here permanently, it's just here for now to enable 
	public struct Coord
	{
		public int X;
		public int Y;
		public int Z;

		public Coord(int _x, int _y, int _z)
		{
			// should this include a catch to prevent out-of-bands errors?
			X = _x;
			Y = _y;
			Z = _z;
		}

		public void Deconstruct(out int x, out int y, out int z)
		{
			x = X;
			y = Y;
			z = Z;
		}

		public bool Equals(Coord c)
			=> X == c.X && Y == c.Y && Z == c.Z;
	}


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

		public T? this[int x, int y, int z]
		{
			get
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
			set
			{
				if (x < 0 || x >= _x || y < 0 || y >= _y || z < 0 || z >= _z)
				{
					throw new IndexOutOfRangeException(String.Format("{0} {1} {2}", x, y, z));
				}
				else
				{
					int n = z * _x * _y + x * _y + y;
					if (value == null)
					{
						if (dict[n] != null)
						{
							dict.Remove(n);
						}
					}
					else
					{
						dict[n] = value;
					}
				}
			}
		}


		// an experiment with named tuples
		public T? this[(int x, int y, int z) c]
		{
			get
			{
				return this[c.x, c.y, c.z];
			}
			set
			{
				this[c.x, c.y, c.z] = value;
			}
		}

		public T? this[Coord c]
		{
			get
			{
				return this[c.X, c.Y, c.Z];
			}
			set
			{
				this[c.X, c.Y, c.Z] = value;
			}
		}

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