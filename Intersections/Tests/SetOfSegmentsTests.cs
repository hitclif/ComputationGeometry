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
                new Segment(0, -7, -3, 2, 6),
                new Segment(1, -7, 2, 9, -2),
                new Segment(2, 3, -3, -3, 6)
            };

            Execute(segments, 3);
        }

        [TestMethod]
        public void Case2()
        {
            var segments = new[]
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

        private void Execute(IReadOnlyCollection<Segment> segments, int expectedCount)
        {
            var svg = segments.ToSvg();

            var intersections = segments.FindIntersections().ToArray();
            Console.WriteLine("Intersections count: {0}", intersections.Length);

            foreach (var intersection in intersections)
            {
                Console.WriteLine(intersection.U.ToString() + " " + intersection.V.ToString());
            }

            Assert.AreEqual(expectedCount, intersections.Length);
        }
    }
}
