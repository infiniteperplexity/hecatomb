﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace Hecatomb8
{
    using static HecatombAliases;
    // Tiles is a grouping of convenience, for static methods involving grid geometry
    public static partial class Tiles
    {
        public static double Distance(int x0, int y0, int z0, int x1, int y1, int z1)
        {
            return Math.Sqrt((x0 - x1) * (x0 - x1) + (y0 - y1) * (y0 - y1) + (z0 - z1) * (z0 - z1));
        }

        public static double Distance(TileEntity t, int x1, int y1, int z1)
        {
            return Distance((int)t.X!, (int)t.Y!, (int)t.Z!, x1, y1, z1);
        }

        public static double Distance(int x0, int y0, int z0, TileEntity t)
        {
            return Distance(x0, y0, z0, (int)t.X!, (int)t.Y!, (int)t.Z!);
        }

        public static double Distance(TileEntity t0, TileEntity t1)
        {
            return Distance((int)t0.X!, (int)t0.Y!, (int)t0.Z!, (int)t1.X!, (int)t1.Y!, (int)t1.Z!);
        }

        // Bresenham's 2D line algorithm
        public static List<Coord> GetLine(int x0, int y0, int x1, int y1)
        {
            List<Coord> line = new List<Coord>();
            int z = -1;
            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx = (x1 > x0) ? 1 : -1;
            int sy = (y1 > y0) ? 1 : -1;
            int err = dx - dy;
            do
            {
                line.Add(new Coord(x0, y0, z));
                int e2 = err * 2;
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            } while (x0 != x1 || y0 != y1);
            return line;
        }

        private static List<Coord> getNeighbors(int x, int y, int z, Coord[] c, Func<int, int, int, bool>? where = null)
        {
            where = where ?? ((int xx, int yy, int zz) => (true));
            List<Coord> l = new List<Coord>();
            foreach (Coord d in c)
            {
                if (x < 0 || x > GameState.World!.Width - 1 || y < 0 || y > GameState.World!.Height - 1 || z < 0 || z > GameState.World!.Depth - 1)
                {
                    continue;
                }
                if (where(d.X + x, d.Y + y, d.Z + z))
                {
                    l.Add(new Coord(d.X + x, d.Y + y, d.Z + z));
                }
            }
            return l;
        }
        public static List<Coord> GetNeighbors4(int x, int y, int z, Func<int, int, int, bool>? where = null)
        {
            return getNeighbors(x, y, z, Coord.Directions4, where: where);
        }

        public static List<Coord> GetNeighbors8(int x, int y, int z, Func<int, int, int, bool>? where = null)
        {
            return getNeighbors(x, y, z, Coord.Directions8, where: where);
        }

        public static List<Coord> GetNeighbors6(int x, int y, int z, Func<int, int, int, bool>? where = null)
        {
            return getNeighbors(x, y, z, Coord.Directions6, where: where);
        }

        public static List<Coord> GetNeighbors10(int x, int y, int z, Func<int, int, int, bool>? where = null)
        {
            return getNeighbors(x, y, z, Coord.Directions10, where: where);
        }

        public static List<Coord> GetNeighbors26(int x, int y, int z, Func<int, int, int, bool>? where = null)
        {
            return getNeighbors(x, y, z, Coord.Directions26, where: where);
        }

        private static float uniformCost(int x0, int y0, int z0, int x1, int y1, int z1)
        {
            return 1;
        }
        private static bool defaultStandable(int x1, int y1, int z1)
        {
            if (x1 < 0 || x1 >= GameState.World!.Width || y1 < 0 || y1 >= GameState.World!.Height || z1 < 0 || z1 >= GameState.World!.Depth)
            {
                return false;
            }
            Terrain t = GameState.World!.Terrains.GetWithBoundsChecked(x1, y1, z1);
            return (!t.Solid && !t.Fallable);
        }

        private static bool sameSquare(int x0, int y0, int z0, int x1, int y1, int z1)
        {
            return (x0 == x1 && y0 == y1 && z0 == z1);
        }
        private static bool defaultMovable(int x0, int y0, int z0, int x1, int y1, int z1)
        {
            if (x1 < 0 || x1 >= GameState.World!.Width || y1 < 0 || y1 >= GameState.World!.Height || z1 < 0 || z1 >= GameState.World!.Depth)
            {
                return false;
            }
            Terrain t = GameState.World!.Terrains.GetWithBoundsChecked(x1, y1, z1);
            return (!t.Solid && !t.Fallable);
        }

        public static LinkedList<Coord> FindPath(
            Movement m, TileEntity t, bool useLast = true,
            //Func<int, int, int, int, int, int, float> cost = null,
            int maxTries = 25000,
            int cacheMissesFor = 10,
            bool useCache = true,
            int cacheHitsFor = 10, //need to perturb this to avoid stacked lag hits
            int cacheHitDistance = 35,
            Func<int, int, int, bool>? standable = null,
            Func<int, int, int, int, int, int, bool>? movable = null
            )
        {

            //var misses = Game.World.GetState<PathHandler>().PathMisses;
            //var hits = Game.World.GetState<PathHandler>().PathHits;
            //if (useCache && misses.ContainsKey(m.Entity.EID) && misses[m.Entity.EID].ContainsKey(t.EID))
            //{
            //    Debug.WriteLine("respected cached pathfinding failure");
            //    return new LinkedList<Coord>();
            //}
            if (!t.Placed)
            {
                return new LinkedList<Coord>();
            }
            var entity = m.Entity?.UnboxBriefly();
            if (entity is null || !entity.Placed)
            {
                return new LinkedList<Coord>();
            }
            var te = (TileEntity)entity;
            int x0 = (int) te.X!;
            int y0 = (int) te.Y!;
            int z0 = (int) te.Z!;
            int x1 = (int) t.X!;
            int y1 = (int) t.Y!;
            int z1 = (int) t.Z!;
            movable = movable ?? m.CouldMoveBounded;
            standable = standable ?? m.CanStandBounded;
            Func<int, int, int, int, int, int, bool> condition;
            if (useLast)
            {
                condition = sameSquare;
            }
            else
            {
                condition = (int x, int y, int z, int xx, int yy, int zz) =>
                {
                    return m.CouldTouchBounded(x, y, z, xx, yy, zz);
                };
            }
            // where does this come from now?
            //cost = cost ?? m.MovementCost;
            var path = FindPath(
                x0, y0, z0, x1, y1, z1,
                condition: condition,
                movable: movable,
                standable: standable,
                useLast: useLast,
                fromEntity: m.Entity!.UnboxBriefly()!,
                toEntity: t,
                //cost: cost,
                maxTries: maxTries
            );
            //if (path.Count == 0 && cacheMissesFor > 0 && useCache)
            //{
            //    Debug.Print("{0} failed to find a path to {1} at {2} {3} {4}", m.Entity.Entity.Describe(), t.Describe(), t.X, t.Y, t.Z);
            //    if (!misses.ContainsKey(m.Entity.EID))
            //    {
            //        misses[m.Entity.EID] = new Dictionary<int, int>();
            //    }
            //    misses[m.Entity.EID][t.EID] = cacheMissesFor;
            //}
            //else if (useCache && path.Count > 0 && cacheHitsFor > 0 && Tiles.QuickDistance(m.Entity, t) > cacheHitDistance)
            //{
            //    if (!hits.ContainsKey(m.Entity.EID))
            //    {
            //        hits[m.Entity.EID] = new Dictionary<int, (int, LinkedList<Coord>)>();
            //    }
            //    hits[m.Entity.EID][t.EID] = (cacheHitsFor, new LinkedList<Coord>(path));
            //}
            return new LinkedList<Coord>(path);
        }

        public static LinkedList<Coord> FindPath(
            Movement m, int x1, int y1, int z1, bool useLast = true,
            Func<int, int, int, int, int, int, float>? cost = null,
            int maxTries = 25000,
            Func<int, int, int, bool>? standable = null,
            Func<int, int, int, int, int, int, bool>? movable = null)
        {
            var entity = m.Entity?.UnboxBriefly();
            if (entity is null || !entity.Placed)
            {
                return new LinkedList<Coord>();
            }
            int x0 = (int)entity.X!;
            int y0 = (int)entity.Y!;
            int z0 = (int)entity.Z!;
            movable = movable ?? m.CouldMoveBounded;
            standable = standable ?? m.CanStandBounded;
            cost = cost ?? m.GetMoveCostBounded;
            Func<int, int, int, int, int, int, bool> condition;
            if (useLast)
            {
                condition = sameSquare;
            }
            else
            {
                condition = (int x, int y, int z, int xx, int yy, int zz) =>
                {
                    return m.CouldTouchBounded(x, y, z, xx, yy, zz);
                };
            }
            return FindPath(
                x0, y0, z0, x1, y1, z1,
                useLast: useLast,
                condition: condition,
                movable: movable,
                standable: standable,
                fromEntity: m.Entity!.UnboxBriefly()!,
                cost: cost,
                maxTries: maxTries
            );
        }
        public static LinkedList<Coord> FindPath(
            //		public static Coord? FindPath(
            int x0, int y0, int z0, int x1, int y1, int z1,
            bool useFirst = false,
            bool useLast = true,
            Func<int, int, int, int, int, int, double>? heuristic = null,
            Func<int, int, int, int, int, int, bool>? condition = null,
            Func<int, int, int, int, int, int, bool>? movable = null,
            Func<int, int, int, int, int, int, float>? cost = null,
            Func<int, int, int, bool>? standable = null,
            int maxTries = 25000,
            TileEntity? fromEntity = null,
            TileEntity? toEntity = null
        )
        {
            StatefulRandom r = GameState.World!.Random;
            int seedEID = (fromEntity == null) ? -1 : (int)fromEntity.EID!;
            //			Debug.WriteLine("Finding path from {0} {1} {2} to {3} {4} {5}",x0,y0,z0,x1,y1,z1);
            // default value for the cost estimation heuristic
            heuristic = heuristic ?? Distance;
            // default value for allowable movement
            condition = condition ?? sameSquare;
            movable = movable ?? defaultMovable;
            standable = standable ?? defaultStandable;
            cost = cost ?? uniformCost;
            // !should check enclosed right up front
            // should this be dirs10 or dirs26?
            //Coord[] dirs = Movement.Directions10.OrderBy((Coord c)=>Game.World.Random.NextDouble()).ToArray();
            // this always gets the same result doofus
            Coord[] dirs = Coord.Directions10.OrderBy((Coord c) => r.Arbitrary(c.OwnSeed() + seedEID)).ToArray();
            //Coord current = new Coord(x0, y0, z0);
            int current = Coord.Numberize(x0, y0, z0);
            // cost for the best known path to each cell
            Dictionary<int, float> gscores = new Dictionary<int, float>() { { current, 0 } };
            // estimated cost to destination cell
            Dictionary<int, float> fscores = new Dictionary<int, float>() { { current, 0 } };
            // cells we know can't be on the path
            HashSet<int> closedSet = new HashSet<int>();
            // trace for the best known path to each cell
            Dictionary<int, int> retrace = new Dictionary<int, int>();
            // queue sorted by fscore of coordinates that need checking
            LinkedList<int> queue = new LinkedList<int>();
            queue.AddFirst(current);
            // next coordinate to check
            float newScore, stepCost, fscore;
            bool success = false;
            // check for complete enclosure, which is a common late failure condition
            bool accessible = false;
            foreach (Coord dir in dirs)
            {
                // can I factor out useLast?
                if (useLast && movable(x1 + dir.X, y1 + dir.Y, z1 + dir.Z, x1, y1, z1))
                {
                    accessible = true;
                }
                else if (!useLast && standable(x1 + dir.X, y1 + dir.Y, z1 + dir.Z) && condition(x1 + dir.X, y1 + dir.Y, z1 + dir.Z, x1, y1, z1))
                {
                    accessible = true;
                }
            }
            if (!accessible)
            {
                Debug.WriteLine("Due to accessibility, failed to find a path from {0} {1} {2} to {3} {4} {5}", x0, y0, z0, x1, y1, z1);
                return new LinkedList<Coord>();
            }
            //
            int tally = 0;
            //int maxtries = 2500;
            while (queue.Count > 0)
            {
                tally += 1;
                if (tally >= maxTries)
                {
                    Debug.WriteLine("After {6} tries, failed to find a path from {0} {1} {2} to {3} {4} {5}", x0, y0, z0, x1, y1, z1, tally);
                    return new LinkedList<Coord>();
                }
                current = queue.First!.Value;
                queue.RemoveFirst();
                // ***** if we found the goal, retrace our steps ****
                //if (condition(current.X, current.Y, current.Z, x1, y1, z1))
                var (X, Y, Z) = new Coord(current);
                if (condition(X, Y, Z, x1, y1, z1))
                {
                    success = true;
                }
                if (success)
                {
                    LinkedList<Coord> path = new LinkedList<Coord> { };
                    path.AddFirst(new Coord(current));
                    // ***trace backwards
                    //Coord previous = current;
                    int previous = current;
                    while (retrace.ContainsKey(current))
                    {
                        previous = retrace[current];

                        if (retrace.ContainsKey(previous))
                        {
                            current = previous;
                        }
                        else
                        {
                            //							if (useFirst) return previous
                            //return current;
                            return path;
                        }
                        path.AddFirst(new Coord(current));
                    }
                    //					return current;
                    return path;
                }
                // ***************************************************
                //Coord neighbor;
                int neighbor;
                //LinkedListNode<Coord> node;
                LinkedListNode<int>? node;
                // ***** check passability and scores for neighbors **
                foreach (Coord dir in dirs)
                {
                    //neighbor = new Coord(current.X+dir.X, current.Y + dir.Y, current.Z + dir.Z);
                    int Xn = X + dir.X;
                    int Yn = Y + dir.Y;
                    int Zn = Z + dir.Z;
                    neighbor = Coord.Numberize(Xn, Yn, Zn);

                    if (!closedSet.Contains(neighbor))
                    {
                        // !!!! I think there's a performance bottleneck here, specifically when getting the Movement component.
                        //if (movable(current.X, current.Y, current.Z, neighbor.X, neighbor.Y, neighbor.Z))
                        if (movable(X, Y, Z, Xn, Yn, Zn))
                        {
                            // cost of taking this step
                            //stepCost = 1;
                            // this seems to have absolutely no effect
                            stepCost = cost(X, Y, Z, Xn, Yn, Zn);
                            // actual score along this path	
                            // !!! somehow I ran into a situation where this key was missing?							
                            newScore = gscores[current] + stepCost;
                            // if this is the best known path to the cell...
                            // !!! I've seen hints that this is a potential place for optimization
                            if (!gscores.ContainsKey(neighbor) || gscores[neighbor] > newScore)
                            {
                                //fscore = newScore + (int) Math.Ceiling(heuristic(neighbor.X, neighbor.Y, neighbor.Z, x1, y1, z1));
                                fscore = newScore + (int)Math.Ceiling(heuristic(Xn, Yn, Zn, x1, y1, z1));
                                fscores[neighbor] = fscore;
                                // loop through the queue to insert in sorted order
                                node = queue.First;

                                // somehow in here it's possible to add the node without setting the gscore?
                                if (node == null)
                                {
                                    queue.AddFirst(neighbor);
                                }
                                else while (node != null)
                                    {
                                        if (fscores[node.Value] > fscore)
                                        {
                                            queue.AddBefore(node, neighbor);
                                            node = null;
                                        }
                                        else
                                        {
                                            node = node.Next;
                                            // if we made it all the way to the end, insert it there
                                            if (node == null)
                                            {
                                                queue.AddLast(neighbor);
                                            }
                                        }

                                    }
                                retrace[neighbor] = current;
                                gscores[neighbor] = newScore;
                            }
                        }
                        // if the square is not standable and it is not the end square
                        else if (!standable(Xn, Yn, Zn) && !(Xn == x1 && Yn == y1 && Zn == z1))
                        {
                            // is the cell non-standable regardless of where we are moving from?
                            closedSet.Add(neighbor);
                        }
                    }
                }
                // ***************************************************
            }
            // ***** failed to find a path ***************************
            Debug.WriteLine("failed to find a path from {0} {1} {2} to {3} {4} {5}", x0, y0, z0, x1, y1, z1);
            //			return null;
            return new LinkedList<Coord>();
        }
    }
}