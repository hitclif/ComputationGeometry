using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SetOfSegments
{
    class Program
    {
        static void Main(string[] args)
        {
#if test
            var segments = TestData2();
#else
            var segments = ReadSegments();
#endif

            var intersections = segments
                .FindIntersections()
                .ToArray();

            Console.WriteLine(intersections.Length);
            foreach(var intersection in intersections)
            {
                Console.WriteLine(intersection.U.ToString() + " " + intersection.V.ToString());
            }

            Console.ReadLine();
        }

        private static IEnumerable<Segment> TestData1()
        {
            yield return new Segment(0, -7, -3, 2, 6);
            yield return new Segment(1, -7, 2, 9, -2);
            yield return new Segment(2, 3, -3, -3, 6);
        }

        private static IEnumerable<Segment> TestData2()
        {
            yield return new Segment(0, 29, 48, 30, 13);
            yield return new Segment(1, 49, 57, -68, -24);
            yield return new Segment(2, 17, -30, 55, 16);
            yield return new Segment(3, 19, -61, 91, - 2);
            yield return new Segment(4, 44, - 22, 21, 73);
            yield return new Segment(5, -68, -38, -77, 90);
            yield return new Segment(6, -74, 84, 55, -32);
            yield return new Segment(7, 26, 72, -91, 64);
            yield return new Segment(8, -21, - 41, - 3, 75);
        }

        private static IEnumerable<Segment> ReadSegments()
        {
            var count = Convert.ToInt32(Console.ReadLine());
            for (var i = 0; i < count; i++)
            {
                var line = Console.ReadLine().Trim().Split(' ').ToArray();
                yield return new Segment(
                    i,
                    new Point(Convert.ToInt64(line[0]), Convert.ToInt64(line[1])),
                    new Point(Convert.ToInt64(line[2]), Convert.ToInt64(line[3])));
            }
        }
    }

    [DebuggerDisplay("{X},{Y}")]
    public struct Point
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

        public override string ToString()
        {
            return X + " " + Y;
        }
    }

    [DebuggerDisplay("{A},{B}")]
    public struct Segment
    {
        public Segment(int id, long ax, long ay, long bx, long by) : this(id, new Point(ax, ay), new Point(bx, by))
        {
        }

        public Segment(int id, Point A, Point B)
        {
            var points = new[] { A, B }
                .OrderBy(p => p.X)
                .ThenBy(p => p.Y)
                .ToArray();

            this.Id = id;
            this.A = points[0];
            this.B = points[1];
        }

        public int Id { get; }
        public Point A { get; } // left bottom
        public Point B { get; } // right top

        public Point CalculatePointOnLine(double t)
        {
            var x = A.X + (B.X - A.X) * t;
            var y = A.Y + (B.Y - A.Y) * t;

            return new Point((long)x, (long)y);
        }

        public override string ToString()
        {
            return A.ToString() + " " + B.ToString();
        }
    }

    public static class Tools
    {
        public static IEnumerable<Intersection> FindIntersections(this IEnumerable<Segment> segments)
        {
            var events = segments
                .SelectMany(s => new[] { new Event(s.A.X, s), new Event(s.B.X, s) })
                .OrderBy(e => e.Time)
                .ToList();

            var intersections = new List<Intersection>();
            var status = new SweepStatus();

            while (events.Count > 0)
            {
                var segment = events[0].Segment;
                events.RemoveAt(0);

                if (!status.Contains(segment))
                {
                    status.Add(segment);
                    continue;
                }

                var intr = status.FindIntersectionsAndRemove(segment).ToArray();
                intersections.AddRange(intr);
            }

            return intersections;
        }

        public static IEnumerable<Intersection> FindIntersectionsSlow(this IEnumerable<Segment> segments)
        {
            var events = segments
                .SelectMany(s => new[] { new Event(s.A.X, s), new Event(s.B.X, s) })
                .OrderByDescending(e => e.Time)
                .ToList();

            var intersections = new List<Intersection>();
            var bag = new Dictionary<int, Segment>();

            while(events.Count > 0)
            {
                var segment = events[0].Segment;
                events.RemoveAt(0);

                if (bag.ContainsKey(segment.Id))
                {
                    bag.Remove(segment.Id);
                }
                else
                {
                    foreach (var v in bag.Values)
                    {
                        if (segment.Intersects(v))
                        {
                            intersections.Add(new Intersection(segment, v));
                        }
                    }

                    bag.Add(segment.Id, segment);
                }
            }

            return intersections;
        }

        public static bool Intersects(this Segment u, Segment v)
        {
            var aP = u.GetPointPositionOf(v.A);
            var bP = u.GetPointPositionOf(v.B);

            if(aP == Position.CollinearInside || bP == Position.CollinearInside)
            {
                return true;
            }

            if(aP == bP)
            {
                return false;
            }

            var t = u.CalculateIntersectionParameter(v);
            return t >= 0 && t <= 1;
        }

        private static Position GetPointPositionOf(this Segment v, Point point)
        {
            var doubledArea = (v.B.X - v.A.X) * (point.Y - v.A.Y) - (point.X - v.A.X) * (v.B.Y - v.A.Y);
            if (doubledArea == 0)
            {
                return point.IsOnSegment(v)
                    ? Position.CollinearInside
                    : Position.CollinearOutside;
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
    }

    enum Position
    {
        Left,
        Right,
        CollinearOutside,
        CollinearInside
    }

    public class Intersection
    {
        public Intersection(Segment u, Segment v)
        {
            this.U = u;
            this.V = v;
        }

        public Segment U { get; }
        public Segment V { get; }
    }

    internal class Event : IComparable<Event>
    {
        public Event(long time, Segment segment)
        {
            this.Time = time;
            this.Segment = segment;
        }

        public long Time { get; }
        public Segment Segment { get; }

        public int CompareTo(Event other)
        {
            return Math.Sign(this.Time - other.Time);
        }
    }

    internal class SweepStatus
    {
                            //x              y
        private SortedList<long, SortedList<long, Segment>> _status = new SortedList<long, SortedList<long, Segment>>();

        public void Add(Segment s)
        {
            if(!_status.ContainsKey(s.A.X))
            {
                _status.Add(s.A.X, new SortedList<long, Segment>());
            }

            _status[s.A.X].Add(s.A.Y, s);
        }

        public bool Contains(Segment s)
        {
            if (!_status.ContainsKey(s.A.X))
            {
                return false;
            }

            return _status[s.A.X].ContainsKey(s.A.Y);
        }

        public IEnumerable<Intersection> FindIntersectionsAndRemove(Segment s)
        {
            var intersections = new List<Intersection>();

            var currentIndex = _status.IndexOfKey(s.A.X);
            this.Remove(s);

            while(currentIndex >= 0)
            {
                var checkForIntersection = _status.ElementAt(currentIndex);
                foreach(var v in checkForIntersection.Value)
                {
                    if (s.Intersects(v.Value))
                    {
                        intersections.Add(new Intersection(s, v.Value));
                    }
                }
                currentIndex--;
            }

            currentIndex = _status.IndexOfKey(s.A.X) + 1;
            while(currentIndex < _status.Count)
            {
                var checkForIntersection = _status.ElementAt(currentIndex);
                if(checkForIntersection.Key > s.B.X)
                {
                    break;
                }

                foreach (var v in checkForIntersection.Value)
                {
                    if (s.Intersects(v.Value))
                    {
                        intersections.Add(new Intersection(s, v.Value));
                    }
                }

                currentIndex++;
            }

            return intersections;
        }

        private void Remove(Segment s)
        {
            _status[s.A.X].Remove(s.A.Y);
        }
    }
}
