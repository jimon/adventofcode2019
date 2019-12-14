using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace day14
{
    class Chemical
    {
        public string name;
        public ulong amount;

        public Chemical(string chemicalLine)
        {
            var arr = chemicalLine.Split(" ").Select(s => s.Trim()).ToArray();
            amount = ulong.Parse(arr[0]);
            name = arr[1];
        }
    }
    
    class Rule
    {
        public Chemical output;
        public Chemical[] inputs;

        public Rule(string ruleLine)
        {
            var lr = ruleLine.Split(" => ").ToArray();
            output = new Chemical(lr[1]);
            inputs = lr[0].Split(", ").Select(s => new Chemical(s)).ToArray();
        }

        public static Dictionary<string, Rule> Parse(string filename)
        {
            return File.ReadAllLines(filename).Select(r => new Rule(r)).ToDictionary(r => r.output.name, r => r);
        }
    };
    
    class Program
    {
        static ulong OreFor(Dictionary<string, Rule> rules, string chemical, ulong need, Dictionary<string, ulong> waste)
        {
            if (chemical == "ORE")
                return need;
            
            var available = waste.GetValueOrDefault(chemical, (ulong)0);
            if (available > 0)
            {
                if (available >= need)
                {
                    waste[chemical] -= need;
                    return 0;
                }
                else
                {
                    waste[chemical] -= available;
                    need -= available;
                }
            }

            if (!rules.ContainsKey(chemical))
                return 0;
            
            var produce = rules[chemical].output.amount;
            var times = (need / produce) + (ulong)(need % produce != 0 ? 1 : 0);
            var output = produce * times;
            var extra = output - need;

            waste[chemical] = waste.GetValueOrDefault(chemical, (ulong)0) + extra;

            ulong ore = 0;
            foreach (var input in rules[chemical].inputs)
                ore += OreFor(rules, input.name, input.amount * times, waste);
            return ore;
        }

        static ulong OreForOneFuel(Dictionary<string, Rule> rules)
        {
            return OreFor(rules, "FUEL", 1, new Dictionary<string, ulong>());
        }

        static ulong FuelForOre(Dictionary<string, Rule> rules, ulong ore = 1000000000000)
        {
            // find one that need more ore
            ulong max = 1; 
            while (true)
                if (OreFor(rules, "FUEL", max, new Dictionary<string, ulong>()) > ore)
                    break;
                else
                    max *= 10;
            
            // binary search
            ulong min = max / 10;
            while (true)
            {
                if (min == max)
                    break;
                
                ulong cur = (min + max) / 2;
                var t = OreFor(rules, "FUEL", cur, new Dictionary<string, ulong>());

                if (t < ore)
                    min = cur + 1;
                else if (t > ore)
                    max = cur - 1;
                else
                    return cur;
            }

            return min;
        }

        static void Main(string[] args)
        {
            Console.WriteLine($"ref1 = {OreForOneFuel(Rule.Parse("ref1.txt"))} = 31");
            Console.WriteLine($"ref2 = {OreForOneFuel(Rule.Parse("ref2.txt"))} = 165");
            Console.WriteLine($"ref3 = {OreForOneFuel(Rule.Parse("ref3.txt"))} = 13312");
            Console.WriteLine($"ref4 = {OreForOneFuel(Rule.Parse("ref4.txt"))} = 180697");
            Console.WriteLine($"ref5 = {OreForOneFuel(Rule.Parse("ref5.txt"))} = 2210736");

            Console.WriteLine($"part1 = {OreForOneFuel(Rule.Parse("input1.txt"))} = 483766");

            Console.WriteLine($"ref3 = {FuelForOre(Rule.Parse("ref3.txt"))} = 82892753");
            Console.WriteLine($"ref4 = {FuelForOre(Rule.Parse("ref4.txt"))} = 5586022");
            Console.WriteLine($"ref5 = {FuelForOre(Rule.Parse("ref5.txt"))} = 460664");

            Console.WriteLine($"part2 = {FuelForOre(Rule.Parse("input1.txt"))} = 3061522");
        }
    }
}