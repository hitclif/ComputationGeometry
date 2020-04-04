using System;
using System.Collections.Generic;
using System.Linq;

namespace PointsToConvexPolygon
{
    public class Program
    {
        static void Main(string[] args)
        {
            var polygon = ReadPolygon();
            var pointsCount = Convert.ToInt32(Console.ReadLine());

            for (var i = 0; i < pointsCount; i++)
            {
                var point = ReadPoint();

                var position = point.CalculatePositionTo(polygon);
                Console.WriteLine(position);
            }

            Console.ReadLine();
        }

        internal static Polygon ReadPolygon()
        {
            var vertiesCount = Convert.ToInt32(Console.ReadLine());
            var line = Console.ReadLine()
                .Split(' ')
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .ToArray();

            var points = new List<Point>();

            for(var i = 0; i < line.Length - 1; i = i + 2)
            {
                var point = Tools.ToPoint(line[i], line[i + 1]);
                points.Add(point);
            }

            var polygon = new Polygon(points.ToArray());
            return polygon;
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

    internal struct Edge
    {
        public Edge(Point a, Point b)
        {
            this.A = a;
            this.B = b;
        }

        public Point A { get; }
        public Point B { get; }
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

    internal struct Polygon
    {
        public Polygon(Point[] points)
        {
            this.Points = points;

            var edges = new List<Edge>();
            for (var i = 0; i < points.Length - 1; i++)
            {
                edges.Add(new Edge(points[i], points[i + 1]));
            }

            edges.Add(new Edge(points.Last(), points.First()));
            this.Edges = edges.ToArray();
            // this.Triangles = triangles;
        }

        public Point[] Points { get; }
        public Edge[] Edges { get; }
        // public Triangle[] Triangles { get; }
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

        public static string CalculatePositionTo(this Point q, Polygon polygon)
        {
            var positions = polygon.Edges
                .Select(e => q.CalculatePositionTo(e))
                .Distinct()
                .ToArray();

            if(positions.Any(p => p == PositionToVector.Right || p == PositionToVector.OnLine))
            {
                return "OUTSIDE";
            }

            if (positions.Any(p => p == PositionToVector.OnVector))
            {
                return "BORDER";
            }

            return "INSIDE";
        }

        public static string CalculatePositionTo(this Point q, Triangle triangle)
        {
            var p1 = q.CalculatePositionTo(triangle.A, triangle.B);
            var p2 = q.CalculatePositionTo(triangle.B, triangle.C);
            var p3 = q.CalculatePositionTo(triangle.C, triangle.A);
            var positions = new[] { p1, p2, p3 };

            if (positions.Any(p => p == PositionToVector.Right))
            {
                return "OUTSIDE";
            }

            if (positions.All(p => p == PositionToVector.Left))
            {
                return "INSIDE";
            }

            return "BORDER";
        }

        public static PositionToVector CalculatePositionTo(this Point a, Edge edge)
            => CalculatePositionTo(a, edge.A, edge.B);

        public static PositionToVector CalculatePositionTo(this Point a, Point vectorStart, Point vectorEnd)
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

            return ToPoint(a);
        }

        public static Point ToPoint(params string[] coordinates)
        {
            return new Point(Convert.ToInt32(coordinates[0]), Convert.ToInt32(coordinates[1]));
        }
    }
}
