using System;
using System.Collections.Generic;
using System.Linq;

namespace ConvexPolygon
{
    class Program
    {
        static void Main(string[] args)
        {
            var polygon = ReadPolygon();
            var convexityResult = polygon.TestConvexity();
            Console.WriteLine(convexityResult);
            Console.ReadLine();
        }

        private static Polygon ReadPolygon()
        {
            var expectedPointsCount = Convert.ToInt32(Console.ReadLine());

            var pointsLine = Console
                .ReadLine()
                .Split(' ');

            var points = new List<Point>();
            for(var i = 0; i < pointsLine.Length; i = i + 2)
            {
                points.Add(new Point(Convert.ToInt32(pointsLine[i]), Convert.ToInt32(pointsLine[i + 1])));
            }

            if(expectedPointsCount != points.Count)
            {
                throw new Exception("incorrect points count");
            }

            return new Polygon(points);
        }
    }

    struct Point
    {
        public Point(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public int X { get; }
        public int Y { get; }
    }

    struct Polygon
    {
        public Polygon(IEnumerable<Point> points)
        {
            this.Points = points.ToArray();
        }

        public IReadOnlyCollection<Point> Points { get; }
    }

    static class Tools
    {
        public static string TestConvexity(this Polygon polygon)
        {
            if(polygon.Points.Count <= 3)
            {
                return "CONVEX";
            }

            var points = polygon.Points.Concat(new[] { polygon.Points.ElementAt(0), polygon.Points.ElementAt(1) }).ToArray();

            var signedArea = SignedArea(points[0], points[1], points[2]);
            for (var i = 1; i < points.Length - 2; i++)
            {
                var nextSignedArea = SignedArea(points[i], points[i + 1], points[i + 2]);
                var signChanged = signedArea * nextSignedArea < 0;
                if (signChanged)
                {
                    return "NOT_CONVEX";
                }
            }

            return "CONVEX";
        }

        public static decimal SignedArea(Point a, Point b, Point c)
        {
            var areaDoubled = (b.X - a.X) * (c.Y - a.Y) - (c.X - a.X) * (b.Y - a.Y);
            var area = areaDoubled / 2m;
            return area;
        }
    }
}
