using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace day03
{
    class Program
    {
        struct Move
        {
            public int Dx, Dy, Len;

            public Move(int x, int y, int len)
            {
                Dx = x;
                Dy = y;
                Len = len;
            }
        };

        struct Line
        {
            public Move[] Moves;
        };

        private static Move ParseMove(string move)
        {
            var v = Int32.Parse(move.Substring(1));
            switch (move.First())
            {
                case 'U': return new Move(0, -1, v);
                case 'D': return new Move(0, 1,v);
                case 'L': return new Move(-1, 0, v);
                case 'R': return new Move(1, 0, v);
            }
            throw new ArgumentException($"invalid move {move}");
        }

        private static Line ParseLine(string line)
        {
            var l = new Line();
            l.Moves = line.Split(",").Select(m => ParseMove(m)).ToArray();
            return l;
        }

        private static IEnumerable<int> Crossings(int size, Line[] lines)
        {
            int w = size;
            int h = size;
            int cx = w / 2;
            int cy = h / 2;
            var map = new int[w,h];

            for (int lineIdx = 1; lineIdx <= lines.Length; ++lineIdx)
            {
                int x = cx;
                int y = cy;
                foreach (var move in lines[lineIdx - 1].Moves)
                {
                    for (int i = 0; i < move.Len; ++i)
                    {
                        x += move.Dx;
                        y += move.Dy;
                        
                        var value = map[x, y];
                        if (x != cx && y != cy && value != 0 && value != lineIdx)
                        {
                            if (value != -1)
                            {
                                var dist = Math.Abs(cx - x) + Math.Abs(cy - y);
                                yield return dist;
                            }

                            map[x, y] = -1;
                        }
                        else
                        {
                            map[x, y] = lineIdx;
                        }
                    }
                }
            }

//            for (int y = 0; y < h; ++y)
//            {
//                for (int x = 0; x < w; ++x)
//                    Console.Write(map[x, y]);
//                Console.Write("\n");
//            }
        }
        
        static void Main(string[] args)
        {
            Console.WriteLine($"example1 {Crossings(20, new Line[] {ParseLine("R8,U5,L5,D3"), ParseLine("U7,R6,D4,L4")}).Min()} = 6");
            Console.WriteLine($"example2 {Crossings(500, new Line[] {ParseLine("R75,D30,R83,U83,L12,D49,R71,U7,L72"), ParseLine("U62,R66,U55,R34,D71,R55,D58,R83")}).Min()} = 159");
            Console.WriteLine($"example3 {Crossings(500, new Line[] {ParseLine("R98,U47,R26,D63,R33,U87,L62,D20,R33,U53,R51"), ParseLine("U98,R91,D20,R16,D67,R40,U7,R15,U6,R7")}).Min()} = 135");

            Console.WriteLine($"part1 {Crossings(20000, File.ReadAllLines("input1.txt").Select(line => ParseLine(line)).ToArray()).Min()}");


        }
    }
}