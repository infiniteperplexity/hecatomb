/*
 * Created by SharpDevelop.
 * User: Glenn Wright
 * Date: 9/21/2018
 * Time: 12:07 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections;
using System.Collections.Generic;

namespace Hecatomb
{
	/// <summary>
	/// Description of Sparse3DArray.
	/// </summary>
	public class SparseArray3D<T> : IEnumerable<T>
	{
        //private Dictionary<Tuple<int, int, int>, T> dict;
        private Dictionary<int, T> dict;
		public readonly int X;
		public readonly int Y;
		public readonly int Z;
		
		public SparseArray3D(int x, int y, int z)
		{
			X = x;
			Y = y;
			Z = z;
			dict = new Dictionary<int, T>();
		}
		
		public T this[int x, int y, int z]
	   	{
			get
			{	
				if (x<0 || x>=X || y<0 || y>=Y || z<0 || z>=Z) {
					throw new IndexOutOfRangeException(String.Format("{0} {1} {2}", x, y, z));
				} else {
					T value;
					if (dict.TryGetValue(z * X * Y + x * Y + y, out value)) {
						return value;
					} else {
						return default(T);
					}
				}
			}
			set
			{
				if (x<0 || x>=X || y<0 || y>=Y || z<0 || z>=Z) {
					throw new IndexOutOfRangeException(String.Format("{0} {1} {2}", x, y, z));
				} else {
                    int n = z * X * Y + x * Y + y;
                    if (value==null) {
                        if (dict[n]!=null)
						{
							dict.Remove(n);
						}
					} else {
						dict[n] = value;
					}
				}
			}
	   	}


        public T this[Coord c]
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

    public class OldSparseArray3D<T> : IEnumerable<T>
    {
        private Dictionary<Tuple<int, int, int>, T> dict;
        public readonly int X;
        public readonly int Y;
        public readonly int Z;

        public OldSparseArray3D(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
            dict = new Dictionary<Tuple<int, int, int>, T>();
        }

        public T this[int x, int y, int z]
        {
            get
            {
                if (x < 0 || x >= X || y < 0 || y >= Y || z < 0 || z >= Z)
                {
                    throw new IndexOutOfRangeException(String.Format("{0} {1} {2}", x, y, z));
                }
                else
                {
                    Tuple<int, int, int> t = new Tuple<int, int, int>(x, y, z);
                    T value;
                    if (dict.TryGetValue(t, out value))
                    {
                        return value;
                    }
                    else
                    {
                        return default(T);
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
                    Tuple<int, int, int> t = new Tuple<int, int, int>(x, y, z);
                    if (value == null)
                    {
                        if (dict[t] != null)
                        {
                            dict.Remove(t);
                        }
                    }
                    else
                    {
                        dict[t] = value;
                    }
                }
            }
        }


        public T this[Coord c]
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

    public class SparseJaggedArray3D<T> : SparseArray3D<T>, IEnumerable<T>
	{
		private Dictionary<Tuple<int, int, int>, List<T>> dict;
		
		public SparseJaggedArray3D(int x, int y, int z) : base(x, y, z)
		{
			dict = new Dictionary<Tuple<int, int, int>, List<T>>();
		}
		
		public new List<T> this[int x, int y, int z]
	   	{
			get
			{	
				if (x<0 || x>=X || y<0 || y>=Y || z<0 || z>=Z) {
					throw new IndexOutOfRangeException(String.Format("{0} {1} {2}", x, y, z));
				} else {
					Tuple<int, int, int> t = new Tuple<int, int, int>(x, y, z);
					List<T> value;
					if (dict.TryGetValue(t, out value)) {
						return value;
					} else {
						return new List<T>();
					}
				}
			}
			set
			{
				if (x<0 || x>=X || y<0 || y>=Y || z<0 || z>=Z) {
					throw new IndexOutOfRangeException(String.Format("{0} {1} {2}", x, y, z));
				} else {
					Tuple<int, int, int> t = new Tuple<int, int, int>(x, y, z);
					if (value==null || value.Count==0) {
						if (dict[t]!=null)
						{
							dict.Remove(t);
						}
					}
					else
					{
						dict[t] = value;
					}
				}
			}
	   	}
		
		// okay this gets kind of weird...do we concatenate them all, or return the lists?
		public new IEnumerator<T> GetEnumerator()
	    {
			List<T> list = new List<T>();
			foreach(List<T> lt in dict.Values)
			{
				foreach(T t in lt)
				{
					list.Add(t);
				}
			}
			return list.GetEnumerator();
			
	    }
	}
}