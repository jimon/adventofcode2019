using System;
using System.Collections.Generic;
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
        static int CountVisible(char[][] field, int ax, int ay, int w, int h)
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

        static (int count, int x, int y) FindBestPoint(char[][] field)
        {
            int maxCount = 0;
            int maxX = 0;
            int maxY = 0;
            int w = field[0].Length;
            int h = field.Length;
            for (int y = 0; y < h; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    if (field[y][x] == '#')
                    {
                        var temp = CountVisible(field, x, y, w, h);
                        if (temp > maxCount)
                        {
                            maxCount = temp;
                            maxX = x;
                            maxY = y;
                        }
                    }
                }
            }
            return (count: maxCount, x: maxX, y: maxY);
        }

        static (int x, int y) FindDestroyedAsteroid(char[][] field, int ax, int ay, int number)
        {
            int w = field[0].Length;
            int h = field.Length;

            double maxRadius = Math.Sqrt((double) (w * w + h * h));

            int count = 0;
            
            // iterate through all map points and sort angles for iteration
            var validAnglesList = new List<double>();
            for (int y = 0; y < h; ++y)
                for (int x = 0; x < w; ++x)
                    if (x != ax || y != ay)
                        validAnglesList.Add(-Math.Atan2(x - ax, y - ay) + Math.PI / 2.0);

            var validAngles = validAnglesList.Distinct().OrderBy(x => x).ToArray();
            int angleIndex = 0;
            while (true)
            {
                double angle = validAngles[angleIndex];
                double bx = Math.Cos(angle) * maxRadius + ax;
                double by = Math.Sin(angle) * maxRadius + ay;

                for (double t = 0.0; t <= 1.0; t += 1.0 / maxRadius)
                {
                    double pxf = (bx - ax) * t + ax;
                    double pyf = (by - ay) * t + ay;
                    
                    var px = (int) Math.Round(pxf);
                    var py = (int) Math.Round(pyf);
                    
                    if (px < 0 || py < 0 || px >= w || py >= h)
                        break;

                    if (ax == px && ay == py)
                        continue;

                    if (field[py][px] != '#')
                        continue;
                    
                    if (PointToLineDist(px, py, ax, ay, bx, by) < 0.01)
                    {
                        count++;

                        if (count == number)
                            return (x: px, y: py);
                        field[py][px] = '.';
                        break;
                    }
                }

                angleIndex++;
                if (angleIndex >= validAngles.Length)
                    angleIndex = 0;
            }
        }

        static char[][] Parse(string filename)
        {
            return File.ReadAllLines(filename).Select(x => x.ToArray()).ToArray();
        }
        
        static void Main(string[] args)
        {
            Console.WriteLine($"ref1 = {FindBestPoint(Parse("ref1.txt")).count} = 8");
            Console.WriteLine($"ref2 = {FindBestPoint(Parse("ref2.txt")).count} = 33");
            Console.WriteLine($"ref3 = {FindBestPoint(Parse("ref3.txt")).count} = 35");
            Console.WriteLine($"ref4 = {FindBestPoint(Parse("ref4.txt")).count} = 41");
            Console.WriteLine($"ref5 = {FindBestPoint(Parse("ref5.txt")).count} = 210");
            
            {
                var field = Parse("ref5.txt");
                var best = FindBestPoint(field);
                var point = FindDestroyedAsteroid(field, best.x, best.y, 200);
                Console.WriteLine($"ref5 part2 = {point.x},{point.y} = 8,2");
            }

            {
                var field = Parse("input1.txt");
                var best = FindBestPoint(field);
                Console.WriteLine($"part1 = {best.count} = 282");

                var point = FindDestroyedAsteroid(field, best.x, best.y, 200);
                Console.WriteLine($"part2 = {point.x * 100 + point.y} = 1008");
            }
        }
    }
}