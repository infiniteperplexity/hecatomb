using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hecatomb8
{
    class CellularAutomaton
    {
        public bool[,,] Cells;
        public int Width;
        public int Height;
        public int Depth;
        public StatefulRandom random;
        public CellularAutomaton(int width, int height, int depth = 1)
        {
            Cells = new bool[width, height, depth];
            Width = width;
            Height = height;
            Depth = depth;
            random = GameState.World!.Random;
        }

        public void Initialize(double p = 0.4)
        {
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    for (int k = 0; k < Depth; k++)
                    {
                        if (random.NextDouble() < p)
                        {
                            Cells[i, j, k] = true;
                        }
                        else
                        {
                            Cells[i, j, k] = false;
                        }
                    }
                }
            }
        }

        public void DoSteps(int birth = 56798, int survival = 345678, int steps = 1)
        {
            for (int i = 0; i < steps; i++)
            {
                Iterate(birth, survival);
            }
        }
        //If a living cell has less than two living neighbours, it dies.
        //If a living cell has two or three living neighbours, it stays alive.
        //If a living cell has more than three living neighbours, it dies.
        //If a dead cell has exactly three living neighbours, it becomes alive.

        public void Iterate(int birth = 56798, int survival = 345678)
        {
            int b = birth;
            int s = survival;
            var births = new List<int>();
            var survivals = new List<int>();
            while (b > 0)
            {
                int mod = b % 10;
                births.Add(mod);
                b -= mod;
                b /= 10;
            }
            while (s > 0)
            {
                int mod = s % 10;
                survivals.Add(mod);
                s = (s - mod) / 10;
            }
            var New = new bool[Width, Height, Depth];
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    for (int k = 0; k < Depth; k++)
                    {
                        int n = CountNeighbors(i, j, k);
                        if (Cells[i, j, k] && survivals.Contains(n))
                            New[i, j, k] = true;

                        if (!Cells[i, j, k] && births.Contains(n))
                            New[i, j, k] = true;
                    }
                }
            }
            Cells = New;
        }

        public int CountNeighbors(int x, int y, int z)
        {
            int n = 0;
            for (int i = -1; i <= +1; i++)
            {
                if (x + i < 0 || x + i >= Width)
                    continue;

                for (int j = -1; j <= +1; j++)
                {
                    if (y + j < 0 || y + j >= Height)
                        continue;

                    if (i == 0 && j == 0)
                        continue;

                    if (Cells[x + i, y + j, z])
                    {
                        n += 1;
                    }
                }
            }
            return n;
        }
    }
}
