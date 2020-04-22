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
        public void Case3()
        {
            var segments = new[]
            {
                new Segment(1, -87, 83, 69, 70),
                new Segment(2, -31, -17, -8, 72),
                new Segment(3, 35, -4, -37, 37),
                new Segment(4, -80, -36, 96, 43),
                new Segment(5, 16, -95, 42, 90),
                new Segment(6, 20, -76, 48, -55),
                new Segment(7, 99, 0, 37, -28),
                new Segment(8, -33, -85, 4, -45),
                new Segment(9, 47, 2, 16, -9)
            };

            Execute(segments, 8);
        }

        [TestMethod]
        public void Case4()
        {
            var segments = new[]
            {
                new Segment(1,91,82,-66,-92),
                new Segment(2,58,-69,-34,-3),
                new Segment(3,95,23,-91,-39),
                new Segment(4,70,63,-17,28),
                new Segment(5,83,-44,-25,2),
                new Segment(6,-57,94,-37,4),
                new Segment(7,93,-96,94,65),
            };

            Execute(segments, 6);
        }

        [TestMethod]
        public void Case5()
        {
            var segments = new[]
            {
                new Segment(1, -68,75,28,-5),
                new Segment(1, 48,-88,-4,-9),
                new Segment(1, -63,39,32,60),
                new Segment(1, -8,89,-3,18),
                new Segment(1, 93,50,-48,42),
                new Segment(1, -83,14,-46,32),
                new Segment(1, -8,69,-30,-49),
                new Segment(1, -50,-90,-76,-36),
                new Segment(1, -37,62,10,0)
            };

            Execute(segments, 12);
        }

        [TestMethod]
        public void Case6()
        {
            var segments = new[]
            {
                 new Segment(1, 74, -9, -14, 87),
                 new Segment(2, 93,-90, -95, -39),
                 new Segment(3, -16, 17, -85, -57),
                 new Segment(4, -57, -12, 38, -4),
                 new Segment(5, 3, 98, -83, 34),
                 new Segment(6, 98, 14, -7, -66),
                 new Segment(7, -98, 83, 91, 25),
                 new Segment(8, -65, 9, -89, 86)
            };

            Execute(segments, 9);
        }

        [TestMethod]
        public void Case7()
        {
            var segments = new[]
            {
                new Segment(1, 84,-59,39,-38),
                new Segment(1, 35,19,-32,-11),
                new Segment(1, -34,-82,-100,28),
                new Segment(1, 29,0,42,-68),
                new Segment(1, 75,-90,97,48),
                new Segment(1, -21,-27,84,-9),
                new Segment(1, -15,-24,-34,87),
                new Segment(1, 15,-18,8,-42),
                new Segment(1, -36,-53,-10,-38),
                new Segment(1, 31,39,-11,-6),
                new Segment(1, 71,-71,66,57),
                new Segment(1, 70,83,-71,23)
            };

            Execute(segments, 8);
        }

        [TestMethod]
        public void Case12()
        {
            var segments = new[]
            {
                new Segment(1,74,-9,-14,87),
                new Segment(1,93,-90,-95,-39),
                new Segment(1,-16,17,-85,-57),
                new Segment(1,-57,-12,38,-4),
                new Segment(1,3,98,-83,34),
                new Segment(1,98,14,-7,-66),
                new Segment(1,-98,83,91,25),
                new Segment(1,-65,9,-89,86)
            };

            Execute(segments, 9);
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

            var intersections = new SweepLine(segments, true).FindIntersections().ToArray();
            Console.WriteLine("Intersections count: {0}", intersections.Length);

            foreach (var intersection in intersections)
            {
                Console.WriteLine(intersection.U.ToString() + " " + intersection.V.ToString());
            }

            Assert.AreEqual(expectedCount, intersections.Length);
        }

       
    }
}
