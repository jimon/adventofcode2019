using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace day16
{
    class Program
    { static int[] Step(int[] digits)
        {
            var r = new int[digits.Length];
            var m = new[] {0, 1, 0, -1};
            for (int i = 0; i < digits.Length; ++i)
            {
                int index = i;
                int current = 0;
                int acc = 0;
                for (int j = 0; j < digits.Length; ++j)
                {
                    if (index <= 0)
                    {
                        index = i;
                        current = (current + 1) % m.Length;
                    }
                    else
                    {
                        index--;
                    }
                    acc += digits[j] * m[current];
                }
                r[i] = Math.Abs(acc % 10);
            }
            return r;
        }

        static int[] Part1(int[] digits)
        {
            var r = (int[])digits.Clone();
            for (int i = 0; i < 100; ++i)
                r = Step(r);
            return r.Take(8).ToArray();
        }

        static int[] Part2(int[] digits)
        {
            var r = new int[digits.Length * 10000];
            for (int i = 0; i < r.Length; ++i)
                r[i] = digits[i % digits.Length];
            for (int i = 0; i < 100; ++i)
            {
                Console.WriteLine($"{i}");
                r = Step(r);
                
            }

            return r.Skip(Int32.Parse(IntArrToString(r.Take(8).ToArray()))).Take(8).ToArray();
        }

        static string IntArrToString(int[] digits)
        {
            return string.Join("", digits.Select(x => x.ToString()));
        }
        
        static void Main(string[] args)
        {
            // var digits = File.ReadAllLines("input1.txt")[0].Select(x => Int32.Parse(x.ToString())).ToArray();
            var digits = "03036732577212944063491565474664".Select(x => Int32.Parse(x.ToString())).ToArray();

            Console.WriteLine($"part1 = {IntArrToString(Part1(digits))} = 68764632");
            Console.WriteLine($"part2 = {IntArrToString(Part2(digits))} = 84462026");
        }
    }
}