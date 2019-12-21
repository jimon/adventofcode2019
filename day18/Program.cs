using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using C5;

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

    class CacheKey : IStructuralEquatable
    {
        public char from;
        public bool[] state;

        public bool Equals(object? other, IEqualityComparer comparer)
        {
            var k = (CacheKey) other;
            if (from == k.from && state.Length == k.state.Length)
            {
                for (int i = 0; i < state.Length; ++i)
                    if (state[i] != k.state[i])
                        return false;
                return true;
            }

            return false;
        }

        public int GetHashCode(IEqualityComparer comparer)
        {
            int hash = from.GetHashCode();
            for (int i = 0; i < state.Length; ++i)
                hash = hash * 23 + state[i].GetHashCode();
            return hash;
        }
    };

    class Map
    {
        private Byte[,] map;
        public int keysCount;
        private Dictionary<char, Vec2> keyToPos = new Dictionary<char, Vec2>();
        private Dictionary<char, Vec2> doorToPos = new Dictionary<char, Vec2>();
        private int w;
        private int h;

        public Map(char[,] setMap, int setW, int setH)
        {
            w = setW;
            h = setH;
            map = new byte[w,h];

            for (int x = 0; x < w; ++x)
            {
                for (int y = 0; y < h; ++y)
                {
                    var v = setMap[x, y];
                    
                    var pos = Vec2.Set(x, y);

                    if (v == '#')
                    {
                        map[x, y] = 0;
                    }
                    else if (v == '.')
                    {
                        map[x, y] = byte.MaxValue;
                    }
                    if ((v >= 'a' && v <= 'z') || v == '@')
                    {
                        keysCount++;
                        keyToPos[v] = pos;
                        map[x, y] = byte.MaxValue;
                    }
                    else if (v >= 'A' && v <= 'Z')
                    {
                        var vl = v.ToString().ToLower()[0];
                        doorToPos[vl] = pos;
                        map[x, y] = (byte)(vl - 'a' + 1);
                    }
                }
            }
        }

        private bool[,] visited;
        private IPriorityQueueHandle<Vec2Cost>[,] heapHandles;
        private int[,] distance;
        private bool[,] reachable;
        private IntervalHeap<Vec2Cost> heap;

        public Dictionary<CacheKey, (char key, int dist)[]> cache = new Dictionary<CacheKey, (char key, int dist)[]>();

        public (char key, int dist)[] Distance(char fromKey, bool[] reachedKeys)
        {
            CacheKey cacheKey = new CacheKey();
            cacheKey.from = fromKey;
            cacheKey.state = (bool[])reachedKeys.Clone();
            
            if (cache.ContainsKey(cacheKey))
            {
                 return cache[cacheKey];
            }
            
            if (visited == null)
            {
                visited = new bool[w, h];
                heapHandles = new C5.IPriorityQueueHandle<Vec2Cost>[w, h];
                distance = new int[w, h];
                reachable = new bool[w, h];
                heap = new C5.IntervalHeap<Vec2Cost>();

                for (int x = 0; x < w; ++x)
                {
                    for (int y = 0; y < h; ++y)
                    {
                        heap.Add(ref heapHandles[x, y], new Vec2Cost(Vec2.Set(x, y), Int32.MaxValue));
                    }
                }
            }

            var start = keyToPos[fromKey];

            for (int x = 0; x < w; ++x)
            {
                for (int y = 0; y < h; ++y)
                {
                    var v = map[x, y];
                    var d = (x == start.x && y == start.y) ? 0 : Int32.MaxValue;
                    visited[x, y] = false;
                    distance[x, y] = d;
                    if ((v > 0) && ((v == byte.MaxValue) || reachedKeys[v - 1]))
                    {
                        reachable[x, y] = true;
                        heap.Replace(heapHandles[x, y], new Vec2Cost(Vec2.Set(x, y), d));
                    }
                    else
                        reachable[x, y] = false;
                }
            }

            while (!heap.IsEmpty)
            {
                C5.IPriorityQueueHandle<Vec2Cost> handle = null;
                Vec2Cost p = heap.FindMin(out handle);
                // some nodes are not actually reachable
                if (p.cost == Int32.MaxValue)
                    break;
                heap.Replace(heapHandles[p.pos.x, p.pos.y], new Vec2Cost(p.pos, Int32.MaxValue));
                visited[p.pos.x, p.pos.y] = true;
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

            var dist = new List<(char key, int dist)>();
            foreach (var p in keyToPos)
            {
                if (p.Key != fromKey && p.Key != '@' && reachedKeys[p.Key - 'a'] == false )
                {
                    int d = distance[p.Value.x, p.Value.y];
                    if (d != Int32.MaxValue)
                    {
                        dist.Add((key: p.Key, dist: d));
                    }
                }
            }

            var distArr = dist.ToArray();

            cache[cacheKey] = distArr;
            return distArr;
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
            var reachedKeys = new bool[map.keysCount];
            return Part1Rec(map, '@', reachedKeys, 1);
        }

        static long progress = 0;

        static int Part1Rec(Map map, char key, bool[] reachedKeys, int reachedKeysCount)
        {
            progress++;
            if (progress % 1000 == 0)
            {
                Console.WriteLine($"{progress}");
            }

            if (reachedKeysCount >= map.keysCount)
                return 0;

            var dist = map
                .Distance(key, reachedKeys)
                .Where(x => !reachedKeys[x.key - 'a']);
                //.OrderBy(x => x.dist);

            int best = Int32.MaxValue;

            foreach (var p in dist)
            {
                if (p.dist > best)
                    continue;
                reachedKeys[p.key - 'a'] = true;
                int val = Part1Rec(map, p.key, reachedKeys, reachedKeysCount + 1);
                reachedKeys[p.key - 'a'] = false;
                if (val < Int32.MaxValue && (val + p.dist < best))
                    best = val + p.dist;
            }

            return best;
        }

        static void Main(string[] args)
        {
            Console.WriteLine($"ref1 = {Part1(Parse("ref1.txt"))} = 86");
            Console.WriteLine($"ref2 = {Part1(Parse("ref2.txt"))} = 132");
            Console.WriteLine($"ref3 = {Part1(Parse("ref3.txt"))} = 136");
            Console.WriteLine($"ref4 = {Part1(Parse("ref4.txt"))} = 81");
            
            Console.WriteLine($"part1 = {Part1(Parse("input1.txt"))} = ");
        }
    }
}