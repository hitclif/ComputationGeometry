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
#elif oneline
            while(true)
            {
                var pair = ReadSegments();
                var intersection = Tools.Intersection(pair.Item1, pair.Item2);
                Console.WriteLine(intersection.ToString());
            }
#else
            var u = ReadSegment();
            var v = ReadSegment();

            var intersection = Tools.Intersection(u, v);
            Console.WriteLine(intersection.ToString());
            Console.ReadLine();
#endif

        }

        private static Segment ReadSegment()
        {
            var line = Console.ReadLine().Trim().Split(' ').ToArray();
            return new Segment(
                new Point(Convert.ToInt64(line[0]), Convert.ToInt64(line[1])),
                new Point(Convert.ToInt64(line[2]), Convert.ToInt64(line[3])));
        }

        private static Tuple<Segment,Segment> ReadSegments()
        {
            var line = Console.ReadLine().Trim().Split(' ').ToArray();

            var u = new Segment(
                new Point(Convert.ToInt64(line[0]), Convert.ToInt64(line[1])),
                new Point(Convert.ToInt64(line[2]), Convert.ToInt64(line[3])));

            var v = new Segment(
                new Point(Convert.ToInt64(line[4]), Convert.ToInt64(line[5])),
                new Point(Convert.ToInt64(line[6]), Convert.ToInt64(line[7])));

            return new Tuple<Segment, Segment>(u, v);
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
            if(ReferenceEquals(obj, null) || !(obj is Point))
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
            if(doubledArea == 0)
            {
                if(v.A == point || v.B == point)
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

            if(si.IsInsideSegment() && ti.IsInsideSegment())
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

            if(outside == 4)
            {
                return new EmptyIntersection();
            }

            if(inside > 0 || endpoint == 4)
            {
                return new SegmentIntersection();
            }

            if(endpoint == 2)
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
}
