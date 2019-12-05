using System;
using System.IO;
using System.Linq;

namespace day05
{
    class CPU
    {
        private int[] ram;
        private int[] inputs;
        private int ip = 0;
        private bool halt = false;
        private bool error = false;

        public CPU(int[] rom, int[] setInputs)
        {
            ram = (int[]) rom.Clone();
            inputs = (int[]) setInputs.Clone();
        }

        public bool Run()
        {
            while (!halt && !error)
            {
                if (ip >= ram.Length)
                    return false;

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
                        var input = inputs.First();
                        inputs = inputs.Skip(1).ToArray();
                        Console.WriteLine($"< {input}");
                        WriteParam(ip + 1, input);
                        ip += 2;
                        break;
                    case 4:
                        Console.WriteLine($"> {ReadParam(ip + 1, mode1)}");
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
                        halt = true;
                        break;
                    default:
                        return false;
                }
            }

            return halt && (error == false);
        }

        private int ReadParam(int idx, int mode)
        {
            if (idx < 0)
            {
                error = true;
                return 0;
            }
            
            switch (mode)
            {
                case 0:
                    return ram[ram[idx]];
                case 1:
                    return ram[idx];
                default:
                    error = true;
                    return 0;
            }
        }

        private void WriteParam(int idx, int param)
        {
            if (idx < 0)
            {
                error = true;
                return;
            }
            ram[ram[idx]] = param;
        }
    };
    
    class Program
    {
        static void Main(string[] args)
        {
            var rom = File.ReadAllLines("input1.txt").SelectMany(s => s.Split(',')).Select(l => Int32.Parse(l)).ToArray();
            Console.WriteLine($"part1 ok = {new CPU(rom, new int[] {1}).Run()}");
            Console.WriteLine($"part2 ok = {new CPU(rom, new int[] {5}).Run()}");
        }
    }
}