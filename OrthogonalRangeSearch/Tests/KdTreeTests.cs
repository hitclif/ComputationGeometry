using System.Linq;
using Shared.KdTrees;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class KdTreeTests
    {
        [TestMethod]
        public void Test1()
        {
            var points = "-9 6 -8 3 1 7 3 0 8 -4 -2 8 -6 -3 -1 2 2 -2 0 1 6 5 -5 4 -3 -1".ToPoints().ToArray();
            var kdTree = KdTree.Construct(points);
        }
    }
}
