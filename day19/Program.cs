using System;
using System.IO;
using System.Linq;

namespace day19
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
        public static Int64[] LoadRom(string filename)
        {
            return File.ReadAllLines(filename).SelectMany(s => s.Split(',')).Select(l => Int64.Parse(l)).ToArray();
        }

        private Int64[] ram;
        private Int64[] inputs = new Int64[0];
        private Int64[] outputs = new Int64[0];
        private Int64 ip = 0;
        private Int64 rb = 0;

        private Flags flags = 0;
        public bool HaltedWithNoErrors => ((flags & Flags.Halt) == Flags.Halt) && ((flags & Flags.Error) == 0);
        public bool Running => (flags & Flags.Halt) == 0;

        public CPU(Int64[] rom, Int64[] setInputs, int totalRamSize = 16 * 1024)
        {
            ram = (Int64[]) rom.Clone();
            ram = ram.Concat(new Int64[totalRamSize - rom.Length]).ToArray();
            inputs = (Int64[]) setInputs.Clone();
        }

        public void PushInput(Int64 input)
        {
            inputs = inputs.Append(input).ToArray();
        }

        private Int64? PopInput()
        {
            if (inputs.Length == 0)
                return null;
            var input = inputs.First();
            inputs = inputs.Skip(1).ToArray();
            return input;
        }

        private void PushOutput(Int64 output)
        {
            outputs = outputs.Append(output).ToArray();
        }

        public Int64? PopOutout()
        {
            if (outputs.Length == 0)
                return null;
            var output = outputs.First();
            outputs = outputs.Skip(1).ToArray();
            return output;
        }

        public Int64[] DumpRam()
        {
            return (Int64[]) ram.Clone();
        }

        public void RestoreRam(Int64[] snapshot)
        {
            ram = (Int64[]) snapshot.Clone();
        }

        public void Reset()
        {
            ip = 0;
            rb = 0;
            flags = 0;
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
                Int64[] opcode_raw = ram[ip].ToString().Reverse().Select(x => Int64.Parse(x.ToString())).ToArray();
                Int64 opcode = (opcode_raw.Length >= 2 ? opcode_raw[1] * 10 : 0) + opcode_raw[0];
                Int64 mode1 = (opcode_raw.Length >= 3 ? opcode_raw[2] : 0);
                Int64 mode2 = (opcode_raw.Length >= 4 ? opcode_raw[3] : 0);
                Int64 mode3 = (opcode_raw.Length >= 5 ? opcode_raw[4] : 0);

                switch (opcode)
                {
                    case 1:
                        WriteParam(ip + 3, ReadParam(ip + 1, mode1) + ReadParam(ip + 2, mode2), mode3);
                        ip += 4;
                        break;
                    case 2:
                        WriteParam(ip + 3, ReadParam(ip + 1, mode1) * ReadParam(ip + 2, mode2), mode3);
                        ip += 4;
                        break;
                    case 3:
                        var input = PopInput();
                        if (input.HasValue)
                        {
                            WriteParam(ip + 1, input.Value, mode1);
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
                        WriteParam(ip + 3, ReadParam(ip + 1, mode1) < ReadParam(ip + 2, mode2) ? 1 : 0, mode3);
                        ip += 4;
                        break;
                    case 8:
                        WriteParam(ip + 3, ReadParam(ip + 1, mode1) == ReadParam(ip + 2, mode2) ? 1 : 0, mode3);
                        ip += 4;
                        break;
                    case 9:
                        rb += ReadParam(ip + 1, mode1);
                        ip += 2;
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

        private Int64 ReadParam(Int64 idx, Int64 mode)
        {
            switch (mode)
            {
                case 0: return TryReadFrom(ram[idx]);
                case 1: return TryReadFrom(idx);
                case 2: return TryReadFrom(ram[idx] + rb);
                default:
                    flags |= Flags.HaltWithError;
                    return 0;
            }
        }

        private void WriteParam(Int64 idx, Int64 param, Int64 mode)
        {
            switch (mode)
            {
                case 0:
                    TryWriteTo(ram[idx], param);
                    break;
                case 2:
                    TryWriteTo(ram[idx] + rb, param);
                    break;
                default:
                    flags |= Flags.HaltWithError;
                    return;
            }
        }

        private Int64 TryReadFrom(Int64 idx)
        {
            if (idx >= 0 && idx < ram.Length)
                return ram[idx];
            flags |= Flags.HaltWithError;
            return 0;
        }

        private void TryWriteTo(Int64 idx, Int64 value)
        {
            if (idx >= 0 && idx < ram.Length)
                ram[idx] = value;
            else
                flags |= Flags.HaltWithError;
        }
    };
    
    class Program
    {
        static int Part1(CPU cpu, Int64[] ramDump, int w = 50, int h = 50)
        {
            int count = 0;
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    cpu.RestoreRam(ramDump);
                    cpu.Reset();
                    cpu.PushInput(x);
                    cpu.PushInput(y);
                    cpu.Run();
                    switch (cpu.PopOutout().Value)
                    {
                        case 0:
                            //Console.Write('.');
                            break;
                        case 1:
                            //Console.Write('#');
                            count++;
                            break;
                    }
                }
                //Console.Write('\n');
            }
            return count;
        }

        static int FindEdge(CPU cpu, Int64[] ramDump, int fromX, int toX, int y)
        {
            int dx = (fromX < toX ? 1 : -1);
            for (int x = fromX; x != toX + dx; x += dx)
            {
                cpu.RestoreRam(ramDump);
                cpu.Reset();
                cpu.PushInput(x);
                cpu.PushInput(y);
                cpu.Run();
                switch (cpu.PopOutout().Value)
                {
                    case 0:
                        break;
                    case 1:
                        return x;
                }
            }

            return -1;
        }

        static int Part2(CPU cpu, Int64[] ramDump, int sx = 6, int sy = 5)
        {
            int xl = sx;
            int xr = sx;
            int y = sy;
            
            var prev = new (int xl, int xr)[10000];
            
            while (true)
            {
                y++;
                xl = FindEdge(cpu, ramDump, xl - 4, xl + 4, y);
                xr = FindEdge(cpu, ramDump, xr + 4, xr - 4, y);

                prev[y] = (xl: xl, xr: xr);

                if (y >= sy + 100)
                {
                    var p = prev[y - 100 + 1];

                    if (xl >= p.xl && xl + 99 <= p.xr)
                    {
                        return xl * 10000 + (y - 100 + 1);
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            var rom = CPU.LoadRom("input1.txt");
            var cpu = new CPU(rom, new long[0], 1024);
            var ramDump = cpu.DumpRam();

            Console.WriteLine($"part1 = {Part1(cpu, ramDump)} = 118");
            Console.WriteLine($"part2 = {Part2(cpu, ramDump)} = 18651593");
        }
    }
}