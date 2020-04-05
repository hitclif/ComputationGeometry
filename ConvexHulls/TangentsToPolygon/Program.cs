using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TangentsToPolygon
{
    class Program
    {
        static void Main(string[] args)
        {
            var polygon = ReadPolygon();
            var points = ReadPoints();

            foreach(var point in points)
            {
                var tangents = point
                    .FindTangents(polygon);

                Console.WriteLine($"{tangents.Item1.X} {tangents.Item1.Y} {tangents.Item2.X} {tangents.Item2.Y}");
            }

            // Console.ReadLine();
        }

        static Point[] ReadPolygon()
        {
            var expectedCount = Convert.ToInt32(Console.ReadLine());
            var coordinates = Console.ReadLine()
                .Split(' ')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToArray();

            var points = new List<Point>();
            for (var i = 0; i < coordinates.Length; i = i + 2)
            {
                var newPoint = new Point(Convert.ToInt64(coordinates[i]), Convert.ToInt64(coordinates[i + 1]));
                points.Add(newPoint);
            }

            if (expectedCount != points.Count)
            {
                throw new Exception("incorrect points count");
            }

            return points.ToArray();
        }

        static Point[] ReadPoints()
        {
            var expectedCount = Convert.ToInt32(Console.ReadLine());
            var points = new List<Point>();
            for (var i = 0; i < expectedCount; i++)
            {
                var coordinates = Console.ReadLine().Split(' ').Where(s => !string.IsNullOrEmpty(s)).ToArray();
                var newPoint = new Point(Convert.ToInt64(coordinates[0]), Convert.ToInt64(coordinates[1]));
                points.Add(newPoint);
            }
            return points.ToArray();
        }
    }

    [DebuggerDisplay("{X},{Y}")]
    struct Point
    {
        public Point(long x, long y)
        {
            this.X = x;
            this.Y = y;
        }

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

    [DebuggerDisplay("{A}<->{B}")]
    struct Edge
    {
        public Edge(Point a, Point b)
        {
            this.A = a;
            this.B = b;
        }

        public Point A { get; }
        public Point B { get; }
    }

    static class Tools
    {
        public static Tuple<Point, Point> FindTangents(this Point point, IReadOnlyCollection<Point> polygon)
        {
            bool leftFound = false;
            bool rightFound = false;

            Point leftTangent = new Point();
            Point rightTangent = new Point();

            for (var i = 0; i < polygon.Count; i++)
            {
                var previousEdge = polygon.GetPreviousEdge(i);
                var nextEdge = polygon.GetNextEdge(i);

                var toPreviousEdgePosition = point.Position(previousEdge);
                var toNextEdgePosition = point.Position(nextEdge);

                if (toPreviousEdgePosition == toNextEdgePosition)
                {
                    continue;
                }

                if(toPreviousEdgePosition.IsLeft())
                {
                    leftTangent = polygon.ElementAt(i);
                    leftFound = true;
                }

                if (toPreviousEdgePosition.IsRight())
                {
                    rightTangent = polygon.ElementAt(i);
                    rightFound = true;
                }

                if(leftFound && rightFound)
                {
                    break;
                }
            }

            return new Tuple<Point, Point>(leftTangent, rightTangent);
        }

        private static Edge GetPreviousEdge(this IReadOnlyCollection<Point> polygon, int pointIndex)
        {
            return new Edge(polygon.GetPreviousPoint(pointIndex), polygon.ElementAt(pointIndex));
        }

        private static Edge GetNextEdge(this IReadOnlyCollection<Point> polygon, int pointIndex)
        {
            var nextIndex = (pointIndex + 1) % polygon.Count;
            return new Edge(polygon.ElementAt(pointIndex), polygon.ElementAt(nextIndex));
        }

        private static Point GetPreviousPoint(this IReadOnlyCollection<Point> polygon, int index)
        {
            var previousIndex = index == 0 ? polygon.Count - 1 : index - 1;
            return polygon.ElementAt(previousIndex);
        }

        private static long Position(this Point point, Edge toEdge)
        {
            var v1 = toEdge.A;
            var v2 = toEdge.B;

            return ((v2.X - v1.X) * (point.Y - v1.Y) - (point.X - v1.X) * (v2.Y - v1.Y)).Sign();
        }

        private static bool IsLeft(this long position)
        {
            return position.Sign() > 0;
        }

        private static bool IsRight(this long position)
        {
            return position.Sign() < 0;
        }

        private static long Sign(this long value)
        {
            return Math.Sign(value);
        }
    }
}
