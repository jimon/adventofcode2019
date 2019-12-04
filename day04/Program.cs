using System;
using System.Linq;

namespace day04
{
    class Program
    {
        private static int Iterator(int from, int to)
        {
            // brute force version
            int valid = 0;
            for (int number = from; number <= to; ++number)
            {
                var arr = number.ToString().Select(x => Int32.Parse(x.ToString())).ToArray();
                int? prev = null;
                bool pair = false;
                bool incr = true;
                foreach (var num in arr)
                {
                    if (prev.HasValue)
                    {
                        if (num < prev.Value)
                        {
                            incr = false;
                            break;
                        }
                        else if (num == prev.Value)
                            pair = true;
                            
                    }
                    prev = num;
                }

                if (pair && incr)
                    valid++;
            }
            return valid;
        }
        
        static void Main(string[] args)
        {
            Console.WriteLine($"part1 = {Iterator(347312, 805915)}");
        }
    }
}