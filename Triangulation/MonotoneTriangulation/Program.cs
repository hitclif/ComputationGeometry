using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonotoneTriangulation
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ReadLine();
            var polygon = Console.ReadLine().ToPolygon();

            var diagonals = new MonotoneTriangulator()
                .Triangulate(polygon)
                .ToArray();

            Console.WriteLine(diagonals.Length);
            foreach (var d in diagonals)
            {
                Console.WriteLine(d.ToString());
            }
        }
    }

    [DebuggerDisplay("{X},{Y}")]
    public class Point
    {
        public Point(long x, long y)
        {
            this.X = x;
            this.Y = y;
        }

        public long X { get; }

        public long Y { get; }

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
            var area2 = (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
            return area2;
        }

        public override string ToString()
        {
            return this.X.ToString() + " " + this.Y.ToString();
        }

        public override bool Equals(object obj)
        {
            var point = obj as Point;
            return !ReferenceEquals(point, null)
                && this.X == point.X
                && this.Y == point.Y;
        }

        public override int GetHashCode()
        {
            int hashCode = 1861411795;
            hashCode = hashCode * -1521134295 + this.X.GetHashCode();
            hashCode = hashCode * -1521134295 + this.Y.GetHashCode();
            return hashCode;
        }
    }

    public enum PointPosition
    {
        Left,
        Right,
        CollinearOutside,
        CollinearInside,
        IsEndPoint
    }

    [DebuggerDisplay("{A}:{B}")]
    public class Segment
    {
        public Segment(Point a, Point b)
        {
            this.A = a;
            this.B = b;
        }

        public Point A { get; }
        public Point B { get; }

        public override string ToString()
        {
            return this.A.ToString() + " " + this.B.ToString();
        }

        public PointPosition PositionOf(Point point)
        {
            var area2 = Point.Area2(this.A, this.B, point);
            if (area2 > 0)
            {
                return PointPosition.Left;
            }

            if (area2 < 0)
            {
                return PointPosition.Right;
            }

            if (this.A == point || this.B == point)
            {
                return PointPosition.IsEndPoint;
            }

            return this.IsInSegmentBox(point)
                ? PointPosition.CollinearInside
                : PointPosition.CollinearOutside;
        }

        public double CalculateIntersecitonParameter(Segment other)
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

        private bool IsInSegmentBox(Point point)
        {
            var isOnSegment = point.X <= Math.Max(this.A.X, this.B.X)
                          && point.X >= Math.Min(this.A.X, this.B.X)
                          && point.Y <= Math.Max(this.A.Y, this.B.Y)
                          && point.Y >= Math.Min(this.A.Y, this.B.Y);
            return isOnSegment;
        }
    }

    [DebuggerDisplay("{Branch},{Point}")]
    internal class Poped
    {
        public Poped(Point point, PointPosition branch)
        {
            this.Point = point;
            this.Branch = branch;
        }

        public Point Point { get; }
        public PointPosition Branch { get; }
    }

    public class MonotoneTriangulator
    {
        private Stack<Point> _stack;
        private Stack<Point> _leftWing;
        private Stack<Point> _rightWing;

        private int UprocessedPoints
        {
            get
            {
                return _leftWing.Count + _rightWing.Count;
            }
        }

        public IEnumerable<Segment> Triangulate(IReadOnlyCollection<Point> points)
        {
            this.InitializeStacks(points);

            var next = this.PopNext();
            _stack.Push(next.Point);

            next = this.PopNext();
            _stack.Push(next.Point);

            var currentBranching = next.Branch;
            var segments = new List<Segment>();

            while(this.UprocessedPoints > 1)
            {
                next = this.PopNext();

                var triangulatedDiagonals = next.Branch == currentBranching
                    ? this.ProcessPointInFunnel(next)
                    : this.ProcessPointOppositeToFunnel(next);

                currentBranching = next.Branch;

                segments.AddRange(triangulatedDiagonals);
            }

            return segments;
        }

        private void InitializeStacks(IReadOnlyCollection<Point> points)
        {
            var maxIndex = -1;
            var maxY = long.MinValue;

            var minIndex = -1;
            var minY = long.MaxValue;

            for (int i = 0; i < points.Count; i++)
            {
                if(points.ElementAt(i).Y > maxY)
                {
                    maxIndex = i;
                    maxY = points.ElementAt(i).Y;
                }

                if(points.ElementAt(i).Y < minY)
                {
                    minIndex = i;
                    minY = points.ElementAt(i).Y;
                }
            }

            var leftWing = new List<Point>();
            var rightWing = new List<Point>();

            if(maxIndex > minIndex)
            {
                leftWing.AddRange(points.Skip(maxIndex));
                leftWing.AddRange(points.Take(minIndex));

                rightWing.AddRange(points.Skip(minIndex).Take(maxIndex - minIndex));
            }
            else
            {
                leftWing.AddRange(points.Skip(maxIndex).Take(minIndex - maxIndex));

                rightWing.AddRange(points.Skip(minIndex));
                rightWing.AddRange(points.Take(maxIndex));
            }

            leftWing.Reverse();

            _leftWing = new Stack<Point>(leftWing);
            _rightWing = new Stack<Point>(rightWing);
            _stack = new Stack<Point>();
        }

        private Poped PopNext()
        {
            if(_leftWing.Count == 0)
            {
                return new Poped(_rightWing.Pop(), PointPosition.Right);
            }

            if(_rightWing.Count == 0)
            {
                return new Poped(_leftWing.Pop(), PointPosition.Left);
            }

            if(_leftWing.Peek().Y > _rightWing.Peek().Y)
            {
                var p = _leftWing.Pop();
                return new Poped(p, PointPosition.Left);
            }
            else
            {
                var p = _rightWing.Pop();
                return new Poped(p, PointPosition.Right);
            }
        }

        private IEnumerable<Segment> ProcessPointInFunnel(Poped poped)
        {
            // report all diagonals and remove points
            var diagonals = new List<Segment>();

            var stop = false;
            do
            {
                var middle = _stack.Pop();
                var first = _stack.Pop();

                var candidate = new Segment(poped.Point, first);
                var middlePosition = candidate.PositionOf(middle);

                if(middlePosition == poped.Branch)
                {
                    diagonals.Add(candidate);
                    _stack.Push(first);
                    // middle point omitted intetionaly
                }
                else
                {
                    _stack.Push(first);
                    _stack.Push(middle);
                    stop = true;
                }
            } while (!stop && _stack.Count > 1);

            _stack.Push(poped.Point);
            return diagonals;
        }

        private IEnumerable<Segment> ProcessPointOppositeToFunnel(Poped poped)
        {
            var diagonals = new List<Segment>();

            var lastFunnelPoint = _stack.Pop();
            diagonals.Add(new Segment(poped.Point, lastFunnelPoint));
            while(_stack.Count > 0)
            {
                var p = _stack.Pop();
                if (_stack.Count > 0) // ignore last point - it is neighbour or the diagonal has already been reported
                {
                    diagonals.Add(new Segment(poped.Point, p));
                }
            }

            _stack.Push(lastFunnelPoint);
            _stack.Push(poped.Point);

            return diagonals;
        }
    }

    public static class Tools
    {
        public static IReadOnlyCollection<Point> ToPolygon(this string vertexCoordinates)
        {
            var coords = vertexCoordinates.Trim().Split(' ');
            var points = new List<Point>();
            for (var i = 0; i < coords.Length; i = i + 2)
            {
                points.Add(new Point(Convert.ToInt64(coords[i]), Convert.ToInt64(coords[i + 1])));
            }

            return points;
        }

        public static int NextIndex(this int currentIndex, int count)
        {
            return (currentIndex + 1) % count;
        }

        public static int PreviousIndex(this int currentIndex, int count)
        {
            return (currentIndex + count - 1) % count;
        }

        public static int NextIndex<T>(this IEnumerable<T> items, int currentIndex)
        {
            return currentIndex.NextIndex(items.Count());
        }

        public static int PreviousIndex<T>(this IEnumerable<T> items, int currentIndex)
        {
            return currentIndex.PreviousIndex(items.Count());
        }

        public static T NextItem<T>(this IReadOnlyCollection<T> items, int currentIndex)
        {
            var nextIndex = items.NextIndex(currentIndex);
            return items.ElementAt(nextIndex);
        }

        public static T PreviousItem<T>(this IReadOnlyCollection<T> items, int currentIndex)
        {
            var previousIndex = items.PreviousIndex(currentIndex);
            return items.ElementAt(previousIndex);
        }
    }
}
