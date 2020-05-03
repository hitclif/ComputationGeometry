using Microsoft.VisualStudio.TestTools.UnitTesting;
using NumberOfTriangulations;

namespace Tests
{
    [TestClass]
    public class NumberOfTriangulationsTests
    {
        [TestMethod]
        public void N3()
        {
            var result = Tools.TotalNumberOfTriangulations(3);

            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void N4()
        {
            var result = Tools.TotalNumberOfTriangulations(4);
            Assert.AreEqual(2, result);
        }

        [TestMethod]
        public void N5()
        {
            var result = Tools.TotalNumberOfTriangulations(5);
            Assert.AreEqual(5, result);
        }

        [TestMethod]
        public void N6()
        {
            var result = Tools.TotalNumberOfTriangulations(6);
            Assert.AreEqual(14, result);
        }

        [TestMethod]
        public void N7()
        {
            var result = Tools.TotalNumberOfTriangulations(7);
            Assert.AreEqual(42, result);
        }

        [TestMethod]
        public void N8()
        {
            var result = Tools.TotalNumberOfTriangulations(8);
            Assert.AreEqual(132, result);
        }

        [TestMethod]
        public void N9()
        {
            var result = Tools.TotalNumberOfTriangulations(9);
            Assert.AreEqual(429, result);
        }

        [TestMethod]
        public void N10()
        {
            var result = Tools.TotalNumberOfTriangulations(10);
            Assert.AreEqual(1430, result);
        }

        [TestMethod]
        public void N11()
        {
            var result = Tools.TotalNumberOfTriangulations(11);
            Assert.AreEqual(4862, result);
        }

        [TestMethod]
        public void N12()
        {
            var result = Tools.TotalNumberOfTriangulations(12);
            Assert.AreEqual(16796, result);
        }

        [TestMethod]
        public void N30()
        {
            var result = Tools.TotalNumberOfTriangulations(30);
            Assert.AreEqual(263747951750360, result);
        }
    }
}
