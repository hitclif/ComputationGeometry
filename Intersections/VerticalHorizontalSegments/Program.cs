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
            var segments = TestData2();
#else
            var segments = ReadSegments();
#endif

            var intersections = segments.CountInterserctionsBinary();
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
    internal struct Point : IEquatable<Point>
    {
        public Point(long x, long y)
        {
            this.X = x;
            this.Y = y;
        }

        public long X { get; }
        public long Y { get; }

        public bool Equals(Point other)
        {
            return other.X == this.X && other.Y == this.Y;
        }
    }

    [DebuggerDisplay("{}")]
    internal struct Segment
    {
        public Segment(long[] coordinates) : this(coordinates[0], coordinates[1], coordinates[2], coordinates[3])
        {
        }

        public Segment(long ax, long ay, long bx, long by) : this(new Point(ax, ay), new Point(bx, by))
        {
        }

        public Segment(Point a, Point b)
        {
            if(a.X == b.X && a.Y == b.Y)
            {
                throw new Exception("single point");
            }

            if(a.X != b.X && a.Y != b.Y)
            {
                throw new Exception("not horizontal neither vertical");
            }

            this.A = a;
            this.B = b;
        }

        public Point A { get; }
        public Point B { get; }

        public Point Left => this.A.X <= this.B.X ? this.A : this.B;
        public Point Right => this.A.X > this.B.X ? this.A : this.B;

        public Point Bottom => this.A.Y <= this.B.Y ? this.A : this.B;
        public Point Top => this.A.Y > this.B.Y ? this.A : this.B;

        public bool IsHorizontal => this.A.Y == this.B.Y;
        public bool IsVertical => this.A.X == this.B.X;
    }

    internal static class Tools
    {
        public static int CountIntersections(this IEnumerable<Segment> segments)
        {
            var count = 0;
            var all = segments.ToList();
            while (all.Count > 0)
            {
                var segment = all[0];
                all.RemoveAt(0);

                var intersections = all.Count(s => segment.Insersects(s));
                count += intersections;
            }

            return count;
        }

        public static int CountInterserctionsBinary(this IEnumerable<Segment> segments)
        {
            var segmentGroupComparer = new SegmentGroupComparer();
            var groups = segments
                .Where(s => s.IsVertical)
                .GroupBy(s => s.Left.X, s => s, (c, s) =>
                {
                    return new SegmentGroup(s.ToArray(), c);
                })
                .ToList();

            groups.Sort(segmentGroupComparer);

            var horizontalSegments = segments
                .Where(s => s.IsHorizontal)
                .ToArray();

            var count = 0;
            foreach (var h in horizontalSegments)
            {
                var temp = groups
                    .Where(g => g.Coordinate >= h.Left.X && g.Coordinate <= h.Right.X)
                    .SelectMany(g => g.Segments)
                    .Where(v => v.Top.Y >= h.Top.Y && v.Bottom.Y <= h.Top.Y)
                    .ToArray();

                count += temp.Length;
            }

            return count;
        }

        private static bool IsBetween(this long v, long a, long b)
        {
            return a < b
                ? v >= a && v <= b
                : v >= b && v <= a;
        }

        private static bool Insersects(this Segment segment, Segment other)
        {
            if(segment.IsHorizontal == other.IsHorizontal)
            {
                return ParallelIntersection(segment, other);
            }

            return PerpendicularIntersection(segment, other);
        }

        private static bool PerpendicularIntersection(Segment u, Segment v)
        {
            var horizontal = u.IsHorizontal ? u : v;
            var vertical = u.IsHorizontal ? v : u;

            return horizontal.A.Y.IsBetween(vertical.A.Y, vertical.B.Y)
                && vertical.A.X.IsBetween(horizontal.A.X, horizontal.B.X);
        }

        private static bool ParallelIntersection(Segment u, Segment v)
        {
            return u.IsHorizontal
                ? HorizontalsIntersection(u, v)
                : VerticalsIntersection(u, v);
        }

        private static bool HorizontalsIntersection(Segment segment, Segment other)
        {
            if (segment.A.Y == other.A.Y)
            {
                if (segment.A.X.IsBetween(other.A.X, other.B.X)
                    || segment.B.X.IsBetween(other.A.X, other.B.X)
                    || other.A.X.IsBetween(segment.A.X, segment.B.X)
                    || other.B.X.IsBetween(segment.A.X, segment.B.X))
                {
                    throw new Exception("Intersection between horizontal segments");
                }

                return false;
            }
            else
            {
                return false;
            }
        }

        private static bool VerticalsIntersection(Segment segment, Segment other)
        {
            if (segment.A.X == other.A.X)
            {
                if (segment.A.Y.IsBetween(other.A.Y, other.B.Y)
                    || segment.B.Y.IsBetween(other.A.Y, other.B.Y)
                    || other.A.Y.IsBetween(segment.A.Y, segment.B.Y)
                    || other.B.Y.IsBetween(segment.A.Y, segment.B.Y))
                {
                    throw new Exception("Intersection between horizontal segments");
                }

                return false;
            }
            else
            {
                return false;
            }
        }

        //private static IEnumerable<Segment> Find(this IReadOnlyCollection<ValuedSegment> sortedSegments, long minValue, long maxValue)
        //{
        //    return new Segment[0];
        //}

        //private static int FindFirstIndex(this IReadOnlyCollection<ValuedSegment> sortedSegments, long minValue, long maxValue)
        //{
        //}
    }

    internal class HorizontalComparer : IComparer<Segment>
    {
        public int Compare(Segment u, Segment v)
        {
            return Math.Sign(u.Left.X - v.Left.X);
        }
    }

    internal class VerticalComparer : IComparer<Segment>
    {
        public int Compare(Segment u, Segment v)
        {
            return Math.Sign(u.Bottom.Y - v.Bottom.Y);
        }
    }

    internal class SegmentGroupComparer : IComparer<SegmentGroup>
    {
        public int Compare(SegmentGroup x, SegmentGroup y)
        {
            return Math.Sign(x.Coordinate - y.Coordinate);
        }
    }

    internal class SegmentGroup
    {
        public SegmentGroup(Segment[] segments, long coordinate)
        {
            this.Segments = segments;
            this.Coordinate = coordinate;
        }

        public Segment[] Segments { get; }
        public long Coordinate { get; }
    }
}
