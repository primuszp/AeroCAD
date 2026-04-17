using System;
using System.Collections.Generic;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Layers;

namespace Primusz.AeroCAD.Core.Documents
{
    public interface ICadDocumentService
    {
        IReadOnlyList<Layer> Layers { get; }

        IEnumerable<Entity> Entities { get; }

        event EventHandler<LayerChangedEventArgs> LayerAdded;

        event EventHandler<LayerChangedEventArgs> LayerRemoved;

        event EventHandler<EntityChangedEventArgs> EntityAdded;

        event EventHandler<EntityChangedEventArgs> EntityRemoved;

        Layer CreateLayer(string name, Color color);

        void AddLayer(Layer layer);

        bool RemoveLayer(Guid layerId);

        Layer GetLayer(Guid layerId);

        Layer GetLayerForEntity(Entity entity);

        void AddEntity(Guid layerId, Entity entity);

        void RemoveEntity(Entity entity);
    }
}

