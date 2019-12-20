using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace day18
{
    struct Vec2
    {
        public int x;
        public int y;

        public static Vec2 Set(int x, int y)
        {
            var v = new Vec2();
            v.x = x;
            v.y = y;
            return v;
        }
    };

    class Vec2Cost : IComparable<Vec2Cost>
    {
        public Vec2 pos;
        public int cost;

        public Vec2Cost(Vec2 setPos, int setCost)
        {
            pos = setPos;
            cost = setCost;
        }

        public int CompareTo(Vec2Cost other)
        {
            return cost.CompareTo(other.cost);
        }
    };

    class Map
    {
        private char[,] map;
        public int keysCount;
        private Dictionary<char, Vec2> keyToPos = new Dictionary<char, Vec2>();
        private Dictionary<char, Vec2> doorToPos = new Dictionary<char, Vec2>();
        private int w;
        private int h;

        public Map(char[,] setMap, int setW, int setH)
        {
            map = setMap;
            w = setW;
            h = setH;

            for (int x = 0; x < w; ++x)
            {
                for (int y = 0; y < h; ++y)
                {
                    var v = map[x, y];
                    var pos = Vec2.Set(x, y);

                    if ((v >= 'a' && v <= 'z') || v == '@')
                    {
                        keysCount++;
                        keyToPos[v] = pos;
                        map[x, y] = '.';
                    }
                    else if (v >= 'A' && v <= 'Z')
                    {
                        var vl = v.ToString().ToLower()[0];
                        doorToPos[vl] = pos;
                        map[x, y] = vl;
                    }
                }
            }
        }

        public (char key, int dist)[] Distance(char fromKey, bool[] reachedKeys)
        {
            var visited = new bool[w, h];
            var heapHandles = new C5.IPriorityQueueHandle<Vec2Cost>[w, h];
            var distance = new int[w, h];
            var reachable = new bool[w, h];
            var heap = new C5.IntervalHeap<Vec2Cost>();

            var start = keyToPos[fromKey];

            for (int x = 0; x < w; ++x)
            {
                for (int y = 0; y < h; ++y)
                {
                    var v = map[x, y];
                    var d = (x == start.x && y == start.y) ? 0 : Int32.MaxValue;
                    distance[x, y] = d;
                    if ((v != '#') && ((v == '.') || reachedKeys[v]))
                    {
                        reachable[x, y] = true;
                        heap.Add(ref heapHandles[x, y], new Vec2Cost(Vec2.Set(x, y), d));
                    }
                }
            }

            while (!heap.IsEmpty)
            {
                C5.IPriorityQueueHandle<Vec2Cost> handle = null;
                Vec2Cost p = heap.DeleteMin(out handle);
                // some nodes are not actually reachable
                if (p.cost == Int32.MaxValue)
                    break;
                visited[p.pos.x, p.pos.y] = true;
                heapHandles[p.pos.x, p.pos.y] = null;
                foreach (var adj in AdjacentTo(p.pos))
                {
                    if (!reachable[adj.x, adj.y])
                        continue;
                    var old_cost = distance[adj.x, adj.y];
                    var new_cost = p.cost + 1;
                    if (new_cost < old_cost)
                    {
                        distance[adj.x, adj.y] = new_cost;
                        if (!visited[adj.x, adj.y])
                            heap.Replace(heapHandles[adj.x, adj.y], new Vec2Cost(adj, new_cost));
                    }
                }
            }
            //
            // for (int y = 0; y < h; ++y)
            // {
            //     for (int x = 0; x < w; ++x)
            //     {
            //         Console.Write((distance[x, y] < Int32.MaxValue) ? 1 : 0);
            //     }
            //     Console.Write('\n');
            // }

            var dist = new List<(char key, int dist)>();
            foreach (var p in keyToPos)
            {
                if (p.Key != fromKey)
                {
                    int d = distance[p.Value.x, p.Value.y];
                    if (d != Int32.MaxValue)
                    {
                        dist.Add((key: p.Key, dist: d));
                    }
                }
            }
            return dist.ToArray();
        }

        private IEnumerable<Vec2> AdjacentTo(Vec2 p)
        {
            if (p.y > 0)
                yield return Vec2.Set(p.x, p.y - 1);
            if (p.y < h - 1)
                yield return Vec2.Set(p.x, p.y + 1);
            if (p.x > 0)
                yield return Vec2.Set(p.x - 1, p.y);
            if (p.x < w - 1)
                yield return Vec2.Set(p.x + 1, p.y);
        }
    };

    class Program
    {
        static Map Parse(string filename)
        {
            string[] str = File.ReadAllLines(filename).ToArray();

            int w = str[0].Length;
            int h = str.Length;
            var map = new char[w, h];

            for (int y = 0; y < h; ++y)
            for (int x = 0; x < w; ++x)
                map[x, y] = str[y][x];

            return new Map(map, w, h);
        }

        static int Part1(Map map)
        {
            var reachedKeys = new bool[255];
            reachedKeys['@'] = true;
            return Part1Rec(map, '@', reachedKeys, 1);
        }

        static int Part1Rec(Map map, char key, bool[] reachedKeys, int reachedKeysCount)
        {
            var dist = map
                .Distance(key, reachedKeys)
                .Where(x => !reachedKeys[x.key])
                .OrderBy(x => x.dist);

            int best = Int32.MaxValue;

            foreach (var p in dist)
            {
                reachedKeys[p.key] = true;
                int val = Part1Rec(map, p.key, reachedKeys, reachedKeysCount + 1);
                reachedKeys[p.key] = false;
                if (val < Int32.MaxValue && (val + p.dist < best))
                    best = val + p.dist;
            }

            return best < Int32.MaxValue ? best : (reachedKeysCount == map.keysCount ? 0 : Int32.MaxValue);
        }

        static void Main(string[] args)
        {
            Console.WriteLine($"ref1 = {Part1(Parse("ref1.txt"))} = 86");
            Console.WriteLine($"ref2 = {Part1(Parse("ref2.txt"))} = 132");
            Console.WriteLine($"ref3 = {Part1(Parse("ref3.txt"))} = 136");
            Console.WriteLine($"ref4 = {Part1(Parse("ref4.txt"))} = 81");
        }
    }
}