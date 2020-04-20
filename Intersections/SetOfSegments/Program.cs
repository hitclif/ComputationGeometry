using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SetOfSegments
{
    static class Program
    {
        static void Main(string[] args)
        {
#if test
            var segments = TestData2();
#else
            var segments = ReadSegments();
#endif

            var intersections = new SweepLine(segments, false)
                .FindIntersections()
                .ToArray();

            Console.WriteLine(intersections.Length);
            foreach (var intersection in intersections)
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
            yield return new Segment(3, 19, -61, 91, -2);
            yield return new Segment(4, 44, -22, 21, 73);
            yield return new Segment(5, -68, -38, -77, 90);
            yield return new Segment(6, -74, 84, 55, -32);
            yield return new Segment(7, 26, 72, -91, 64);
            yield return new Segment(8, -21, -41, -3, 75);
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

    [DebuggerDisplay("[{X},{Y}]")]
    public struct Point
    {
        public static readonly Point Empty = new Point(long.MinValue, long.MinValue);

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
            return !(a == b);
        }

        public static long Area2(Point a, Point b, Point c)
        {
            return (b.X - a.X) * (c.Y - a.Y) - (c.X - a.X) * (b.Y - a.Y);
        }

        public override string ToString()
        {
            // do not change, it's important test output
            return X + " " + Y;
        }

        public override int GetHashCode()
        {
            int hashCode = 1861411795;
            hashCode = hashCode * -1521134295 + this.X.GetHashCode();
            hashCode = hashCode * -1521134295 + this.Y.GetHashCode();
            return hashCode;
        }
    }

    [DebuggerDisplay("S{Id}: {A},{B}")]
    public class Segment
    {
        public static readonly Segment Empty = new Segment(-1, long.MinValue, long.MinValue, long.MaxValue, long.MaxValue);

        private readonly double _tan;

        public Segment(int id, long ax, long ay, long bx, long by) : this(id, new Point(ax, ay), new Point(bx, by))
        {
        }

        public Segment(int id, Point A, Point B)
        {
            this.Id = id;

            this.Points = new[] { A, B }
                .OrderBy(p => p.X)
                .ThenBy(p => p.Y)
                .ToArray();

            _tan = 1d * (this.B.Y - this.A.Y) / (this.B.X - this.A.X);
        }

        public int Id { get; }
        public Point A => this.Points[0]; // left bottom
        public Point B => this.Points[1]; // right top
        public Point[] Points { get; }

        //public Point CalculatePointAtX(long x)
        //{
        //    var t = 1d * (x - A.X) / (B.X - A.X);
        //    var py = (long)Math.Round(this.A.Y + t * (this.B.Y - this.A.Y));

        //    return this.CalculatePointOnLine(t);
        //}

        public double CalculateHeightAtX(long x)
        {
            //var t = 1d * (x - A.X) / (B.X - A.X);
            //var v = this.A.Y + t * (this.B.Y - this.A.Y);
            var v = this.A.Y + _tan * (x - this.A.X);
            return v;
        }

        public Point CalculatePointOnLine(double t)
        {
            var x = Math.Round(A.X + (B.X - A.X) * t);
            var y = Math.Round(A.Y + (B.Y - A.Y) * t);

            return new Point((long)x, (long)y);
        }

        public Point Intersection(Segment other)
        {
            var p1 = this.A;
            var p2 = this.B;
            var p3 = other.A;
            var p4 = other.B;

            var t = 1d *
                ((p1.X - p3.X) * (p3.Y - p4.Y) - (p1.Y - p3.Y) * (p3.X - p4.X))
                /
                ((p1.X - p2.X) * (p3.Y - p4.Y) - (p1.Y - p2.Y) * (p3.X - p4.X));

            if(t< 0 || t > 1)
            {
                return Point.Empty;
            }

            var intersectionPoint = this.CalculatePointOnLine(t);
            return intersectionPoint;
        }

        public bool IsPointOnSegment(Point point)
        {
            var isOnSegment = point.X <= Math.Max(this.A.X, this.B.X)
                          && point.X >= Math.Min(this.A.X, this.B.X)
                          && point.Y <= Math.Max(this.A.Y, this.B.Y)
                          && point.Y >= Math.Min(this.A.Y, this.B.Y);

            return isOnSegment;
        }

        public long Area2(Point point)
        {
            return (this.B.X - this.A.X) * (point.Y - this.A.Y) - (point.X - this.A.X) * (this.B.Y - this.A.Y);
        }

        public static Point CommonIntersectionPoint(Segment u, Segment v)
        {
            var pi1 = u.Intersection(v);
            var pi2 = v.Intersection(u);

            return pi1 == pi2 && pi1 != Point.Empty
                ? pi1
                : Point.Empty;
        }

        public override string ToString()
        {
            return A.ToString() + " " + B.ToString();
        }
    }

    internal enum EventType
    {
        BeginSegment = 0,
        Intersection = 1,
        EndSegment = 2
    }

    [DebuggerDisplay("{Time}:{EventName}")]
    internal class Event : IComparable<Event>, IEquatable<Event>
    {
        private Event(EventType eventType, Point time, Segment segment1, Segment segment2)
        {
            this.EventType = eventType;
            this.Time = time;
            this.Segment1 = segment1;
            this.Segment2 = segment2;
        }

        public EventType EventType { get; }
        public Point Time { get; }
        public Segment Segment1 { get; }
        public Segment Segment2 { get; }

        public string EventName 
        {
            get
            {
                switch (this.EventType)
                {
                    case EventType.BeginSegment:
                        return "Begin S" + this.Segment1.Id;
                    case EventType.EndSegment:
                        return "End S" + this.Segment1.Id;
                    case EventType.Intersection:
                        return "Intersection: S" + this.Segment1.Id + "xS" + this.Segment2.Id;
                }

                return string.Empty;
            }
        }

        public int CompareTo(Event other)
        {
            if (this.Time.X != other.Time.X)
            {
                return Math.Sign(this.Time.X - other.Time.X);
            }

            if(this.EventType != other.EventType)
            {
                return this.EventType - other.EventType;
            }

            return -Math.Sign(this.Time.Y - other.Time.Y);
        }

        public static Event BeginSegment(Segment segment)
        {
            return new Event(EventType.BeginSegment, segment.A, segment, Segment.Empty);
        }

        public static Event EndSegment(Segment segment)
        {
            return new Event(EventType.EndSegment, segment.B, segment, Segment.Empty);
        }

        public static Event Intersection(Point intersectionPoint, Segment above, Segment below)
        {
            return new Event(EventType.Intersection, intersectionPoint, above, below);
        }

        public bool Equals(Event other)
        {
            return this.Segment1.Id == other.Segment1.Id
                && this.Segment2.Id == other.Segment2.Id;
        }

        public override string ToString()
        {
            return this.EventName;
        }
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

    internal class EventQueue
    {
        private List<Event> _events = new List<Event>();
        private readonly bool _doLog;

        public EventQueue(bool doLog)
        {
            _doLog = doLog;
        }

        public void Clear()
        {
            _events = new List<Event>();
        }

        public int Count => _events.Count;

        public void Enqueue(Event @event)
        {
            var index = _events.BinarySearch(@event);

            if (index < 0)
            {
                index = ~index;
            }

            _events.Insert(index, @event);
        }

        public Event Dequeue()
        {
            var @event = _events[0];
            _events.RemoveAt(0);

            if (_doLog)
            {
                Console.WriteLine(@event);
            }
            return @event;
        }

        public void RemoveFuture(Event @event)
        {
            var index = _events.BinarySearch(@event);

            if (index < 0)
            {
                return;
            }

            var i = this.IndexOfEvent(@event, index);
            if(i == -1)
            {
                return;
            }

            _events.RemoveAt(i);
        }

        private int IndexOfEvent(Event @event, int fromIndex)
        {
            var temp = fromIndex;
            while (temp < _events.Count)
            {
                if (_events[temp].Time.X > @event.Time.X)
                {
                    return -1;
                }

                if (_events[temp].Equals(@event))
                {
                    return temp;
                }

                temp++;
            }

            return -1;
        }
    }

    [DebuggerDisplay("{ToDebugDisplay()}")]
    internal class Status
    {
        private List<Segment> _status = new List<Segment>();

        public int Length => _status.Count;

        public void Clear()
        {
            _status = new List<Segment>();
        }

        public Segment GetAtIndex(int index)
        {
            return _status[index];
        }

        public int Insert(Segment segment)
        {
            var index = this.FindIndexToInsert(segment);
            _status.Insert(index, segment);
            return index;
        }

        public void RemoveAtIndex(int index)
        {
            _status.RemoveAt(index);
        }

        public int FindIndex(Segment segment, long time)
        {
            var index = this.FindIndexOfSegmentAtTime(time, segment);
            return index;
        }

        public void Swap(Segment segment1, Segment segment2, long time)
        {
            var indexOf1 = this.FindIndexOfSegmentAtTime(time, segment1);

            _status.RemoveAt(indexOf1);
            _status.Insert(indexOf1 + 1, segment1);
        }

        private int FindIndexToInsert(Segment segment)
        {
            var comparer = new SegmentTimeComparer(segment.A.X);
            var index = _status.BinarySearch(segment, comparer);
            if (index < 0)
            {
                index = ~index;
            }

            return index;
        }

        private int FindIndexOfSegmentAtTime(long time, Segment segment)
        {
            var comparer = new SegmentTimeComparer(time);
            var index = _status.BinarySearch(segment, comparer);
            if (index < 0)
            {
                index = ~index;
            }

            if (_status[index] == segment)
            {
                return index;
            }

            if (index > 0 && _status[index - 1] == segment)
            {
                return index - 1;
            }

            if (index < _status.Count - 1 && _status[index + 1] == segment)
            {
                return index + 1;
            }

            throw new Exception("FindIndex failed");
        }

        public override string ToString()
        {
            return string.Join(" ", _status.Select(s => s.ToString()).ToArray());
        }

        private string ToDebugDisplay()
        {
            return string.Join(" ", _status.Select(s => "S" +s.Id).ToArray());
        }
    }

    public class SweepLine
    {
        private readonly IEnumerable<Segment> _segments;
        private readonly bool _log;
        private List<Intersection> _intersections = new List<Intersection>();
        private Status _status = new Status();
        private EventQueue _eventQueue;

        public SweepLine(IEnumerable<Segment> segments, bool log)
        {
            _eventQueue = new EventQueue(log);
            _segments = segments;
            _log = log;
        }

        public IEnumerable<Intersection> FindIntersections()
        {
            _status.Clear();
            _intersections = new List<Intersection>();

            this.PrepereEventQueue();

            while(_eventQueue.Count > 0)
            {
                var @event = _eventQueue.Dequeue();
                switch (@event.EventType)
                {
                    case EventType.BeginSegment:
                        this.InsertToStatus(@event);
                        break;
                    case EventType.EndSegment:
                        this.RemoveFromStatus(@event);
                        break;
                    case EventType.Intersection:
                        this.HandleIntersection(@event);
                        break;
                }
            }

            return _intersections.ToArray();
        }

        private void PrepereEventQueue()
        {
            _eventQueue.Clear();

            var events = _segments
                .SelectMany(s => new[] { Event.BeginSegment(s), Event.EndSegment(s) })
                .ToArray();

            foreach (var @event in events)
            {
                _eventQueue.Enqueue(@event);
            }
        }

        private void InsertToStatus(Event @event)
        {
            var index = _status.Insert(@event.Segment1);

            if(index > 0 && index < _status.Length - 1)
            {
                this.RemovePotentialFutureEvent(_status.GetAtIndex(index - 1), _status.GetAtIndex(index + 1));
            }

            if(index > 0)
            {
                this.EnqueuePotentialIntersectionEvent(_status.GetAtIndex(index - 1), _status.GetAtIndex(index));
            }

            if (index < _status.Length - 1)
            {
                this.EnqueuePotentialIntersectionEvent(_status.GetAtIndex(index), _status.GetAtIndex(index + 1));
            }
        }

        private void RemoveFromStatus(Event @event)
        {
            var index = _status.FindIndex(@event.Segment1, @event.Time.X);

            if(index > 0 && index < _status.Length - 1)
            {
                this.EnqueuePotentialIntersectionEvent(_status.GetAtIndex(index - 1), _status.GetAtIndex(index + 1));
            }

            _status.RemoveAtIndex(index);
        }

        private void HandleIntersection(Event @event)
        {
            var intersection = new Intersection(@event.Segment1, @event.Segment2);
            _intersections.Add(intersection);

            var index = _status.FindIndex(@event.Segment1, @event.Time.X); // this.FindIndexOfSegmentAtTime(@event.Time, @event.Segment1);

            if(index > 0)
            {
                this.RemovePotentialFutureEvent(_status.GetAtIndex(index - 1), _status.GetAtIndex(index));
            }
            if(index < _status.Length - 2)
            {
                this.RemovePotentialFutureEvent(_status.GetAtIndex(index + 1), _status.GetAtIndex(index + 2));
            }

            if (index > 0 && index < _status.Length - 1)
            {
                this.EnqueuePotentialIntersectionEvent(_status.GetAtIndex(index - 1), _status.GetAtIndex(index + 1));
            }

            if(index < _status.Length - 2)
            {
                this.EnqueuePotentialIntersectionEvent(_status.GetAtIndex(index), _status.GetAtIndex(index + 2));
            }

            _status.Swap(@event.Segment1, @event.Segment2, @event.Time.X);
        }

        private void RemovePotentialFutureEvent(Segment above, Segment below)
        {
            var @event = this.CreateEvent(above, below);
            if (@event == null)
            {
                return;
            }

            _eventQueue.RemoveFuture(@event);
        }

        private void EnqueuePotentialIntersectionEvent(Segment above, Segment below)
        {
            var @event = this.CreateEvent(above, below);
            if(@event == null)
            {
                return;
            }

            _eventQueue.Enqueue(@event);
        }

        private Event CreateEvent(Segment above, Segment below)
        {
            var compare = new SegmentTimeComparer(above.A.X).Compare(above, below);

            if (compare > 0)
            {
                return null;
            }
            var intersectionPoint = Segment.CommonIntersectionPoint(above, below);
            if(intersectionPoint == Point.Empty)
            {
                return null;
            }

            return Event.Intersection(intersectionPoint, above, below);
        }
    }

    public class SegmentTimeComparer : IComparer<Segment>
    {
        private readonly long _time;

        public SegmentTimeComparer(long time)
        {
            _time = time;
        }

        public int Compare(Segment x, Segment y)
        {
            var intersectionPoint = Segment.CommonIntersectionPoint(x, y);

            var result = intersectionPoint == Point.Empty
                ? CompareNonIntersecting(x, y)
                : CompareIntersecting(x, y, intersectionPoint);

            return result;
        }

        private int CompareIntersecting(Segment x, Segment y, Point intersectionPoint)
        {
            if(_time <= intersectionPoint.X)
            {
                var area = Point.Area2(x.A, x.B, y.A);
                return Math.Sign(area);
            }
            else
            {
                var area = Point.Area2(x.A, x.B, y.B);
                return Math.Sign(area);
            }
        }

        private int CompareNonIntersecting(Segment x, Segment y)
        {
            if(this.IsWider(x, y))
            {
                var area = Point.Area2(x.A, x.B, y.A);
                return -Math.Sign(area);
            }

            if(this.IsWider(y, x))
            {
                var area = Point.Area2(y.A, y.B, x.A);
                return Math.Sign(area);
            }

            var c = y.Points
                .Where(p => p.X >= x.A.X && p.X <= x.B.X)
                .First();

            var areaC = Point.Area2(x.A, x.B, c);
            return -Math.Sign(areaC);
        }

        private bool IsWider(Segment u, Segment v)
        {
            return u.A.X <= v.A.X && u.B.X >= v.B.X;
        }
    }
}
