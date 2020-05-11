using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Shared.KdTrees
{
    [DebuggerDisplay("{X} {Y}")]
    public class Point
    {
        public Point(long x, long y)
        {
            this.X = x;
            this.Y = y;
        }

        public long X { get; }
        public long Y { get; }
    }

    [DebuggerDisplay("{_root}")]
    public class KdTree
    {
        private KdNode _root;

        private KdTree(KdNode root)
        {
            _root = root;
        }

        public void Accept(IKdVisitor visitor)
        {
            if(_root == null)
            {
                return;
            }

            _root.Accept(visitor);
        }

        public static KdTree Construct(IReadOnlyCollection<Point> points)
        {
            if(points.Count == 0)
            {
                return null;
            }

            var root = KdNode.Construct(Split.X, points, 0);
            return new KdTree(root);
        }
    }

    public enum Split
    {
        X,
        Y
    }

    [DebuggerDisplay("{Left} | {Value} | {Right}")]
    public class KdNode
    {
        private KdNode(Point value, KdNode left, KdNode right, Split split, int depth)
        {
            this.Value = value;
            this.Left = left;
            this.Right = right;
            this.Split = split;
            this.Depth = depth;
        }

        public Point Value { get; private set; }
        public KdNode Left { get; private set; }
        public KdNode Right { get; private set; }
        public Split Split { get; private set; }
        public int Depth { get; private set; }

        public void Accept(IKdVisitor kdVisitor)
        {
            var proceed = kdVisitor.Visit(this);

            if (this.Left != null && proceed.HasFlag(Proceed.Left))
            {
                this.Left.Accept(kdVisitor);
            }

            if (this.Right != null && proceed.HasFlag(Proceed.Right))
            {
                this.Right.Accept(kdVisitor);
            }
        }

        public static KdNode Construct(Split split, IEnumerable<Point> points, int parentDepth)
        {
            var data = split == Split.X
                ? points.SplitByXMedian()
                : points.SplitByYMedian();

            var newSplit = split == Split.X
                ? Split.Y
                : Split.X;

            var depth = parentDepth + 1;

            var left = data.Item2.Count == 0
                ? null
                : KdNode.Construct(newSplit, data.Item2, depth);

            var right = data.Item3.Count == 0
                ? null
                : KdNode.Construct(newSplit, data.Item3, depth);

            
            return new KdNode(data.Item1, left, right, split, depth);
        }
    }


    [Flags]
    public enum Proceed
    {
        No = 0,
        Left = 1,
        Right = 2,
        Both = Left | Right
    }

    public interface IKdVisitor
    {
        Proceed Visit(KdNode node);
    }

    public static class Tools
    {
        public static IEnumerable<Point> ToPoints(this string coordinates)
        {
            var points = new List<Point>();
            var x = coordinates
                .Trim()
                .Split(' ')
                .Select(c => Convert.ToInt32(c))
                .ToArray();

            for(var i = 0; i < x.Length; i = i + 2)
            {
                var p = new Point(x[i], x[i + 1]);
                points.Add(p);
            }

            return points;
        }

        public static Tuple<Point, IReadOnlyCollection<Point>, IReadOnlyCollection<Point>> SplitByXMedian(this IEnumerable<Point> points)
        {
            var xValues = points
                .Select(p => p.X)
                .Distinct()
                .ToArray();

            var splitX = xValues[xValues.Length / 2];

            var lowerEqual = points
                .Where(p => p.X <= splitX)
                .OrderBy(p => p.X)
                .ToList();

            var splitPoint = lowerEqual.Last();
            lowerEqual.RemoveAt(lowerEqual.Count - 1);

            var higher = points
                .Where(p => p.X > splitX)
                .ToArray();

            return new Tuple<Point, IReadOnlyCollection<Point>, IReadOnlyCollection<Point>>(splitPoint, lowerEqual, higher);
        }

        public static Tuple<Point, IReadOnlyCollection<Point>, IReadOnlyCollection<Point>> SplitByYMedian(this IEnumerable<Point> points)
        {
            var yValues = points
                .Select(p => p.Y)
                .Distinct()
                .ToArray();

            var splitY = yValues[yValues.Length / 2];

            var lowerEqual = points
                .Where(p => p.Y <= splitY)
                .OrderBy(p => p.Y)
                .ToList();

            var splitPoint = lowerEqual.Last();
            lowerEqual.RemoveAt(lowerEqual.Count - 1);


            var higher = points
                .Where(p => p.Y > splitY)
                .ToArray();

            return new Tuple<Point, IReadOnlyCollection<Point>, IReadOnlyCollection<Point>>(splitPoint, lowerEqual, higher);
        }
    }
}
