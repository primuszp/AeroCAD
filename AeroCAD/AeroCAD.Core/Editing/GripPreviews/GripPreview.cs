using System.Collections.Generic;
using System.Linq;

namespace Primusz.AeroCAD.Core.Editing.GripPreviews
{
    public sealed class GripPreview
    {
        private static readonly GripPreview empty = new GripPreview(Enumerable.Empty<GripPreviewStroke>());

        public IReadOnlyList<GripPreviewStroke> Strokes { get; }

        public bool HasContent => Strokes.Count > 0;

        public static GripPreview Empty => empty;

        public GripPreview(IEnumerable<GripPreviewStroke> strokes)
        {
            Strokes = (strokes ?? Enumerable.Empty<GripPreviewStroke>())
                .Where(stroke => stroke != null)
                .ToList()
                .AsReadOnly();
        }
    }
}

