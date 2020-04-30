using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PolygonIntersection
{
    class Program
    {
        static void Main(string[] args)
        {
            var polygon1 = ReadPolygon().ToList();
            var polygon2 = ReadPolygon().ToList();

            var intersectionPolygon = new PolygonIntersectionFinder()
                .FindIntersection(polygon1, polygon2)
                .ToArray();

            Console.WriteLine(intersectionPolygon.Length);
            Console.WriteLine(string.Join(" ", intersectionPolygon.Select(p => p.ToString()).ToArray()));
        }

        private static IEnumerable<Point> ReadPolygon()
        {
            var count = Convert.ToInt32(Console.ReadLine());
            return Tools.PolygonSegmentsFromStringCoordinates(Console.ReadLine());
        }
    }

    [DebuggerDisplay("{X},{Y}")]
    public struct Point
    {
        public Point(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }

        public double X { get; }
        public double Y { get; }

        //public override bool Equals(object obj)
        //{
        //    if (ReferenceEquals(obj, null) || !(obj is Point))
        //    {
        //        return false;
        //    }

        //    var point = (Point)obj;
        //    return point == this;
        //}

        //public static bool operator ==(Point a, Point b)
        //{
        //    return a.X == b.X && a.Y == b.Y;
        //}

        //public static bool operator !=(Point a, Point b)
        //{
        //    return a.X == b.X && a.Y == b.Y;
        //}

        public override string ToString()
        {
            return this.X + " " + this.Y;
        }
    }

    [DebuggerDisplay("{A},{B}")]
    public struct Segment
    {
        public Segment(Point A, Point B)
        {
            this.A = A;
            this.B = B;
        }

        public Point A { get; }
        public Point B { get; }

        public Point CalculatePointOnLine(double t)
        {
            var x = A.X + (B.X - A.X) * t;
            var y = A.Y + (B.Y - A.Y) * t;

            return new Point(x, y);
        }
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

    //class SinglePointIntersection : Intersection
    //{
    //    private readonly double _x;
    //    private readonly double _y;

    //    public SinglePointIntersection(Point point) : this(point.X, point.Y)
    //    {
    //    }

    //    public SinglePointIntersection(long x, long y)
    //    {
    //        _x = x;
    //        _y = y;
    //    }

    //    public override int Weight => 1;

    //    public override string ToString()
    //    {
    //        return $"The intersection point is ({_x}, {_y}).";
    //    }
    //}

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

    enum Position
    {
        Left,
        Right,
        CollinearOutside,
        CollinearInside,
        IsEndPoint
    }

    [Flags]
    enum FutureInclude
    {
        None,
        A,
        B
    }

    public class PolygonIntersectionFinder
    {
        public IEnumerable<Point> FindIntersection(
            IReadOnlyCollection<Point> polygon1,
            IReadOnlyCollection<Point> polygon2)
        {
            var intersectionPoints = polygon2;

#if test
            var minY = polygon1.Union(polygon2).Min(p => p.Y);
            var maxY = polygon1.Union(polygon2).Max(p => p.Y);
            var height = Math.Abs(maxY - minY);
#endif

            for (var i = 0; i < polygon1.Count; i++)
            {
                var halfplaneSegment = new Segment(polygon1.ElementAt(i), polygon1.ElementAt((i + 1) % polygon1.Count));
                intersectionPoints = this.InterserctionWithHalfPlane(intersectionPoints, halfplaneSegment);

#if test
                Console.WriteLine();
                Console.WriteLine(i);
                Console.WriteLine("Halfplane {0},{1}", halfplaneSegment.A, halfplaneSegment.B);
                Console.WriteLine("Intersection: {0}", string.Join(" ", intersectionPoints.ToArray()));
                var e = intersectionPoints
                    .Select(p => p.X.ToString() + "," + (height - p.Y + 1).ToString())
                    .ToArray();
                Console.WriteLine("<polygon points='{0}' style='fill:none;stroke:purple;stroke-width:1' />", string.Join(" ", e));
#endif
            }

            return intersectionPoints
                .Select(p => new Point(Math.Round(p.X), Math.Round(p.Y)))
                .ToArray();
        }

        private IReadOnlyCollection<Point> InterserctionWithHalfPlane(IReadOnlyCollection<Point> polygon, Segment segment)
        {
            if(polygon.Count == 0)
            {
                return polygon;
            }

            var intersectionPolygon = new List<Point>();

            var previousPoint = polygon.ElementAt(0);
            var previousPointPosition = segment.GetPointPositionOf(previousPoint);

            if (previousPointPosition != Position.Right)
            {
                intersectionPolygon.Add(previousPoint);
            }

            var future = this.GetFuture(previousPoint, previousPointPosition, double.NaN, segment);

            for(var i = 1; i < polygon.Count; i++)
            {
                var point2 = polygon.ElementAt(i);
                var position2 = segment.GetPointPositionOf(point2);
                future = this.UpdateIntersections(
                    intersectionPolygon,
                    previousPoint,
                    previousPointPosition,
                    point2,
                    position2,
                    segment,
                    future,
                    false);

                previousPoint = point2;
                previousPointPosition = position2;
            }

            if(polygon.Count > 1)
            {
                var point2 = polygon.ElementAt(0);
                var position2 = segment.GetPointPositionOf(point2);

                this.UpdateIntersections(
                   intersectionPolygon,
                   previousPoint,
                   previousPointPosition,
                   point2,
                   position2,
                   segment,
                   future,
                   true);
            }

            return intersectionPolygon;
        }

        private FutureInclude UpdateIntersections(
            List<Point> intersections,
            Point previous,
            Position previousPosition,
            Point current,
            Position currentPosition,
            Segment segment,
            FutureInclude future,
            bool ignoreCurrent)
        {
            var t = segment.CalculateIntersectionParameter(previous, current);

            switch (currentPosition)
            {
                case Position.Left:
                    if (previousPosition == Position.Right)
                    {
                        if(future.HasFlag(FutureInclude.A) && t > 0)
                        {
                            intersections.Add(segment.A);
                        }

                        if(future.HasFlag(FutureInclude.B) && t > 1)
                        {
                            intersections.Add(segment.B);
                        }

                        var intersectionPoint = segment.CalculatePointOnLine(t);
                        intersections.Add(intersectionPoint);
                    }

                    if (!ignoreCurrent)
                    {
                        intersections.Add(current);
                    }
                    break;
                case Position.Right:
                    if (previousPosition == Position.Left)
                    {
                        var intersectionPoint = segment.CalculatePointOnLine(t);
                        intersections.Add(intersectionPoint);
                    }
                    break;
                case Position.CollinearOutside:
                case Position.CollinearInside:
                case Position.IsEndPoint:
                    if (!ignoreCurrent)
                    {
                        intersections.Add(current);
                    }
                    break;
                default:
                    throw new Exception("");
            }

            return previousPosition == currentPosition
                ? future
                : this.GetFuture(current, currentPosition, t, segment);
        }

        private FutureInclude GetFuture(Point current, Position position, double t, Segment segment)
        {
            switch (position)
            {
                case Position.Left:
                    return FutureInclude.None;
                case Position.Right:
                    var r = FutureInclude.None;

                    if (t < 0)
                    {
                        r |= FutureInclude.A;
                    }

                    if (t < 1)
                    {
                        r |= FutureInclude.B;
                    }
                    return r;
                case Position.CollinearOutside:
                case Position.CollinearInside:
                case Position.IsEndPoint:
                    return (int)Math.Round(t) == 0
                        ? FutureInclude.B
                        : FutureInclude.None;
                default:
                    throw new Exception("");
            }
        }
    }

    public static class Tools
    {
        public static IEnumerable<Point> PolygonSegmentsFromStringCoordinates(string coordinates)
        {
            var polygon = new List<Point>();

            if (string.IsNullOrWhiteSpace(coordinates))
            {
                return polygon;
            }

            var line = coordinates.Trim().Split(' ').ToArray();
            for (var i = 0; i < line.Length; i = i + 2)
            {
                polygon.Add(new Point(Convert.ToInt64(line[i]), Convert.ToInt64(line[i + 1])));
            }

            return polygon;
        }

        //internal static Intersection Intersection(Segment u, Segment v)
        //{
        //    var areCollinear = u.IsCollinear(v);
        //    var intersection = areCollinear
        //        ? CalculateIntersectionOfCollinearSegments(u, v)
        //        : CalculateIntersectionOfNonCollinearSegments(u, v);

        //    return intersection;
        //}

        internal static Position GetPointPositionOf(this Segment v, Point point)
        {
            var doubledArea = (v.B.X - v.A.X) * (point.Y - v.A.Y) - (point.X - v.A.X) * (v.B.Y - v.A.Y);
            if (doubledArea == 0)
            {
                return Position.CollinearInside;

                //if (v.A == point || v.B == point)
                //{
                //    return Position.IsEndPoint;
                //}

                //return point.IsOnSegment(v)
                //    ? Position.CollinearInside
                //    : Position.CollinearOutside;
            }

            return doubledArea > 0
                ? Position.Left
                : Position.Right;
        }

        private static bool IsCollinear(this Segment u, Segment v)
        {
            var a = u.GetPointPositionOf(v.A);
            var b = u.GetPointPositionOf(v.B);

            return a.IsCollinearPosition() && b.IsCollinearPosition();
        }

        internal static bool IsCollinearPosition(this Position position)
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

        //private static Intersection CalculateIntersectionOfNonCollinearSegments(Segment u, Segment v)
        //{
        //    var si = u.CalculateIntersectionParameter(v);
        //    var ti = v.CalculateIntersectionParameter(u);

        //    if (si.IsInsideSegment() && ti.IsInsideSegment())
        //    {
        //        var i = u.CalculatePointOnLine(si);
        //        return new SinglePointIntersection(i);
        //    }

        //    return new EmptyIntersection();
        //}

        //private static Intersection CalculateIntersectionOfCollinearSegments(Segment u, Segment v)
        //{
        //    var positions = new[]
        //    {
        //        new { pos = v.GetPointPositionOf(u.A) , point = u.A },
        //        new { pos = v.GetPointPositionOf(u.B) , point = u.B },
        //        new { pos = u.GetPointPositionOf(v.A) , point = v.A },
        //        new { pos = u.GetPointPositionOf(v.B) , point = v.B }
        //    };

        //    int inside = 0;
        //    int outside = 0;
        //    int endpoint = 0;

        //    foreach (var p in positions)
        //    {
        //        switch (p.pos)
        //        {
        //            case Position.CollinearOutside:
        //                outside++;
        //                break;
        //            case Position.CollinearInside:
        //                inside++;
        //                break;
        //            case Position.IsEndPoint:
        //                endpoint++;
        //                break;
        //            default:
        //                throw new Exception();
        //        }
        //    }

        //    if (outside == 4)
        //    {
        //        return new EmptyIntersection();
        //    }

        //    if (inside > 0 || endpoint == 4)
        //    {
        //        return new SegmentIntersection();
        //    }

        //    if (endpoint == 2)
        //    {
        //        var point = positions.First(p => p.pos == Position.IsEndPoint).point;
        //        return new SinglePointIntersection(point);
        //    }

        //    throw new Exception();
        //}

        public static double CalculateIntersectionParameter(this Segment u, Segment v)
        {
            return u.CalculateIntersectionParameter(v.A, v.B);
        }

        public static double CalculateIntersectionParameter(this Segment u, Point segmentA, Point segmentB)
        {
            var p1 = u.A;
            var p2 = u.B;
            var p3 = segmentA;
            var p4 = segmentB;

            var t = 1d *
                ((p1.X - p3.X) * (p3.Y - p4.Y) - (p1.Y - p3.Y) * (p3.X - p4.X))
                /
                ((p1.X - p2.X) * (p3.Y - p4.Y) - (p1.Y - p2.Y) * (p3.X - p4.X));

            return Math.Round(t, 6);
        }

        private static bool IsInsideSegment(this double d) => d >= 0 && d <= 1;
    }
}
