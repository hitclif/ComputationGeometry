using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SetOfSegments;

namespace Tests
{
    [TestClass]
    public class SetOfSegmentsTests
    {
        [TestMethod]
        public void Case1()
        {
            var segments = new[]
            {
                new Segment(1, -7, -3, 2, 6),
                new Segment(2, -7, 2, 9, -2),
                new Segment(3, 3, -3, -3, 6)
            };

            Execute(segments, 3);
        }

        [TestMethod]
        public void Case2()
        {
            var segments = new[]
            {
                new Segment(1, 29, 48, 30, 13),
                new Segment(2, 49, 57, -68, -24),
                new Segment(3, 17, -30, 55, 16),
                new Segment(4, 19, -61, 91, -2),
                new Segment(5, 44, -22, 21, 73),
                new Segment(6, -68, -38, -77, 90),
                new Segment(7, -74, 84, 55, -32),
                new Segment(8, 26, 72, -91, 64),
                new Segment(9, -21, -41, -3, 75)
            };

            Execute(segments, 13);
        }

        [TestMethod]
        public void Simple1()
        {
            var segments = new[]
            {
                new Segment(1, 0, 3, 3, 0),
                new Segment(2, 0, 0, 3,3)
            };

            Execute(segments, 1);
        }

        [TestMethod]
        public void EndPointRight()
        {
            var segments = new[]
            {
                new Segment(1, 5, 5, 10, 5),
                new Segment(2, 10, 5, 11, 11)
            };

            Execute(segments, 1);
        }

        [TestMethod]
        public void EndPointLeft()
        {
            var segments = new[]
            {
                new Segment(1, 5, 5, 10, 5),
                new Segment(2, 5, 5, 11, 11)
            };

            Execute(segments, 1);
        }

        private void Execute(IReadOnlyCollection<Segment> segments, int expectedCount)
        {
            var svg = segments.ToSvg();

            var intersections = new SweepLine(segments).FindIntersections().ToArray();
            Console.WriteLine("Intersections count: {0}", intersections.Length);

            foreach (var intersection in intersections)
            {
                Console.WriteLine(intersection.U.ToString() + " " + intersection.V.ToString());
            }

            Assert.AreEqual(expectedCount, intersections.Length);
        }
    }
}
