using System;
using System.IO;
using System.Linq;

namespace day10
{
    class Program
    {
        // point x0,y0, line x1,y1,x2,y2
        static double PointToLineDist(double x0, double y0, double x1, double y1, double x2, double y2)
        {
            return Math.Abs((y2 - y1) * x0 - (x2 - x1) * y0 + x2 * y1 - y2 * x1) /
                   Math.Sqrt((y2 - y1) * (y2 - y1) + (x2 - x1) * (x2 - x1));
        }
        static int CountVisible(char[][] field, int ax, int ay, int w, int h, double deltaAngle, double maxRadius)
        {
            int count = 0;
            
            for (int by = 0; by < h; ++by)
            {
                for (int bx = 0; bx < w; ++bx)
                {
                    if (bx == ax && by == ay)
                        continue;
                    
                    if (field[by][bx] != '#')
                        continue;
                    
                    double dist = Math.Sqrt((bx - ax) * (bx - ax) + (by - ay) * (by - ay));
                    bool notOk = false;
                    for (double t = 0.0; t <= 1.0f; t += 1.0f / dist)
                    {
                        var pxf = (double) (bx - ax) * t + (double) ax;
                        var pyf = (double) (by - ay) * t + (double) ay;

                        var px = (int) Math.Round(pxf);
                        var py = (int) Math.Round(pyf);

                        if ((ax == px && ay == py) || (bx == px && by == py))
                            continue;

                        if (field[py][px] != '#')
                            continue;

                        if (PointToLineDist(px, py, ax, ay, bx, by) < 0.01)
                        {
                            notOk = true;
                            break;
                        }
                    }

                    if (!notOk)
                    {
                        count++;
                    }
                }
            }
            
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
//                        Console.Write($"{temp}");
                    
                        if (temp > max)
                            max = temp;
                    }
//                    else
//                        Console.Write(".");
                }
//                Console.Write("\n");
            }
            return max;
        }
        
        static void Main(string[] args)
        {
            Console.WriteLine($"ref1 = {FindBestPoint("ref1.txt")} = 8");
            Console.WriteLine($"ref2 = {FindBestPoint("ref2.txt")} = 33");
            Console.WriteLine($"ref3 = {FindBestPoint("ref3.txt")} = 35");
            Console.WriteLine($"ref4 = {FindBestPoint("ref4.txt")} = 41");
            Console.WriteLine($"ref5 = {FindBestPoint("ref5.txt")} = 210");
            Console.WriteLine($"input1 = {FindBestPoint("input1.txt")} = 282");
        }
    }
}