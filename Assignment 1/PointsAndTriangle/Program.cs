using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointsAndTriangle
{
    class Program
    {
        static void Main(string[] args)
        {
            var triangle = ReadTriangle();
            var pointsCount = Convert.ToInt32(Console.ReadLine());

            for (var i = 0; i < pointsCount; i++)
            {
                var point = ReadPoint();

                var position = point.CalculatePositionToTriangle(triangle);
                Console.WriteLine(position);
            }
        }

        internal static Triangle ReadTriangle()
        {
            var line = Console.ReadLine()
                .Split(' ')
                .ToArray();

            var a = line.Take(2).ToArray().ToPoint();
            var b = line.Skip(2).Take(2).ToArray().ToPoint();
            var c = line.Skip(4).Take(2).ToArray().ToPoint();

            var sorted = Tools.SortCounterClockwise(new Triangle(a, b, c));
            return sorted;
        }

        private static Point ReadPoint()
        {
            var line = Console.ReadLine();
            var point = line.ToPoint();
            return point;
        }
    }

    internal struct Triangle
    {
        public Triangle(Point a, Point b, Point c)
        {
            this.A = a;
            this.B = b;
            this.C = c;
        }

        public Point A { get; }
        public Point B { get; }
        public Point C { get; }
    }

    internal struct Point
    {
        public Point(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public int X { get; }
        public int Y { get; }
    }

    internal static class Tools
    {
        public enum PositionToVector
        {
            Left,
            Right,
            OnVector,
            OnLine
        }

        public static Triangle SortCounterClockwise(Triangle triangle)
        {
            var isCounterClockwise = triangle.SignedArea() > 0;

            return isCounterClockwise
                ? triangle
                : new Triangle(triangle.C, triangle.B, triangle.A);
        }

        public static string CalculatePositionToTriangle(this Point q, Triangle triangle)
        {
            var p1 = q.CalculatePositionToVector(triangle.A, triangle.B);
            var p2 = q.CalculatePositionToVector(triangle.B, triangle.C);
            var p3 = q.CalculatePositionToVector(triangle.C, triangle.A);
            var positions = new[] { p1, p2, p3 };

            if (positions.Any(p => p == PositionToVector.Right))
            {
                return "OUTSIDE";
            }

            if(positions.All(p => p == PositionToVector.Left))
            {
                return "INSIDE";
            }

            return "BORDER";
        }

        public static PositionToVector CalculatePositionToVector(this Point a, Point vectorStart, Point vectorEnd)
        {
            var area = SignedArea(vectorStart, vectorEnd, a);

            if (area > 0)
            {
                return PositionToVector.Left;
            }

            if (area < 0)
            {
                return PositionToVector.Right;
            }

            var isOnSegment = a.X <= Math.Max(vectorStart.X, vectorEnd.X)
                           && a.X >= Math.Min(vectorStart.X, vectorEnd.X)
                           && a.Y <= Math.Max(vectorStart.Y, vectorEnd.Y)
                           && a.Y >= Math.Min(vectorStart.Y, vectorEnd.Y);

            return isOnSegment
                ? PositionToVector.OnVector
                : PositionToVector.OnLine;
        }

        public static decimal SignedArea(this Triangle triangle)
        {
            return SignedArea(triangle.A, triangle.B, triangle.C);
        }

        public static decimal SignedArea(Point a, Point b, Point c)
        {
            var areaDoubled = (b.X - a.X) * (c.Y - a.Y) - (c.X - a.X) * (b.Y - a.Y);
            var area = areaDoubled / 2m;
            return area;
        }

        public static Point ToPoint(this string textCoordinates)
        {
            var a = textCoordinates
                .Split(' ')
                .ToArray();

            return a.ToPoint();
        }

        public static Point ToPoint(this string[] coordinates)
        {
            return new Point(Convert.ToInt32(coordinates[0]), Convert.ToInt32(coordinates[1]));
        }
    }
}
