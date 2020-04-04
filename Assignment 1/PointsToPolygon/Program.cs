using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointsToPolygon
{
    class Program
    {
        static void Main(string[] args)
        {
            var polygon = ReadPolygon();
            var pointsCount = Convert.ToInt32(Console.ReadLine());

            for (var i = 0; i < pointsCount; i++)
            {
                var point = ReadPoint();

                var position = point.CalculatePositionToSimple(polygon);
                Console.WriteLine(position);
            }
        }

        internal static Polygon ReadPolygon()
        {
            var vertiesCount = Convert.ToInt32(Console.ReadLine());
            var line = Console.ReadLine()
                .Split(' ')
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .ToArray();

            var points = new List<Point>();

            for (var i = 0; i < line.Length - 1; i = i + 2)
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

    [DebuggerDisplay("[{X},{Y}]")]
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

    internal enum EdgeOrientation
    {
        Up,
        Horizontal,
        Down,
    }

    [DebuggerDisplay("{A}:{B}")]
    internal struct Edge
    {
        public Edge(Point a, Point b)
        {
            this.A = a;
            this.B = b;
        }

        public Point A { get; }
        public Point B { get; }

        
        public Point LowerPoint => A.Y < B.Y ? A : B;
        public Point UpperPoint => A.Y >= B.Y ? A : B;
        public Point LeftPoint => A.X < B.X ? A : B;
        public Point RightPoint => A.X >= B.X ? A : B;

        public EdgeOrientation Orientation
        {
            get
            {
                if(A.Y == B.Y)
                {
                    return EdgeOrientation.Horizontal;
                }

                if (A.Y > B.Y)
                {
                    return EdgeOrientation.Down;
                }

                return EdgeOrientation.Up;
            }
        }
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
        }

        public Point[] Points { get; }
        public Edge[] Edges { get; }
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

        public static string CalculatePositionToSimple(this Point q, Polygon polygon)
        {
            var intersections = 0;
            foreach(var edge in polygon.Edges)
            {
                var position = q.CalculatePositionTo(edge);
                if(position == PositionToVector.OnVector)
                {
                    return "BORDER";
                }

                intersections += q.CalculateIntersectionIncrement(edge);
            }

            return intersections % 2 == 0
                ? "OUTSIDE"
                : "INSIDE";
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

        public static decimal SignedArea(Point a, Point b, Point c)
        {
            var areaDoubled = (b.X - a.X) * (c.Y - a.Y) - (c.X - a.X) * (b.Y - a.Y);
            var area = areaDoubled / 2m;
            return area;
        }

        public static int CalculateIntersectionIncrement(this Point q, Edge edge)
        {
            if(q.IsNotInVerticalSpace(edge) || edge.Orientation == EdgeOrientation.Horizontal)
            {
                return 0;
            }

            var position = q.CalculatePositionTo(edge);
            switch (position)
            {
                case PositionToVector.Left:
                    return edge.Orientation == EdgeOrientation.Down || q.Y == edge.LowerPoint.Y
                        ? 0
                        : 1;
                case PositionToVector.Right:
                    return edge.Orientation == EdgeOrientation.Up || q.Y == edge.LowerPoint.Y
                        ? 0
                        : 1;
                default:
                    return 0;
            }
        }

        public static bool IsNotInVerticalSpace(this Point q, Edge edge)
        {
            return q.Y < Math.Min(edge.A.Y, edge.B.Y) || q.Y > Math.Max(edge.A.Y, edge.B.Y);
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
