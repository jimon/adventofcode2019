using System;
using System.IO;
using System.Linq;

namespace day12
{
    class Planet
    {
        public int[] p = new int[3];
        public int[] v = new int[3];

        public Planet(int[] setP)
        {
            p[0] = setP[0];
            p[1] = setP[1];
            p[2] = setP[2];
            v[0] = 0;
            v[1] = 0;
            v[2] = 0;
        }

        public Planet Copy()
        {
            var n = new Planet(p);
            n.v = (int[]) v.Clone();
            return n;
        }
    };
    
    class Program
    {
        static Planet Parse(string line)
        {
            var fields = line
                .Trim(new char[] {' ', '<', '>'})
                .Split(',')
                .Select(s => s
                    .Trim()
                    .Split('=')
                    .ToArray()
                );

            var p = new int[3];
            
            foreach (var field in fields)
            {
                var value = Int32.Parse(field[1]);
                if (field[0] == "x")
                    p[0] = value;
                else if (field[0] == "y")
                    p[1] = value;
                else if (field[0] == "z")
                    p[2] = value;
            }
            
            return new Planet(p);
        }
        
        static Planet[] Load(string filename)
        {
            return File.ReadAllLines(filename).Select(s => Parse(s)).ToArray();
        }

        static void Debug(Planet[] planets)
        {
            Console.WriteLine($"-------------- energy = {Energy(planets)}");
            foreach (var p in planets)
                Console.WriteLine($"pos=<x={p.p[0]}, y={p.p[1]}, z={p.p[2]}>, vel=<x={p.v[0]}, y={p.v[1]}, z={p.v[2]}>");
        }

        static void Step(Planet[] planets, int axis)
        {
            var dv = new int[planets.Length];
            for(int i = 0; i < planets.Length; ++i)
            {
                var p = planets[i];
                foreach (var n in planets)
                {
                    if (p.p[axis] < n.p[axis])
                        dv[i] += 1;
                    else if (p.p[axis] > n.p[axis])
                        dv[i] -= 1;
                }
            }

            for (int i = 0; i < planets.Length; ++i)
            {
                var p = planets[i];
                p.v[axis] += dv[i];
                p.p[axis] += p.v[axis];
            }
        }

        static int Energy(Planet[] planets)
        {
            return planets.Select(p =>
                    p.p.Select(x => Math.Abs(x)).Sum() *
                    p.v.Select(x => Math.Abs(x)).Sum()
                    )
                .Sum();
        }

        static UInt64 ResonanceInterval(Planet[] planetsRef, int axis)
        {
            var planets = planetsRef.Select(p => p.Copy() ).ToArray();

            UInt64 count = 0;
            while (true)
            {
                count++;
                Step(planets, axis);

                bool valid = true;
                for (int i = 0; i < planets.Length; ++i)
                {
                    if (!planets[i].p.SequenceEqual(planetsRef[i].p) || !planets[i].v.SequenceEqual(planetsRef[i].v))
                    {
                        valid = false;
                        break;
                    }
                }

                if (valid)
                    return count;
            }
        }
        
        // https://stackoverflow.com/questions/18541832/c-sharp-find-the-greatest-common-divisor
        private static UInt64 GCD(UInt64 a, UInt64 b)
        {
            while (a != 0 && b != 0)
            {
                if (a > b)
                    a %= b;
                else
                    b %= a;
            }

            return a == 0 ? b : a;
        }
        
        // https://stackoverflow.com/questions/13569810/least-common-multiple
        private static UInt64 LCM(UInt64 a, UInt64 b)
        {
            return (a / GCD(a, b)) * b;
        }
        
        static void Main(string[] args)
        {
            var planets = Load("input1.txt");
            for (int i = 0; i < 100; ++i)
            {
                Step(planets, 0);
                Step(planets, 1);
                Step(planets, 2);
            }
            Console.WriteLine($"part1 = {Energy(planets)} = 10198");

            var r = new UInt64[]
            {
                ResonanceInterval(planets, 0),
                ResonanceInterval(planets, 1),
                ResonanceInterval(planets, 2)
            };

            var subperiod1 = LCM(r[0], r[1]);
            var subperiod2 = LCM(r[1], r[2]);
            var period = LCM(subperiod1, subperiod2);
            Console.WriteLine($"part2 = {period} = 271442326847376");
        }
    }
}