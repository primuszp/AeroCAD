namespace Primusz.AeroCAD.Core.Plugins
{
    /// <summary>
    /// Bundles all strategies and runtime registrations needed to integrate a new entity type.
    /// The plugin exposes a single normalized descriptor so the host does not need to stitch
    /// together separate capability properties and factory methods.
    /// </summary>
    public interface IEntityPlugin
    {
        EntityPluginDescriptor Descriptor { get; }
    }
}
