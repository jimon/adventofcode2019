using System;
using System.IO;
using System.Linq;

namespace day11
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

    enum RobotDirection
    {
        Left,
        Up,
        Right,
        Down
    };

    class Program
    {
        static void Main(string[] args)
        {
            var rom = CPU.LoadRom("input1.txt");
            var cpu = new CPU(rom, new Int64[0] { });
            var direction = RobotDirection.Up;

            bool part2 = true;

            int w = part2 ? 100 : 200;
            int h = part2 ? 20 : 200;
            int x = w / 2;
            int y = h / 2;
            var map = new int[w, h];
            int count = 0;

            if (part2)
                map[x, y] = 2;

            while (!cpu.HaltedWithNoErrors)
            {
                if (x < 0 || y < 0 || x >= w || y >= h)
                {
                    Console.WriteLine($"pos out of bounds {x}, {y}");
                    break;
                }

                cpu.PushInput(map[x, y] == 2 ? 1 : 0);
                cpu.Run();
                var newColor = cpu.PopOutout();
                var turn = cpu.PopOutout();
                if (newColor.HasValue && turn.HasValue)
                {
                    if (map[x, y] == 0)
                        count++;
                    map[x, y] = (int) newColor.Value == 0 ? 1 : 2;

                    if (turn.Value == 0)
                    {
                        switch (direction)
                        {
                            case RobotDirection.Left:
                                direction = RobotDirection.Down;
                                break;
                            case RobotDirection.Up:
                                direction = RobotDirection.Left;
                                break;
                            case RobotDirection.Right:
                                direction = RobotDirection.Up;
                                break;
                            case RobotDirection.Down:
                                direction = RobotDirection.Right;
                                break;
                        }
                    }
                    else
                    {
                        switch (direction)
                        {
                            case RobotDirection.Left:
                                direction = RobotDirection.Up;
                                break;
                            case RobotDirection.Up:
                                direction = RobotDirection.Right;
                                break;
                            case RobotDirection.Right:
                                direction = RobotDirection.Down;
                                break;
                            case RobotDirection.Down:
                                direction = RobotDirection.Left;
                                break;
                        }
                    }

                    switch (direction)
                    {
                        case RobotDirection.Left:
                            x -= 1;
                            break;
                        case RobotDirection.Up:
                            y -= 1;
                            break;
                        case RobotDirection.Right:
                            x += 1;
                            break;
                        case RobotDirection.Down:
                            y += 1;
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("I/O error");
                    break;
                }
            }

            if (!part2)
                Console.WriteLine($"part1 = {count} = 1967");
            else
            {
                Console.WriteLine("part2: ");
                for (int yd = 0; yd < h; ++yd)
                {
                    for (int xd = 0; xd < w; ++xd)
                        Console.Write((map[xd, yd] == 2) ? "*" : " ");
                    Console.Write("\n");
                }
            }
        }
    }
}