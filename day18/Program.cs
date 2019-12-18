using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dijkstra.NET.Graph;
using Dijkstra.NET.ShortestPath;

namespace day18
{
    class Map
    {
        private Graph<(int x, int y), int> map;
        private char[,] cmap;
        private Dictionary<(int x, int y), uint> posToMapIndex;
        private Dictionary<char, (int x, int y)> keyToPos;
        private Dictionary<char, (int x, int y)> doorToPos;
        private HashSet<char> openDoors;
        private int w;
        private int h;

        public Map(char[,] setCmap, int setW, int setH)
        {
            cmap = setCmap;
            w = setW;
            h = setH;
            ResetDoors();
        }

        public char[] Keys()
        {
            return keyToPos.Keys.ToArray();
        }

        public int? Distance(char fromKey, char toKey)
        {
            var r = map.Dijkstra(posToMapIndex[keyToPos[fromKey]], posToMapIndex[keyToPos[toKey]]);

            if (r.IsFounded)
                return r.Distance;
            else
                return null;
        }

        public void OpenDoor(char door)
        {
            if (!openDoors.Contains(door))
                openDoors.Add(door);

            var pos = doorToPos[door];
            var posMapIndex = posToMapIndex[pos];
            foreach (var adj in AdjacentTo(pos.x, pos.y, w, h))
            {
                if (cmap[adj.x, adj.y] == '#')
                    continue;
                ConnectCells(posMapIndex, adj, posToMapIndex[adj]);
            }
        }

        public void ResetDoors()
        {
            // WTF?, is there a better way to disconnect edges in this library? 
            map = new Graph<(int x, int y), int>();
            openDoors = new HashSet<char>();
            posToMapIndex = new Dictionary<(int x, int y), uint>();
            keyToPos = new Dictionary<char, (int x, int y)>();
            doorToPos = new Dictionary<char, (int x, int y)>();

            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    if (cmap[x, y] == '#')
                        continue;

                    var pos = (x: x, y: y);
                    posToMapIndex[pos] = map.AddNode(pos);
                }
            }

            foreach (var p in posToMapIndex)
            {
                var pos = (x: p.Key.x, y: p.Key.y);
                char v = cmap[pos.x, pos.y];

                if ((v >= 'a' && v <= 'z') || v == '@')
                    keyToPos[v] = pos;
                else if (v >= 'A' && v <= 'Z')
                {
                    doorToPos[v.ToString().ToLower()[0]] = pos;
                    continue;
                }

                foreach (var adj in AdjacentTo(pos.x, pos.y, w, h))
                {
                    var va = cmap[adj.x, adj.y];
                    if ((va == '#') || (va >= 'A' && va <= 'Z'))
                        continue;

                    ConnectCells(p.Value, adj, posToMapIndex[adj]);
                }
            }
        }

        private void ConnectCells(uint posMapIndex, (int x, int y) adj, uint adjMapIndex)
        {
            var v = cmap[adj.x, adj.y];
            if (v != '#' && ((v == '.') || (v == '@') || (v >= 'a' && v <= 'z') ||
                             (v >= 'A' && v <= 'Z' && openDoors.Contains(v))))
            {
                map.Connect(posMapIndex, adjMapIndex, 1, 0);
                map.Connect(adjMapIndex, posMapIndex, 1, 0);
            }
        }

        private static IEnumerable<(int x, int y)> AdjacentTo(int x, int y, int w, int h)
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
            // ref1 = 86
            // ref2 = 132
            // ref3 = 136
            // ref4 = 81

            var map = Parse("ref1.txt");

            Console.WriteLine($"{map.Distance('@', 'a')}");

            map.OpenDoor('c');
            map.OpenDoor('a');

            Console.WriteLine($"{map.Distance('@', 'b')}");
            Console.WriteLine($"{map.Distance('@', 'e')}");

            map.ResetDoors();

            Console.WriteLine($"{map.Distance('@', 'b')}");
        }
    }
}