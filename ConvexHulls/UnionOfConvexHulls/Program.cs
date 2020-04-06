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
            polygons.Add(ReadPolygon("2 17 -17 6 -15 -5 -6 -10 1 -11 18 15"));
            polygons.Add(ReadPolygon("-16 10 -6 -8 0 2"));
#else
            var polygonCount = Console.ReadLine().ToInt();
            for(var i = 0; i < polygonCount; i++)
            {
                var expectedVerticesCount = Convert.ToInt32(Console.ReadLine());
                var polygon = ReadPolygon(Console.ReadLine());
                polygons.Add(polygon);
            }
#endif
            var convexHull = polygons.MergeToConvexHull();
            Console.WriteLine(convexHull.Points.Count);

            var points = string.Join(" ", convexHull.Points.Select(p => $"{p.X} {p.Y}").ToArray());
            Console.WriteLine(points);
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

    [DebuggerDisplay("{StartPoint}<->{EndPoint}")]
    struct Tangent
    {
        public Tangent(Point startPoint, int indexOfStartPoint, Point endPoint, int indexOfEndPoint)
        {
            this.StartPoint = startPoint;
            this.IndexOfStartPoint = indexOfStartPoint;
            this.EndPoint = endPoint;
            this.IndexOfEndPoint = indexOfEndPoint;
        }

        public Point StartPoint { get; }
        public int IndexOfStartPoint { get; }
        public Point EndPoint { get; }
        public int IndexOfEndPoint { get; }

        public Tangent Reverse() => new Tangent(EndPoint, IndexOfEndPoint, StartPoint, IndexOfStartPoint);

        public Tangent SetStartPoint(Point startPoint, int indexOfStartPoint)
        {
            return new Tangent(startPoint, indexOfStartPoint, this.EndPoint, this.IndexOfEndPoint);
        }

        public Tangent SetEndPoint(Point endPoint, int indexOfEndPoint)
        {
            return new Tangent(this.StartPoint, this.IndexOfStartPoint, endPoint, indexOfEndPoint);
        }
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

            this.CircularEdges = this.Edges.Count == 0
                ? this.Edges
                : this.Edges
                    .Concat(new[] { new Edge(temp.Last(), temp.First()) })
                    .ToArray();
        }

        public IReadOnlyCollection<Point> Points { get; }
        public IReadOnlyCollection<Edge> Edges { get; }
        private IReadOnlyCollection<Edge> CircularEdges { get; }

        public Point At(int index)
        {
            return this.Points.ElementAt(index);
        }

        public Tuple<Point, int> Previous(int toIndex)
        {
            var index = (toIndex - 1 + this.Points.Count) % this.Points.Count;
            return new Tuple<Point, int>(this.At(index), index);
        }

        public Tuple<Point, int> Next(int toIndex)
        {
            var index = (toIndex + 1) % this.Points.Count;
            return new Tuple<Point, int>(this.At(index), index);
        }

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

        public IEnumerable<Point> PointsBetween(int startIndex, int endIndex)
        {
            var points = new List<Point>();

            var currentIndex = startIndex - 1;
            do
            {
                var search = this.Next(currentIndex);
                points.Add(search.Item1);
                currentIndex = search.Item2;
            } while (currentIndex != endIndex);

            return points;
        }
    }

    static class Tools
    {
        public static IReadOnlyCollection<Point> ConvexHullGrahamScan(this IReadOnlyCollection<Point> points)
        {
            var zero = points.FindZeroPoint();

            var comparer = new AngleDistanceComparer(zero);
            var sortedPoints = points
                .Where(p => p.NotEqualTo(zero))
                .OrderBy(p => p, comparer)
                .ToArray();

            var convexHull = new List<Point> { zero, sortedPoints.ElementAt(0) };
            for (var i = 1; i < sortedPoints.Length; i++)
            {
                convexHull.Add(sortedPoints.ElementAt(i));
                convexHull.RemoveInnerConcavePoints();
            }

            convexHull.Add(zero);
            convexHull.RemoveInnerConcavePoints();
            convexHull.RemoveAt(convexHull.Count - 1);

            return convexHull;
        }

        private static Point FindZeroPoint(this IReadOnlyCollection<Point> points)
        {
            var zero = new Point(long.MaxValue, long.MaxValue);

            foreach (var point in points)
            {
                if (zero.Y > point.Y)
                {
                    zero = point;
                    continue;
                }

                if (zero.Y < point.Y)
                {
                    continue;
                }

                if (zero.X < point.X) // because zero.Y == point.Y
                {
                    zero = point;
                }
            }

            return zero;
        }

        private static void RemoveInnerConcavePoints(this List<Point> convexHullCandidate)
        {
            var finished = convexHullCandidate.Count <= 2;
            while (!finished)
            {
                var position = convexHullCandidate[convexHullCandidate.Count - 1].Position(new Edge(convexHullCandidate[convexHullCandidate.Count - 3], convexHullCandidate[convexHullCandidate.Count - 2]));
                // var isLeft =   Math.Sign(IsLeft(convexHullCandidate[convexHullCandidate.Count - 3], convexHullCandidate[convexHullCandidate.Count - 2], convexHullCandidate[convexHullCandidate.Count - 1]));
                switch (position)
                {
                    case -1: // is to the right
                    case 0:  // is on the line, however further
                        convexHullCandidate.RemoveAt(convexHullCandidate.Count - 2);
                        break;
                    case 1:
                        // ok
                        break;
                }

                finished = convexHullCandidate.Count <= 2 || position > 0;
            }
        }

        public static Polygon MergeToConvexHull(this IReadOnlyCollection<Polygon> polygons)
        {
            var allPoints = polygons
                .SelectMany(p => p.Points)
                .ToArray();

            var convexHullPoints = allPoints.ConvexHullGrahamScan();
            return new Polygon(convexHullPoints);
            //if(polygons.Count == 0)
            //{
            //    return new Polygon(new Point[0]);
            //}

            //if(polygons.Count == 1)
            //{
            //    return polygons.First();
            //}

            //var convexHull = new Polygon(new Point[0]);
            //for (var i = 0; i < polygons.Count; i++)
            //{
            //    convexHull = convexHull.MergeWithConvexHull(polygons.ElementAt(i));
            //}

            //return convexHull;
        }

        public static Polygon MergeWithConvexHull(this Polygon p1, Polygon p2)
        {
            if(p1.Points.Count == 0 || p2.Points.Count == 0)
            {
                return new Polygon(p1.Points.Concat(p2.Points));
            }

            var startingTangents = FindStartingTangents(p1, p2);
            var leftTangent = FindLeftTangent(startingTangents.Item1, p1, p2);
            var rightTangent = FindRightTangent(startingTangents.Item2, p1, p2);

            var points = p1.PointsBetween(leftTangent.IndexOfStartPoint, rightTangent.IndexOfStartPoint)
                .Concat(p2.PointsBetween(rightTangent.IndexOfEndPoint, leftTangent.IndexOfEndPoint));

            return new Polygon(points);
        }

        private static Tuple<Tangent, Tangent> TangentsTo(this Point point, int indexOfPoint, Polygon polygon)
        {
            var angleComparer = new AngleDistanceComparer(point);

            var sortedPoints = polygon
                .Points
                .Select((p, i) => new { p, i })
                .OrderByDescending(o => o.p, angleComparer)
                .ToArray();

            var first = sortedPoints.First();
            var last = sortedPoints.Last();

            var leftTangent = new Tangent(point, indexOfPoint, first.p, first.i);
            var rightTangent = new Tangent(point, indexOfPoint, last.p, last.i);

            return new Tuple<Tangent, Tangent>(leftTangent, rightTangent);
        }

        private static Tuple<Tangent, Tangent> FindStartingTangents(Polygon p1, Polygon p2)
        {
            var tangents = p1.Points.ElementAt(0).TangentsTo(0, p2);

            var leftTangentCandidate = tangents.Item1.EndPoint.TangentsTo(tangents.Item1.IndexOfEndPoint, p1).Item2.Reverse();
            var rightTangentCandidate = tangents.Item2.EndPoint.TangentsTo(tangents.Item2.IndexOfEndPoint, p1).Item1.Reverse();

            var leftTangent = FindLeftTangent(leftTangentCandidate, p1, p2);
            var rightTangent = FindRightTangent(rightTangentCandidate, p1, p2);

            return new Tuple<Tangent, Tangent>(leftTangent, rightTangent);
        }

        private static Tangent FindLeftTangent(this Tangent startSearch, Polygon p1, Polygon p2)
        {
            var candidate = startSearch;

            var test = candidate.TestLeft(p1, p2);

            while (!(test.Item1 && test.Item2))
            {
                if (!test.Item1)
                {
                    var next = p1.Next(candidate.IndexOfStartPoint);
                    candidate = candidate.SetStartPoint(next.Item1, next.Item2);
                }

                if (!test.Item2)
                {
                    var previous = p2.Previous(candidate.IndexOfEndPoint);
                    candidate = candidate.SetEndPoint(previous.Item1, previous.Item2);
                }

                test = candidate.TestLeft(p1, p2);
            }

            return candidate;
        }

        private static Tangent FindRightTangent(this Tangent startSearch, Polygon p1, Polygon p2)
        {
            var candidate = startSearch;

            var test = candidate.TestRight(p1, p2);

            while (!(test.Item1 && test.Item2))
            {
                if (!test.Item1)
                {
                    var previous = p1.Next(candidate.IndexOfStartPoint);
                    candidate = candidate.SetStartPoint(previous.Item1, previous.Item2);
                }

                if (!test.Item2)
                {
                    var next = p2.Previous(candidate.IndexOfEndPoint);
                    candidate = candidate.SetEndPoint(next.Item1, next.Item2);
                }

                test = candidate.TestLeft(p1, p2);
            }

            return candidate;
        }

        private static Tuple<bool, bool> TestLeft(this Tangent tangent, Polygon p1, Polygon p2)
        {
            var edge = tangent.ToEdge();
            var p1Next = p1.Next(tangent.IndexOfStartPoint);
            var p2Previous = p2.Previous(tangent.IndexOfEndPoint);

            var p1IsRight = p1Next.Item1.Position(edge).IsRight();
            var p2IsRight = p2Previous.Item1.Position(edge).IsRight();

            return new Tuple<bool, bool>(p1IsRight, p2IsRight);
        }

        private static Tuple<bool, bool> TestRight(this Tangent tangent, Polygon p1, Polygon p2)
        {
            var edge = tangent.ToEdge();
            var p1Next = p1.Next(tangent.IndexOfStartPoint);
            var p2Previous = p2.Previous(tangent.IndexOfEndPoint);

            var p1IsLeft = p1Next.Item1.Position(edge).IsLeft();
            var p2IsLeft = p2Previous.Item1.Position(edge).IsLeft();

            return new Tuple<bool, bool>(p1IsLeft, p2IsLeft);
        }

        private static Edge ToEdge(this Tangent tangent)
        {
            return new Edge(tangent.StartPoint, tangent.EndPoint);
        }

        public static int ToInt(this string text) => Convert.ToInt32(text);
        public static long ToLong(this string text) => Convert.ToInt64(text);

        public static long Position(this Point point, Edge toEdge)
        {
            var v1 = toEdge.A;
            var v2 = toEdge.B;

            return ((v2.X - v1.X) * (point.Y - v1.Y) - (point.X - v1.X) * (v2.Y - v1.Y)).Sign();
        }

        public static bool IsLeft(this long position)
        {
            return position.Sign() > 0;
        }

        public static bool IsRight(this long position)
        {
            return position.Sign() < 0;
        }

        public static long Sign(this long value)
        {
            return Math.Sign(value);
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
            var position = y.Position(new Edge(_zeroPoint, x));
            if (position != 0)
            {
                return -Math.Sign(position);
            }

            var diff = Math.Abs(_zeroPoint.Y - x.Y) - Math.Abs(_zeroPoint.Y - y.Y);
            return Math.Sign(diff);
        }
    }
}