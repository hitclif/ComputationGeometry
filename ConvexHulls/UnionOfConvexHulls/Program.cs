using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace UnionOfConvexHulls
{
    class Program
    {
        static void Main(string[] args)
        {
            var polygons = new List<Polygon>();
#if test
            polygons.Add(ReadPolygon("-5 4 -3 3 2 2 8 2 5 5 -1 7"));
            polygons.Add(ReadPolygon("-1 -4 4 -2 1 1 -3 2 -5 -1"));
#else
            var polygonCount = Console.ReadLine().ToInt();
            for(var i = 0; i < polygonCount; i++)
            {
                var expectedVerticesCount = Convert.ToInt32(Console.ReadLine());
                var polygon = ReadPolygon(Console.ReadLine());
                polygons.Add(polygon);
            }
#endif
            var convexHull = polygons.MergeToConvexHulls();

            Console.ReadLine();
        }

        private static Polygon ReadPolygon(string polygonData)
        {
            var coordinates = polygonData
               .Split(' ')
               .Where(s => !string.IsNullOrWhiteSpace(s))
               .ToArray();

            var points = new List<Point>();
            for (var i = 0; i < coordinates.Length; i = i + 2)
            {
                var newPoint = new Point(Convert.ToInt64(coordinates[i]), Convert.ToInt64(coordinates[i + 1]));
                points.Add(newPoint);
            }

            return new Polygon(points);
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

    struct Polygon
    {
        public Polygon(IEnumerable<Point> points)
        {
            var temp = points.ToArray();
            this.Points = temp;

            this.Edges = this
                .Points
                .Take(temp.Length - 1)
                .Select((p, index) => new Edge(p, temp[index + 1]))
                .ToArray();

            this.CircularEdges = this.Edges
                .Concat(new[] { new Edge(temp.Last(), temp.First()) })
                .ToArray();
        }

        public IReadOnlyCollection<Point> Points { get; }
        public IReadOnlyCollection<Edge> Edges { get; }
        private IReadOnlyCollection<Edge> CircularEdges { get; }

        public Edge StartingEdgeIn(int pointIndex)
        {
            var edge = this.CircularEdges.ElementAt(pointIndex);
            return edge;
        }

        public Edge EndingEdgeIn(int pointIndex)
        {
            var edge = this.CircularEdges.ElementAt(pointIndex + 1);
            return edge;
        }
    }

    static class Tools
    {
        public static Polygon MergeToConvexHulls(this IReadOnlyCollection<Polygon> polygons)
        {
            return new Polygon();
        }

        public static int ToInt(this string text) => Convert.ToInt32(text);
        public static long ToLong(this string text) => Convert.ToInt64(text);
    }
}