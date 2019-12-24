using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dijkstra.NET.Graph;
using Dijkstra.NET.ShortestPath;

namespace day18
{
    class Vec2 : IEquatable<Vec2>
    {
        public int x;
        public int y;

        public Vec2(int setX, int setY)
        {
            x = setX;
            y = setY;
        }

        public bool Equals(Vec2 other)
        {
            return x == other.x && y == other.y;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Vec2) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x, y);
        }
    };
    
    // class HeapValue : IComparable<HeapValue>
    // {
    //     public char key;
    //     public int steps;
    //     public HashSet<char> hasKeys;
    //
    //     public HeapValue(char setKey, int setSteps, HashSet<char> setHasKeys)
    //     {
    //         key = setKey;
    //         steps = setSteps;
    //         hasKeys = setHasKeys;
    //     }
    //
    //     public int CompareTo(HeapValue other)
    //     {
    //         return steps.CompareTo(other.steps);
    //     }
    // };
    
    class Map
    {
        private char[,] map;
        private Dictionary<char, Vec2> keyToPos = new Dictionary<char, Vec2>();
        private Dictionary<char, Vec2> doorToPos = new Dictionary<char, Vec2>();
        private int w;
        private int h;

        private Graph<Vec2, int> mapGraph = new Graph<Vec2, int>();
        private Dictionary<Vec2, uint> posToMapKey = new Dictionary<Vec2, uint>();
        private Dictionary<uint, char> mapKeyToNeedsAKey = new Dictionary<uint, char>();

        public Dictionary<char, Dictionary<char, (int steps, int neededKeys)>> keyToKeyMap =
            new Dictionary<char, Dictionary<char, (int steps, int neededKeys)>>();

        private int keysCount;

        static int KeyToBits(char key)
        {
            if (key == '@')
                return 1;
            return 1 << (key - 'a' + 1);
        }
        
        static IEnumerable<char> BitsToKeys(int bits)
        {
            for (int i = 0; i < 32; ++i)
                if ((bits & (1 << i)) != 0)
                    yield return i == 0 ? '@' : (char)(i - 1 + 'a');
        }

        public Map(char[,] setMap, int setW, int setH)
        {
            w = setW;
            h = setH;
            map = (char[,]) setMap.Clone();

            for (int x = 0; x < w; ++x)
            {
                for (int y = 0; y < h; ++y)
                {
                    var v = map[x, y];
                    if (v == '#')
                        continue;

                    var pos = new Vec2(x, y);
                    var node = mapGraph.AddNode(pos);
                    posToMapKey[pos] = node;

                    if ((v >= 'a' && v <= 'z') || (v == '@'))
                    {
                        keysCount++;
                        keyToPos[v] = pos;
                    }
                    else if (v >= 'A' && v <= 'Z')
                    {
                        doorToPos[v] = pos;
                        mapKeyToNeedsAKey[node] = v.ToString().ToLower().First();
                    }
                }
            }

            for (int x = 0; x < w; ++x)
            {
                for (int y = 0; y < h; ++y)
                {
                    var v = map[x, y];
                    if (v == '#')
                        continue;

                    var pos = new Vec2(x, y);
                    var key1 = posToMapKey[pos];

                    foreach (var adj in AdjacentTo(pos))
                    {
                        var a = map[adj.x, adj.y];
                        if (a == '#')
                            continue;
                        var key2 = posToMapKey[adj];
                        mapGraph.Connect(key1, key2, 1, 0);
                    }
                }
            }

            // sad that Dijkstra.NET is cannot provide actual map of results
            // so have to do O(N^2) here 
            var r = new Dictionary<char, Dictionary<char, (int steps, HashSet<char> needsKeys)>>();
            foreach (var fromKeyPair in keyToPos)
            {
                keyToKeyMap[fromKeyPair.Key] = new Dictionary<char, (int steps, int needsKeys)>();
                foreach (var toKeyPair in keyToPos)
                {
                    var mapKey1 = posToMapKey[fromKeyPair.Value];
                    var mapKey2 = posToMapKey[toKeyPair.Value];
                    var t = mapGraph.Dijkstra(mapKey1, mapKey2);

                    if (t.IsFounded == false)
                        throw new ArgumentException($"no path from {fromKeyPair} -> {toKeyPair}");

                    var neededKeys = new HashSet<char>();
                    foreach (var u in t.GetPath())
                    {
                        var needsAKey = mapKeyToNeedsAKey.GetValueOrDefault(u, (char) 0);
                        if (needsAKey != 0)
                            neededKeys.Add(needsAKey);
                    }

                    keyToKeyMap[fromKeyPair.Key][toKeyPair.Key] = (steps: t.Distance, neededKeys.Select(x => KeyToBits(x)).Sum());
                }
            }
        }

        private IEnumerable<Vec2> AdjacentTo(Vec2 p)
        {
            if (p.y > 0)
                yield return new Vec2(p.x, p.y - 1);
            if (p.y < h - 1)
                yield return new Vec2(p.x, p.y + 1);
            if (p.x > 0)
                yield return new Vec2(p.x - 1, p.y);
            if (p.x < w - 1)
                yield return new Vec2(p.x + 1, p.y);
        }
        
        //private Dictionary<char, (int steps, HashSet<char> keys)> visited = new Dictionary<char, (int steps, HashSet<char> keys)>();
        //private C5.IntervalHeap<HeapValue> heap = new C5.IntervalHeap<HeapValue>();

        public int Part1()
        {
            return Part1Rec('@', KeyToBits('@'), 1);
        }

        private Dictionary<(char, int), int> cache = new Dictionary<(char, int), int>();

        private int Part1Rec(char key, int hasKeys, int deep)
        {
            if (deep == keysCount)
                return 0;

            var tuple = (key, hasKeys);
            int cachedSteps = cache.GetValueOrDefault(tuple, 0);
            if (cachedSteps > 0)
                return cachedSteps;

            var thisAndHasKeys = KeyToBits(key) | hasKeys;

            var traverse = keyToKeyMap[key]
                .Where(x => ((thisAndHasKeys & KeyToBits(x.Key)) == 0) &&
                            ((thisAndHasKeys & x.Value.neededKeys) == x.Value.neededKeys));
            
            int best = Int32.MaxValue;
            foreach (var p in traverse)
            {
                int v = Part1Rec(p.Key, thisAndHasKeys, deep + 1) + p.Value.steps;
                if (v < best)
                    best = v;
            }

            cache[tuple] = best;

            return best;
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

        static void Main(string[] args)
        {
            Console.WriteLine($"ref1 = {Parse("ref1.txt").Part1()} = 86");
            Console.WriteLine($"ref2 = {Parse("ref2.txt").Part1()} = 132");
            Console.WriteLine($"ref3 = {Parse("ref3.txt").Part1()} = 136");
            Console.WriteLine($"ref4 = {Parse("ref4.txt").Part1()} = 81");

            Console.WriteLine($"part1 = {Parse("input1.txt").Part1()} = 5964");
            
            // ref5 = 8
            // ref6 = 24
            // ref7 = 32
            // ref8 = 72
        }
    }
}