using System;
using System.Collections;
using System.IO;
using System.Linq;

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
                int v = (state & (1 << (y * 5 + x))) != 0 ? 1 : 0;
                int l = x > 0 && (state & (1 << (y * 5 + x - 1))) != 0 ? 1 : 0;
                int r = x < 4 && (state & (1 << (y * 5 + x + 1))) != 0 ? 1 : 0;
                int t = y > 0 && (state & (1 << ((y - 1) * 5 + x))) != 0 ? 1 : 0;
                int b = y < 4 && (state & (1 << ((y + 1) * 5 + x))) != 0 ? 1 : 0;

                int sum = l + r + t + b;

                if (v != 0 && sum == 1 || v == 0 && sum > 0 && sum < 3)
                    next |= 1 << (y * 5 + x);
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

        static void Main(string[] args)
        {
            Console.WriteLine($"part1 = {Part1(Parse("input1.txt"))} = 1113073");
        }
    }
}