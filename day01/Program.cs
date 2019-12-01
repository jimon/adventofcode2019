using System;
using System.IO;
using System.Linq;

namespace day01
{
    class Program
    {
        static int FuelRequirementSimple(int moduleMass)
        {
            return (moduleMass / 3) - 2;
        }

        static int FuelRequirementExtra(int moduleMass)
        {
            int totalFuel = 0;
            int extraFuel = FuelRequirementSimple(moduleMass);
            
            while (extraFuel > 0)
            {
                totalFuel += extraFuel;
                extraFuel = FuelRequirementSimple(extraFuel);
            }

            return totalFuel;
        }
        
        static void Main(string[] args)
        {
            var allModuleMasses = File.ReadAllLines("input1.txt").Select(l => Int32.Parse(l));
            
            Console.WriteLine($"fuel simple = {allModuleMasses.Select(m => FuelRequirementSimple(m)).Sum()}");
            Console.WriteLine($"fuel extra = {allModuleMasses.Select(m => FuelRequirementExtra(m)).Sum()}");
        }
    }
}