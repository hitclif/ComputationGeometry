using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NumberOfPointsInRectangle;

namespace Tests
{
    [TestClass]
    public class NumberOfPointsInRectangleTests
    {
        [TestMethod]
        public void TestCase1_1()
        {
            this.Execute(
                "-9 6 -8 3 1 7 3 0 8 -4 -2 8 -6 -3 -1 2 2 -2 0 1 6 5 -5 4 -3 -1",
                "-10 2 -3 7",
                3);
        }

        [TestMethod]
        public void TestCase1_2()
        {
            this.Execute(
                "-9 6 -8 3 1 7 3 0 8 -4 -2 8 -6 -3 -1 2 2 -2 0 1 6 5 -5 4 -3 -1",
                "-4 -2 -2 -1",
                1);
        }

        [TestMethod]
        public void TestCase1_3()
        {
            this.Execute(
                "-9 6 -8 3 1 7 3 0 8 -4 -2 8 -6 -3 -1 2 2 -2 0 1 6 5 -5 4 -3 -1",
                "4 2 7 4",
                0);
        }

        [TestMethod]
        public void TestCase1_4()
        {
            this.Execute(
                "-9 6 -8 3 1 7 3 0 8 -4 -2 8 -6 -3 -1 2 2 -2 0 1 6 5 -5 4 -3 -1",
                "-9 -4 8 8",
                13);
        }

        private void Execute(string pointCoordinates, string rectangleCoordinates, int expectedNumber)
        {
            var points = pointCoordinates
               .ToPoints()
               .ToArray();

            var tree = KdTree.Construct(points);

            var corners = rectangleCoordinates.ToPoints().ToArray();
            var visitor = new NumberOfPointsVisitor(corners[0], corners[1]);
            tree.Accept(visitor);
            Console.WriteLine(visitor.Result);
            Assert.AreEqual(expectedNumber, visitor.Result);
        }
    }
}
