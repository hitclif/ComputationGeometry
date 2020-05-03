using System;
using System.Collections.Generic;
using System.Linq;
using Diagonal;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class DiagonalTests
    {
        [TestMethod]
        public void TestCase1()
        {
            var expected = new[] {
                "-6 1 1 1 INTERNAL",
                "-6 1 2 -2 INTERSECT",
                "-6 1 6 2 INTERNAL",
                "-6 1 2 6 INTERSECT",
                "-6 1 -2 5 EXTERNAL",
                "-4 -1 2 -2 EXTERNAL",
                "-4 -1 6 2 INTERSECT",
                "-4 -1 2 6 INTERNAL",
                "-4 -1 -2 5 INTERSECT",
                "-4 -1 -2 2 INTERNAL",
                "1 1 6 2 INTERNAL",
                "1 1 2 6 INTERNAL",
                "1 1 -2 5 INTERNAL",
                "1 1 -2 2 INTERNAL",
                "2 -2 2 6 INTERNAL",
                "2 -2 -2 5 INTERSECT",
                "2 -2 -2 2 INTERSECT",
                "6 2 -2 5 INTERNAL",
                "6 2 -2 2 INTERNAL",
                "2 6 -2 2 INTERNAL"
            };
            this.Execute("-6 1 -4 -1 1 1 2 -2 6 2 2 6 -2 5 -2 2", expected);
        }

        private void Execute(string coordinates, string[] expectedOutput)
        {
            var polygon = coordinates.ToPolygon();

            var diagonals = new DiagonalFinder()
                .FindDiagonals(polygon)
                .ToArray();

            Console.WriteLine(diagonals.Length);
            foreach (var d in diagonals)
            {
                Console.WriteLine(d.ToString());
            }

            for(var  i = 0; i < diagonals.Length; i++)
            {
                Assert.AreEqual(expectedOutput[i], diagonals[i].ToString());
            }

            Assert.AreEqual(expectedOutput.Length, diagonals.Length);
        }
    }
}
