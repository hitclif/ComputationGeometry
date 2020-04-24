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

            var intersections = new IntersectionCounter().CountIntersections(segments);
            Console.WriteLine(intersections);

#if local
            Console.ReadLine();
#endif
        }

        private static IEnumerable<Segment> TestData1()
        {
            return new[]
            {
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
    public struct Point : IEquatable<Point>
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

    [DebuggerDisplay("{A}_{B}")]
    public struct Segment
    {
        public Segment(long[] coordinates) : this(coordinates[0], coordinates[1], coordinates[2], coordinates[3])
        {
        }

        public Segment(long ax, long ay, long bx, long by) : this(new Point(ax, ay), new Point(bx, by))
        {
        }

        public Segment(Point a, Point b)
        {
            if (a.X != b.X && a.Y != b.Y)
            {
                throw new Exception("not horizontal neither vertical");
            }

            var points = new[] { a, b }
                .OrderBy(p => p.X)
                .ThenBy(p => p.Y)
                .ToArray();

            this.A = points[0];
            this.B = points[1];
        }

        public Point A { get; }
        public Point B { get; }

        public bool IsHorizontal => this.A.Y == this.B.Y;
        public bool IsVertical => this.A.X == this.B.X;

        public bool ZeroLengthSegment => this.IsHorizontal && this.IsVertical;
    }

    public enum EventType
    {
        Start,
        End
    }

    public class Event
    {
        public Event(Segment segment, EventType eventType)
        {
            this.Segment = segment;
            this.EventType = eventType;
            this.Time = eventType == EventType.Start
                ? segment.A
                : segment.B;
        }

        public Point Time { get; }
        public Segment Segment { get; }
        public EventType EventType { get; }
    }


    public class VerticalSegmentGroup : IComparable<VerticalSegmentGroup>
    {
        public VerticalSegmentGroup(long x) : this(x, new Segment[0])
        {
        }

        public VerticalSegmentGroup(long x, IEnumerable<Segment> verticalSegments)
        {
            this.X = x;
            this.VerticalSegments = verticalSegments.ToArray();
        }

        public long X { get; }
        public Segment[] VerticalSegments { get; }

        public int CompareTo(VerticalSegmentGroup other)
        {
            return Math.Sign(this.X - other.X);
        }
    }

    public class IntersectionCounter
    {
        private SortedSet<VerticalSegmentGroup> _verticals;
        // private SortedSet<Segment> _status = new SortedSet<Segment>(new YComparer());
        // private Queue<Event> _eventQueue = new Queue<Event>();
        private Queue<Segment> _segmentQueue;

        public int CountIntersections(IEnumerable<Segment> segments)
        {
            this.PrepareData(segments.ToArray());

            var count = 0;
            while(_segmentQueue.Count > 0)
            {
                var segment = _segmentQueue.Dequeue();
                var verticals = _verticals.GetViewBetween(new VerticalSegmentGroup(segment.A.X), new VerticalSegmentGroup(segment.B.X));
                foreach(var vg in verticals)
                {
                    foreach(var v in vg.VerticalSegments)
                    {
                        var i = segment.Insersects(v);
                        count += i ? 1 : 0;
                    }
                }
            }

            return count;
        }

        private void PrepareData(IReadOnlyCollection<Segment> segments)
        {
            var groups = segments
                .Where(s => s.IsVertical && !s.IsHorizontal)
                .GroupBy(s => s.A.X, s => s)
                .Select(g => new VerticalSegmentGroup(g.Key, g))
                .ToArray();

            _verticals = new SortedSet<VerticalSegmentGroup>(groups);

            //var events = segments
            //    .Where(s => s.IsHorizontal && !s.IsVertical)
            //    .SelectMany(s => new[] { new Event(s, EventType.Start), new Event(s, EventType.End) })
            //    .OrderBy(e => e.Time.X)
            //    .ThenBy(e => e.EventType);

            //_eventQueue = new Queue<Event>(events);

            _segmentQueue = new Queue<Segment>(segments.Where(s => s.IsHorizontal));
        }
    }


    public class YComparer : IComparer<Segment>
    {
        public int Compare(Segment x, Segment y)
        {
            return Math.Sign(x.A.Y - y.B.Y);
        }
    }

    public static class Tools
    {
        private enum Orientation
        {
            horizontal,
            vertical
        }

        public static bool Insersects(this Segment horizontal, Segment vertical)
        {
            var aHSign = AreaSign(horizontal, vertical.A);
            var bHSign = AreaSign(horizontal, vertical.B);

            var aVSign = AreaSign(vertical, horizontal.A);
            var bVSign = AreaSign(vertical, horizontal.B);

            return aHSign != bHSign && aVSign != bVSign;
            // var doubledArea = (v.B.X - v.A.X) * (point.Y - v.A.Y) - (point.X - v.A.X) * (v.B.Y - v.A.Y);

            //return vertical.Top.X.IsBetween(horizontal.A.X, horizontal.B.X)
            //    && horizontal.Bottom.Y.IsBetween(vertical.A.Y, vertical.B.Y);
        }

        private static int AreaSign(Segment s, Point p)
        {
            var doubledArea = (s.B.X - s.A.X) * (p.Y - s.A.Y) - (p.X - s.A.X) * (s.B.Y - s.A.Y);
            return Math.Sign(doubledArea);
        }

        private static bool IsBetween(this long v, long a, long b)
        {
            return a < b
                ? v >= a && v <= b
                : v >= b && v <= a;
        }
    }
}
