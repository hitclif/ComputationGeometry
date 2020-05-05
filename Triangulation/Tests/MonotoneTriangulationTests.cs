using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MonotoneTriangulation;

namespace Tests
{
    [TestClass]
    public class MonotoneTriangulationTests
    {
        [TestMethod]
        public void TestCase1()
        {
            var expected = new[] 
            {
                "-5 6 11 7",
                "-2 0 11 7",
                "5 -1 -2 0"
            };

            this.Execute("0 8 -5 6 -2 0 1 -8 5 -1 11 7", expected);
        }

        [TestMethod]
        public void TestCase2()
        {
            var expected = new[]
            {
                "-6 5 26 11",
                "0 -7 26 11",
                "34 -10 0 -7",
                "21 -12 0 -7",
                "-30 -13 21 -12"
            };

            this.Execute("-17 12 -6 5 0 -7 -30 -13 3 -14 21 -12 34 -10 26 11", expected);
        }

        [TestMethod]
        public void TestCase3()
        {
            var expected = new[]
            {
                "2 7 2 10",
            };

            this.Execute("0 0 2 7 5 8 2 10", expected);
        }

        [TestMethod]
        public void TestCase4()
        {
            var expected = new[]
            {
                "2 5 2 8",
                "1 3 2 5",
                "1 3 2 8"
            };

            this.Execute("0 0 1 3 5 4 2 5 6 6 2 8", expected);
        }

        private void Execute(string coordinates, string[] expectedOutput)
        {
            var polygon = coordinates.ToPolygon();
            var svg = polygon.ToSvg();

            var diagonals = new MonotoneTriangulator()
               .Triangulate(polygon)
               .ToArray();

            Console.WriteLine(diagonals.Length);
            foreach (var d in diagonals)
            {
                Console.WriteLine(d.ToString());
            }

            for (var i = 0; i < diagonals.Length; i++)
            {
                Assert.AreEqual(expectedOutput[i], diagonals[i].ToString());
            }

            Assert.AreEqual(expectedOutput.Length, diagonals.Length);
        }
    }
}
