using System;
using System.IO;
using System.Linq;
using System.Numerics;

namespace day22
{
    class Program
    {
        // convert operations to a*x + b ≡ c (mod deckSize)
        static (BigInteger a, BigInteger b) Parse(string filename, BigInteger deckSize)
        {
            BigInteger a = new BigInteger(1);
            BigInteger b = new BigInteger(0);

            foreach (var l in File.ReadAllLines(filename))
            {
                if (l == "deal into new stack")
                {
                    a *= -1;
                    b *= -1;
                    b += deckSize - 1;
                }
                else
                {
                    int o = Int32.Parse(l.Split(" ").Last());

                    if (l.StartsWith("cut "))
                    {
                        b += deckSize - o;
                    }
                    else if (l.StartsWith("deal with increment "))
                    {
                        a *= o;
                        b *= o;
                    }
                }
            }

            // not strictly required, but helps to keep numbers down
            return (BringValueToPositiveRange(a, deckSize), BringValueToPositiveRange(b, deckSize));
        }
        
        static (BigInteger a, BigInteger b) ParseInverse(string filename, BigInteger deckSize)
        {
            BigInteger a = new BigInteger(1);
            BigInteger b = new BigInteger(0);

            foreach (var l in File.ReadAllLines(filename).Reverse())
            {
                if (l == "deal into new stack")
                {
                    b += 1;
                    a *= -1;
                    b *= -1;
                }
                else
                {
                    int o = Int32.Parse(l.Split(" ").Last());

                    if (l.StartsWith("cut "))
                    {
                        b += o;
                    }
                    else if (l.StartsWith("deal with increment "))
                    {
                        var o2 = BigInteger.ModPow(o, deckSize - 2, deckSize);
                        a *= o2;
                        b *= o2;
                    }
                }
            }

            // not strictly required, but helps to keep numbers down
            return (BringValueToPositiveRange(a, deckSize), BringValueToPositiveRange(b, deckSize));
        }


        static void RefPart1(string filename, string refStr)
        {
            var deckSize = 10;
            var (a, b) = Parse(filename, deckSize);
            var deck = new int[deckSize];
            for (int i = 0; i < deckSize; ++i)
                deck[(int)CalculateDiophantine(a, i, b, deckSize)] = i;
            var deckStr = string.Join(" ", deck);
            Console.WriteLine(
                $"{filename} = {deckStr} = {refStr} ({(deckStr == refStr ? "ok" : "not ok")})");
        }

        static int Part1(int card, int deckSize)
        {
            var (a, b) = Parse("input1.txt", deckSize);
            return (int)CalculateDiophantine(a, card, b, deckSize);
        }

        // repeats a*x + b ≡ c (mod deckSize) amount of times
        // meaning nesting one into another, as in:
        // a * (a*x+b) + b ≡ c (mod deckSize)
        // a^2*x + a*b + b ≡ c (mod deckSize)
        // so
        // a^times*x + a^times-1*b + a^times-2*b + ... + b ≡ c (mod deckSize)
        static (BigInteger a, BigInteger b) RepeatConjectureTimes(BigInteger a, BigInteger b, BigInteger times, BigInteger deckSize)
        {
            var a2 = BigInteger.ModPow(a, times, deckSize);
            //var b2 = b * (a ^ times - 1) / (a - 1);
            var b2 = b * (BigInteger.ModPow(a, times, deckSize) + deckSize - 1) * BigInteger.ModPow(a - 1, deckSize - 2, deckSize);
            // not strictly required, but helps to keep numbers down
            return (BringValueToPositiveRange(a2, deckSize), BringValueToPositiveRange(b2, deckSize));
        }

        static BigInteger Part2(BigInteger card, BigInteger deckSize, BigInteger times)
        {
            var (a, b) = ParseInverse("input1.txt", deckSize);
            (a, b) = RepeatConjectureTimes(a, b, times, deckSize);
            return CalculateDiophantine(a, card, b, deckSize);
        }

        // calculate c given
        // a * x + b ≡ c (mod deckSize)
        // which means calculating simple linear diophantine
        // a * x + b = c - deckSize * y 
        static BigInteger CalculateDiophantine(BigInteger a, BigInteger x, BigInteger b, BigInteger deckSize)
        {
            return BringValueToPositiveRange(a * x + b, deckSize);
        }
        
        // brings value to [0, end)
        static BigInteger BringValueToPositiveRange(BigInteger value, BigInteger end)
        {
            return (value % end + end) % end;
        }

        static void Main(string[] args)
        {
            RefPart1("ref1.txt", "0 3 6 9 2 5 8 1 4 7");
            RefPart1("ref2.txt", "3 0 7 4 1 8 5 2 9 6");
            RefPart1("ref3.txt", "6 3 0 7 4 1 8 5 2 9");
            RefPart1("ref4.txt", "9 2 5 8 1 4 7 0 3 6");
            
            Console.WriteLine($"part1 = {Part1(2019, 10007)} = 6129");
            Console.WriteLine($"part2 = {Part2(2020, 119315717514047, 101741582076661)} = 71345377301237");
        }
    }
}