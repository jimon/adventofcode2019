using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace day17
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
        enum Direction
        {
            Left,
            Up,
            Right,
            Down
        };

        static (int x, int y) Move(int x, int y, Direction direction)
        {
            switch (direction)
            {
                case Direction.Left:
                    return (x: x - 1, y: y);
                case Direction.Up:
                    return (x: x, y: y - 1);
                case Direction.Right:
                    return (x: x + 1, y: y);
                case Direction.Down:
                    return (x: x, y: y + 1);
                default:
                    throw new ArgumentException($"invalid {direction}");
            }
        }

        static int Part1(char[,] map, int w, int h, int sx, int sy, Direction direction)
        {
            int sumcount = 0;

            var visits = new int[w, h];
            int cx = sx;
            int cy = sy;

            var run = true;
            while (run)
            {
                if (cx < 0 || cy < 0 || cx >= w || cy >= h)
                    break;

                // count intersections
                visits[cx, cy]++;
                if (visits[cx, cy] == 2)
                    sumcount += cx * cy;

                var (nx, ny) = Move(cx, cy, direction);
                // if for some reason next cell is not valid
                if (nx < 0 || ny < 0 || nx >= w || ny >= h || map[nx, ny] != '#')
                {
                    // try steer the robot

                    if (cx - 1 >= 0 && direction != Direction.Right && map[cx - 1, cy] == '#')
                        direction = Direction.Left;
                    else if (cx + 1 < w && direction != Direction.Left && map[cx + 1, cy] == '#')
                        direction = Direction.Right;
                    else if (cy - 1 >= 0 && direction != Direction.Down && map[cx, cy - 1] == '#')
                        direction = Direction.Up;
                    else if (cy + 1 < h && direction != Direction.Up && map[cx, cy + 1] == '#')
                        direction = Direction.Down;
                    else
                    {
                        run = false;
                        break;
                    }

                    (cx, cy) = Move(cx, cy, direction);
                    
                    //Console.WriteLine($"steering {direction} and new pos {cx}, {cy}");
                }
                else
                {
                    // move keeping same direction
                    cx = nx;
                    cy = ny;

                    //Console.WriteLine($"keeping {direction} and new pos {cx}, {cy}");
                }

                // for (int y = 0; y < h; ++y)
                // {
                //     for (int x = 0; x < w; ++x)
                //     {
                //         if (cx == x && cy == y)
                //         {
                //             switch (direction)
                //             {
                //                 case Direction.Left:
                //                     Console.Write('<');
                //                     break;
                //                 case Direction.Up:
                //                     Console.Write('^');
                //                     break;
                //                 case Direction.Right:
                //                     Console.Write('>');
                //                     break;
                //                 case Direction.Down:
                //                     Console.Write('v');
                //                     break;
                //             }
                //         }
                //         else
                //         {
                //             Console.Write(map[x, y]);
                //         }
                //     }
                //     Console.Write('\n');
                // }
            }

            return sumcount;
        }

        static (char[,] map, int w, int h, int sx, int sy, Direction direction) ParseMap(string[] map)
        {
            var w = map[0].Length;
            var h = map.Length;
            var r = new char[w, h];
            int sx = -1;
            int sy = -1;
            Direction direction = Direction.Up;
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    var v = map[y][x];

                    switch (v)
                    {
                        case '<':
                        case '^':
                        case '>':
                        case 'v':
                            sx = x;
                            sy = y;
                            r[x, y] = '#';
                            switch (v)
                            {
                                case '<':
                                    direction = Direction.Left;
                                    break;
                                case '^':
                                    direction = Direction.Up;
                                    break;
                                case '>':
                                    direction = Direction.Right;
                                    break;
                                case 'v':
                                    direction = Direction.Down;
                                    break;
                            }

                            break;
                        default:
                            r[x, y] = v;
                            break;
                    }
                }
            }

            return (map: r, w: w, h: h, sx: sx, sy: sy, direction: direction);
        }

        static IEnumerable<string> GetMapCharsFromInputProgram()
        {
            var rom = CPU.LoadRom("input1.txt");
            var cpu = new CPU(rom, new Int64[] { });
            cpu.Run();
            IEnumerable<char> r = new char[0]; 
            while (true)
            {
                var val = cpu.PopOutout();

                if (!val.HasValue || val.Value == (int) '\n')
                {
                    var s = string.Join("", r);
                    if (s.Length > 0)
                        yield return s;
                    r = new char[0];

                    if (!val.HasValue)
                        break;
                }
                else
                    r = r.Append((char) val.Value);
            }
        }
        
        static void Part2(char[,] map, int w, int h, int sx, int sy, Direction direction)
        {
            var prevDirection = direction;
            int cx = sx;
            int cy = sy;
            int step = 0;
            var steps = new List<string>();
            var run = true;
            while (run)
            {
                if (cx < 0 || cy < 0 || cx >= w || cy >= h)
                    break;

                var (nx, ny) = Move(cx, cy, direction);
                // if for some reason next cell is not valid
                if (nx < 0 || ny < 0 || nx >= w || ny >= h || map[nx, ny] != '#')
                {
                    // try steer the robot
                    //steps.Add((count: step, direction: direction));
                    
                    if (step > 0)
                        steps.Add($"{step}");
                    step = 0;
                    
                    if (cx - 1 >= 0 && direction != Direction.Right && map[cx - 1, cy] == '#')
                        direction = Direction.Left;
                    else if (cx + 1 < w && direction != Direction.Left && map[cx + 1, cy] == '#')
                        direction = Direction.Right;
                    else if (cy - 1 >= 0 && direction != Direction.Down && map[cx, cy - 1] == '#')
                        direction = Direction.Up;
                    else if (cy + 1 < h && direction != Direction.Up && map[cx, cy + 1] == '#')
                        direction = Direction.Down;
                    else
                    {
                        run = false;
                        break;
                    }

                    char? rotate = null;
                    switch (prevDirection)
                    {
                        case Direction.Left:
                        {
                            switch (direction)
                            {
                                case Direction.Up:
                                    rotate = 'R';
                                    break;
                                case Direction.Down:
                                    rotate = 'L';
                                    break;
                            }

                            break;
                        }
                        case Direction.Up:
                        {
                            switch (direction)
                            {
                                case Direction.Left:
                                    rotate = 'L';
                                    break;
                                case Direction.Right:
                                    rotate = 'R';
                                    break;
                            }

                            break;
                        }
                        case Direction.Right:
                        {
                            switch (direction)
                            {
                                case Direction.Up:
                                    rotate = 'L';
                                    break;
                                case Direction.Down:
                                    rotate = 'R';
                                    break;
                            }

                            break;
                        }
                        case Direction.Down:
                        {
                            switch (direction)
                            {
                                case Direction.Left:
                                    rotate = 'R';
                                    break;
                                case Direction.Right:
                                    rotate = 'L';
                                    break;
                            }
                            break;
                        }
                    }
                    
                    if (rotate.HasValue)
                        steps.Add($"{rotate}");
                    
                    prevDirection = direction; 

                    (cx, cy) = Move(cx, cy, direction);

                    //Console.WriteLine($"steering {direction} and new pos {cx}, {cy}");
                }
                else
                {
                    step++;
                    // move keeping same direction
                    cx = nx;
                    cy = ny;
                }
            }

            Console.WriteLine($"steps: {string.Join("", steps)}");
            
            // numbers here are off by 1
            // A R5L9R7R7
            // B R11L9R5L9
            // C R11L7L9
            //
            // A C A B C B A C A B
            //
            
            var rom = CPU.LoadRom("input1.txt");
            rom[0] = 2;
            var cpu = new CPU(rom, new Int64[] { });

            cpu.Run();
            
            while (true)
            {
                var val = cpu.PopOutout();
            
                if (!val.HasValue)
                    break;
                
                Console.Write((char)val.Value);
            }
            
            PushStringToCpu(cpu, "A,C,A,B,C,B,A,C,A,B");
            PushStringToCpu(cpu, "R,6,L,10,R,8,R,8");
            PushStringToCpu(cpu, "R,12,L,10,R,6,L,10");
            PushStringToCpu(cpu, "R,12,L,8,L,10");
            PushStringToCpu(cpu, "n");

            cpu.Run();
            
            while (true)
            {
                var val = cpu.PopOutout();
            
                if (!val.HasValue)
                    break;

                if (val.Value > 255)
                {
                    Console.WriteLine($"part2 = {val.Value} = 1168948");
                }
                else
                    Console.Write((char)val.Value);
            }
        }

        static void PushStringToCpu(CPU cpu, string str)
        {
            foreach (var c in str.Select(x => x))
            {
                cpu.PushInput((Int64)c);
            }
            cpu.PushInput(10);
        }

        static void Main(string[] args)
        {
            var (ref1map,
                ref1w,
                ref1h,
                ref1sx,
                ref1sy,
                ref1d) = ParseMap(new string[]
            {
                "..#..........",
                "..#..........",
                "#######...###",
                "#.#...#...#.#",
                "#############",
                "..#...#...#..",
                "..#####...^.."
            });

            Console.WriteLine($"ref1 = {Part1(ref1map, ref1w, ref1h, ref1sx, ref1sy, ref1d)} = 76");
            
            var (map, 
                w,
                h,
                sx,
                sy,
                d) = ParseMap(GetMapCharsFromInputProgram().ToArray());
            Console.WriteLine($"part1 = {Part1(map, w, h, sx, sy, d)} = 8408");
            Part2(map, w, h, sx, sy, d);
        }
    }
}