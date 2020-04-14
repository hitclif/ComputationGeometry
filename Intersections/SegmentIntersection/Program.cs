using System;
using System.Diagnostics;
using System.Linq;

namespace SegmentIntersection
{
    class Program
    {
        static void Main(string[] args)
        {
#if test
            //var u = new Segment(new Point(0, 0), new Point(5, 5));
            //var v = new Segment(new Point(0, 6), new Point(6, 0));

            //var u = new Segment(new Point(0, 0), new Point(5, 5));
            //var v = new Segment(new Point(5, 0), new Point(3, 2));

            //var u = new Segment(new Point(0, 0), new Point(10, 0));
            //var v = new Segment(new Point(1, 0), new Point(2, 0));

            var u = new Segment(new Point(0, 0), new Point(10, 0));
            var v = new Segment(new Point(5, 0), new Point(15, 0));
#else
            var u = ReadSegment();
            var v = ReadSegment();
#endif
            var intersection = Tools.Intersection(u, v);
            Console.WriteLine(intersection.ToString());
            Console.ReadLine();
        }

        private static Segment ReadSegment()
        {
            var line = Console.ReadLine().Trim().Split(' ').ToArray();
            return new Segment(
                new Point(Convert.ToInt64(line[0]), Convert.ToInt64(line[1])),
                new Point(Convert.ToInt64(line[2]), Convert.ToInt64(line[3])));
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

        public long Multiply(Point point)
        {
            return this.X * point.X + this.Y * point.Y;
        }

        public static long operator *(Point a, Point b)
        {
            return a.X * b.X + a.Y * b.Y;
        }
    }

    [DebuggerDisplay("{A},{B}")]
    internal struct Segment
    {
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
        Collinear
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
        public static Intersection Intersection(Segment u, Segment v)
        {
            var aPosition = u.GetPointPositionOf(v.A);
            switch (aPosition)
            {
                case Position.Left:
                    return IntersectionForFirstIsLeft(u, v);
                case Position.Right:
                    return IntersectionForFirstIsRight(u, v);
                case Position.Collinear:
                    return IntersectionForFirstIsCollinear(u, v);
            }

            return new EmptyIntersection();
        }

        private static Intersection IntersectionForFirstIsLeft(Segment u, Segment v)
        {
            var bPosition = u.GetPointPositionOf(v.B);
            switch (bPosition)
            {
                case Position.Left:
                case Position.Right:
                    return CalculateIntersectionOfNonCollinearSegments(u, v);
                case Position.Collinear:
                    return v.B.IsOnSegment(u)
                        ? (Intersection)new SinglePointIntersection(v.B)
                        : new EmptyIntersection();
            }

            return new EmptyIntersection();
        }

        private static Intersection IntersectionForFirstIsRight(Segment u, Segment v)
        {
            var bPosition = u.GetPointPositionOf(v.B);
            switch (bPosition)
            {
                case Position.Left:
                case Position.Right:
                    return CalculateIntersectionOfNonCollinearSegments(u, v);
                case Position.Collinear:
                    return v.B.IsOnSegment(u)
                        ? (Intersection)new SinglePointIntersection(v.B)
                        : new EmptyIntersection();
            }

            return new EmptyIntersection();
        }

        private static Intersection IntersectionForFirstIsCollinear(Segment u, Segment v)
        {
            var bPosition = u.GetPointPositionOf(v.B);
            switch (bPosition)
            {
                case Position.Left:
                case Position.Right:
                    return CalculateIntersectionOfNonCollinearSegments(u, v);
                case Position.Collinear:
                    return CalculateIntersectionOfCollinearSegments(u, v);
            }

            return new EmptyIntersection();
        }

        private static Position GetPointPositionOf(this Segment v, Point point)
        {
            var doubledArea = (v.B.X - v.A.X) * (point.Y - v.A.Y) - (point.X - v.A.X) * (v.B.Y - v.A.Y);
            if(doubledArea == 0)
            {
                return Position.Collinear;
            }

            return doubledArea < 0
                ? Position.Left
                : Position.Right;
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

            if(si.IsInsideSegment() && ti.IsInsideSegment())
            {
                var i = u.CalculatePointOnLine(si);
                return new SinglePointIntersection(i);
            }

            return new EmptyIntersection();
        }

        private static Intersection CalculateIntersectionOfCollinearSegments(Segment u, Segment v)
        {
            var intersections = IntersectionPoint.None
                | (v.A.IsOnSegment(u) ? IntersectionPoint.V1 : IntersectionPoint.None)
                | (v.B.IsOnSegment(u) ? IntersectionPoint.V2 : IntersectionPoint.None)
                | (u.A.IsOnSegment(v) ? IntersectionPoint.V3 : IntersectionPoint.None)
                | (u.B.IsOnSegment(v) ? IntersectionPoint.V4 : IntersectionPoint.None)
                ;

            switch (intersections)
            {
                case IntersectionPoint.None:
                    return new EmptyIntersection();
                case IntersectionPoint.V1:
                    return new SinglePointIntersection(v.A);
                case IntersectionPoint.V2:
                    return new SinglePointIntersection(v.B);
                case IntersectionPoint.V3:
                    return new SinglePointIntersection(u.A);
                case IntersectionPoint.V4:
                    return new SinglePointIntersection(u.B);
                default:
                    return new SegmentIntersection();
            }
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
}
