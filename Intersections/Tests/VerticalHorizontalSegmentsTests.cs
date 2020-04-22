using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerticalHorizontalSegments;

namespace Tests
{
    [TestClass]
    public class VerticalHorizontalSegmentsTests
    {
        [TestMethod]
        public void Case1()
        {
            var segments = new[] {
                new Segment(-10,3,-5,3),
                new Segment(-5,0,-5,8),
                new Segment(-7,4,-7,7),
                new Segment(-7,7,-5,7),
                new Segment(-8,5,-8,6),
                new Segment(-8,5,-5,5),
                new Segment(-8,6,-3,6),
            };

            Execute(segments, 9);
        }

        [TestMethod]
        public void Case2()
        {
            var segments = new[] {
                new Segment(22, -4, 22, -12),
                new Segment(-14,1,-5,1),
                new Segment(-23,-3,-6,-3),
                new Segment(-6,13,-6,12),
                new Segment(-1,19,1,19),
                new Segment(-17,4,12,4),
                new Segment(-15,15,-15,11),
                new Segment(-6,-22,2,-22),
            };

            Execute(segments, 0);
        }

        [TestMethod]
        public void Case3()
        {
            var segments = new[] {
                new Segment(-9,1,-9,2),
                new Segment(9,7,9,9),
                new Segment(4,0,3,0),
                new Segment(10,17,10,0),
                new Segment(17,-22,-16,-22),
                new Segment(5,-22,5,-4),
                new Segment(6,4,6,11),
                new Segment(20,15,-18,15),
                new Segment(0,12,0,-24),
            };

            Execute(segments, 3);
        }

        [TestMethod]
        public void C1()
        {
            var segments = new[] {
                new Segment(0,0,10,0),
                new Segment(0,10,0,0),
                new Segment(10,10,10,0),
            };

            Execute(segments, 2);
        }

        private void Execute(IReadOnlyCollection<Segment> segments, int expectedCount)
        {
            var svg = segments.ToSvg();

            var intersections = segments.CountIntersections();
            Console.WriteLine("Intersections count: {0}", intersections);

            Assert.AreEqual(expectedCount, intersections);
        }
    }
}
