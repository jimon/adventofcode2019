using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace day13
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
        static void Part1()
        {
            var rom = CPU.LoadRom("input1.txt");
            var cpu = new CPU(rom, new Int64[0]);
            cpu.Run();
            if (!cpu.HaltedWithNoErrors)
                Console.WriteLine("cpu haven't halted");
            var map = new Dictionary<(int x, int y), int>();
            while (true)
            {
                var wx = cpu.PopOutout();
                var wy = cpu.PopOutout();
                var wtype = cpu.PopOutout();

                if (!wx.HasValue || !wy.HasValue || !wtype.HasValue)
                    break;

                var x = (int)wx.Value;
                var y = (int)wy.Value;
                var type = (int)wtype.Value;
                map[(x: x, y: y)] = type;
            }
            Console.WriteLine($"x = [{map.Select(x => x.Key.x).Min()}, {map.Select(x => x.Key.x).Max()}]");
            Console.WriteLine($"y = [{map.Select(x => x.Key.y).Min()}, {map.Select(x => x.Key.y).Max()}]");
            Console.WriteLine($"part1 = {map.Where(p => p.Value == 2).Count()} = 260");
        }

        static void Part2()
        {
            var rom = CPU.LoadRom("input1.txt");
            rom[0] = 2; // playing for free :)
            
            var cpu = new CPU(rom, new Int64[0]);

            var w = 35;
            var h = 23;
            var map = new int[w, h];

            var bx = 0;
            var px = 0;
            var score = 0;

            while (true)
            {
                cpu.Run();
                
                while (true)
                {
                    var wx = cpu.PopOutout();
                    var wy = cpu.PopOutout();
                    var wtype = cpu.PopOutout();

                    if (!wx.HasValue || !wy.HasValue || !wtype.HasValue)
                        break;

                    var x = (int)wx.Value;
                    var y = (int)wy.Value;
                    var type = (int)wtype.Value;

                    if (x == -1 && y == 0)
                    {
                        Console.SetCursorPosition(0, h + 1);
                        Console.WriteLine($"dt {type - score}");
                        score = type;
                        
                        Console.SetCursorPosition(0, 0);
                        Console.WriteLine($"score = {score}");
                    }
                    else
                    {
                        map[x, y] = type;

                        Console.SetCursorPosition(x, y + 1);
                        char s = ' ';
                        switch (type)
                        {
                            case 1:
                                s = '#';
                                break;
                            case 2:
                                s = 'O';
                                break;
                            case 3:
                                s = '_';
                                px = x;
                                break;
                            case 4:
                                s = '*';
                                bx = x;
                                break;
                        }

                        Console.Write(s);
                    }
                }

                bool anyBlocks = false;
                for (int y = 0; y < h; ++y)
                {
                    for (int x = 0; x < w; ++x)
                    {
                        if (map[x, y] == 2)
                        {
                            anyBlocks = true;
                            break;
                        }
                    }

                    if (anyBlocks)
                        break;
                }

                if (!anyBlocks || cpu.HaltedWithNoErrors)
                    break;

                if (bx < px)
                    cpu.PushInput(-1);
                else if (bx > px)
                    cpu.PushInput(1);
                else
                    cpu.PushInput(0);

//                var key = Console.ReadKey().Key;
//                
//                if (key == ConsoleKey.LeftArrow)
//                    cpu.PushInput(-1);
//                else if (key == ConsoleKey.RightArrow)
//                    cpu.PushInput(1);
//                else if (key == ConsoleKey.Q)
//                    break;
//                else
//                    cpu.PushInput(0);
            }
            
            Console.SetCursorPosition(0, h + 1);
            Console.WriteLine($"part2 = {score} = 12952");
        }

        static void Main(string[] args)
        {
            //Part1();
            Part2();
        }
    }
}