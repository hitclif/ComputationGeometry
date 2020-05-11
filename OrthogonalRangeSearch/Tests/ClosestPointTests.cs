using System;
using System.Collections.Generic;
using System.Linq;
using ClosestPoint;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class ClosestPointTests
    {
        private Random _random = new Random();

        [TestMethod]
        public void TestCase1()
        {
            this.Execute(
                "-7 3 8 12 -3 -10",
                new [] { -12, 13, -5, -3, 5, -2, 0 },
                new [] { -10, 12, -3, -3, 3, -3, 3 }
                );
        }

        [TestMethod]
        public void TestCaseX()
        {
            this.Execute(
                "-7 3 8 12 -3 -10",
                new[] { -12 },
                new[] { -10 }
                );
        }

        [TestMethod]
        public void TestCaseR1()
        {
            var p = "-272365 230774 238148 -33596 -556196 -795523 875824 625687 318184 880844 632542 574443 -558208 208552 157296 895912 -194273 -310229 -962501 -805895";
            var q = new [] { -303992 };

            var list = new List<int>(p.ToPoints());
            list.Sort();
            var expected = this.FindClosestPointTo(list , q[0]);

            this.Execute(p, q, new[] { expected });
        }

        [TestMethod]
        public void Random()
        {
            var points = this.RandomPoints(1000);
            Console.WriteLine("Points:");
            Console.WriteLine(points);

            var list = new List<int>(points.ToPoints());
            list.Sort();

            var queryPoints = this.RandomQueryPoints(10);
            Console.WriteLine("Query points:");
            Console.WriteLine(string.Join(" ", queryPoints));

            var expected = queryPoints
                .Select(p => this.FindClosestPointTo(list, p))
                .ToArray();

            Console.WriteLine("Expected results:");
            Console.WriteLine(string.Join(" ", expected));

            this.Execute(
                points,
                queryPoints,
                expected);
        }

        private void Execute(
            string pointsCoordinates,
            int[] queryPoints,
            int[] expected)
        {
            var points = pointsCoordinates.ToPoints();
            var finder = new ClosestPointFinder(points);

            var result = queryPoints
                .Select((p, i) => {
                    var c = finder.FindClosestPointTo(p);
                    Console.WriteLine("Search for {0}; Found {1}; Expected {2}", p, c, expected[i]);
                    return c;
                })
                .ToArray();

            for(var i =0; i < result.Length; i++)
            {
                Assert.AreEqual(expected[i], result[i]);
            }
        }

        private string RandomPoints(int approximateCount)
        {
            var points = Enumerable.Repeat(0, approximateCount)
                .Select(_ => _random.Next(2000000) - 1000000)
                .Distinct()
                .ToArray();

            return string.Join(" ", points);
        }

        private int[] RandomQueryPoints(int approximateCount)
        {
            var points = Enumerable.Repeat(0, approximateCount)
                .Select(_ => _random.Next(2000000) - 1000000)
                .Distinct()
                .ToArray();

            return points;
        }

        public int FindClosestPointTo(List<int> points, int point)
        {
            var index = points.BinarySearch(point);

            if (index < 0)
            {
                index = ~index;
            }

            if (index >= points.Count)
            {
                return points.Last();
            }

            var p1 = points[index];
            if (index == 0)
            {
                return p1;
            }

            var p2 = points[index - 1];

            var d1 = Math.Abs(p1 - point);
            var d2 = Math.Abs(p2 - point);

            return d1 <= d2
                ? p1
                : p2;
        }
    }
}
