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
		public readonly int _x;
		public readonly int _y;
		public readonly int _z;

		public ListArray3D(int x, int y, int z)
		{
			_x = x;
			_y = y;
			_z = z;
			dict = new Dictionary<int, List<T>>();
		}

		public List<T> this[int x, int y, int z]
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
					if (!dict.ContainsKey(n))
					{
						dict[n] = new List<T>();
					}
					return dict[n];
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
					dict[n] = value;
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
