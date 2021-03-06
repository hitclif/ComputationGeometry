﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ClosestPoint
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ReadLine();
            var points = Console.ReadLine().ToPoints();
            var finder = new ClosestPointFinder(points);

            var count = Convert.ToInt32(Console.ReadLine());
            for(var i = 0; i < count; i++)
            {
                var point = Convert.ToInt32(Console.ReadLine());
                var closest = finder.FindClosestPointTo(point);
                Console.WriteLine(closest);
            }
        }
    }

    public class ClosestPointFinder
    {
        private AvlTree<int> _points = new AvlTree<int>();
        private ClosestPointVisitor _visitor = new ClosestPointVisitor();

        public ClosestPointFinder(IEnumerable<int> points)
        {
            foreach(var p in points)
            {
                _points.Add(p);
            }
        }

        public int FindClosestPointTo(int point)
        {
            _visitor.Reset(point);
            _points.Accept(_visitor);
            return _visitor.Result;
            //var index = _points.BinarySearch(point);

            //if(index < 0)
            //{
            //    index = ~index;
            //}

            //if(index >= _points.Count)
            //{
            //    return _points.Last();
            //}

            //var p1 = _points[index];
            //if(index == 0)
            //{
            //    return p1;
            //}

            //var p2 = _points[index - 1];

            //var d1 = Math.Abs(p1 - point);
            //var d2 = Math.Abs(p2 - point);

            //return d1 <= d2
            //    ? p1
            //    : p2;
        }
    }

    public static class Tools
    {
        public static IEnumerable<int> ToPoints(this string line)
        {
            var points = line
                .Trim()
                .Split(' ')
                .Select(p => Convert.ToInt32(p));
            return points;
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

        internal void Accept(IAvlNodeVisitor<T> visitor)
        {
            _root.Accept(visitor);
        }
    }

    internal interface IAvlNode<T> : IEnumerable<T>
    {
        T Value { get; }
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

        void Accept(IAvlNodeVisitor<T> visitor);
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

        public T Value
        {
            get
            {
                return _value;
            }
        }

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

        public void Accept(IAvlNodeVisitor<T> visitor)
        {
            var proceed = visitor.Visit(this);
            if (proceed.HasFlag(Proceed.Left))
            {
                this.Left.Accept(visitor);
            }

            if (proceed.HasFlag(Proceed.Right))
            {
                this.Right.Accept(visitor);
            }
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

        public T Value
        {
            get
            {
                return default(T);
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

        public void Accept(IAvlNodeVisitor<T> visitor)
        {
            visitor.Visit(this);
        }
    }

    internal enum Comparison
    {
        Lower = -1,
        Equal = 0,
        Higher = 1
    }

    [Flags]
    internal enum Proceed
    {
        No = 0,
        Left = 1,
        Right = 2
    }

    internal interface IAvlNodeVisitor<T>
    {
        Proceed Visit(IAvlNode<T> node);
    }

    internal class ClosestPointVisitor : IAvlNodeVisitor<int>
    {
        private int _queryPoint;
        private int _currentDistance = int.MaxValue;

        public int Result { get; private set; }

        public void Reset(int queryPoint)
        {
            _queryPoint = queryPoint;
            _currentDistance = int.MaxValue;
            this.Result = int.MaxValue;
        }

        public Proceed Visit(IAvlNode<int> node)
        {
            if (node.IsEmpty)
            {
                return Proceed.No;
            }

            if(_queryPoint == node.Value)
            {
                this.Result = node.Value;
                return Proceed.No;
            }

            var proceed = _queryPoint < node.Value
                ? Proceed.Left
                : Proceed.Right;

            var distance = Math.Abs(_queryPoint - node.Value);
            if (distance < _currentDistance)
            {
                _currentDistance = distance;
                this.Result = node.Value;
            }

            return proceed;
        }
    }
}
