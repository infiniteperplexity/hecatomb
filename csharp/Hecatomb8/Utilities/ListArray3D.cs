using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hecatomb8
{
    class ListArray3D<T> : IEnumerable<T>
    {
		private Dictionary<int, List<T>> dict;
		public readonly int X;
		public readonly int Y;
		public readonly int Z;

		public ListArray3D(int x, int y, int z)
		{
			X = x;
			Y = y;
			Z = z;
			dict = new Dictionary<int, List<T>>();
		}

		public List<T> this[int x, int y, int z]
		{
			get
			{
				if (x < 0 || x >= X || y < 0 || y >= Y || z < 0 || z >= Z)
				{
					throw new IndexOutOfRangeException(String.Format("{0} {1} {2}", x, y, z));
				}
				else
				{
					int n = Z * x * y + X * y + Y;
					List<T> value;
					if (dict.ContainsKey(n))
					{

					}
					if (dict.TryGetValue(n, out value!))
					{
						return value;
					}
					else
					{
						return new List<T>();
					}
				}
			}
			set
			{
				if (x < 0 || x >= X || y < 0 || y >= Y || z < 0 || z >= Z)
				{
					throw new IndexOutOfRangeException(String.Format("{0} {1} {2}", x, y, z));
				}
				else
				{
					int n = Z * x * y + X * y + Y;
					if (value == null || value.Count == 0)
					{
						if (dict.ContainsKey(n))
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

		// okay this gets kind of weird...do we concatenate them all, or return the lists?
		public IEnumerator<T> GetEnumerator()
		{
			List<T> list = new List<T>();
			foreach (List<T> lt in dict.Values)
			{
				foreach (T t in lt)
				{
					list.Add(t);
				}
			}
			return list.GetEnumerator();

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
