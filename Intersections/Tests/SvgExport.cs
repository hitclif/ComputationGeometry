﻿using System;
using System.Collections.Generic;
using System.Linq;
using SetOfSegments;

namespace Tests
{
    internal static class SvgExport
    {
        public static string ToSvg(this IEnumerable<Segment> segments)
        {
            var allPoints = segments
                .SelectMany(s => new[] { s.A, s.B })
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
                    + $"<title>{a} | {b}</title>"
                    + Environment.NewLine
                    + "</line>"
                    + Environment.NewLine
                    ;

            }

            output += "</svg>";

            return output;
        }
    }
}