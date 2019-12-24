using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Numerics;

namespace day24
{
    class Program
    {
        static int Parse(string filename)
        {
            return File.ReadAllLines(filename).SelectMany(x => x).Select((v, i) => (v == '#' ? 1 << i : 0)).Sum();
        }

        static void Debug(int state)
        {
            Console.WriteLine("-----------");
            for (int y = 0; y < 5; ++y)
            {
                for (int x = 0; x < 5; ++x)
                {
                    Console.Write((state & (1 << (y * 5 + x))) == 0 ? '.' : '#');
                }

                Console.Write('\n');
            }
        }

        static int Step(int state)
        {
            int next = 0;
            for (int y = 0; y < 5; ++y)
            for (int x = 0; x < 5; ++x)
            {
                int v = CountBits(state & XYToMask(x, y));
                int sum = CountBits(state & (XYToMask(x - 1, y) | XYToMask(x + 1, y) | XYToMask(x, y - 1) |
                                             XYToMask(x, y + 1)));

                if (v != 0 && sum == 1 || v == 0 && sum > 0 && sum < 3)
                    next |= XYToMask(x, y);
            }

            return next;
        }

        static int Part1(int state)
        {
            var seen = new BitArray(1 << 25);

            while (true)
            {
                if (seen[state] == true)
                    return state;
                else
                    seen[state] = true;
                state = Step(state);
            }
        }

        static int XYToMask(int x, int y)
        {
            if (x < 0 || x > 4 || y < 0 || y > 4)
                return 0;
            return 1 << (y * 5 + x);
        }

        static int CountBits(int bits)
        {
            return BitOperations.PopCount((uint) bits);
        }

        static int Part2SumOfAdj(int layerPrev, int layerCur, int layerNext, int x, int y)
        {
            int sum = 0;

            int pl = CountBits(layerPrev & XYToMask(1, 2));
            int pt = CountBits(layerPrev & XYToMask(2, 1));
            int pr = CountBits(layerPrev & XYToMask(3, 2));
            int pb = CountBits(layerPrev & XYToMask(2, 3));

            int nl = CountBits(layerNext & (XYToMask(0, 0) |
                                            XYToMask(0, 1) |
                                            XYToMask(0, 2) |
                                            XYToMask(0, 3) |
                                            XYToMask(0, 4)));
            int nt = CountBits(layerNext & (XYToMask(0, 0) |
                                            XYToMask(1, 0) |
                                            XYToMask(2, 0) |
                                            XYToMask(3, 0) |
                                            XYToMask(4, 0)));
            int nr = CountBits(layerNext & (XYToMask(4, 0) |
                                            XYToMask(4, 1) |
                                            XYToMask(4, 2) |
                                            XYToMask(4, 3) |
                                            XYToMask(4, 4)));
            int nb = CountBits(layerNext & (XYToMask(0, 4) |
                                            XYToMask(1, 4) |
                                            XYToMask(2, 4) |
                                            XYToMask(3, 4) |
                                            XYToMask(4, 4)));

            int l = 0;
            int t = 0;
            int r = 0;
            int b = 0;

            if (x == 0)
                l = pl;
            else if (x == 3 && y == 2)
                l = nr;
            else
                l = CountBits(layerCur & XYToMask(x - 1, y));

            if (x == 4)
                r = pr;
            else if (x == 1 && y == 2)
                r = nl;
            else
                r = CountBits(layerCur & XYToMask(x + 1, y));

            if (y == 0)
                t = pt;
            else if (x == 2 && y == 3)
                t = nb;
            else
                t = CountBits(layerCur & XYToMask(x, y - 1));
            
            if (y == 4)
                b = pb;
            else if (x == 2 && y == 1)
                b = nt;
            else
                b = CountBits(layerCur & XYToMask(x, y + 1));
            
            return l + t + r + b;
        }

        static int Part2(int initialState, int times, int layersHint = 100)
        {
            var state = new int[layersHint];
            state[state.Length / 2] = initialState;

            for (int time = 0; time < times; ++time)
            {
                var next = new int[state.Length];

                for (int layer = 1; layer < state.Length - 1; ++layer)
                {
                    for (int y = 0; y < 5; ++y)
                    {
                        for (int x = 0; x < 5; ++x)
                        {
                            if (x == 2 && y == 2)
                                continue;
                            
                            int v = state[layer] & XYToMask(x, y);
                            int sum = Part2SumOfAdj(state[layer - 1], state[layer], state[layer + 1], x, y);
                            if (v != 0 && sum == 1 || v == 0 && sum > 0 && sum < 3)
                                next[layer] |= XYToMask(x, y);
                        }
                    }
                }

                state = next;
            }

            if (CountBits(state[1]) + CountBits(state[state.Length - 2]) > 0)
            {
                Console.WriteLine("detected bits in far ends");
            }

            int sumTotal = 0;
            for (int layer = 1; layer < state.Length - 1; ++layer)
            {
                sumTotal += CountBits(state[layer]);
                //Debug(state[layer]);
            }
            
            return sumTotal;
        }

        static void Main(string[] args)
        {
            Console.WriteLine($"ref1 = {Part1(Parse("ref1.txt"))} = 2129920");
            Console.WriteLine($"part1 = {Part1(Parse("input1.txt"))} = 1113073");

            Console.WriteLine($"ref2 = {Part2(Parse("ref1.txt"), 10, 20)} = 99");
            Console.WriteLine($"part2 = {Part2(Parse("input1.txt"), 200, 500)} = 1928");
        }
    }
}