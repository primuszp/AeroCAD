using System;
using System.Collections.Generic;
using System.Linq;
using Primusz.AeroCAD.Core.GeometryMath;

namespace Primusz.AeroCAD.Core.Editing.TrimExtend
{
    internal static class TrimExtendIntersectionDeduper
    {
        public static IReadOnlyList<T> ByParameter<T>(IEnumerable<T> items, Func<T, double> parameterSelector, double tolerance = 1e-6)
        {
            return (items ?? Enumerable.Empty<T>())
                .GroupBy(item => Math.Round(parameterSelector(item) / tolerance))
                .Select(group => group.First())
                .OrderBy(parameterSelector)
                .ToList();
        }

        public static IReadOnlyList<T> ByAngle<T>(IEnumerable<T> items, Func<T, double> angleSelector, double tolerance = 1e-6)
        {
            return (items ?? Enumerable.Empty<T>())
                .GroupBy(item => Math.Round(angleSelector(item) / tolerance))
                .Select(group => group.First())
                .OrderBy(angleSelector)
                .ToList();
        }
    }
}
