using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;

namespace day07
{
    [Flags]
    public enum Flags
    {
        Halt = 0b_0001,
        Error = 0b_0010,

        HaltWithError = Halt | Error
    };

    class CPU
    {
        private int[] ram;
        private int[] inputs = new int[0];
        private int[] outputs = new int[0];
        private int ip = 0;

        private Flags flags = 0;
        public bool HaltedWithNoErrors => ((flags & Flags.Halt) == Flags.Halt) && ((flags & Flags.Error) == 0);
        public bool Running => (flags & Flags.Halt) == 0;

        public CPU(int[] rom, int[] setInputs)
        {
            ram = (int[]) rom.Clone();
            inputs = (int[]) setInputs.Clone();
        }

        public void PushInput(int input)
        {
            inputs = inputs.Append(input).ToArray();
        }

        private int? PopInput()
        {
            if (inputs.Length == 0)
                return null;
            var input = inputs.First();
            inputs = inputs.Skip(1).ToArray();
            return input;
        }

        private void PushOutput(int output)
        {
            outputs = outputs.Append(output).ToArray();
        }

        public int? PopOutout()
        {
            if (outputs.Length == 0)
                return null;
            var output = outputs.First();
            outputs = outputs.Skip(1).ToArray();
            return output;
        }

        public void Run()
        {
            while (Running)
            {
                if (ip >= ram.Length)
                {
                    flags |= Flags.HaltWithError;
                    return;
                }

                // decoder
                int[] opcode_raw = ram[ip].ToString().Reverse().Select(x => Int32.Parse(x.ToString())).ToArray();
                int opcode = (opcode_raw.Length >= 2 ? opcode_raw[1] * 10 : 0) + opcode_raw[0];
                int mode1 = (opcode_raw.Length >= 3 ? opcode_raw[2] : 0);
                int mode2 = (opcode_raw.Length >= 4 ? opcode_raw[3] : 0);
                int mode3 = (opcode_raw.Length >= 5 ? opcode_raw[4] : 0);

                switch (opcode)
                {
                    case 1:
                        WriteParam(ip + 3, ReadParam(ip + 1, mode1) + ReadParam(ip + 2, mode2));
                        ip += 4;
                        break;
                    case 2:
                        WriteParam(ip + 3, ReadParam(ip + 1, mode1) * ReadParam(ip + 2, mode2));
                        ip += 4;
                        break;
                    case 3:
                        var input = PopInput();
                        if (input.HasValue)
                        {
                            WriteParam(ip + 1, input.Value);
                            ip += 2;
                            break;
                        }
                        else
                            return;
                    case 4:
                        PushOutput(ReadParam(ip + 1, mode1));
                        ip += 2;
                        break;
                    case 5:
                        if (ReadParam(ip + 1, mode1) != 0)
                            ip = ReadParam(ip + 2, mode2);
                        else
                            ip += 3;
                        break;
                    case 6:
                        if (ReadParam(ip + 1, mode1) == 0)
                            ip = ReadParam(ip + 2, mode2);
                        else
                            ip += 3;
                        break;
                    case 7:
                        WriteParam(ip + 3, ReadParam(ip + 1, mode1) < ReadParam(ip + 2, mode2) ? 1 : 0);
                        ip += 4;
                        break;
                    case 8:
                        WriteParam(ip + 3, ReadParam(ip + 1, mode1) == ReadParam(ip + 2, mode2) ? 1 : 0);
                        ip += 4;
                        break;
                    case 99:
                        flags |= Flags.Halt;
                        break;
                    default:
                        flags |= Flags.HaltWithError;
                        return;
                }
            }
        }

        private int ReadParam(int idx, int mode)
        {
            if (idx >= 0 && (mode == 0 || mode == 1))
                return mode == 0 ? ram[ram[idx]] : ram[idx];
            flags |= Flags.HaltWithError;
            return 0;
        }

        private void WriteParam(int idx, int param)
        {
            if (idx >= 0)
                ram[ram[idx]] = param;
            else
                flags |= Flags.HaltWithError;
        }
    };

    class Program
    {
        static int AmpRun(int[] rom, int[] phaseSetting)
        {
            var array = phaseSetting.Select(x => new CPU(rom, new int[] {x})).ToArray();
            array[0].PushInput(0);

            while (array.Where(amp => amp.Running).Any())
            {
                for (int i = 0; i < array.Length; ++i)
                {
                    if (array[i].Running)
                    {
                        var io = array[i > 0 ? i - 1 : array.Length - 1].PopOutout();
                        if (io.HasValue)
                            array[i].PushInput(io.Value);
                        array[i].Run();
                    }
                }
            }
            foreach (var cpu in array)
                Debug.Assert(cpu.HaltedWithNoErrors);
            return array.Last().PopOutout().Value;
        }

        // https://stackoverflow.com/questions/16265247/printing-all-contents-of-array-in-c-sharp
        static IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> list, int length)
        {
            if (length == 1) return list.Select(t => new T[] {t});

            return GetPermutations(list, length - 1)
                .SelectMany(t => list.Where(e => !t.Contains(e)),
                    (t1, t2) => t1.Concat(new T[] {t2}));
        }

        static int FindMaxSignal(int[] rom, int[] possibleValues)
        {
            int? max = 0;
            foreach (var phaseSetting in GetPermutations(possibleValues, possibleValues.Length))
            {
                int current = AmpRun(rom, phaseSetting.ToArray());
                if (!max.HasValue || max.Value < current)
                    max = current;
            }

            return max.Value;
        }

        static int[] LoadRom(string filename)
        {
            return File.ReadAllLines(filename).SelectMany(s => s.Split(',')).Select(l => Int32.Parse(l)).ToArray();
        }

        static void Main(string[] args)
        {
            Console.WriteLine($"ref1 = {AmpRun(LoadRom("ref1.txt"), new int[] {4, 3, 2, 1, 0})} = 43210");
            Console.WriteLine($"ref2 = {AmpRun(LoadRom("ref2.txt"), new int[] {0, 1, 2, 3, 4})} = 54321");
            Console.WriteLine($"ref3 = {AmpRun(LoadRom("ref3.txt"), new int[] {1, 0, 4, 3, 2})} = 65210");

            Console.WriteLine($"part1 = {FindMaxSignal(LoadRom("input1.txt"), new int[] {0, 1, 2, 3, 4})} = 338603");

            Console.WriteLine($"ref4 = {AmpRun(LoadRom("ref4.txt"), new int[] {9,8,7,6,5})} = 139629729");
            Console.WriteLine($"ref5 = {AmpRun(LoadRom("ref5.txt"), new int[] {9,7,8,5,6})} = 18216");

            Console.WriteLine($"part2 = {FindMaxSignal(LoadRom("input1.txt"), new int[] {5, 6, 7, 8, 9})} = 63103596");
        }
    }
}