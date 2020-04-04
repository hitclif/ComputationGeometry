using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ConvexHull
{
    class Program
    {
        static void Main(string[] args)
        {
            var points = ReadPoints();
            var convexHull = points.ConvexHullGrahamScan();

            Console.WriteLine(convexHull.Count);

            var output = convexHull
                .Aggregate(string.Empty, (acc, p) => acc + $" {p.X} {p.Y}")
                .Trim();

            Console.WriteLine(output);
            Console.ReadLine();
        }

        static Point[] ReadPoints()
        {
            var expectedCount = Convert.ToInt32(Console.ReadLine());
            var coordinates = Console.ReadLine()
                .Split(' ')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToArray();

            var points = new List<Point>();
            for (var i = 0; i < coordinates.Length; i = i + 2)
            {
                var newPoint = new Point($"p{i + 1}", Convert.ToInt64(coordinates[i]), Convert.ToInt64(coordinates[i + 1]));
                points.Add(newPoint);
            }

            if (expectedCount != points.Count)
            {
                throw new Exception("incorrect points count");
            }

            return points.ToArray();
        }
    }

    [DebuggerDisplay("{Name}:{X},{Y}")]
    struct Point
    {
        public Point(string name, long x, long y)
        {
            this.Name = name;
            this.X = x;
            this.Y = y;
        }

        public string Name { get; }
        public long X { get; }
        public long Y { get; }

        public bool EqualTo(Point point)
        {
            return this.X == point.X && this.Y == point.Y;
        }

        public bool NotEqualTo(Point point)
        {
            return !this.EqualTo(point);
        }
    }

    static class Tools
    {
        public static IReadOnlyCollection<Point> ConvexHullGrahamScan(this IReadOnlyCollection<Point> points)
        {
            var zero = points.FindZeroPoint();
            var sortedPoints = points
                .Where(p => p.NotEqualTo(zero))
                .SortByAngleAndDistance(zero);

            var convexHull = new List<Point> { zero, sortedPoints.ElementAt(0) };
            for (var i = 1; i < sortedPoints.Count; i++)
            {
                convexHull.Add(sortedPoints.ElementAt(i));
                convexHull.RemoveInnerConcavePoints();
            }

            convexHull.Add(zero);
            convexHull.RemoveInnerConcavePoints();
            convexHull.RemoveAt(convexHull.Count - 1);

            return convexHull;
        }

        private static void RemoveInnerConcavePoints(this List<Point> convexHullCandidate)
        {
            var finished = convexHullCandidate.Count <= 2;
            while (!finished)
            {
                var isLeft = Math.Sign(IsLeft(convexHullCandidate[convexHullCandidate.Count - 3], convexHullCandidate[convexHullCandidate.Count - 2], convexHullCandidate[convexHullCandidate.Count - 1]));
                switch (isLeft)
                {
                    case -1: // is to the right
                    case 0:  // is on the line, however further
                        convexHullCandidate.RemoveAt(convexHullCandidate.Count - 2);
                        break;
                    case 1:
                        // ok
                        break;
                }

                finished = convexHullCandidate.Count <= 2 || isLeft > 0;
            }
        }

        private static Point FindZeroPoint(this IReadOnlyCollection<Point> points)
        {
            var zero = new Point("zero", long.MaxValue, long.MaxValue);

            foreach(var point in points)
            {
                if(zero.Y > point.Y)
                {
                    zero = point;
                    continue;
                }

                if(zero.Y < point.Y)
                {
                    continue;
                }

                if(zero.X < point.X) // because zero.Y == point.Y
                {
                    zero = point;
                }
            }

            return zero;
            //var minY = points
            //    .Min(p => p.Y);

            //var pointsWithMinY = points
            //    .Where(p => p.Y == minY)
            //    .ToArray();
            //    ;

            //var minX = pointsWithMinY.Min(p => p.X);
            //var zero = pointsWithMinY.Where(p => p.X == minX).First();
            //return zero;
        }

        /// <summary>
        /// > 0 point is to the left of line
        /// = 0 point is on line
        /// < 0 point is the right of line
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static long IsLeft(Point v1, Point v2, Point point)
        {
            return (v2.X - v1.X) * (point.Y - v1.Y) - (point.X - v1.X) * (v2.Y - v1.Y);
        }

        private static IReadOnlyCollection<Point> SortByAngleAndDistance(this IEnumerable<Point> points, Point zero)
        {
            var comparer = new AngleDistanceComparer(zero);
            var result = points
                .OrderBy(p => p, comparer)
                .ToArray();

            return result;
        }
    }

    class AngleDistanceComparer : IComparer<Point>
    {
        private readonly Point _zeroPoint;

        public AngleDistanceComparer(Point zeroPoint)
        {
            _zeroPoint = zeroPoint;
        }

        public int Compare(Point x, Point y)
        {
            var yIsLeft = Tools.IsLeft(_zeroPoint, x, y); // > 0 means y is left, thus y has bigger angle to zero (y is bigger than x)
            var sign = Math.Sign(yIsLeft);
            if(sign != 0)
            {
                // we have to switch sign
                return -sign;
            }

            var diff = Math.Abs(_zeroPoint.Y - x.Y) - Math.Abs(_zeroPoint.Y - y.Y);
            return Math.Sign(diff);
        }
    }
}
