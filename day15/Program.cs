using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dijkstra.NET.ShortestPath;

namespace day15
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

    class ExplorationPoint
    {
        public int x = 0;
        public int y = 0;
        public int dx = 0;
        public int dy = 0;
        public int cmd = 0;
        public Int64[] ram = new Int64[0];

        public ExplorationPoint(int setX, int setY, int setDx, int setDy, int setCmd, Int64[] setRam)
        {
            x = setX;
            y = setY;
            dx = setDx;
            dy = setDy;
            cmd = setCmd;
            ram = setRam;
        }
    };

    class Program
    {
        static IEnumerable<ExplorationPoint> ExplorationPointsFor(char[,] map, int x, int y, Int64[] ram)
        {
            if (map[x, y - 1] == 0)
                yield return new ExplorationPoint(x, y, 0, -1, 1, ram);
            if (map[x, y + 1] == 0)
                yield return new ExplorationPoint(x, y, 0, 1, 2, ram);
            if (map[x - 1, y] == 0)
                yield return new ExplorationPoint(x, y, -1, 0, 3, ram);
            if (map[x + 1, y] == 0)
                yield return new ExplorationPoint(x, y, 1, 0, 4, ram);
        }

        static (char[,] map, int w, int h, int sx, int sy, int ex, int ey) FindMap(Int64[] rom, int guessW, int guessH)
        {
            var cpu = new CPU(rom, new Int64[0]);

            int sx = guessW / 2;
            int sy = guessH / 2;

            int ex = -1;
            int ey = -1;

            var map = new char[guessW, guessH];
            map[sx, sy] = '.';

            var explore = new Queue<ExplorationPoint>();
            foreach (var explorationPoint in ExplorationPointsFor(map, sx, sy, cpu.DumpRam()))
                explore.Enqueue(explorationPoint);

            while (explore.Count > 0)
            {
                var explorationPoint = explore.Dequeue();

                cpu.RestoreRam(explorationPoint.ram);
                cpu.PushInput(explorationPoint.cmd);
                cpu.Run();
                var io = cpu.PopOutout();

                int x = explorationPoint.x;
                int y = explorationPoint.y;

                int dx = explorationPoint.dx;
                int dy = explorationPoint.dy;

                if (io == 0)
                {
                    map[x + dx, y + dy] = '#';
                }
                else if (io == 1 || io == 2)
                {
                    x += dx;
                    y += dy;
                    map[x, y] = '.';

                    if (io == 2)
                    {
                        ex = x;
                        ey = y;
                    }

                    foreach (var p in ExplorationPointsFor(map, x, y, cpu.DumpRam()))
                        explore.Enqueue(p);
                }
            }

            return (map: map, w: guessW, h: guessH, sx: sx, sy: sy, ex: ex, ey: ey);
        }

        static IEnumerable<(int x, int y)> AdjacentTo(int x, int y, int w, int h)
        {
            if (y > 0)
                yield return (x: x, y: y - 1);
            if (y < h - 1)
                yield return (x: x, y: y + 1);
            if (x > 0)
                yield return (x: x - 1, y: y);
            if (x < w - 1)
                yield return (x: x + 1, y: y);
        }

        static int FindShortestPath(char[,] map, int w, int h, int sx, int sy, int ex, int ey)
        {
            var graph = new Dijkstra.NET.Graph.Graph<(int x, int y), int>();

            var posToKey = new Dictionary<(int x, int y), uint>();

            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    if (map[x, y] == '.')
                    {
                        var pos = (x: x, y: y);
                        posToKey[pos] = graph.AddNode(pos);
                    }
                }
            }

            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    if (map[x, y] != '.')
                        continue;
                    
                    var pos = (x: x, y: y);
                    foreach (var adj in AdjacentTo(x, y, w, h))
                    {
                        if (map[adj.x, adj.y] == '.')
                            graph.Connect(posToKey[pos], posToKey[adj], 1, 0);
                    }
                }
            }

            return graph.Dijkstra(posToKey[(x: sx, y: sy)], posToKey[(x: ex, y: ey)]).Distance;
        }

        static int FindFills(char[,] mapRef, int w, int h, int ex, int ey)
        {
            char[,] map = (char[,]) mapRef.Clone();
            var todo = new HashSet<(int x, int y)>();
            todo.Add((x: ex, y: ey));
            map[ex, ey] = 'O';

            var adjDeltas = new[] {(dx: 0, dy: -1), (dx: 0, dy: 1), (dx: -1, dy: 0), (dx: 1, dy: 0)};

            int time = 0;
            while (true)
            {
                bool any = false;
                for (int i = 0; i < h; ++i)
                {
                    for (int j = 0; j < w; ++j)
                    {
                        if (map[j, i] == '.')
                        {
                            any = true;
                            break;
                        }
                    }

                    if (any)
                        break;
                }

                if (!any)
                    break;

                var newTodo = new HashSet<(int x, int y)>();
                foreach (var pos in todo)
                {
                    foreach (var adjDelta in adjDeltas)
                    {
                        int t = 0;
                        while (true)
                        {
                            var np = (x: pos.x + adjDelta.dx, y: pos.y + adjDelta.dy);
                            if (map[np.x, np.y] != '.')
                                break;
                            map[np.x, np.y] = 'O';
                            newTodo.Add(np);
                            t++;
                        }
                    }
                }
                todo = newTodo;
                time++;
            }

            return time;
        }

        static void Main(string[] args)
        {
            var rom = CPU.LoadRom("input1.txt");

            var (map, w, h, sx, sy, ex, ey) = FindMap(rom, 42, 42);
            Console.WriteLine($"{sx}, {sy} -> {ex}, {ey}");

            for (int i = 0; i < h; ++i)
            {
                for (int j = 0; j < w; ++j)
                {
                    Console.Write(map[j, i]);
                }

                Console.Write('\n');
            }

            Console.WriteLine($"part1 = {FindShortestPath(map, w, h, sx, sy, ex, ey)} = 380");
            Console.WriteLine($"part1 = {FindFills(map, w, h, ex, ey)} = 410");
        }
    }
}