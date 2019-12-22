using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace day22
{
    enum OpType
    {
        Cut,
        Deal,
        Inverse
    };

    class Op
    {
        public OpType type;
        public int offset;

        public Op(OpType setType, int setOffset = 0)
        {
            type = setType;
            offset = setOffset;
        }
    };

    class Program
    {
        static IEnumerable<Op> ParseEnum(string filename)
        {
            foreach (var l in File.ReadAllLines(filename))
            {
                if (l == "deal into new stack")
                    yield return new Op(OpType.Inverse);
                else
                {
                    int o = Int32.Parse(l.Split(" ").Last());
                    if (l.StartsWith("cut "))
                        yield return new Op(OpType.Cut, o);
                    else if (l.StartsWith("deal with increment "))
                        yield return new Op(OpType.Deal, o);
                }
            }
        }

        static Op[] Parse(string filename)
        {
            return ParseEnum(filename).ToArray();
        }

        static int[] ShuffleDeck(int[] deck, Op[] ops)
        {
            foreach (var op in ops)
            {
                switch (op.type)
                {
                    case OpType.Cut:
                        if (op.offset > 0)
                            deck = deck.Skip(op.offset).Concat(deck.Take(op.offset)).ToArray();
                        else
                            deck = deck.TakeLast(-op.offset).Concat(deck.SkipLast(-op.offset)).ToArray();
                        break;
                    case OpType.Deal:
                        var t = new int[deck.Length];
                        for (int i = 0; i < deck.Length; ++i)
                            t[i] = Int32.MinValue;
                        for (int i = 0; i < deck.Length; ++i)
                        {
                            var j = (i * op.offset) % deck.Length;
                            if (t[j] != Int32.MinValue)
                                throw new ArgumentException("hm?");
                            t[j] = deck[i];
                        }

                        deck = t;
                        break;
                    case OpType.Inverse:
                        deck = deck.Reverse().ToArray();
                        break;
                }
            }

            return deck;
        }

        static int[] Deck(int count)
        {
            return Enumerable.Range(0, count).ToArray();
        }

        static string DeckToString(int[] deck)
        {
            return string.Join(" ", deck);
        }

        static void RefPart1(string filename, string refStr)
        {
            Console.WriteLine(
                $"{filename} = {DeckToString(ShuffleDeck(Deck(10), Parse(filename)))} = {refStr}");
        }

        static int Part1()
        {
            var deck = ShuffleDeck(Deck(10007), Parse("input1.txt"));
            for(int i = 0; i < deck.Length; ++i)
                if (deck[i] == 2019)
                    return i;
            return Int32.MinValue;
        }

        static void Main(string[] args)
        {
            RefPart1("ref1.txt", "0 3 6 9 2 5 8 1 4 7");
            RefPart1("ref2.txt", "3 0 7 4 1 8 5 2 9 6");
            RefPart1("ref3.txt", "6 3 0 7 4 1 8 5 2 9");
            RefPart1("ref4.txt", "9 2 5 8 1 4 7 0 3 6");
            
            Console.WriteLine($"part1 = {Part1()} = 6129");
        }
    }
}