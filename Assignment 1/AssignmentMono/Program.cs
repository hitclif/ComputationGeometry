using System;
using System.Linq;

namespace AssignmentMono
{
    class Program
    {
        static void Main(string[] args)
        {
            var (a, b) = ReadVector();
            var pointsCount = Convert.ToInt32(Console.ReadLine());

            for (var i = 0; i < pointsCount; i++)
            {
                var point = ReadPoint();
                var position = point.CalculatePositionToVector(a, b);
                Console.WriteLine(position);
            }
        }

        private static (Point, Point) ReadVector()
        {
            var numbers = Console
                .ReadLine()
                .Split(' ')
                .ToArray();

            var a = string.Join(" ", numbers[0], numbers[1]).ToPoint();
            var b = string.Join(" ", numbers[2], numbers[3]).ToPoint();

            return (a, b);
        }

        private static Point ReadPoint()
        {
            var line = Console.ReadLine();
            var point = line.ToPoint();
            return point;
        }
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
        public static string CalculatePositionToVector(this Point a, Point vectorStart, Point vectorEnd)
        {
            var area = SignedArea(vectorStart, vectorEnd, a);

            if (area > 0)
            {
                return "LEFT";
            }

            if (area < 0)
            {
                return "RIGHT";
            }

            var isOnSegment = a.X <= Math.Max(vectorStart.X, vectorEnd.X)
                           && a.X >= Math.Min(vectorStart.X, vectorEnd.X)
                           && a.Y <= Math.Max(vectorStart.Y, vectorEnd.Y)
                           && a.Y >= Math.Min(vectorStart.Y, vectorEnd.Y);

            return isOnSegment
                ? "ON_SEGMENT"
                : "ON_LINE";
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

            return new Point(Convert.ToInt32(a[0]), Convert.ToInt32(a[1]));
        }
    }
}
