using System;
using System.Linq;

namespace day04
{
    class Program
    {
        private static int Iterator(int from, int to, bool part2)
        {
            // brute force version
            int valid = 0;
            for (int number = from; number <= to; ++number)
            {
                var arr = number.ToString().Select(x => Int32.Parse(x.ToString())).ToArray();

                var a = arr[0];
                var b = arr[1];
                var c = arr[2];
                var d = arr[3];
                var e = arr[4];
                var f = arr[5];

                if (a > b || b > c || c > d || d > e || e > f)
                    continue;

                if (part2)
                {
                    if ((a == b && b != c) ||
                        (a != b && b == c && c != d) ||
                        (b != c && c == d && d != e) ||
                        (c != d && d == e && e != f) ||
                        (d != e && e == f))
                        valid++;
                }
                else
                {
                    if (a == b || b == c || c == d || d == e || e == f)
                        valid++;
                }
            }
            return valid;
        }
        
        static void Main(string[] args)
        {
            Console.WriteLine($"test = {Iterator(112233, 112233, true)} (should be 1)");
            Console.WriteLine($"test = {Iterator(123444, 123444, true)} (should be 0)");
            Console.WriteLine($"test = {Iterator(111122, 111122, true)} (should be 1)");
            
            Console.WriteLine($"part1 = {Iterator(347312, 805915, false)} (should be 594)");
            Console.WriteLine($"part2 = {Iterator(347312, 805915, true)} (should be 364)");
        }
    }
}