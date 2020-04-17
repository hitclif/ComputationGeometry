using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace VerticalHorizontalSegments
{
    class Program
    {
        static void Main(string[] args)
        {
#if td1
            var segments = TestData1();
#else
            var segments = ReadSegments();
#endif

            var intersections = segments.CountIntersections();
            Console.WriteLine(intersections);

#if local
            Console.ReadLine();
#endif
        }

        private static IEnumerable<Segment> TestData1()
        {
            return new[]
            {
                //new Segment(2,1,3,1),
                //new Segment(2,1,3,1),
                //new Segment(2,1,2,2),
                new Segment(-10,3,-5,3),
                new Segment(-5,0,-5,8),
                new Segment(-7,4,-7,7),
                new Segment(-7,7,-5,7),
                new Segment(-8,5,-8,6),
                new Segment(-8,5,-5,5),
                new Segment(-8,6,-3,6)
            };
        }

        private static IEnumerable<Segment> TestData2()
        {
            return new[]
            {
                new Segment(22,-4,22,-12),
                new Segment(- 14,1,-5,1),
                new Segment(- 23,-3,-6,-3),
                new Segment(- 6,13,-6,12),
                new Segment(- 1,19,1,19),
                new Segment(- 17,4,12,4),
                new Segment(- 15,15,-15,11),
                new Segment(- 6,-22,2,-22),
            };
        }

        private static IEnumerable<Segment> ReadSegments()
        {
            var segments = new List<Segment>();

            var count = Convert.ToInt32(Console.ReadLine());
            for(var i = 0; i < count; i++)
            {
                var coordinates = Console
                    .ReadLine()
                    .Trim()
                    .Split(' ')
                    .Select(s => Convert.ToInt64(s))
                    .ToArray();

                segments.Add(new Segment(coordinates));
            }

            return segments;
        }
    }

    [DebuggerDisplay("{X},{Y}")]
    internal struct Point
    {
        public Point(long x, long y)
        {
            this.X = x;
            this.Y = y;
        }

        public long X { get; }
        public long Y { get; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null) || !(obj is Point))
            {
                return false;
            }

            var point = (Point)obj;
            return point == this;
        }

        public static bool operator ==(Point a, Point b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        public static bool operator !=(Point a, Point b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        public static long operator *(Point a, Point b)
        {
            return a.X * b.X + a.Y * b.Y;
        }
    }

    [DebuggerDisplay("{A},{B}")]
    internal struct Segment
    {
        public Segment(long[] coordinates): this(coordinates[0], coordinates[1], coordinates[2], coordinates[3])
        {

        }

        public Segment(long ax, long ay, long bx, long by) : this(new Point(ax, ay), new Point(bx, by))
        {

        }

        public Segment(Point A, Point B)
        {
            this.A = A;
            this.B = B;
        }

        public Point A { get; }
        public Point B { get; }

        public Segment Perpendicular()
        {
            return new Segment(this.A, new Point(B.Y, -B.X));
        }

        public Point CalculatePointOnLine(double t)
        {
            var x = A.X + (B.X - A.X) * t;
            var y = A.Y + (B.Y - A.Y) * t;

            return new Point((long)x, (long)y);
        }
    }

    enum Position
    {
        Left,
        Right,
        CollinearOutside,
        CollinearInside,
        IsEndPoint
    }

    [Flags]
    enum IntersectionPoint
    {
        None = 0,
        V1 = 1,
        V2 = 2,
        V3 = 4,
        V4 = 8
    }

    abstract class Intersection
    {
        public abstract int Weight { get; }
    }

    class EmptyIntersection : Intersection
    {
        public override int Weight => 0;

        public override string ToString()
        {
            return "No common points.";
        }
    }

    class SinglePointIntersection : Intersection
    {
        private readonly double _x;
        private readonly double _y;

        public SinglePointIntersection(Point point) : this(point.X, point.Y)
        {
        }

        public SinglePointIntersection(long x, long y)
        {
            _x = x;
            _y = y;
        }

        public override int Weight => 1;

        public override string ToString()
        {
            return $"The intersection point is ({_x}, {_y}).";
        }
    }

    class SegmentIntersection : Intersection
    {
        public SegmentIntersection()
        {
        }

        public override int Weight => int.MaxValue;

        public override string ToString()
        {
            return "A common segment of non-zero length.";
        }
    }

    internal static class Tools
    {
        public static int CountIntersections(this IEnumerable<Segment> segments)
        {
            var temp = segments.ToList();

            var count = 0;

            while(temp.Count > 0)
            {
                var segment = temp[0];
                temp.RemoveAt(0);

                var i = temp
                    .Select(s => Intersection(s, segment))
                    .Where(s => s is SinglePointIntersection || s is SegmentIntersection)
                    .Count();

                count += i;
            }

            return count;
        }

        public static Intersection Intersection(Segment u, Segment v)
        {
            var areCollinear = u.IsCollinear(v);
            var intersection = areCollinear
                ? CalculateIntersectionOfCollinearSegments(u, v)
                : CalculateIntersectionOfNonCollinearSegments(u, v);

            return intersection;
        }

        private static Position GetPointPositionOf(this Segment v, Point point)
        {
            var doubledArea = (v.B.X - v.A.X) * (point.Y - v.A.Y) - (point.X - v.A.X) * (v.B.Y - v.A.Y);
            if (doubledArea == 0)
            {
                if (v.A == point || v.B == point)
                {
                    return Position.IsEndPoint;
                }

                return point.IsOnSegment(v)
                    ? Position.CollinearInside
                    : Position.CollinearOutside;
            }

            return doubledArea < 0
                ? Position.Left
                : Position.Right;
        }

        private static bool IsCollinear(this Segment u, Segment v)
        {
            var a = u.GetPointPositionOf(v.A);
            var b = u.GetPointPositionOf(v.B);

            return a.IsCollinearPosition() && b.IsCollinearPosition();
        }

        private static bool IsCollinearPosition(this Position position)
        {
            return position == Position.CollinearInside || position == Position.CollinearOutside || position == Position.IsEndPoint;
        }

        private static bool IsOnSegment(this Point point, Segment v)
        {
            var isOnSegment = point.X <= Math.Max(v.A.X, v.B.X)
                          && point.X >= Math.Min(v.A.X, v.B.X)
                          && point.Y <= Math.Max(v.A.Y, v.B.Y)
                          && point.Y >= Math.Min(v.A.Y, v.B.Y);

            return isOnSegment;
        }

        private static Intersection CalculateIntersectionOfNonCollinearSegments(Segment u, Segment v)
        {
            var si = u.CalculateIntersectionParameter(v);
            var ti = v.CalculateIntersectionParameter(u);

            if (si.IsInsideSegment() && ti.IsInsideSegment())
            {
                var i = u.CalculatePointOnLine(si);
                return new SinglePointIntersection(i);
            }

            return new EmptyIntersection();
        }

        private static Intersection CalculateIntersectionOfCollinearSegments(Segment u, Segment v)
        {
            var positions = new[]
            {
                new { pos = v.GetPointPositionOf(u.A) , point = u.A },
                new { pos = v.GetPointPositionOf(u.B) , point = u.B },
                new { pos = u.GetPointPositionOf(v.A) , point = v.A },
                new { pos = u.GetPointPositionOf(v.B) , point = v.B }
            };

            int inside = 0;
            int outside = 0;
            int endpoint = 0;

            foreach (var p in positions)
            {
                switch (p.pos)
                {
                    case Position.CollinearOutside:
                        outside++;
                        break;
                    case Position.CollinearInside:
                        inside++;
                        break;
                    case Position.IsEndPoint:
                        endpoint++;
                        break;
                    default:
                        throw new Exception();
                }
            }

            if (outside == 4)
            {
                return new EmptyIntersection();
            }

            if (inside > 0 || endpoint == 4)
            {
                return new SegmentIntersection();
            }

            if (endpoint == 2)
            {
                var point = positions.First(p => p.pos == Position.IsEndPoint).point;
                return new SinglePointIntersection(point);
            }

            throw new Exception();
        }

        private static double CalculateIntersectionParameter(this Segment u, Segment v)
        {
            var p1 = u.A;
            var p2 = u.B;
            var p3 = v.A;
            var p4 = v.B;
            var t = 1d *
                ((p1.X - p3.X) * (p3.Y - p4.Y) - (p1.Y - p3.Y) * (p3.X - p4.X))
                /
                ((p1.X - p2.X) * (p3.Y - p4.Y) - (p1.Y - p2.Y) * (p3.X - p4.X));

            return t;
        }

        private static bool IsInsideSegment(this double d) => d >= 0 && d <= 1;
    }

    //[DebuggerDisplay("{X},{Y}")]
    //internal struct Point : IEquatable<Point>
    //{
    //    public Point(long x, long y)
    //    {
    //        this.X = x;
    //        this.Y = y;
    //    }

    //    public long X { get; }
    //    public long Y { get; }

    //    public bool Equals(Point other)
    //    {
    //        return other.X == this.X && other.Y == this.Y;
    //    }
    //}

    //[DebuggerDisplay("{}")]
    //internal struct Segment
    //{
    //    public Segment(long[] coordinates) : this(coordinates[0], coordinates[1], coordinates[2], coordinates[3])
    //    {
    //    }

    //    public Segment(long ax, long ay, long bx, long by) : this(new Point(ax, ay), new Point(bx, by))
    //    {
    //    }

    //    public Segment(Point a, Point b)
    //    {
    //        if(a.X == b.X && a.Y == b.Y)
    //        {
    //            throw new Exception("single point");
    //        }

    //        if(a.X != b.X && a.Y != b.Y)
    //        {
    //            throw new Exception("not horizontal neither vertical");
    //        }

    //        this.A = a;
    //        this.B = b;
    //    }

    //    public Point A { get; }
    //    public Point B { get; }

    //    public Point Left => this.A.X <= this.B.X ? this.A : this.B;
    //    public Point Right => this.A.X > this.B.X ? this.A : this.B;

    //    public Point Bottom => this.A.Y <= this.B.Y ? this.A : this.B;
    //    public Point Top => this.A.Y > this.B.Y ? this.A : this.B;

    //    public bool IsHorizontal => this.A.Y == this.B.Y;
    //    public bool IsVertical => this.A.X == this.B.X;
    //}

    //internal static class Tools
    //{
    //    public static int CountIntersections(this IEnumerable<Segment> segments)
    //    {
    //        var count = 0;
    //        var all = segments.ToList();
    //        while (all.Count > 0)
    //        {
    //            var segment = all[0];
    //            all.RemoveAt(0);

    //            var intersections = all.Count(s => segment.Insersects(s));
    //            count += intersections;
    //        }

    //        return count;
    //    }

    //    public static int CountInterserctionsBinary(this IEnumerable<Segment> segments)
    //    {
    //        var segmentGroupComparer = new SegmentGroupComparer();
    //        var groups = segments
    //            .Where(s => s.IsVertical)
    //            .GroupBy(s => s.Left.X, s => s, (c, s) =>
    //            {
    //                return new SegmentGroup(s.ToArray(), c);
    //            })
    //            .ToList();

    //        groups.Sort(segmentGroupComparer);

    //        var horizontalSegments = segments
    //            .Where(s => s.IsHorizontal)
    //            .ToArray();

    //        var count = 0;
    //        foreach (var h in horizontalSegments)
    //        {
    //            var temp = groups
    //                .Where(g => g.Coordinate >= h.Left.X && g.Coordinate <= h.Right.X)
    //                .SelectMany(g => g.Segments)
    //                .Where(v => v.Top.Y >= h.Top.Y && v.Bottom.Y <= h.Top.Y)
    //                .ToArray();

    //            count += temp.Length;
    //        }

    //        return count;
    //    }

    //    private static bool IsBetween(this long v, long a, long b)
    //    {
    //        return a < b
    //            ? v >= a && v <= b
    //            : v >= b && v <= a;
    //    }

    //    private static bool Insersects(this Segment segment, Segment other)
    //    {
    //        if(segment.IsHorizontal == other.IsHorizontal)
    //        {
    //            return ParallelIntersection(segment, other);
    //        }

    //        return PerpendicularIntersection(segment, other);
    //    }

    //    private static bool PerpendicularIntersection(Segment u, Segment v)
    //    {
    //        var horizontal = u.IsHorizontal ? u : v;
    //        var vertical = u.IsHorizontal ? v : u;

    //        return horizontal.A.Y.IsBetween(vertical.A.Y, vertical.B.Y)
    //            && vertical.A.X.IsBetween(horizontal.A.X, horizontal.B.X);
    //    }

    //    private static bool ParallelIntersection(Segment u, Segment v)
    //    {
    //        return u.IsHorizontal
    //            ? HorizontalsIntersection(u, v)
    //            : VerticalsIntersection(u, v);
    //    }

    //    private static bool HorizontalsIntersection(Segment segment, Segment other)
    //    {
    //        if (segment.A.Y == other.A.Y)
    //        {
    //            if (segment.A.X.IsBetween(other.A.X, other.B.X)
    //                || segment.B.X.IsBetween(other.A.X, other.B.X)
    //                || other.A.X.IsBetween(segment.A.X, segment.B.X)
    //                || other.B.X.IsBetween(segment.A.X, segment.B.X))
    //            {
    //                throw new Exception("Intersection between horizontal segments");
    //            }

    //            return false;
    //        }
    //        else
    //        {
    //            return false;
    //        }
    //    }

    //    private static bool VerticalsIntersection(Segment segment, Segment other)
    //    {
    //        if (segment.A.X == other.A.X)
    //        {
    //            if (segment.A.Y.IsBetween(other.A.Y, other.B.Y)
    //                || segment.B.Y.IsBetween(other.A.Y, other.B.Y)
    //                || other.A.Y.IsBetween(segment.A.Y, segment.B.Y)
    //                || other.B.Y.IsBetween(segment.A.Y, segment.B.Y))
    //            {
    //                throw new Exception("Intersection between horizontal segments");
    //            }

    //            return false;
    //        }
    //        else
    //        {
    //            return false;
    //        }
    //    }

    //    //private static IEnumerable<Segment> Find(this IReadOnlyCollection<ValuedSegment> sortedSegments, long minValue, long maxValue)
    //    //{
    //    //    return new Segment[0];
    //    //}

    //    //private static int FindFirstIndex(this IReadOnlyCollection<ValuedSegment> sortedSegments, long minValue, long maxValue)
    //    //{
    //    //}
    //}

    //internal class HorizontalComparer : IComparer<Segment>
    //{
    //    public int Compare(Segment u, Segment v)
    //    {
    //        return Math.Sign(u.Left.X - v.Left.X);
    //    }
    //}

    //internal class VerticalComparer : IComparer<Segment>
    //{
    //    public int Compare(Segment u, Segment v)
    //    {
    //        return Math.Sign(u.Bottom.Y - v.Bottom.Y);
    //    }
    //}

    //internal class SegmentGroupComparer : IComparer<SegmentGroup>
    //{
    //    public int Compare(SegmentGroup x, SegmentGroup y)
    //    {
    //        return Math.Sign(x.Coordinate - y.Coordinate);
    //    }
    //}

    //internal class SegmentGroup
    //{
    //    public SegmentGroup(Segment[] segments, long coordinate)
    //    {
    //        this.Segments = segments;
    //        this.Coordinate = coordinate;
    //    }

    //    public Segment[] Segments { get; }
    //    public long Coordinate { get; }
    //}
}
