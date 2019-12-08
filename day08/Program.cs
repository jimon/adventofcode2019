using System;
using System.IO;
using System.Linq;

namespace day08
{
    class Program
    {
        static void Main(string[] args)
        {
            var w = 25;
            var h = 6;
            var data = File
                .ReadAllLines("input1.txt")
                .SelectMany(x => x)
                .Select(x => Int32.Parse(x.ToString()))
                .Select((v, i) => (index: i, value: v))
                .GroupBy(x => x.index / (w * h));

            var minLayer = data
                .Select(l => (layer: l, count: l.Where(x => x.value == 0).Count()))
                .OrderBy(l => l.count)
                .First().layer;
            var count1 = minLayer.Where(x => x.value == 1).Count();
            var count2 = minLayer.Where(x => x.value == 2).Count();
            Console.WriteLine(
                $"part1 = {count1 * count2} = 1441");

            Console.WriteLine("part2: ");
            var pic = new int[w * h];
            foreach (var v in data.Reverse().SelectMany(l => l))
                if (v.value != 2)
                    pic[v.index % (w * h)] = v.value;

            for (int j = 0; j < h; ++j)
            {
                for (int i = 0; i < w; ++i)
                    Console.Write(pic[w * j + i] == 1 ? "*" : " ");
                Console.Write("\n");
            }
        }
    }
}