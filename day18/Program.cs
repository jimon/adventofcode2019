using System;
using System.Collections;
//using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
//using C5;
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

        public Dictionary<char, Dictionary<char, (int steps, HashSet<char> neededKeys)>> keyToKeyMap =
            new Dictionary<char, Dictionary<char, (int steps, HashSet<char> neededKeys)>>();

        public int keysCount;

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
                        keyToPos[v] = pos;
                        keysCount++;
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
                keyToKeyMap[fromKeyPair.Key] = new Dictionary<char, (int steps, HashSet<char> needsKeys)>();
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

                    keyToKeyMap[fromKeyPair.Key][toKeyPair.Key] = (steps: t.Distance, neededKeys);
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
            //var reachedKeys = new bool[map.keysCount];
            //return Part1Rec(map, '@', reachedKeys, 1);
            return Part1Rec(map, '@', new HashSet<char> {'@'});
        }


        static int Part1Rec(Map map, char fromKey, HashSet<char> hasKeys, int keysCount = 1)
        {
            if (keysCount == map.keysCount)
                return 0;

            var traverse = map
                .keyToKeyMap[fromKey]
                .Where(x => !hasKeys.Contains(x.Key))
                .Where(x => !x.Value.neededKeys.Except(hasKeys).Any());
                //.OrderBy(x => x.Value.steps);

            int bestSteps = Int32.MaxValue;
            int v = 0;
            foreach (var keyValuePair in traverse)
            {
                int stepsToReachKey = keyValuePair.Value.steps;
                if (stepsToReachKey >= bestSteps)
                    continue;

                //Console.WriteLine($"{Enumerable.Repeat(' ', keysCount)}trying {fromKey} -> {keyValuePair.Key}");

                int stepsRec = Part1Rec(map, keyValuePair.Key, hasKeys.Append(keyValuePair.Key).ToHashSet(),
                    keysCount + 1);
                if (stepsRec == Int32.MaxValue)
                    continue;

                if (stepsRec + stepsToReachKey < bestSteps)
                {
                    bestSteps = stepsRec + stepsToReachKey;
                }
            }


            return bestSteps;
        }

        //static long progress = 0;

        /*
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
        */

        static void Main(string[] args)
        {
            Console.WriteLine($"ref1 = {Part1(Parse("ref1.txt"))} = 86");
            Console.WriteLine($"ref2 = {Part1(Parse("ref2.txt"))} = 132");
            Console.WriteLine($"ref3 = {Part1(Parse("ref3.txt"))} = 136");
            Console.WriteLine($"ref4 = {Part1(Parse("ref4.txt"))} = 81");

            //Console.WriteLine($"part1 = {Part1(Parse("input1.txt"))} = ");
        }
    }
}