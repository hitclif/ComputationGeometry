using System;
using System.Collections.Generic;
using System.Linq;
using SetOfSegments;

namespace Tests
{
    internal static class SvgExport
    {
        public static string ToSvg(
            this IEnumerable<IReadOnlyCollection<PolygonIntersection.Point>> polygons,
            IEnumerable<PolygonIntersection.Point> intersections)
        {
            var allSegments = polygons
                .SelectMany(points =>
                {
                    var polygonSegments = new List<S>();
                    for (var i = 0; i < points.Count; i++)
                    {
                        var p1 = points.ElementAt(i);
                        var p2 = points.ElementAt((i + 1 ) % points.Count);

                        var s = new S 
                        {
                            Name = i.ToString(),
                            A = new P
                            {
                                X = (long)p1.X,
                                Y = (long)p1.Y
                            },
                            B = new P
                            {
                                X = (long)p2.X,
                                Y = (long)p2.Y
                            },
                        };

                        polygonSegments.Add(s);
                    }
                    return polygonSegments;
                })
                .ToArray();

            var ip = intersections
                .Select(i => new P { X = (long)i.X, Y = (long)i.Y })
                .ToArray();

            return allSegments.ToSvg(ip);
        }

        public static string ToSvg(this IEnumerable<Segment> segments)
        {
            var svg = segments
                .Select(s => new S
                {
                    Name = "S" + s.Id,
                    A = new P { X = s.A.X, Y = s.A.Y },
                    B = new P { X = s.B.X, Y = s.B.Y },
                })
                .ToSvg(new P[0]);

            return svg;
        }

        public static string ToSvg(this IEnumerable<VerticalHorizontalSegments.Segment> segments)
        {
            var svg = segments
                .Select((s, i) => new S
                {
                    Name = "S" + (i+ 1),
                    A = new P { X = s.A.X, Y = s.A.Y},
                    B = new P { X = s.B.X, Y = s.B.Y},
                })
                .ToSvg(new P[0]);

            return svg;
        }

        private static string ToSvg(this IEnumerable<S> segments, IReadOnlyCollection<P> points)
        {
            var allPoints = segments
                .SelectMany(s => new[] { s.A, s.B })
                .Union(points)
                .ToArray();

            var minX = allPoints
                .Min(s => s.X);
            var minY = allPoints
                .Min(s => s.Y);

            var maxX = allPoints
                .Max(s => s.X);
            var maxY = allPoints
                .Max(s => s.Y);

            var width = Math.Abs(maxX - minX);
            var height = Math.Abs(maxY - minY);

            var output = $"<svg viewbox='{minX - 1} {height - maxY - 1} {width + 2} {height + 2}' widht='500' height='500'>"
                + Environment.NewLine
                + $"<line x1='{minX - 1}' y1='{height - minY + 1}' x2='{minX - 1}' y2='{height - maxY + 1}' style='stroke: rgb(0, 200, 0); stroke-width:0.5;'></line>"
                + Environment.NewLine
                + $"<line x1='{minX - 1}' y1='{height - minY + 1}' x2='{width + 2}' y2='{height - minY + 1}' style='stroke: rgb(0, 200, 0); stroke-width:0.5;'></line>"
                + Environment.NewLine
                + Environment.NewLine
                ;

            foreach (var segment in segments)
            {
                var a = segment.A;
                var b = segment.B;
                output += $"<line x1='{a.X}' y1='{height - a.Y + 1}' x2='{b.X}' y2='{height + 1 - b.Y}' style='stroke: rgb(0, 0, 0); stroke-width:0.5'>"
                    + Environment.NewLine
                    + $"<title>{segment.Name}: {a.X},{a.Y} | {b.X},{b.Y}</title>"
                    + Environment.NewLine
                    + "</line>"
                    + Environment.NewLine
                    ;
            }

            var r = 0.5;
            if(points.Count > 10)
            {
                r = 2;
            }

            foreach(var point in points)
            {
                output += $"<circle cx='{point.X}' cy='{height - point.Y + 1}' r='{r}' fill='red'>"
                    + Environment.NewLine
                    + $"<title>{point.X},{point.Y}</title>"
                    + Environment.NewLine
                    + "</circle>"
                    + Environment.NewLine;
            }

            output += "</svg>";

            return "<html>"
                + Environment.NewLine
                + "<head></head>"
                + Environment.NewLine
                + "<body>"
                + Environment.NewLine
                + output
                + Environment.NewLine
                + "</body>";
        }

        private class P
        {
            public long X { get; set; }
            public long Y { get; set; }
        }

        private class S
        {
            public string Name { get; set; }
            public P A { get; set; }
            public P B { get; set; }
        }
    }
}
