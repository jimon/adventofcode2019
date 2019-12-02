using System;
using System.IO;
using System.Linq;

namespace day02
{
    class Program
    {
        static bool CPU(ref int[] ram)
        {
            uint ip = 0;
            bool halt = false;

            while (!halt)
            {
                if (ip >= ram.Length)
                    return false;

                switch (ram[ip])
                {
                    case 1:
                        ram[ram[ip + 3]] = ram[ram[ip + 1]] + ram[ram[ip + 2]];
                        ip += 4;
                        break;
                    case 2:
                        ram[ram[ip + 3]] = ram[ram[ip + 1]] * ram[ram[ip + 2]];
                        ip += 4;
                        break;
                    case 99:
                        halt = true;
                        break;
                    default:
                        return false;
                }
            }

            return true;
        }
        
        static void Main(string[] args)
        {
            var rom = File.ReadAllLines("input1.txt").SelectMany(s => s.Split(',')).Select(l => Int32.Parse(l)).ToArray();

            {
                var ram = (int[])rom.Clone();
                ram[1] = 12;
                ram[2] = 2;
                CPU(ref ram);
                Console.WriteLine($"part1 {ram[0]}");
            }

            for (int pos1 = 0; pos1 <= 99; pos1++)
            {
                for (int pos2 = 0; pos2 <= 99; pos2++)
                {
                    var ram = (int[])rom.Clone();
                    ram[1] = pos1;
                    ram[2] = pos2;
                    if (CPU(ref ram))
                    {
                        if (ram[0] == 19690720)
                        {
                            Console.WriteLine($"part2 {pos1} {pos2}");
                        }
                    }
                }
            }
        }
    }
}