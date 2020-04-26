using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SetOfSegments;

namespace Tests
{
    [TestClass]
    public class AvlTreeTests
    {
        private const int PerformanceValuesCount = 30000;

        private AvlTree<int> _testee = new AvlTree<int>();

        [TestMethod]
        public void Insert_1_Test()
        {
            _testee.Add(1);

            Assert.AreEqual(1, _testee.Count);
            Assert.AreEqual(0, _testee.IndexOf(1));
            Assert.AreEqual(0, _testee.BalanceFactor);
        }

        [TestMethod]
        public void Insert_213_Test()
        {
            _testee.Add(2);
            Assert.AreEqual(0, _testee.BalanceFactor);
            _testee.Add(1);
            Assert.AreEqual(-1, _testee.BalanceFactor);
            _testee.Add(3);
            Assert.AreEqual(0, _testee.BalanceFactor);

            Assert.AreEqual(3, _testee.Count);
            Assert.AreEqual(0, _testee.IndexOf(1));
            Assert.AreEqual(1, _testee.IndexOf(2));
            Assert.AreEqual(2, _testee.IndexOf(3));
        }

        [TestMethod]
        public void Insert_123_Test()
        {
            _testee.Add(1);
            Assert.AreEqual(0, _testee.BalanceFactor);
            _testee.Add(2);
            Assert.AreEqual(1, _testee.BalanceFactor);
            _testee.Add(3);
            Assert.AreEqual(0, _testee.BalanceFactor);

            Assert.AreEqual(3, _testee.Count);
            Assert.AreEqual(0, _testee.IndexOf(1));
            Assert.AreEqual(1, _testee.IndexOf(2));
            Assert.AreEqual(2, _testee.IndexOf(3));
        }

        [TestMethod]
        public void Insert_01234567891012345_Test()
        {
            this.InsertAndAssert(0);
            this.InsertAndAssert(1);
            this.InsertAndAssert(2);
            this.InsertAndAssert(3);
            this.InsertAndAssert(4);
            this.InsertAndAssert(5);
            this.InsertAndAssert(6);
            this.InsertAndAssert(7);
            this.InsertAndAssert(8);
            this.InsertAndAssert(9);
            this.InsertAndAssert(10);
            this.InsertAndAssert(11);
            this.InsertAndAssert(12);
            this.InsertAndAssert(13);
            this.InsertAndAssert(14);
            this.InsertAndAssert(15);

            Assert.AreEqual(16, _testee.Count);
            Assert.AreEqual(0, _testee.IndexOf(0));
            Assert.AreEqual(1, _testee.IndexOf(1));
            Assert.AreEqual(2, _testee.IndexOf(2));
            Assert.AreEqual(3, _testee.IndexOf(3));
            Assert.AreEqual(4, _testee.IndexOf(4));
            Assert.AreEqual(5, _testee.IndexOf(5));
            Assert.AreEqual(6, _testee.IndexOf(6));
            Assert.AreEqual(7, _testee.IndexOf(7));
            Assert.AreEqual(8, _testee.IndexOf(8));
            Assert.AreEqual(9, _testee.IndexOf(9));
        }

        [TestMethod]
        public void Insert_543210987654321_Test()
        {
            this.InsertAndAssert(15);
            this.InsertAndAssert(14);
            this.InsertAndAssert(13);
            this.InsertAndAssert(12);
            this.InsertAndAssert(11);
            this.InsertAndAssert(10);
            this.InsertAndAssert(9);
            this.InsertAndAssert(8);
            this.InsertAndAssert(7);
            this.InsertAndAssert(6);
            this.InsertAndAssert(5);
            this.InsertAndAssert(4);
            this.InsertAndAssert(3);
            this.InsertAndAssert(2);
            this.InsertAndAssert(1);
            this.InsertAndAssert(0);

            Assert.AreEqual(16, _testee.Count);
            Assert.AreEqual(0, _testee.IndexOf(0));
            Assert.AreEqual(1, _testee.IndexOf(1));
            Assert.AreEqual(2, _testee.IndexOf(2));
            Assert.AreEqual(3, _testee.IndexOf(3));
            Assert.AreEqual(4, _testee.IndexOf(4));
            Assert.AreEqual(5, _testee.IndexOf(5));
            Assert.AreEqual(6, _testee.IndexOf(6));
            Assert.AreEqual(7, _testee.IndexOf(7));
            Assert.AreEqual(8, _testee.IndexOf(8));
            Assert.AreEqual(9, _testee.IndexOf(9));
        }

        [TestMethod]
        public void T()
        {
            var values = new[] { 72, 58, 14, 32, 86, 49, 21, 41, 10, 69, 44 };
            foreach (var v in values)
            {
                InsertAndAssert(v);
            }
        }

        [TestMethod]
        public void Performance_List()
        {
            var values = this.GenerateRandomValues(PerformanceValuesCount, PerformanceValuesCount * 10);

            var list = new List<int>();
            var sw = new Stopwatch();
            sw.Start();
            foreach (var v in values)
            {
                var i = list.BinarySearch(v);
                if(i < 0)
                {
                    i = ~i;
                }

                list.Insert(i, v);
            }
            sw.Stop();

            Console.WriteLine("Insert values count: {0}", values.Length);
            Console.WriteLine("Total time: {0} s", sw.ElapsedMilliseconds / 1000d);
        }

        [TestMethod]
        public void Performance_List_ReversedInsert()
        {
            var values = Enumerable.Range(0, PerformanceValuesCount)
                .OrderByDescending(v => v)
                .ToArray();

            var list = new List<int>();
            var sw = new Stopwatch();
            sw.Start();
            foreach (var v in values)
            {
                var i = list.BinarySearch(v);
                if (i < 0)
                {
                    i = ~i;
                }

                list.Insert(i, v);
            }
            sw.Stop();

            Console.WriteLine("Insert values count: {0}", values.Length);
            Console.WriteLine("Total time: {0} s", sw.ElapsedMilliseconds / 1000d);
        }

        [TestMethod]
        public void Performance_Avl()
        {
            var values = this.GenerateRandomValues(PerformanceValuesCount, PerformanceValuesCount * 10);

            //Console.WriteLine("Test values:");
            //Console.WriteLine(string.Join(", ", values));

            var sw = new Stopwatch();
            sw.Start();
            foreach(var v in values)
            {
                // InsertAndAssert(v);
                _testee.Add(v);
            }
            sw.Stop();

            Console.WriteLine("Insert values count: {0}", values.Length);
            Console.WriteLine("Total time: {0} s", sw.ElapsedMilliseconds / 1000d);

            Assert.AreEqual(values.Length, _testee.Count);
            Assert.IsTrue(_testee.BalanceFactor >= -1 && _testee.BalanceFactor <= 1);
            //Console.WriteLine("Sorted values:");
            //Console.WriteLine(string.Join(", ", _testee.ToArray()));
        }

        [TestMethod]
        public void Performance_Avl_ReversedInsert()
        {
            var values = Enumerable.Range(0, PerformanceValuesCount)
                .OrderByDescending(v => v)
                .ToArray();

            //Console.WriteLine("Test values:");
            //Console.WriteLine(string.Join(", ", values));

            var sw = new Stopwatch();
            sw.Start();
            foreach (var v in values)
            {
                // InsertAndAssert(v);
                _testee.Add(v);
            }
            sw.Stop();

            Console.WriteLine("Insert values count: {0}", values.Length);
            Console.WriteLine("Total time: {0} s", sw.ElapsedMilliseconds / 1000d);

            Assert.AreEqual(values.Length, _testee.Count);
            Assert.IsTrue(_testee.BalanceFactor >= -1 && _testee.BalanceFactor <= 1);
        }

        [TestMethod]
        public void RemoveRandom()
        {
            const int count = 100;
            var values = this.GenerateRandomValues(count, 10000);

            Console.WriteLine("Insert values count: {0}", string.Join(", ", values));
            _testee.Add(values);

            var randomIndex = new Random().Next(count - 1);
            var toRemove = values[randomIndex];

            Console.WriteLine("Removing value: {0}", toRemove);

            var sw = new Stopwatch();
            sw.Start();
            _testee.Remove(toRemove);
            sw.Stop();

            Console.WriteLine("Removed value: {0}", toRemove);
            Console.WriteLine("Duration: {0}s", sw.ElapsedMilliseconds / 1000);

            Assert.AreEqual(-1, _testee.IndexOf(toRemove));
            Assert.IsTrue(_testee.BalanceFactor >= -1 && _testee.BalanceFactor <= 1);
            Assert.AreEqual(values.Length - 1, _testee.Count);
        }

        [TestMethod]
        public void RemoveRandomUntilEmpty()
        {
            const int count = 100;
            var values = this
                .GenerateRandomValues(count, 10000)
                .ToList();

            Console.WriteLine("Insert values count: {0}", string.Join(", ", values));

            _testee.Add(values.ToArray());

            values.Sort();

            while(values.Count > 0)
            {
                var index = new Random().Next(values.Count - 1);
                var toRemove = values[index];

                Console.WriteLine("Removing value: {0}", toRemove);

                _testee.Remove(toRemove);

                values.Remove(toRemove);

                
                Assert.AreEqual(-1, _testee.IndexOf(toRemove));
                Assert.IsTrue(_testee.BalanceFactor >= -1 && _testee.BalanceFactor <= 1);

                Assert.AreEqual(values.Count, _testee.Count);

                var remaining = _testee.ToArray();
                for (var i = 0; i < remaining.Length; i++)
                {
                    Assert.AreEqual(values[i], remaining[i], "Sorting broken");
                }
            }
        }

        [TestMethod]
        public void IndexOfRandom()
        {
            var values = this.GenerateRandomValues(100, 10000);
            var sortedValues = values.OrderBy(v => v).ToArray();

            var randomIndex = new Random().Next(99);
            var value = sortedValues[randomIndex];

            _testee.Add(values);
            var index = _testee.IndexOf(value);

            Assert.AreEqual(randomIndex, index);
        }

        [TestMethod]
        public void RemoveSmall()
        {
            var values = new[]
            {
                 5, 6, 9, 19, 7, 1, 8
            };

            Console.WriteLine("Insert values count: {0}", string.Join(", ", values));
            _testee.Add(values);
            _testee.Remove(19);

            Assert.AreEqual(values.Length - 1, _testee.Count, "Resulting count");
            Assert.IsTrue(_testee.BalanceFactor >= -1 && _testee.BalanceFactor <= 1);
        }

        [TestMethod]
        public void Remove()
        {
            var values = new[]
            {
                 2095, 5813, 3667, 1703, 3059, 4831, 7496, 5605, 4483, 1852, 9775, 5095, 7379, 5482, 9992, 5052, 9175, 7436, 8816, 6698, 2392, 7641, 6986, 3534, 5171, 6496, 8943, 9696, 6814, 1480, 3563, 8144, 9697, 6207, 571, 1714, 9019, 7265, 9013, 8516, 7710, 8809, 2425, 7954, 4517, 8371, 9574, 1512, 5338, 4437, 8042, 827, 6460, 2586, 6811, 4454, 8827, 133, 6531, 6563, 5888, 7799, 8791, 3002, 8288, 1630, 5397, 1171, 4910, 8278, 6032, 1909, 8422, 300, 8987, 3582, 5216, 9032, 9016, 6800, 6921, 7430, 4357, 2376, 3438, 2735, 1684, 7111, 9396, 6117, 2886, 8886, 733, 2450, 2627, 9911, 18, 9422, 9665
            };

            Console.WriteLine("Insert values count: {0}", string.Join(", ", values));
            _testee.Add(values);
            _testee.Remove(2095);

            var remainingValues = _testee.ToArray();
            Assert.AreEqual(values.Length - 1, _testee.Count, "Resulting count");
            Assert.IsTrue(_testee.BalanceFactor >= -1 && _testee.BalanceFactor <= 1);
        }

        [TestMethod]
        public void RemoveMultiple()
        {
            var values = new[]
            {
                 9288, 6461, 846, 8521, 4425, 8896, 3897, 3289, 6722, 398, 959, 2104, 7756, 7686, 4427, 2618, 307, 1230, 6771, 4816, 6850, 2754, 9640, 720, 5784, 8585, 2550, 3860, 5124, 4989, 6143, 3726, 7008, 3708, 862, 1579, 9040, 6034, 2009, 4568, 5287, 9194, 9010, 6869, 3664, 8668, 6800, 6122, 5804, 9378, 7668, 6917, 9450, 8855, 6028, 6533, 6820, 126, 2736, 5839, 6345, 36, 8165, 1733, 4255, 7232, 5096, 4047, 6824, 2847, 3577, 4272, 9220, 2202, 9529, 7655, 3744, 2771, 7055, 7115, 1785, 6427, 8055, 5745, 7320, 9225, 4276, 8152, 7680, 4328, 4759, 8914, 3298, 6169, 8223, 5250, 1029, 7276, 2613, 6432
            };
            
            _testee.Add(values);

            var sortedValues = values.ToList();
            sortedValues.Sort();

            RemoveFromBoth(9288, _testee, sortedValues);

            Assert.AreEqual(99, _testee.Count, "Resulting count");
            Assert.IsTrue(_testee.BalanceFactor >= -1 && _testee.BalanceFactor <= 1);
        }

        [TestMethod]
        public void InvertedComparer()
        {
            _testee = new AvlTree<int>(new InvertedComparer());

            var values = Enumerable.Range(0, PerformanceValuesCount)
                .OrderByDescending(v => v)
                .ToArray();

            foreach (var v in values)
            {
                _testee.Add(v);
            }

            var reverseSorted = values.OrderByDescending(v => v).ToArray();

            var testeeArray = _testee.ToArray();
            for(var i = 0; i < reverseSorted.Length; i++)
            {
                Assert.AreEqual(reverseSorted[i], testeeArray[i]);
            }
        }

        private static void RemoveFromBoth(int value, AvlTree<int> tree, List<int> sortedValue)
        {
            tree.Remove(value);
            sortedValue.Remove(value);
        }

        private void InsertAndAssert(int value)
        {
            _testee.Add(value);
            Assert.IsTrue(_testee.BalanceFactor >= -1 && _testee.BalanceFactor <= 1, "Insert value: " + value + " caused Balance factor is: " + _testee.BalanceFactor);
        }

        private int[] GenerateRandomValues(int approximateCount, int maxValue)
        {
            var r = new Random();
            var values = Enumerable
                .Range(0, approximateCount)
                .Select(_ => r.Next(maxValue))
                .Distinct()
                .ToArray();

            return values;
        }
    }

    internal class InvertedComparer : IComparer<int>
    {
        public int Compare(int x, int y)
        {
            return y - x;
        }
    }
}
