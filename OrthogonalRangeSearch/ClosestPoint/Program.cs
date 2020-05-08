using System;
using System.Collections.Generic;
using System.Linq;

namespace ClosestPoint
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ReadLine();
            var points = Console.ReadLine().ToPoints();
            var finder = new ClosestPointFinder(points);

            var count = Convert.ToInt32(Console.ReadLine());
            for(var i = 0; i < count; i++)
            {
                var point = Convert.ToInt32(Console.ReadLine());
                var closest = finder.FindClosestPointTo(point);
                Console.WriteLine(closest);
            }
        }
    }

    public class ClosestPointFinder
    {
        private List<int> _points;

        public ClosestPointFinder(IEnumerable<int> points)
        {
            _points = new List<int>(points);
            _points.Sort();
        }

        public int FindClosestPointTo(int point)
        {
            var index = _points.BinarySearch(point);

            if(index < 0)
            {
                index = ~index;
            }

            if(index >= _points.Count)
            {
                return _points.Last();
            }

            var p1 = _points[index];
            if(index == 0)
            {
                return p1;
            }

            var p2 = _points[index - 1];

            var d1 = Math.Abs(p1 - point);
            var d2 = Math.Abs(p2 - point);

            return d1 <= d2
                ? p1
                : p2;
        }
    }

    public static class Tools
    {
        public static IEnumerable<int> ToPoints(this string line)
        {
            var points = line
                .Trim()
                .Split(' ')
                .Select(p => Convert.ToInt32(p));
            return points;
        }
    }
}
