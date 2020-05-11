using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Shared
{
    [DebuggerDisplay("{ToDebugString()}")]
    public class AvlTree<T> : IEnumerable<T>
    {
        private AvlNode<T> _root;
        private NodePool<T> _pool;
        private readonly Func<T, string> _toDebugString;

        public AvlTree(int capacity) : this(capacity, Comparer<T>.Default)
        {
        }

        public AvlTree(int capacity, IComparer<T> comparer, Func<T, string> toDebugString = null)
        {
            _pool = new NodePool<T>(comparer);
            _pool.Initialize(capacity);
            _toDebugString = toDebugString;
        }

        public int Count
        {
            get
            {
                return _root == null ? 0 : _root.Count;
            }
        }

        public int BalanceFactor
        {
            get
            {
                return _root == null ? 0 : _root.BalanceFactor;
            }
        }

        public void Add(params T[] values)
        {
            for (var i = 0; i < values.Length; i++)
            {
                if (_root == null)
                {
                    _root = _pool.Create(values[i]);
                }
                else
                {
                    _root = _root.Add(values[i]);
                }
            }
        }

        public int IndexOf(T value)
        {
            return _root == null ? -1 : _root.IndexOf(value, 0);
        }

        public bool Contains(T value)
        {
            return _root == null ? false : _root.Contains(value);
        }

        public void Remove(T value)
        {
            if (_root == null)
            {
                throw new Exception("Element does not exist");
            }

            _root = _root.Remove(value);
        }

        public void RemoveAt(int index)
        {
            if (_root == null)
            {
                throw new Exception("Element does not exist");
            }

            var v = _root.ElementAt(index);
            this.Remove(v);
        }

        public void Clear()
        {
            _root = null; // new EmptyNode<T>(_comparer);
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

    internal class NodePool<T>
    {
        private readonly IComparer<T> _comparer;
        private Stack<AvlNode<T>> _stack;

        public NodePool(IComparer<T> comparer)
        {
            _comparer = comparer;
        }

        public void Initialize(int capacity)
        {
            _stack = new Stack<AvlNode<T>>(capacity);
            for(var i = 0; i < capacity; i++)
            {
                var node = new AvlNode<T>(_comparer);
                _stack.Push(node);
            }
        }

        public AvlNode<T> Create(T value)
        {
            var node = _stack.Pop();
            node.NodePool = this;
            node.Reset(value);
            return node;
        }

        public void Return(AvlNode<T> node)
        {
            if(node.Left != null || node.Right != null)
            {
                throw new Exception("Reset node to empty before return to pool");
            }

            node.NodePool = null;
            _stack.Push(node);
        }
    }

    [DebuggerDisplay("{Left.ValuesList()} | {Value} | {Right.ValuesList()}")]
    internal class AvlNode<T> : IEnumerable<T>
    {
        private readonly IComparer<T> _comparer;
        private AvlNode<T> _left;
        private AvlNode<T> _right;

        public AvlNode(IComparer<T> comparer)
        {
            _comparer = comparer;
            this.IsEmpty = true;
        }

        public AvlNode(T value, IComparer<T> comparer)
        {
            Value = value;
            _comparer = comparer;
            _left = null;
            _right = null;
            this.UpdateValues();
        }

        public NodePool<T> NodePool { get; set; }

        public AvlNode<T> Left
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

        public AvlNode<T> Right
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
            get;
            private set;
        }

        public bool IsHighest => this.Right.IsEmpty;

        public int Count { get; private set; }

        public int BalanceFactor { get; private set; }

        public int Height { get; private set; }

        public T Value { get; private set; }

        public AvlNode<T> Add(T item)
        {
            switch (this.CompareTo(item))
            {
                case Comparison.Equal:
                    throw new Exception("Item with same value already in the tree");
                case Comparison.Higher:
                    this.Left = this.Left == null
                        ? this.NodePool.Create(item)
                        : this.Left.Add(item);
                    break;
                case Comparison.Lower:
                    this.Right = this.Right == null
                        ? this.NodePool.Create(item)
                        : this.Right.Add(item);
                    break;
            }

            var newRoot = this.Rebalance();
            return newRoot;
        }

        public AvlNode<T> Remove(T item)
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
            if (_left != null)
            {
                foreach (var i in this.Left)
                {
                    yield return i;
                }
            }

            yield return Value;

            if (_right != null)
            {
                foreach (var i in this.Right)
                {
                    yield return i;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private Comparison CompareTo(T item)
        {
            return (Comparison)Math.Sign(_comparer.Compare(Value, item));
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

        private AvlNode<T> RemoveSelf()
        {
            var left = this.Left;
            var right = this.Right;

            this.Left = null;
            this.Right = null;
            this.NodePool.Return(this);

            if(left == null)
            {
                return right;
            }

            if(right == null)
            {
                return left;
            }

            var pair = left.ElevateHighest();
            var replacement = pair.Item1;
            replacement.Right = right;
            replacement.Left = pair.Item2;
            return replacement;
        }

        private Tuple<AvlNode<T>, AvlNode<T>> ElevateHighest()
        {
            if (this.Right.IsEmpty)
            {
                return new Tuple<AvlNode<T>, AvlNode<T>>(this, this.Left);
            }

            var pair = this.Right.ElevateHighest();
            this.Right = pair.Item2;
            var newRoot = this.Rebalance();
            return new Tuple<AvlNode<T>, AvlNode<T>>(pair.Item1, newRoot);
        }

        public void UpdateValues()
        {
            var leftHeight = _left == null
                ? 0
                : _left.Height;


            var rightHeight = _right == null
                ? 0
                : _right.Height;

            Height = Math.Max(leftHeight, rightHeight) + 1;
            Count = (_left == null ? 0 : _left.Count) + (_right == null ? 0 :_right.Count) + 1;
            BalanceFactor = rightHeight - leftHeight;
        }

        // make sure subtreeRoot is not the highest
        private static AvlNode<T> ParentOfHighestValue(AvlNode<T> subtreeRoot)
        {
            var right = subtreeRoot.Right;
            if (right.IsHighest)
            {
                return subtreeRoot;
            }

            return ParentOfHighestValue(right);
        }

        private static AvlNode<T> RotateLeft(AvlNode<T> x)
        {
            var r = x.Right;
            x.Right = r.Left;
            r.Left = x;
            return r;
        }

        private static AvlNode<T> RotateRight(AvlNode<T> x)
        {
            var l = x.Left;
            x.Left = l.Right;
            l.Right = x;
            return l;
        }

        private static AvlNode<T> RotateRightLeft(AvlNode<T> x)
        {
            var right = x.Right;
            right = RotateRight(right);
            x.Right = right;

            var root = RotateLeft(x);
            return root;
        }

        private static AvlNode<T> RotateLeftRight(AvlNode<T> x)
        {
            var left = x.Left;
            left = RotateLeft(left);
            x.Left = left;

            var root = RotateRight(x);
            return root;
        }

        public string ValuesList()
        {
            var left = _left == null ? "" : this.Left.ValuesList();
            var right = _right == null ? "" : this.Right.ValuesList();

            return left
                + (string.IsNullOrWhiteSpace(left) ? "" : ", ")
                + Value
                + (string.IsNullOrWhiteSpace(right) ? "" : ", ")
                + right;
        }

        public void Reset(T value)
        {
            this.Value = value;
            this.Count = 1;
            this.Height = 1;
            this.IsEmpty = false;
        }

        public void ResetToEmpty()
        {
            this.Left = null;
            this.Right = null;
            this.Value = default(T);
            this.Count = 0;
            this.Height = 0;
            this.IsEmpty = true;
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
        Proceed Visit(AvlNode<T> node);
    }
}
