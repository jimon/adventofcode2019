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
        private Dictionary<char, Vec2> keyToPos = new Dictionary<char, Vec2>();
        private Dictionary<char, Vec2> doorToPos = new Dictionary<char, Vec2>();
        private HashSet<char> openDoors = new HashSet<char>();
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
                        keyToPos[v] = pos;
                        map[x, y] = '.';
                    }
                    else if (v >= 'A' && v <= 'Z')
                    {
                        doorToPos[v.ToString().ToLower()[0]] = pos;
                        map[x, y] = '.';
                    }
                }
            }
        }

        public char[] Keys()
        {
            return keyToPos.Keys.ToArray();
        }

        public int? Distance(char fromKey, char toKey)
        {
            var visited = new bool[w, h];
            var dst = new C5.IPriorityQueueHandle<Vec2Cost>[w, h]; 
            var heap = new C5.IntervalHeap<Vec2Cost>();

            var start = keyToPos[fromKey];
            for (int x = 0; x < w; ++x)
            {
                for (int y = 0; y < h; ++y)
                {
                    heap.Add(ref dst[x, y], new Vec2Cost(Vec2.Set(x, y), x == start.x && y == start.y ? 0 : Int32.MaxValue));
                }
            }

            while (!heap.IsEmpty)
            {
                C5.IPriorityQueueHandle<Vec2Cost> handle = null;
                Vec2Cost p = heap.DeleteMin(out handle);
                visited[p.pos.x, p.pos.y] = true;
                

                foreach (var adj in AdjacentTo(p.pos))
                {
                    var v = map[adj.x, adj.y];
                    if (v == '#')
                        continue;

                    var old_cost = dst[adj.x, adj.y];
                    var new_cost = p.cost + 1;
                    if (new_cost < old_cost)
                    {
                        distance[adj.x, adj.y] = new_cost;
                        if (!visited)
                        {
                            heap.Replace();
                        }
                    }
                }
            }
            
            return null;
        }

        public void OpenDoor(char door)
        {
            if (!openDoors.Contains(door))
                openDoors.Add(door);
        }

        public void ResetDoors()
        {
            openDoors = new HashSet<char>();
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

        static void Main(string[] args)
        {
            // ref1 = 86
            // ref2 = 132
            // ref3 = 136
            // ref4 = 81

            var map = Parse("ref1.txt");

            Console.WriteLine($"{map.Distance('@', 'a')}");

            // map.OpenDoor('c');
            // map.OpenDoor('a');
            //
            // Console.WriteLine($"{map.Distance('@', 'b')}");
            // Console.WriteLine($"{map.Distance('@', 'e')}");
            //
            // map.ResetDoors();
            //
            // Console.WriteLine($"{map.Distance('@', 'b')}");
        }
    }
}