using System.Windows.Media;

namespace Primusz.AeroCAD.Core.Drawing.Entities
{
    public enum EntityColorKind
    {
        ByLayer,
        ByBlock,
        Indexed,
        TrueColor
    }

    /// <summary>
    /// Represents an entity color following AutoCAD's color model:
    /// ByLayer (inherits from layer), ByBlock, ACI index (1–255), or explicit RGB.
    /// </summary>
    public readonly struct EntityColor
    {
        public static readonly EntityColor ByLayer = new EntityColor(EntityColorKind.ByLayer, 0, default);
        public static readonly EntityColor ByBlock = new EntityColor(EntityColorKind.ByBlock, 0, default);

        private readonly EntityColorKind kind;
        private readonly byte aciIndex;
        private readonly Color trueColor;

        private EntityColor(EntityColorKind kind, byte aciIndex, Color trueColor)
        {
            this.kind = kind;
            this.aciIndex = aciIndex;
            this.trueColor = trueColor;
        }

        public EntityColorKind Kind => kind;
        public bool IsByLayer => kind == EntityColorKind.ByLayer;
        public bool IsByBlock => kind == EntityColorKind.ByBlock;
        public byte AciIndex => aciIndex;

        public static EntityColor FromAci(byte index) =>
            new EntityColor(EntityColorKind.Indexed, index, default);

        public static EntityColor FromRgb(Color color) =>
            new EntityColor(EntityColorKind.TrueColor, 0, color);

        /// <summary>
        /// Resolves to the actual render color, using <paramref name="layerColor"/> for ByLayer/ByBlock.
        /// </summary>
        public Color Resolve(Color layerColor)
        {
            switch (kind)
            {
                case EntityColorKind.Indexed:  return AciPalette.GetColor(aciIndex);
                case EntityColorKind.TrueColor: return trueColor;
                default: return layerColor; // ByLayer and ByBlock both inherit from layer
            }
        }
    }
}
