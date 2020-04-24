using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

        [TestMethod]
        public void Case6()
        {
            var segments = new[] {
                new Segment(18,1,21,1),
                new Segment(-22,-6,15,-6),
                new Segment(-7,16,8,16),
                new Segment(11,17,24,17),
                new Segment(-4,13,9,13),
                new Segment(-22,2,-12,2),
                new Segment(4,-9,-20,-9),
                new Segment(-23,11,-7,11),
                new Segment(7,4,7,4),
                new Segment(-22,21,21,21),
                new Segment(18,-16,-1,-16),
                new Segment(-10,-19,-10,12)
            };

            Execute(segments, 3);
        }

        [TestMethod]
        public void ManySegments()
        {
            var hSegments = Enumerable.Range(2, 25000)
                .Select(i => new Segment(0, i, 50000, i));

            var vSegments = Enumerable.Range(1, 25000)
                .Select(i => new Segment(i, 0, i, 26000));

            Execute(hSegments.Union(vSegments).ToArray(), 25000 * 25000);
        }

        private void Execute(IReadOnlyCollection<Segment> segments, int expectedCount)
        {
            var sw = new Stopwatch();
            // var svg = segments.ToSvg();

            sw.Start();
            var intersections = new IntersectionCounter().CountIntersections(segments);
            sw.Stop();

            Console.WriteLine("Intersections count: {0}", intersections); 
            Console.WriteLine("Elapsed time: {0}ms", sw.ElapsedMilliseconds);

            Assert.AreEqual(expectedCount, intersections);
        }
    }
}
