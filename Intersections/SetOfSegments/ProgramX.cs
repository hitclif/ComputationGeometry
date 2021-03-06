﻿//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace SetOfSegments
//{
//    class ProgramX
//    {
//        static void Main(string[] args)
//        {
//#if test
//            var segments = TestData2();
//#else
//            var segments = ReadSegments();
//#endif

//            var intersections = segments
//                .FindIntersections()
//                .ToArray();

//            Console.WriteLine(intersections.Length);
//            foreach(var intersection in intersections)
//            {
//                Console.WriteLine(intersection.U.ToString() + " " + intersection.V.ToString());
//            }

//            Console.ReadLine();
//        }

//        private static IEnumerable<Segment> TestData1()
//        {
//            yield return new Segment(0, -7, -3, 2, 6);
//            yield return new Segment(1, -7, 2, 9, -2);
//            yield return new Segment(2, 3, -3, -3, 6);
//        }

//        private static IEnumerable<Segment> TestData2()
//        {
//            yield return new Segment(0, 29, 48, 30, 13);
//            yield return new Segment(1, 49, 57, -68, -24);
//            yield return new Segment(2, 17, -30, 55, 16);
//            yield return new Segment(3, 19, -61, 91, - 2);
//            yield return new Segment(4, 44, - 22, 21, 73);
//            yield return new Segment(5, -68, -38, -77, 90);
//            yield return new Segment(6, -74, 84, 55, -32);
//            yield return new Segment(7, 26, 72, -91, 64);
//            yield return new Segment(8, -21, - 41, - 3, 75);
//        }

//        private static IEnumerable<Segment> ReadSegments()
//        {
//            var count = Convert.ToInt32(Console.ReadLine());
//            for (var i = 0; i < count; i++)
//            {
//                var line = Console.ReadLine().Trim().Split(' ').ToArray();
//                yield return new Segment(
//                    i,
//                    new Point(Convert.ToInt64(line[0]), Convert.ToInt64(line[1])),
//                    new Point(Convert.ToInt64(line[2]), Convert.ToInt64(line[3])));
//            }
//        }
//    }

//    [DebuggerDisplay("{X},{Y}")]
//    public struct Point
//    {
//        public Point(long x, long y)
//        {
//            this.X = x;
//            this.Y = y;
//        }

//        public long X { get; }
//        public long Y { get; }

//        public override bool Equals(object obj)
//        {
//            if (ReferenceEquals(obj, null) || !(obj is Point))
//            {
//                return false;
//            }

//            var point = (Point)obj;
//            return point == this;
//        }

//        public static bool operator ==(Point a, Point b)
//        {
//            return a.X == b.X && a.Y == b.Y;
//        }

//        public static bool operator !=(Point a, Point b)
//        {
//            return a.X == b.X && a.Y == b.Y;
//        }

//        public static long operator *(Point a, Point b)
//        {
//            return a.X * b.X + a.Y * b.Y;
//        }

//        public override string ToString()
//        {
//            return X + " " + Y;
//        }
//    }

//    [DebuggerDisplay("{A},{B}")]
//    public struct Segment
//    {
//        public Segment(int id, long ax, long ay, long bx, long by) : this(id, new Point(ax, ay), new Point(bx, by))
//        {
//        }

//        public Segment(int id, Point A, Point B)
//        {
//            var points = new[] { A, B }
//                .OrderBy(p => p.X)
//                .ThenBy(p => p.Y)
//                .ToArray();

//            this.Id = id;
//            this.A = points[0];
//            this.B = points[1];
//        }

//        public int Id { get; }
//        public Point A { get; } // left bottom
//        public Point B { get; } // right top

//        public Point CalculatePointOnLine(double t)
//        {
//            var x = Math.Round(A.X + (B.X - A.X) * t);
//            var y = Math.Round(A.Y + (B.Y - A.Y) * t);

//            return new Point((long)x, (long)y);
//        }

//        public override string ToString()
//        {
//            return A.ToString() + " " + B.ToString();
//        }
//    }

//    public static class Tools
//    {
//        public static IEnumerable<Intersection> FindIntersections(this IEnumerable<Segment> segments)
//        {
//            var events = new SortedList<long, Event>();
//            foreach(var segment in segments)
//            {
//                events.Add(segment.A.X, new Event(segment.A.X, segment, EventType.In));
//                events.Add(segment.B.X, new Event(segment.A.X, segment, EventType.Out));
//            }

//            var intersections = new List<Intersection>();
//            var status = new SweepStatus2();

//            while(events.Count > 0)
//            {
//                var @event = events.ElementAt(0);
//                events.RemoveAt(0);

//                IEnumerable<Event> newEvents = null;
//                switch (@event.Value.EventType)
//                {
//                    case EventType.In:
//                        newEvents = status.Add(@event.Value.Segment);
//                        break;
//                    case EventType.Out:
//                        newEvents = status.Add(@event.Value.Segment);
//                        break;
//                    case EventType.Intersection:
//                        // swap
//                        var intersectionEvent = (IntersectionEvent)@event.Value;
//                        intersections.Add(new Intersection(intersectionEvent.Segment, intersectionEvent.Other));
//                        newEvents = new Event[0];
//                        break;
//                }

//                foreach(var e in newEvents)
//                {
//                    events.Add(e.Time, e);
//                }
//            }

//            return intersections;
//        }

//        public static IEnumerable<Intersection> FindIntersectionsExtremelySlow(this IEnumerable<Segment> segments)
//        {
//            var events = segments
//                .SelectMany(s => new[] { new Event(s.A.X, s, EventType.In), new Event(s.B.X, s, EventType.Out) })
//                .OrderBy(e => e.Time)
//                .ToList();

//            var intersections = new List<Intersection>();
//            var status = new SweepStatus();

//            while (events.Count > 0)
//            {
//                var segment = events[0].Segment;
//                events.RemoveAt(0);

//                if (!status.Contains(segment))
//                {
//                    status.Add(segment);
//                    continue;
//                }

//                var intr = status.FindIntersectionsAndRemove(segment).ToArray();
//                intersections.AddRange(intr);
//            }

//            return intersections;
//        }

//        public static IEnumerable<Intersection> FindIntersectionsSlow(this IEnumerable<Segment> segments)
//        {
//            var events = segments
//                .SelectMany(s => new[] { new Event(s.A.X, s, EventType.In), new Event(s.B.X, s, EventType.Out) })
//                .OrderByDescending(e => e.Time)
//                .ToList();

//            var intersections = new List<Intersection>();
//            var bag = new Dictionary<int, Segment>();

//            while(events.Count > 0)
//            {
//                var segment = events[0].Segment;
//                events.RemoveAt(0);

//                if (bag.ContainsKey(segment.Id))
//                {
//                    bag.Remove(segment.Id);
//                }
//                else
//                {
//                    foreach (var v in bag.Values)
//                    {
//                        if (segment.Intersects(v).Item1)
//                        {
//                            intersections.Add(new Intersection(segment, v));
//                        }
//                    }

//                    bag.Add(segment.Id, segment);
//                }
//            }

//            return intersections;
//        }

//        public static Tuple<bool, Point> Intersects(this Segment u, Segment v)
//        {
//            var aP = u.GetPointPositionOf(v.A);
//            var bP = u.GetPointPositionOf(v.B);

//            if (aP == bP && aP != Position.CollinearInside)
//            {
//                return new Tuple<bool, Point>(false, new Point(0,0));
//            }

//            var t = u.CalculateIntersectionParameter(v);
//            if( t< 0 || t> 1)
//            {
//                new Tuple<bool, Point>(false, new Point(0, 0));
//            }

//            var intersect = u.CalculatePointOnLine(t);

//            return new Tuple<bool, Point>(true, intersect);
//        }

//        private static Position GetPointPositionOf(this Segment v, Point point)
//        {
//            var doubledArea = (v.B.X - v.A.X) * (point.Y - v.A.Y) - (point.X - v.A.X) * (v.B.Y - v.A.Y);
//            if (doubledArea == 0)
//            {
//                return point.IsOnSegment(v)
//                    ? Position.CollinearInside
//                    : Position.CollinearOutside;
//            }

//            return doubledArea < 0
//                ? Position.Left
//                : Position.Right;
//        }

//        private static double CalculateIntersectionParameter(this Segment u, Segment v)
//        {
//            var p1 = u.A;
//            var p2 = u.B;
//            var p3 = v.A;
//            var p4 = v.B;
//            var t = 1d *
//                ((p1.X - p3.X) * (p3.Y - p4.Y) - (p1.Y - p3.Y) * (p3.X - p4.X))
//                /
//                ((p1.X - p2.X) * (p3.Y - p4.Y) - (p1.Y - p2.Y) * (p3.X - p4.X));

//            return t;
//        }

//        private static bool IsOnSegment(this Point point, Segment v)
//        {
//            var isOnSegment = point.X <= Math.Max(v.A.X, v.B.X)
//                          && point.X >= Math.Min(v.A.X, v.B.X)
//                          && point.Y <= Math.Max(v.A.Y, v.B.Y)
//                          && point.Y >= Math.Min(v.A.Y, v.B.Y);

//            return isOnSegment;
//        }
//    }

//    enum Position
//    {
//        Left,
//        Right,
//        CollinearOutside,
//        CollinearInside
//    }

//    public class Intersection
//    {
//        public Intersection(Segment u, Segment v)
//        {
//            this.U = u;
//            this.V = v;
//        }

//        public Segment U { get; }
//        public Segment V { get; }
//    }

//    enum EventType
//    {
//        In,
//        Out,
//        Intersection
//    }

//    internal class Event : IComparable<Event>
//    {
//        public Event(
//            long time,
//            Segment segment,
//            EventType eventType)
//        {
//            this.Time = time;
//            this.Segment = segment;
//            this.EventType = eventType;
//        }

//        public long Time { get; }
//        public Segment Segment { get; }
//        public EventType EventType { get; }

//        public int CompareTo(Event other)
//        {
//            return Math.Sign(this.Time - other.Time);
//        }
//    }

//    internal class IntersectionEvent : Event
//    {
//        public IntersectionEvent(long time, Segment segment, Segment other): base(time, segment, EventType.Intersection)
//        {
//            this.Other = other;
//        }

//        public Segment Other { get; }
//    }

//    internal class SweepStatus2
//    {
//        private SortedList<long, Segment> _status = new SortedList<long, Segment>();

//        public IEnumerable<Event> Add(Segment segment)
//        {
//            _status.Add(segment.A.Y, segment);
//            var index = _status.IndexOfKey(segment.A.Y);
//            return CheckForIntersections(index);
//        }

//        public IEnumerable<Event> Remove(Segment segment)
//        {
//            var index = _status.IndexOfKey(segment.A.Y);
//            _status.RemoveAt(index);

//            return CheckForIntersections(index);
//        }

//        private IEnumerable<Event> CheckForIntersections(int index)
//        {
//            if(index < _status.Count - 1)
//            {
//                yield break;
//            }

//            var current = _status.ElementAt(index).Value;
//            if (index > 0)
//            {
//                var previous = _status.ElementAt(index - 1).Value;
//                var inter = current.Intersects(previous);
//                if (inter.Item1)
//                {
//                    yield return new IntersectionEvent(inter.Item2.X, current, previous);
//                }
//            }

//            if (index < _status.Count - 1)
//            {
//                var next = _status.ElementAt(index + 1).Value;
//                var inter = current.Intersects(next);
//                if (inter.Item1)
//                {
//                    yield return new IntersectionEvent(inter.Item2.X, current, next);
//                }
//            }
//        }
//    }

//    internal class SweepStatus
//    {
//                            //x              y
//        private SortedList<long, SortedList<long, Segment>> _status = new SortedList<long, SortedList<long, Segment>>();

//        public void Add(Segment s)
//        {
//            if(!_status.ContainsKey(s.A.X))
//            {
//                _status.Add(s.A.X, new SortedList<long, Segment>());
//            }

//            _status[s.A.X].Add(s.A.Y, s);
//        }

//        public bool Contains(Segment s)
//        {
//            if (!_status.ContainsKey(s.A.X))
//            {
//                return false;
//            }

//            return _status[s.A.X].ContainsKey(s.A.Y);
//        }

//        public IEnumerable<Intersection> FindIntersectionsAndRemove(Segment s)
//        {
//            var intersections = new List<Intersection>();

//            var currentIndex = _status.IndexOfKey(s.A.X);
//            this.Remove(s);

//            while(currentIndex >= 0)
//            {
//                var checkForIntersection = _status.ElementAt(currentIndex);
//                foreach(var v in checkForIntersection.Value)
//                {
//                    if (s.Intersects(v.Value).Item1)
//                    {
//                        intersections.Add(new Intersection(s, v.Value));
//                    }
//                }
//                currentIndex--;
//            }

//            currentIndex = _status.IndexOfKey(s.A.X) + 1;
//            while(currentIndex < _status.Count)
//            {
//                var checkForIntersection = _status.ElementAt(currentIndex);
//                if(checkForIntersection.Key > s.B.X)
//                {
//                    break;
//                }

//                foreach (var v in checkForIntersection.Value)
//                {
//                    if (s.Intersects(v.Value).Item1)
//                    {
//                        intersections.Add(new Intersection(s, v.Value));
//                    }
//                }

//                currentIndex++;
//            }

//            return intersections;
//        }

//        private void Remove(Segment s)
//        {
//            _status[s.A.X].Remove(s.A.Y);
//        }
//    }
//}
