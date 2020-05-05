using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    internal static class SvgExport
    {
        public static string ToSvg(this IReadOnlyCollection<EarCuttingTriangulation.Point> polygon)
        {
            var segments = new List<S>();
            for(var i = 0; i < polygon.Count; i++)
            {
                segments.Add(new S
                {
                    A = new P
                    {
                        X = polygon.ElementAt(i).X,
                        Y = polygon.ElementAt(i).Y
                    },
                    B = new P
                    {
                        X = polygon.ElementAt((i + 1) % polygon.Count).X,
                        Y = polygon.ElementAt((i + 1) % polygon.Count).Y
                    }
                });
            }

            var points = polygon
                .Select(p => new P
                {
                    X = p.X,
                    Y = p.Y
                })
                .ToArray();

            var output = segments.ToSvg(points);
            return output;
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
