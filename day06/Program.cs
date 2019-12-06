using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace day06
{
    class Program
    {
        class Planet
        {
            public string name;
            public string parent;
            public string[] children = new string[0];

            public Planet(string setName)
            {
                name = setName;
            }
        };

        static Dictionary<string, Planet> CreateTree(IEnumerable<string[]> pairs)
        {
            // create tree
            var planets = new Dictionary<string, Planet>();
            foreach (var pair in pairs)
            {
                string parent = pair[0];
                string name = pair[1];
                if (!planets.ContainsKey(parent))
                    planets[parent] = new Planet(parent);
                if (!planets.ContainsKey(name))
                    planets[name] = new Planet(name);
                planets[parent].children = planets[parent].children.Append(name).ToArray();
                planets[name].parent = parent;
            }

            return planets;
        }

        static int CountOrbits(Dictionary<string, Planet> planets)
        {
            // count orbits
            int orbits = 0;
            foreach (var pair in planets)
            {
                var temp = pair.Value.name;
                while (temp != null)
                {
                    temp = planets[temp].parent;
                    
                    if (temp != null)
                        orbits++;
                }
            }

            return orbits;
        }

        static IEnumerable<string> PathToCOM(Dictionary<string, Planet> planets, string pos)
        {
            while (pos != null)
            {
                pos = planets[pos].parent;
                yield return pos;
            }
        }

        static int OptimalTransfer(Dictionary<string, Planet> planets)
        {
            string[] pathA = PathToCOM(planets, "SAN").ToArray();
            string[] pathB = PathToCOM(planets, "YOU").ToArray();

            // silly bruteforce, maybe we can make it faster
            for(int i = 0; i < pathA.Length; ++i)
            {
                for (int j = 0; j < pathB.Length; ++j)
                {
                    if (pathA[i] == pathB[j])
                    {
                        return i + j;
                    }
                }
            }

            return -1;
        }
        
        static void Main(string[] args)
        {
            var pairs = File.ReadAllLines("input1.txt").Select(s => s.Split(')'));
            var planets = CreateTree(pairs);
            Console.WriteLine($"part1 = {CountOrbits(planets)}");
            Console.WriteLine($"part2 = {OptimalTransfer(planets)}");
        }
    }
}