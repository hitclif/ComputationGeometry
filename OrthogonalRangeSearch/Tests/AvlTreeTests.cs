using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shared;

namespace Tests
{
    [TestClass]
    public class AvlTreeTests
    {
        private Random _random = new Random();
        private AvlTree<int> _testee;

        [TestMethod]
        public void Insert0to99()
        {
            var points = Enumerable.Range(0, 100).ToArray();
            this.Execute(points);

            var expected = 0;
            foreach(var v in _testee)
            {
                Assert.AreEqual(expected, v);
                expected++;
            }
        }

        [TestMethod]
        public void InsertAndRemove0to99()
        {
            var points = Enumerable.Range(0, 100).ToArray();

            this.Execute(points);

            foreach (var v in Enumerable.Range(0, 100))
            {
                _testee.Remove(v);
            }

            Assert.AreEqual(0, _testee.Count);
        }

        [TestMethod]
        public void Test()
        {
            var points = this.RandomPoints(150000);
            this.Execute(points);
        }

        private void Execute(int[] points)
        {
            _testee = new AvlTree<int>(points.Length);

            var sw = new Stopwatch();
            sw.Start();
            foreach(var p in points)
            {
                _testee.Add(p);
            }
            sw.Stop();

            Console.WriteLine("Add {0} values to tree was {1}ms", points.Length, sw.ElapsedMilliseconds);

            Assert.AreEqual(points.Length, _testee.Count);
            var list = new List<int>();
            sw.Reset();

            sw.Start();
            foreach (var p in points)
            {
                var index = list.BinarySearch(p);
                if(index < 0)
                {
                    index = ~index;
                }

                list.Insert(index, p);
            }
            sw.Stop();

            Console.WriteLine("Add {0} values to list was {1}ms", points.Length, sw.ElapsedMilliseconds);
        }

        private int[] RandomPoints(int approximateCount)
        {
            var points = Enumerable.Repeat(0, approximateCount)
                .Select(_ => _random.Next(2000000) - 1000000)
                .Distinct()
                .ToArray();

            return points;
        }
    }
}
