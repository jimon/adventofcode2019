using System;
using System.IO;
using System.Linq;

namespace day10
{
    class Program
    {
        static int CountVisible(char[][] field, int startX, int startY, int w, int h, double deltaAngle, double maxRadius)
        {
            if (field[startY][startX] != '#')
                return 0;
            
            int count = 1;
            var visited = new bool[w, h];
            for (double angle = 0; angle < 2.0 * Math.PI; angle += deltaAngle)
            {
                for (double radius = 0.0; radius < maxRadius; radius += 1.0)
                {
                    int x = (int) (Math.Sin(angle) * radius + (double) startX);
                    int y = (int) (Math.Cos(angle) * radius + (double) startY);

                    if (x < 0 || y < 0 || x >= w || y >= h)
                        break;

                    if (x == startX && y == startY)
                        continue;

                    if (field[y][x] == '#')
                    {
                        if (visited[x, y] == false)
                        {
                            count++;
                            visited[x, y] = true;
                        }
                        break;
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
                    var temp = CountVisible(field, x, y, w, h, deltaAngle, maxRadius);
                    //Console.Write($"{temp}");
                    
                    if (temp > max)
                        max = temp;
                }
                //Console.Write("\n");
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
        }
    }
}