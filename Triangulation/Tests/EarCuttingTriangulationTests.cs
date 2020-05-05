using System;
using System.Linq;
using EarCuttingTriangulation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class EarCuttingTriangulationTests
    {
        [TestMethod]
        public void TestCase1()
        {
            var expected = new[]
            {
                "1 1 4 -1 8 5",
                "1 1 8 5 3 3",
                "1 1 3 3 1 6",
                "1 1 1 6 -2 0",
                "-5 2 -5 6 -7 1",
                "-5 2 -7 1 -5 -2",
                "-5 -2 1 1 -2 0",
                "-5 -2 -2 0 -5 2"
            };

            this.Execute("-7 1 -5 -2 1 1 4 -1 8 5 3 3 1 6 -2 0 -5 2 -5 6", expected);
        }

        private void Execute(string coordinates, string[] expectedOutput)
        {
            var polygon = coordinates.ToPolygon();
            var svg = polygon.ToSvg();

            var triangulations = new EarCuttingTriangulator()
                .Triangulate(polygon)
                .ToArray();

            Console.WriteLine(triangulations.Length);
            foreach (var d in triangulations)
            {
                Console.WriteLine(d.ToString());
            }

            for (var i = 0; i < triangulations.Length; i++)
            {
                Assert.AreEqual(expectedOutput[i], triangulations[i].ToString());
            }

            Assert.AreEqual(expectedOutput.Length, triangulations.Length);
        }
    }
}
