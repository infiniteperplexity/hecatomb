using System;
using System.Collections.Generic;
using System.Text;

namespace Hecatomb8
{
    public class Grid3D<T>
    {
        T[,,] grid;

        public Grid3D(int x, int y, int z)
        {
            grid = new T[x, y, z];
        }

		public T GetWithBoundsChecked(int x, int y, int z)
		{
			if (x < 0 || x >= grid.GetLength(0) || y < 0 || y >= grid.GetLength(1) || z < 0 || z >= grid.GetLength(2))
			{
				throw new IndexOutOfRangeException(String.Format("{0} {1} {2}", x, y, z));
			}
			else
			{
				return grid[x, y, z];
			}
		}

		public void SetWithBoundsChecked(int x, int y, int z, T t)
		{
			if (x < 0 || x >= grid.GetLength(0) || y < 0 || y >= grid.GetLength(1) || z < 0 || z >= grid.GetLength(2) )
			{
				throw new IndexOutOfRangeException(String.Format("{0} {1} {2}", x, y, z));
			}
			else
			{
				grid[x, y, z] = t;
			}
		}

	}
}
