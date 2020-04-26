using System;
using System.Collections;
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

        private static List<Segment> TestData1()
        {
            var segments = new List<Segment> {
                new Segment(0, -7, -3, 2, 6),
                new Segment(1, -7, 2, 9, -2),
                new Segment(2, 3, -3, -3, 6)
            };

            return segments;
        }

        private static List<Segment> TestData2()
        {
            var segments = new List<Segment>
            {
                new Segment(0, 29, 48, 30, 13),
                new Segment(1, 49, 57, -68, -24),
                new Segment(2, 17, -30, 55, 16),
                new Segment(3, 19, -61, 91, -2),
                new Segment(4, 44, -22, 21, 73),
                new Segment(5, -68, -38, -77, 90),
                new Segment(6, -74, 84, 55, -32),
                new Segment(7, 26, 72, -91, 64),
                new Segment(8, -21, -41, -3, 75)
            };

            return segments;
        }

        private static List<Segment> ReadSegments()
        {
            var segments = new List<Segment>();
            var count = Convert.ToInt32(Console.ReadLine());
            for (var i = 0; i < count; i++)
            {
                var line = Console.ReadLine().Trim().Split(' ').ToArray();
                segments.Add(new Segment(
                    i,
                    Convert.ToInt64(line[0]),
                    Convert.ToInt64(line[1]),
                    Convert.ToInt64(line[2]),
                    Convert.ToInt64(line[3])));
            }

            return segments;
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

        public double IntersectionParameter(Segment other)
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

        public double IntersectionTime(Segment other)
        {
            var t = this.IntersectionParameter(other);

            var tx = this.A.X + (this.B.X - this.A.X) * t;
            return Math.Round(tx, 7);
        }

        public Intersection Intersection(Segment other)
        {
            var tp1 = this.IntersectionParameter(other);
            var tp2 = other.IntersectionParameter(this);

            if(tp1 < 0 || tp1 > 1 || tp2 < 0 || tp2 > 1)
            {
                return null;
            }

            return new Intersection(this, other);
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

        public override string ToString()
        {
            return A.ToString() + " " + B.ToString();
        }
    }

    public enum EventType
    {
        BeginSegment = 0,
        Intersection = 1,
        EndSegment = 2
    }

    [DebuggerDisplay("{Time}:{EventName}")]
    public class Event : IComparable<Event>, IEquatable<Event>
    {
        private Event(EventType eventType, Point time, Segment segment1, Segment segment2, double time2)
        {
            this.EventType = eventType;
            this.Time = time;
            this.Segment1 = segment1;
            this.Segment2 = segment2;
            this.Time2 = time2;
        }

        public EventType EventType { get; }
        public Point Time { get; }
        public Segment Segment1 { get; }
        public Segment Segment2 { get; }
        public double Time2 { get; }

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
            var result = -Math.Sign(this.Time2 - other.Time2);
            return result;
        }

        public static Event BeginSegment(Segment segment)
        {
            return new Event(EventType.BeginSegment, segment.A, segment, Segment.Empty, segment.A.X);
        }

        public static Event EndSegment(Segment segment)
        {
            return new Event(EventType.EndSegment, segment.B, segment, Segment.Empty, segment.B.X);
        }

        public static Event Intersection(Intersection intersection, Segment above, Segment below)
        {
            return new Event(EventType.Intersection, intersection.Point, above, below, intersection.Time);
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

            this.Time = u.IntersectionTime(v);
            this.Point = u.CalculatePointOnLine(u.IntersectionParameter(v));
        }

        public Segment U { get; }
        public Segment V { get; }

        public double Time { get; }

        public Point Point { get; }
    }

    [DebuggerDisplay("{IntersectionEvents()}")]
    internal class EventQueue
    {
        private List<Event> _events = new List<Event>();
        private List<Event> _intersections = new List<Event>();

        private readonly bool _doLog;

        public EventQueue(bool doLog)
        {
            _doLog = doLog;
        }

        public void Clear(int segmentsCount)
        {
            _events = new List<Event>(segmentsCount * 2);
            _intersections = new List<Event>(segmentsCount);
        }

        public int Count => _events.Count;

        public void Enqueue(Event @event)
        {
            var queue = @event.EventType == EventType.Intersection
                ? _intersections
                : _events;

            var index = queue.BinarySearch(@event);

            if (index < 0)
            {
                index = ~index;
            }

            queue.Insert(index, @event);
        }

        public Event Dequeue()
        {
            var queue = _intersections.Count == 0 || _intersections[_intersections.Count - 1].Time2 > _events[_events.Count - 1].Time2
                ? _events
                : _intersections;

            var @event = queue[queue.Count - 1];
            queue.RemoveAt(queue.Count - 1);

            if (_doLog)
            {
                Console.WriteLine(@event);
            }
            return @event;
        }

        public void RemoveFuture(Event @event)
        {
            var index = _intersections.BinarySearch(@event);

            if (index < 0)
            {
                return;
            }

            _intersections.RemoveAt(index);
        }

        public string IntersectionEvents()
        {
            var inters = _intersections
                .Select(e => e.ToString())
                .ToArray();

            return string.Join(" ", inters);
        }
    }

    [DebuggerDisplay("{ToDebugDisplay()}")]
    internal class Status
    {
        private List<Segment> _status = new List<Segment>();

        public int Length => _status.Count;

        public void Clear(int capacity)
        {
            _status = new List<Segment>(capacity);
        }

        public Segment GetAtIndex(int index)
        {
            return _status[index];
        }

        public int Insert(Event @event)
        {
            var index = this.FindIndexOfSegmentAtTime(@event);
            _status.Insert(index, @event.Segment1);
            return index;
        }

        public void RemoveAtIndex(int index)
        {
            _status.RemoveAt(index);
        }

        public void SwapWithRight(int index)
        {
            var s1 = _status[index];
            _status.RemoveAt(index);
            _status.Insert(index + 1, s1);
        }

        public int FindIndexOfSegmentAtTime(Event @event)
        {
            var comparer = new SegmentTimeComparer(@event);
            var index = _status.BinarySearch(@event.Segment1, comparer);
            if (index < 0)
            {
                index = ~index;
            }

            return index;
        }

        public override string ToString()
        {
            return string.Join(" ", _status.Select(s => s.ToString()).ToArray());
        }

        public string ToDebugDisplay()
        {
            return string.Join(" ", _status.Select(s => "S" + s.Id).ToArray());
        }
    }

    public class SweepLine
    {
        private readonly IReadOnlyCollection<Segment> _segments;
        private readonly bool _log;
        private List<Intersection> _intersections = new List<Intersection>();
        // private Status _status = new Status();

        private readonly SegmentTimeComparer2 _comparer;
        private readonly AvlTree<Segment> _status;
        private EventQueue _eventQueue;

        public SweepLine(IReadOnlyCollection<Segment> segments, bool log)
        {
            _eventQueue = new EventQueue(log);
            _segments = segments;
            _log = log;

            _comparer = new SegmentTimeComparer2();
            _status = new AvlTree<Segment>(_comparer, s => "S" + s.Id);
        }

        public IEnumerable<Intersection> FindIntersections()
        {
            _intersections = new List<Intersection>();

            _status.Clear();
            _eventQueue.Clear(_segments.Count);

            this.PrepereEventQueue();

            while (_eventQueue.Count > 0)
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

                if (_log)
                {
                    Console.WriteLine(_status.ToDebugString());
                    Console.WriteLine(_eventQueue.IntersectionEvents());
                    Console.WriteLine();
                }
            }

            return _intersections.ToArray();
        }

        private void PrepereEventQueue()
        {
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
            _comparer.Time = @event.Time2;
            _status.Add(@event.Segment1);

            var index = _status.IndexOf(@event.Segment1);

            var above = index > 0 ? _status.ElementAt(index - 1) : null;
            var below = index < _status.Count - 1 ? _status.ElementAt(index + 1) : null;

            if(above != null && below != null)
            {
                this.RemovePotentialFutureEvent(above, below);
            }

            if(above != null)
            {
                this.EnqueuePotentialIntersectionEvent(above, @event.Segment1, @event.Time2);
            }

            if(below != null)
            {
                this.EnqueuePotentialIntersectionEvent(@event.Segment1, below, @event.Time2);
            }
        }

        private void RemoveFromStatus(Event @event)
        {
            _comparer.Time = @event.Time2;
            var index = _status.IndexOf(@event.Segment1);

            if (index > 0 && index < _status.Count - 1)
            {
                var above = _status.ElementAt(index - 1);
                var below = _status.ElementAt(index + 1);
                this.EnqueuePotentialIntersectionEvent(above, below, @event.Time2);
            }

            _status.RemoveAt(index);
        }

        private void HandleIntersection(Event @event)
        {
            var intersection = new Intersection(@event.Segment1, @event.Segment2);
            _intersections.Add(intersection);

            _comparer.Time = @event.Time2;
            var index = _status.IndexOf(@event.Segment1);

            var above = index > 0 ? _status.ElementAt(index - 1) : null;
            var below = index < _status.Count - 2 ? _status.ElementAt(index + 2) : null;
            if (above != null)
            {
                this.RemovePotentialFutureEvent(above, @event.Segment1);
            }

            if (below != null)
            {
                this.RemovePotentialFutureEvent(@event.Segment2, below);
            }

            if(above != null)
            {
                this.EnqueuePotentialIntersectionEvent(above, @event.Segment2, @event.Time2);
            }

            if(below != null)
            {
                this.EnqueuePotentialIntersectionEvent(@event.Segment1, below, @event.Time2);
            }

            _status.Remove(@event.Segment1);
            _status.Remove(@event.Segment2);

            _comparer.Time = @event.Time2 + 0.000002;
            _status.Add(@event.Segment2, @event.Segment1);
        }

        private void RemovePotentialFutureEvent(Segment above, Segment below)
        {
            var @event = this.CreateIntersectionEvent(above, below);
            if (@event == null)
            {
                return;
            }

            _eventQueue.RemoveFuture(@event);
        }

        private void EnqueuePotentialIntersectionEvent(Segment above, Segment below, double currentTime)
        {
            var @event = this.CreateIntersectionEvent(above, below);
            if (@event == null || @event.Time2 < currentTime)
            {
                return;
            }

            _eventQueue.Enqueue(@event);
        }

        private Event CreateIntersectionEvent(Segment above, Segment below)
        {
            var intersection = above.Intersection(below);
            if (intersection == null)
            {
                return null;
            }

            return Event.Intersection(intersection, above, below);
        }
    }

    public class SegmentTimeComparer : IComparer<Segment>
    {
        private readonly Event _event;

        public SegmentTimeComparer(Event @event)
        {
            _event = @event;
        }

        public int Compare(Segment x, Segment y)
        {
            var intersection = x.Intersection(y);

            var result = intersection == null
                ? CompareNonIntersecting(x, y)
                : CompareIntersecting(x, y, intersection);

            return result;
        }

        private int CompareIntersecting(Segment x, Segment y, Intersection intersection)
        {
            if(_event.Time2 <= intersection.Time)
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
            var p1 = Math.Sign(Point.Area2(x.A, x.B, y.A));
            var p2 = Math.Sign(Point.Area2(x.A, x.B, y.B));

            if (p1 == p2)
            {
                return p2;
            }

            p1 = Math.Sign(Point.Area2(y.A, y.B, x.A));
            return -p1;
        }
    }

    public class SegmentTimeComparer2 : IComparer<Segment>
    {
        public double Time { get; set; }

        public SegmentTimeComparer2()
        {
        }

        public int Compare(Segment x, Segment y)
        {
            var intersection = x.Intersection(y);

            var result = intersection == null
                ? CompareNonIntersecting(x, y)
                : CompareIntersecting(x, y, intersection);

            return result;
        }

        private int CompareIntersecting(Segment x, Segment y, Intersection intersection)
        {
            if (this.Time <= intersection.Time)
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
            var p1 = Math.Sign(Point.Area2(x.A, x.B, y.A));
            var p2 = Math.Sign(Point.Area2(x.A, x.B, y.B));

            if (p1 == p2)
            {
                return p2;
            }

            p1 = Math.Sign(Point.Area2(y.A, y.B, x.A));
            return -p1;
        }
    }

    [DebuggerDisplay("{ToDebugString()}")]
    public class AvlTree<T> : IEnumerable<T>
    {
        private IAvlNode<T> _root;
        private readonly IComparer<T> _comparer;
        private readonly Func<T, string> _toDebugString;

        public AvlTree() : this(Comparer<T>.Default)
        {
        }

        public AvlTree(IComparer<T> comparer, Func<T, string> toDebugString = null)
        {
            _root = new EmptyNode<T>(comparer);
            _comparer = comparer;
            _toDebugString = toDebugString;
        }

        public int Count
        {
            get
            {
                return _root.Count;
            }
        }

        public int BalanceFactor
        {
            get
            {
                return _root.BalanceFactor;
            }
        }

        public void Add(params T[] values)
        {
            for (var i = 0; i < values.Length; i++)
            {
                _root = _root.Add(values[i]);
            }
        }

        public int IndexOf(T value)
        {
            return _root.IndexOf(value, 0);
        }

        public bool Contains(T value)
        {
            return _root.Contains(value);
        }

        public void Remove(T value)
        {
            if (_root.Count == 0)
            {
                throw new Exception("Element does not exist");
            }

            _root = _root.Remove(value);
        }

        public void RemoveAt(int index)
        {
            var v = _root.ElementAt(index);
            this.Remove(v);
        }

        public void Clear()
        {
            _root = new EmptyNode<T>(_comparer);
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var i in _root)
            {
                yield return i;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public string ToDebugString()
        {
            var sv = this
                .Select(v => _toDebugString == null ? v.ToString() : _toDebugString(v))
                .ToArray();

            var s = string.Join(", ", sv);
            return s;
        }
    }

    internal interface IAvlNode<T> : IEnumerable<T>
    {
        bool IsEmpty { get; }
        int Count { get; }
        int BalanceFactor { get; }
        int Height { get; }
        int IndexOf(T item, int offset);
        bool Contains(T item);
        IAvlNode<T> Add(T item);
        IAvlNode<T> Remove(T item);

        void UpdateValues();
        string ValuesList();
    }

    [DebuggerDisplay("{Left.ValuesList()} | {_value} | {Right.ValuesList()}")]
    internal class AvlNode<T> : IAvlNode<T>
    {
        private readonly T _value;
        private readonly IComparer<T> _comparer;
        private IAvlNode<T> _left;
        private IAvlNode<T> _right;

        public AvlNode(T value, IComparer<T> comparer)
        {
            _value = value;
            _comparer = comparer;
            _left = new EmptyNode<T>(comparer);
            _right = new EmptyNode<T>(comparer);
            this.UpdateValues();
        }

        public IAvlNode<T> Left
        {
            get
            {
                return _left;
            }
            private set
            {
                _left = value;
                this.UpdateValues();
            }
        }

        public IAvlNode<T> Right
        {
            get
            {
                return _right;
            }
            private set
            {
                _right = value;
                this.UpdateValues();
            }
        }

        public bool IsEmpty
        {
            get
            {
                return false;
            }
        }

        public bool IsHighest => this.Right.IsEmpty;

        public int Count { get; private set; }

        public int BalanceFactor { get; private set; }

        public int Height { get; private set; }

        public IAvlNode<T> Add(T item)
        {
            switch (this.CompareTo(item))
            {
                case Comparison.Equal:
                    throw new Exception("Item with same value already in the tree");
                case Comparison.Higher:
                    this.Left = this.Left.Add(item);
                    break;
                case Comparison.Lower:
                    this.Right = this.Right.Add(item);
                    break;
            }

            var newRoot = this.Rebalance();
            return newRoot;
        }

        public IAvlNode<T> Remove(T item)
        {
            switch (this.CompareTo(item))
            {
                case Comparison.Equal:
                    return this.RemoveSelf();
                case Comparison.Higher:
                    this.Left = this.Left.Remove(item);
                    break;
                case Comparison.Lower:
                    this.Right = this.Right.Remove(item);
                    break;
            }

            var newRoot = this.Rebalance();
            return newRoot;
        }

        public int IndexOf(T item, int offset)
        {
            switch (this.CompareTo(item))
            {
                case Comparison.Higher:
                    return this.Left.IndexOf(item, offset);
                case Comparison.Equal:
                    var myIndex = offset + this.Left.Count;
                    return myIndex;
                case Comparison.Lower:
                    return this.Right.IndexOf(item, offset + this.Left.Count + 1);
            }

            return -1;
        }

        public bool Contains(T item)
        {
            return this.IndexOf(item, 0) > -1;
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var i in this.Left)
            {
                yield return i;
            }

            yield return _value;

            foreach (var i in this.Right)
            {
                yield return i;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        private Comparison CompareTo(T item)
        {
            return (Comparison)Math.Sign(_comparer.Compare(_value, item));
        }

        private AvlNode<T> Rebalance()
        {
            AvlNode<T> root;
            switch (this.BalanceFactor)
            {
                case -1:
                case 0:
                case 1:
                    root = this;
                    break;
                case 2:
                    if (this.Right.BalanceFactor >= 0)
                    {
                        root = RotateLeft(this);
                    }
                    else
                    {
                        root = RotateRightLeft(this);
                    }
                    break;
                case -2:
                    if (this.Left.BalanceFactor <= 0)
                    {
                        root = RotateRight(this);
                    }
                    else
                    {
                        root = RotateLeftRight(this);
                    }
                    break;
                default:
                    throw new Exception("not implemented yet");
            }

            this.UpdateValues();
            root.UpdateValues();
            return root;
        }

        private IAvlNode<T> RemoveSelf()
        {
            if (this.Left.IsEmpty)
            {
                return this.Right;
            }

            var pair = ((AvlNode<T>)this.Left).ElevateHighest();
            var replacement = ((AvlNode<T>)pair.Item1);

            replacement.Right = this.Right;
            replacement.Left = pair.Item2;

            var rpl = replacement.Rebalance();
            return rpl;
        }

        private Tuple<IAvlNode<T>, IAvlNode<T>> ElevateHighest()
        {
            if (this.Right.IsEmpty)
            {
                return new Tuple<IAvlNode<T>, IAvlNode<T>>(this, this.Left);
            }

            var pair = ((AvlNode<T>)this.Right).ElevateHighest();
            this.Right = pair.Item2;
            var newRoot = this.Rebalance();
            return new Tuple<IAvlNode<T>, IAvlNode<T>>(pair.Item1, newRoot);
        }

        public void UpdateValues()
        {
            Height = Math.Max(_left.Height, _right.Height) + 1;
            Count = _left.Count + _right.Count + 1;
            BalanceFactor = _right.Height - _left.Height;
        }

        // make sure subtreeRoot is not the highest
        private static AvlNode<T> ParentOfHighestValue(AvlNode<T> subtreeRoot)
        {
            var right = (AvlNode<T>)subtreeRoot.Right;
            if (right.IsHighest)
            {
                return subtreeRoot;
            }

            return ParentOfHighestValue(right);
        }

        private static AvlNode<T> RotateLeft(AvlNode<T> x)
        {
            var r = (AvlNode<T>)x.Right;
            x.Right = r.Left;
            r.Left = x;
            return r;
        }

        private static AvlNode<T> RotateRight(AvlNode<T> x)
        {
            var l = (AvlNode<T>)x.Left;
            x.Left = l.Right;
            l.Right = x;
            return l;
        }

        private static AvlNode<T> RotateRightLeft(AvlNode<T> x)
        {
            var right = (AvlNode<T>)x.Right;
            right = RotateRight(right);
            x.Right = right;

            var root = RotateLeft(x);
            return root;
        }

        private static AvlNode<T> RotateLeftRight(AvlNode<T> x)
        {
            var left = (AvlNode<T>)x.Left;
            left = RotateLeft(left);
            x.Left = left;

            var root = RotateRight(x);
            return root;
        }

        public string ValuesList()
        {
            var left = this.Left.ValuesList();
            var right = this.Right.ValuesList();
            return left
                + (string.IsNullOrWhiteSpace(left) ? "" : ", ")
                + _value
                + (string.IsNullOrWhiteSpace(right) ? "" : ", ")
                + right;
        }
    }

    [DebuggerDisplay("{ValuesList()}")]
    internal class EmptyNode<T> : IAvlNode<T>
    {
        private readonly IComparer<T> _comparer;

        public EmptyNode(IComparer<T> comparer)
        {
            _comparer = comparer;
        }

        public bool IsEmpty
        {
            get
            {
                return true;
            }
        }

        public int Count
        {
            get
            {
                return 0;
            }
        }

        public int BalanceFactor
        {
            get
            {
                return 0;
            }
        }

        public int Height
        {
            get
            {
                return 0;
            }
        }

        public IAvlNode<T> Add(T item)
        {
            return new AvlNode<T>(item, _comparer);
        }

        public IAvlNode<T> Remove(T item)
        {
            throw new Exception("Item not present in collection");
        }

        public IAvlNode<T> RemoveAt(int index)
        {
            throw new Exception("Index out of range");
        }

        public bool Contains(T item)
        {
            return false;
        }

        public int IndexOf(T item, int offset)
        {
            return -1;
        }

        public IEnumerator<T> GetEnumerator()
        {
            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public string ValuesList()
        {
            return string.Empty;
        }

        public void UpdateValues()
        {
        }
    }

    internal enum Comparison
    {
        Lower = -1,
        Equal = 0,
        Higher = 1
    }
}
