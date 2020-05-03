using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diagonal
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ReadLine();
            var polygon = Console.ReadLine().ToPolygon();

            var diagonals = new DiagonalFinder()
                .FindDiagonals(polygon)
                .ToArray();

            Console.WriteLine(diagonals.Length);
            foreach (var d in diagonals)
            {
                Console.WriteLine(d.ToString());
            }
        }
    }

    [DebuggerDisplay("{X},{Y}")]
    public class Point
    {
        public Point(long x, long y)
        {
            this.X = x;
            this.Y = y;
        }

        public long X { get; }
        public long Y { get; }

        public static bool operator ==(Point a, Point b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        public static bool operator !=(Point a, Point b)
        {
            return !(a == b);
        }

        public static long Area2(Point a, Point b, Point c)
        {
            var area2 = (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
            return area2;
        }

        public override string ToString()
        {
            return this.X.ToString() + " " + this.Y.ToString();
        }

        public override bool Equals(object obj)
        {
            return obj is Point point &&
                   this.X == point.X &&
                   this.Y == point.Y;
        }

        public override int GetHashCode()
        {
            int hashCode = 1861411795;
            hashCode = hashCode * -1521134295 + this.X.GetHashCode();
            hashCode = hashCode * -1521134295 + this.Y.GetHashCode();
            return hashCode;
        }
    }

    public enum Designation
    {
        INTERNAL,
        EXTERNAL,
        INTERSECT
    }

    [DebuggerDisplay("{Segment},{Designation}")]
    public class Diagonal
    {
        public Diagonal(Segment segment, Designation designation)
        {
            this.Segment = segment;
            this.Designation = designation;
        }

        public Segment Segment { get; }

        public Designation Designation { get; }

        public override string ToString()
        {
            return this.Segment.ToString() + " " + this.Designation;
        }
    }

    public enum PointPosition
    {
        Left,
        Right,
        CollinearOutside,
        CollinearInside,
        IsEndPoint
    }

    [DebuggerDisplay("{A}:{B}")]
    public class Segment
    {
        public Segment(Point a, Point b)
        {
            this.A = a;
            this.B = b;
        }

        public Point A { get; }
        public Point B { get; }

        public override string ToString()
        {
            return this.A.ToString() + " " + this.B.ToString();
        }

        public PointPosition PositionOf(Point point)
        {
            var area2 = Point.Area2(this.A, this.B, point);
            if(area2 > 0)
            {
                return PointPosition.Left;
            }

            if(area2 < 0)
            {
                return PointPosition.Right;
            }

            if (this.A == point || this.B == point)
            {
                return PointPosition.IsEndPoint;
            }

            return this.IsInSegmentBox(point)
                ? PointPosition.CollinearInside
                : PointPosition.CollinearOutside;
        }

        public double CalculateIntersecitonParameter(Segment other)
        {
            var p1 = this.A;
            var p2 = this.B;
            var p3 = other.A;
            var p4 = other.B;
            var t = 1d *
                ((p1.X - p3.X) * (p3.Y - p4.Y) - (p1.Y - p3.Y) * (p3.X - p4.X))
                /
                ((p1.X - p2.X) * (p3.Y - p4.Y) - (p1.Y - p2.Y) * (p3.X - p4.X));

            return t;
        }

        private bool IsInSegmentBox(Point point)
        {
            var isOnSegment = point.X <= Math.Max(this.A.X, this.B.X)
                          && point.X >= Math.Min(this.A.X, this.B.X)
                          && point.Y <= Math.Max(this.A.Y, this.B.Y)
                          && point.Y >= Math.Min(this.A.Y, this.B.Y);
            return isOnSegment;
        }
    }

    public class DiagonalFinder
    {
        public IEnumerable<Diagonal> FindDiagonals(IReadOnlyCollection<Point> polygon)
        {
            var edges = polygon
                .Select((p, i) =>
                {
                    var end = polygon.ElementAt((i + 1) % polygon.Count);
                    return new Segment(p, end);
                })
                .ToArray();

            var diagonals = Enumerable.Range(0, polygon.Count)
                .SelectMany(index => this.FindDiagonals(polygon, index, edges))
                .ToArray();

            return diagonals;
        }

        private IEnumerable<Diagonal> FindDiagonals(
            IReadOnlyCollection<Point> polygon,
            int startIndex,
            IReadOnlyCollection<Segment> edges)
        {
            var diagonals = new List<Diagonal>();
            var a = polygon.ElementAt(startIndex);

            var lastIndex = startIndex == 0 ? polygon.Count - 2 : polygon.Count - 1;
            for (var i = startIndex + 2; i <= lastIndex; i++)
            {
                var diagonalSegment = new Segment(a, polygon.ElementAt(i));
                var designation = this.CalculateDesignation(diagonalSegment, startIndex, i, edges);

                diagonals.Add(new Diagonal(diagonalSegment, designation));
            }

            return diagonals;
        }

        private Designation CalculateDesignation(
            Segment diagonal,
            int startPointIndex,
            int endPointIndex,
            IReadOnlyCollection<Segment> edges)
        {
            var previousEdgeIndex = (startPointIndex + edges.Count - 1) % edges.Count;

            for(var i = 0; i < edges.Count; i++)
            {
                if(i == startPointIndex
                   || i == previousEdgeIndex
                   || i == endPointIndex
                   || i == (endPointIndex + edges.Count - 1) % edges.Count)
                {
                    continue;
                }

                var edge = edges.ElementAt(i);
                var it = diagonal.CalculateIntersecitonParameter(edge);
                var it2 = edge.CalculateIntersecitonParameter(diagonal);

                if(it >= 0 && it <= 1
                    && it2 >= 0 && it2 <= 1)
                {
                    return Designation.INTERSECT;
                }
            }

            var nextNeighour = edges.ElementAt(startPointIndex).B;
            var previousNeighour = edges.ElementAt(previousEdgeIndex).A;

            var isInArc = IsInArc(previousNeighour, diagonal.A, nextNeighour, diagonal.B);

            return isInArc
                ? Designation.INTERNAL
                : Designation.EXTERNAL;
        }

        private bool IsInArc(Point a, Point b, Point c, Point x)
        {
            var area2 = Point.Area2(a, b, c);
            var isInArc = area2 >= 0
                ? IsInConvexArc(a, b, c, x)
                : IsInConcaveArc(a, b, c, x);

            return isInArc;
        }

        private bool IsInConvexArc(Point a, Point b, Point c, Point x)
        {
            var a1 = Point.Area2(a, b, x);
            var a2 = Point.Area2(b, c, x);

            return a1 > 0 && a2 > 0;
        }

        private bool IsInConcaveArc(Point a, Point b, Point c, Point x)
        {
            var a1 = Point.Area2(a, b, x);
            var a2 = Point.Area2(b, c, x);

            return !(a1 < 0 && a2 < 0);
        }
    }

    public static class Tools
    {
        public static IReadOnlyCollection<Point> ToPolygon(this string vertexCoordinates)
        {
            var coords = vertexCoordinates.Trim().Split(' ').ToArray();
            var points = new List<Point>();
            for(var i = 0; i < coords.Length; i = i + 2)
            {
                points.Add(new Point(Convert.ToInt64(coords[i]), Convert.ToInt64(coords[i + 1])));
            }

            return points;
        }
    }
}
