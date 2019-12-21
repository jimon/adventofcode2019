using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Dijkstra.NET.Graph;
using Dijkstra.NET.ShortestPath;
using Microsoft.VisualBasic;

namespace day20
{
    class Map
    {
        private Graph<(int x, int y), int> graph = new Graph<(int x, int y), int>();
        private Dictionary<(int x, int y), uint> posToKey = new Dictionary<(int x, int y), uint>();
        private Dictionary<string, List<uint>> labelToKeys = new Dictionary<string, List<uint>>();
        private Dictionary<(int x, int y), string> posToLabel = new Dictionary<(int x, int y), string>();
        private int w;
        private int h;

        public Map(string filename)
        {
            string[] str = File.ReadAllLines(filename).ToArray();

            w = str[0].Length;
            h = str.Length;

            // part1, fill all valid points
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    var v = str[y][x];
                    if (v != '.')
                        continue;

                    var pos = (x: x, y: y);
                    posToKey[pos] = graph.AddNode(pos);
                }
            }

            // part2, find all teleports
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    var v = str[y][x];
                    if (v != '.')
                        continue;

                    var pos = (x: x, y: y);

                    foreach (var adj in AdjacentTo(x, y))
                    {
                        var a1 = str[adj.y][adj.x];

                        if (a1 >= 'A' && a1 <= 'Z')
                        {
                            int dx = adj.x - pos.x;
                            int dy = adj.y - pos.y;
                            var a2 = str[pos.y + dy * 2][pos.x + dx * 2];
                            var l = dx >= 0 && dy >= 0 ? $"{a1}{a2}" : $"{a2}{a1}"; 
                            if (!labelToKeys.ContainsKey(l))
                                labelToKeys[l] = new List<uint>();
                            labelToKeys[l].Add(posToKey[pos]);
                            if (posToLabel.ContainsKey(pos))
                                throw new ArgumentException($"found two labels at pos {pos.x} {pos.y} -> {dx} {dy}");
                            posToLabel[pos] = l;
                        }
                    }
                }
            }

            // part3, connect nodes
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    var pos = (x: x, y: y);
                    var v = str[y][x];
                    if (v != '.')
                        continue;

                    var key1 = posToKey[pos];
            
                    foreach (var adj in AdjacentTo(x, y))
                    {
                        var a = str[adj.y][adj.x];
                        if (a == '.')
                            graph.Connect(key1, posToKey[adj], 1, 0);
                        else if (a >= 'A' && a <= 'Z' && posToLabel.ContainsKey(pos))
                        {
                            var keys = labelToKeys[posToLabel[pos]];
                            if (keys.Count < 2)
                                continue;

                            uint key2 = keys.Where(x => x != key1).First();
                            graph.Connect(key1, key2, 1, 0);
                        }
                    }
                }
            }
        }

        public int? Distance(string from, string to)
        {
            var r = graph.Dijkstra(labelToKeys[from].First(), labelToKeys[to].First());
            if (r.IsFounded)
                return r.Distance;
            return null;
        }

        private IEnumerable<(int x, int y)> AdjacentTo(int x, int y)
        {
            if (y > 0)
                yield return (x: x, y: y - 1);
            if (y < h - 1)
                yield return (x: x, y: y + 1);
            if (x > 0)
                yield return (x: x - 1, y: y);
            if (x < w - 1)
                yield return (x: x + 1, y: y);
        }
    };

    class Program
    {
        static int? Part1(Map map)
        {
            return map.Distance("AA", "ZZ");
        }

        static void Main(string[] args)
        {
            Console.WriteLine($"ref1 = {Part1(new Map("ref1.txt"))} = 23");
            Console.WriteLine($"ref2 = {Part1(new Map("ref2.txt"))} = 58");
            Console.WriteLine($"part1 = {Part1(new Map("input1.txt"))} = ");
        }
    }
}