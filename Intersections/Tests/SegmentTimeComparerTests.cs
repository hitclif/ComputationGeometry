using Microsoft.VisualStudio.TestTools.UnitTesting;
using SetOfSegments;

namespace Tests
{
    [TestClass]
    public class SegmentTimeComparerTests
    {
        /*
        u: \
            \
             \
              \ ---    v
               \
                \
                 \
                  \
                   \
        */
        [TestMethod]
        public void ComparerTest1()
        {
            var u = new Segment(1, 10, 0, 0, 10);
            var v = new Segment(2, 6, 5, 8, 5);

            var compare1 = this.Compare(u, v, 6);
            var compare2 = this.Compare(u, v, 8);
            var compare3 = this.Compare(v, u, 6);
            var compare4 = this.Compare(v, u, 8);

            Assert.AreEqual(1, compare1);
            Assert.AreEqual(1, compare2);

            Assert.AreEqual(-1, compare3);
            Assert.AreEqual(-1, compare4);
        }

        /*
        u: \
            \
             \
              \
               \
                \
                 \
        v:    /   \
             /     \
                    \
        */
        [TestMethod]
        public void ComparerTest2()
        {
            var u = new Segment(1, 10, 0, 0, 10);
            var v = new Segment(2, 1, 1, 3, 3);

            var compare1 = this.Compare(u, v, 1);
            var compare2 = this.Compare(u, v, 3);

            var compare3 = this.Compare(v, u, 1);
            var compare4 = this.Compare(v, u, 3);

            Assert.AreEqual(-1, compare1);
            Assert.AreEqual(-1, compare2);

            Assert.AreEqual(1, compare3);
            Assert.AreEqual(1, compare4);
        }

        /*
        u:      -------
        v:   --------------
        */
        [TestMethod]
        public void ComparerTest3()
        {
            var u = new Segment(1, 3, 0, 5, 0);
            var v = new Segment(2, 1, -1, 6, -1);

            var compare1 = this.Compare(u, v, 3);
            var compare2 = this.Compare(u, v, 5);

            var compare3 = this.Compare(v, u, 3);
            var compare4 = this.Compare(v, u, 5);

            Assert.AreEqual(-1, compare1);
            Assert.AreEqual(-1, compare2);

            Assert.AreEqual(1, compare3);
            Assert.AreEqual(1, compare4);
        }

        /*
        u:  --------------
        v:    ----------
        */
        [TestMethod]
        public void ComparerTest4()
        {
            var u = new Segment(1, 0, 0, 10, 0);
            var v = new Segment(2, 1, -1, 6, -1);

            var compare1 = this.Compare(u, v, 1);
            var compare2 = this.Compare(u, v, 3);

            var compare3 = this.Compare(v, u, 1);
            var compare4 = this.Compare(v, u, 3);

            Assert.AreEqual(-1, compare1);
            Assert.AreEqual(-1, compare2);

            Assert.AreEqual(1, compare3);
            Assert.AreEqual(1, compare4);
        }

        /*
        u: \
            \
             \
              \      /
               \    /
                \  /
                  /
                 /
                /
        v:     /
              /
        */
        [TestMethod]
        public void ComparerTest5()
        {
            var u = new Segment(1, 0, 10, 10, 0);
            var v = new Segment(2, 6, -5, 16, 5);

            var compare1 = this.Compare(u, v, 6);
            var compare2 = this.Compare(u, v, 10);

            var compare3 = this.Compare(v, u, 6);
            var compare4 = this.Compare(v, u, 10);

            Assert.AreEqual(-1, compare1);
            Assert.AreEqual(-1, compare2);

            Assert.AreEqual(1, compare3);
            Assert.AreEqual(1, compare4);
        }

        /*
        u:          /
        v:  \      /
             \    /
              \
               \
                \
                 \
                  \
                   \
                    \
        */
        [TestMethod]
        public void ComparerTest6()
        {
            var u = new Segment(2, 6, 5, 16, 15);
            var v = new Segment(1, 0, 10, 10, 0);

            var compare1 = this.Compare(u, v, 6);
            var compare2 = this.Compare(u, v, 10);

            var compare3 = this.Compare(v, u, 6);
            var compare4 = this.Compare(v, u, 10);

            Assert.AreEqual(-1, compare1);
            Assert.AreEqual(-1, compare2);

            Assert.AreEqual(1, compare3);
            Assert.AreEqual(1, compare4);
        }

        private int Compare(Segment u, Segment v, long time)
        {
            var result = new SegmentTimeComparer(time).Compare(u, v);
            return result;
        }
    }
}
