using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace day22
{
    class Program
    {
        // convert operations to a*x + b ≡ y (mod w)
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

                a %= deckSize;
                b %= deckSize;
            }

            return (a, b);
            
        }
        static int[] ShuffleDeck(int deckSize, BigInteger a, BigInteger b)
        {
            var deck = new int[deckSize];
            for (int i = 0; i < deckSize; ++i)
                deck[(int)SolveDiophantine(a, i, b, deckSize)] = i;
            return deck;
        }

        static string DeckToString(int[] deck)
        {
            return string.Join(" ", deck);
        }

        static void RefPart1(string filename, string refStr)
        {
            var deckSize = 10;
            var (a, b) = Parse(filename, deckSize);
            var deck = ShuffleDeck(deckSize, a, b);
            var refDeck = refStr.Split(" ").Select(x => Int32.Parse(x)).ToArray();
            Console.WriteLine(
                $"{filename} = {DeckToString(deck)} = {refStr} ({(deck.SequenceEqual(refDeck) ? "ok" : "not ok")})");
        }

        static int Part1()
        {
            var deckSize = 10007;
            var (a, b) = Parse("input1.txt", deckSize);
            return (int)SolveDiophantine(a, 2019, b, deckSize);
        }

        // a*x + b ≡ y (mod w)
        // given a, x, b, w, find smallest positive y
        static BigInteger SolveDiophantine(BigInteger a, BigInteger x, BigInteger b, BigInteger w)
        {
            int k = 0;

            // lol bruteforce
            while (true)
            {
                var y = a * x + b + w * k;

                if (y < 0)
                    k++;
                else if (y >= w)
                    k--;
                else
                    return y;
            }
        }

        static void Main(string[] args)
        {
            RefPart1("ref1.txt", "0 3 6 9 2 5 8 1 4 7");
            RefPart1("ref2.txt", "3 0 7 4 1 8 5 2 9 6");
            RefPart1("ref3.txt", "6 3 0 7 4 1 8 5 2 9");
            RefPart1("ref4.txt", "9 2 5 8 1 4 7 0 3 6");

            Console.WriteLine($"part1 = {Part1()} = 6129");



            // part2
            // 119315717514047 cards
            // 101741582076661 times
            // 2020 card value
        }
    }
}