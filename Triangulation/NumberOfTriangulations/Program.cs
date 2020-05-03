using System;

namespace NumberOfTriangulations
{
    class Program
    {
        static void Main(string[] args)
        {
            var n = Convert.ToUInt32(Console.ReadLine().Trim());
            var result = Tools.TotalNumberOfTriangulations(n);
            Console.WriteLine(result);
            Console.ReadLine();
        }
    }

    public static class Tools
    {
        public static long TotalNumberOfTriangulations(uint vertexCount)
        {
            var result = CatalanNumber(vertexCount - 2);
            return result;
        }

        private static long CatalanNumber(uint n)
        {
            long nominator = 1;
            long denominator = 1;

            for(uint k = 2; k <= n; k++)
            {
                nominator *= (n + k);
                denominator *= k;
                var gcd = GreatesCommonDivisor(nominator, denominator);
                nominator /= gcd;
                denominator /= gcd;
            }

            var result = nominator / denominator;
            return result;
        }

        private static long GreatesCommonDivisor(long a, long b)
        {
            var gcd = a > b
                ? Gcd(a - b, b)
                : Gcd(a, b - a);

            return gcd;
        }

        private static long Gcd(long a, long b)
        {
            if(b == 0)
            {
                return a;
            }

            return Gcd(b, a % b);
        }
    }

}
