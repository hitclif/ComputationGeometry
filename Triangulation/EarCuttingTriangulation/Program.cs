using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace EarCuttingTriangulation
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ReadLine();
            var polygon = Console.ReadLine().ToPolygon();

            var triangulations = new EarCuttingTriangulator()
                .Triangulate(polygon)
                .ToArray();

            Console.WriteLine(triangulations.Length);
            foreach (var d in triangulations)
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

    public enum Designation
    {
        INTERNAL,
        EXTERNAL,
        INTERSECT
    }

    [DebuggerDisplay("{Point}:{Designation}")]
    public class Ear
    {
        public Ear(Point point, Segment segment, Designation designation)
        {
            this.Point = point;
            this.Segment = segment;
            this.Designation = designation;
        }

        public Point Point { get; }
        public Segment Segment { get; }
        public Designation Designation { get; }

        public bool IsEar
        {
            get
            {
                return this.Designation == Designation.INTERNAL;
            }
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

    public class Triangle
    {
        public Triangle(Point a, Point b, Point c)
        {
            this.A = a;
            this.B = b;
            this.C = c;
        }

        public Point A { get; }
        public Point B { get; }
        public Point C { get; }

        public override string ToString()
        {
            return A.ToString() + " " + B.ToString() + " " + C.ToString();
        }
    }

    public class EarCuttingTriangulator
    {
        public IEnumerable<Triangle> Triangulate(IReadOnlyCollection<Point> polygon)
        {
            var polygonEdges = polygon
                .Select((p, i) => {
                    return new Segment(p, polygon.NextItem(i));
                })
                .ToList();

            var ears = polygon
                .Select((p, i) =>
                {
                    var startIndex = polygon.PreviousIndex(i);
                    var segment = new Segment(polygon.ElementAt(startIndex), polygon.NextItem(startIndex));
                    var designation = this.CalculateDesignation(segment, startIndex, polygonEdges);

                    return new Ear(p, segment, designation);
                })
                .ToList();


            var triangles = new List<Triangle>();

            var index = 0;
            while(ears.Count > 3)
            {
                // find first ear
                index = this.FindNextEarIndex(ears, index);
                var previousIndex = index.PreviousIndex(ears.Count);

                // add triangle
                triangles.Add(new Triangle(ears.PreviousItem(index).Point, ears[index].Point, ears.NextItem(index).Point));

                // remove ear and updated edges
                ears.RemoveAt(index);
                this.RemoveEdgeAndUpdate(polygonEdges, index);

                // update left and right neighbour segment
                var leftIndex = index == 0 ? ears.Count - 1 : index - 1;
                this.UpdateEar(ears, leftIndex, polygonEdges);

                // update left and right neighbour segment
                var rightIndex = index == ears.Count ? 0 : index;
                this.UpdateEar(ears, rightIndex, polygonEdges);
            }

            triangles.Add(new Triangle(ears[0].Point, ears[1].Point, ears[2].Point));
            return triangles;
        }

        private Designation CalculateDesignation(
            Segment diagonal,
            int startPointIndex,
            IReadOnlyCollection<Segment> edges)
        {
            var previousEdgeIndex = (startPointIndex + edges.Count - 1) % edges.Count;

            for (var i = 0; i < edges.Count; i++)
            {
                var edge = edges.ElementAt(i);
                if(edge.A == diagonal.A
                    || edge.A == diagonal.B
                    || edge.B == diagonal.A
                    || edge.B == diagonal.B)
                {
                    continue;
                }

                var it = diagonal.CalculateIntersecitonParameter(edge);
                var it2 = edge.CalculateIntersecitonParameter(diagonal);

                if (it >= 0 && it <= 1
                    && it2 >= 0 && it2 <= 1)
                {
                    return Designation.INTERSECT;
                }
            }

            var nextNeighour = edges.ElementAt(startPointIndex).B;
            var previousNeighour = edges.ElementAt(previousEdgeIndex).A;

            var isInArc = IsInArc(previousNeighour, diagonal.A, nextNeighour, diagonal.B);

            return isInArc
                ? Designation.INTERNAL
                : Designation.EXTERNAL;
        }

        private bool IsInArc(Point a, Point b, Point c, Point x)
        {
            var area2 = Point.Area2(a, b, c);
            var isInArc = area2 >= 0
                ? IsInConvexArc(a, b, c, x)
                : IsInConcaveArc(a, b, c, x);

            return isInArc;
        }

        private bool IsInConvexArc(Point a, Point b, Point c, Point x)
        {
            var a1 = Point.Area2(a, b, x);
            var a2 = Point.Area2(b, c, x);

            return a1 > 0 && a2 > 0;
        }

        private bool IsInConcaveArc(Point a, Point b, Point c, Point x)
        {
            var a1 = Point.Area2(a, b, x);
            var a2 = Point.Area2(b, c, x);

            return !(a1 < 0 && a2 < 0);
        }

        private void RemoveEdgeAndUpdate(List<Segment> edges, int removeIndex)
        {
            var previousIndex = edges.PreviousIndex(removeIndex)
                - (removeIndex == 0 ? 1 : 0);

            edges.RemoveAt(removeIndex);

            var newNextIndex = edges.NextIndex(previousIndex);
            var newEdge = new Segment(edges[previousIndex].A, edges[newNextIndex].A);
            edges[previousIndex] = newEdge;
        }

        private void UpdateEar(List<Ear> ears, int atIndex, IReadOnlyCollection<Segment> polygonEdges)
        {
            var startIndex = ears.PreviousIndex(atIndex);
            var endIndex = ears.NextIndex(atIndex);

            var segment = new Segment(ears[startIndex].Point, ears[endIndex].Point);
            var designation = this.CalculateDesignation(segment, startIndex, polygonEdges);

            ears[atIndex] = new Ear(ears[atIndex].Point, segment, designation);
        }

        private int FindNextEarIndex(List<Ear> ears, int startFrom)
        {
            var i = startFrom % ears.Count;
            do
            {
                if (ears[i].IsEar)
                {
                    return i;
                }

                i = ears.NextIndex(i);
            } while (true);
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
