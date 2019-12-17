using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace day16
{
    class Program
    {
        private static int[] PatternKernel = new[] {0, 1, 0, -1};

        static int Pattern(int offset, int repeats)
        {
            return PatternKernel[((offset + 1) / repeats) % PatternKernel.Length];
        }

        static int[] Step(int[] digits)
        {
            var r = new int[digits.Length];
            for (int i = 0; i < digits.Length; ++i)
            {
                int acc = 0;
                for (int j = 0; j < digits.Length; ++j)
                    acc += digits[j] * Pattern(j, i + 1);
                r[i] = Math.Abs(acc % 10);
            }

            return r;
        }

        static int[] Part1(int[] digits)
        {
            var r = (int[]) digits.Clone();
            for (int i = 0; i < 100; ++i)
                r = Step(r);
            return r.Take(8).ToArray();
        }

        static int[] Part2(int[] digits)
        {
            var r = new int[digits.Length * 10000];
            for (int i = 0; i < r.Length; ++i)
                r[i] = digits[i % digits.Length];

            int offset = IntArrToInt(digits.Take(7));
            if ((offset + 1) * 2 < digits.Length * 10000)
                throw new ArgumentException("offset is too small for 00001111-like pattern to cover whole sequence");

            for (int times = 0; times < 100; ++times)
            {
                // pattern is 0 0 0 0 0 for [0, offset)
                //        and 1 1 1 1 1 for [offset, length - 1]
                // so this unrolls loop of sums into accumulation loop from the end
                int acc = 0;
                for (int i = r.Length - 1; i >= offset; i--)
                {
                    acc += r[i];
                    r[i] = acc % 10;
                }
            }
            
            return r.Skip(offset).Take(8).ToArray();
        }

        static int[] StringToIntArr(string str)
        {
            return str.Select(x => Int32.Parse(x.ToString())).ToArray();
        }

        static string IntArrToString(IEnumerable<int> digits)
        {
            return string.Join("", digits.Select(x => x.ToString()));
        }

        static int IntArrToInt(IEnumerable<int> digits)
        {
            return Int32.Parse(IntArrToString(digits));
        }

        static void Main(string[] args)
        {
            Console.WriteLine($"ref1 = {IntArrToString(Part1(StringToIntArr("80871224585914546619083218645595")))} = 24176176");
            Console.WriteLine($"ref4 = {IntArrToString(Part2(StringToIntArr("03036732577212944063491565474664")))} = 84462026");
            
            var digits = StringToIntArr(File.ReadAllLines("input1.txt")[0]);
            Console.WriteLine($"part1 = {IntArrToString(Part1(digits))} = 68764632");
            Console.WriteLine($"part2 = {IntArrToString(Part2(digits))} = 52825021");
        }
    }
}