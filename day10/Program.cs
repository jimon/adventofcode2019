using System;
using System.IO;
using System.Linq;

namespace day10
{
    class Program
    {
        static int CountVisible(char[][] field, int ax, int ay, int w, int h, double deltaAngle, double maxRadius)
        {
            int count = 0;
            //var visited = new bool[w, h];
            
            for (int by = 0; by < h; ++by)
            {
                for (int bx = 0; bx < w; ++bx)
                {
                    if (field[by][bx] != '#')
                        continue;
                    
                    double dist = Math.Sqrt((bx - ax) * (bx - ax) + (by - ay) * (by - ay));
                    bool notOk = false;
                    for (double t = 0.0; t <= 1.0f; t += 1.0f / dist)
                    {
                        var pxf = (double) (bx - ax) * t + (double) ax;
                        var pyf = (double) (by - ay) * t + (double) ay;

                        //var px = (int) Math.Round(pxf);
                        //var py = (int) Math.Round(pyf);

                        //if ((ax == pxf && ay == pyf) || (bx == pxf && by == pyf))
                        //    continue;

//                        if (field[py][px] == '#')
//                        {
//                            notOk = true;
//                            break;
//                        }
                    }

                    if (!notOk)
                    {
                        count++;
                    }
                }
            }


//            var visited = new bool[w, h];
//            
//            for (double angle = 0; angle < 2.0 * Math.PI; angle += Math.PI / 1000.0f)
//            {
//                for (double radius = 0.0; radius < maxRadius; radius += 0.1)
//                {
//                    int x = (int) Math.Round(Math.Sin(angle) * radius + (double) startX);
//                    int y = (int) Math.Round(Math.Cos(angle) * radius + (double) startY);
//
//                    if (x < 0 || y < 0 || x >= w || y >= h)
//                        break;
//
//                    if (x == startX && y == startY)
//                        continue;
//
//                    if (field[y][x] == '#')
//                    {
//                        if (visited[x, y] == false)
//                        {
//                            count++;
//                            visited[x, y] = true;
//                        }
//                        break;
//                    }
//                }
//            }
            return count;
        }

        static int FindBestPoint(string filename)
        {
            var field = File.ReadAllLines(filename).Select(x => x.ToArray()).ToArray();
            int max = 0;
            int w = field[0].Length;
            int h = field.Length;
            double deltaAngle = Math.Min(Math.Atan2(1.0, (double)w), Math.Atan2((double)h, 1.0));
            double maxRadius = Math.Sqrt((double) (w * w + h * h));
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    if (field[y][x] == '#')
                    {
                        var temp = CountVisible(field, x, y, w, h, deltaAngle, maxRadius);
                        Console.Write($"{temp}");
                    
                        if (temp > max)
                            max = temp;
                    }
                    else
                        Console.Write(".");
                }
                Console.Write("\n");
            }
            return max;
        }
        
        static void Main(string[] args)
        {

            Console.WriteLine(".7..7");
            Console.WriteLine(".....");
            Console.WriteLine("67775");
            Console.WriteLine("....7");
            Console.WriteLine("...87");
            Console.WriteLine($"ref1 = {FindBestPoint("ref1.txt")} = 8");
//            Console.WriteLine($"ref2 = {FindBestPoint("ref2.txt")} = 33");
//            Console.WriteLine($"ref3 = {FindBestPoint("ref3.txt")} = 35");
//            Console.WriteLine($"ref4 = {FindBestPoint("ref4.txt")} = 41");
//            Console.WriteLine($"ref5 = {FindBestPoint("ref5.txt")} = 210");
        }
    }
}