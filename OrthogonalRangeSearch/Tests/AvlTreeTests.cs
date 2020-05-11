using System;
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

        [TestMethod]
        public void Test()
        {
            var points = this.RandomPoints(500000);
            this.Execute(points);
        }

        private void Execute(int[] points)
        {
            var tree = new AvlTree<int>();

            var sw = new Stopwatch();
            sw.Start();
            foreach(var p in points)
            {
                tree.Add(p);
            }
            sw.Stop();

            Console.WriteLine("Add {0} values to tree was {1}ms", points.Length, sw.ElapsedMilliseconds);
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
