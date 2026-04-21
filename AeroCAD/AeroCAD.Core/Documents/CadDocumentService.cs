using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Rendering;

namespace Primusz.AeroCAD.Core.Documents
{
    public class CadDocumentService : ICadDocumentService
    {
        private readonly List<Layer> layers = new List<Layer>();
        private readonly Dictionary<Guid, Layer> entityOwners = new Dictionary<Guid, Layer>();
        private readonly IEntityRenderService renderService;

        public CadDocumentService(IEntityRenderService renderService)
        {
            this.renderService = renderService;
        }

        public IReadOnlyList<Layer> Layers => layers.AsReadOnly();

        public IEnumerable<Entity> Entities => layers.SelectMany(layer => layer.Entities);

        public event EventHandler<LayerChangedEventArgs> LayerAdded;

        public event EventHandler<LayerChangedEventArgs> LayerRemoved;

        public event EventHandler<EntityChangedEventArgs> EntityAdded;

        public event EventHandler<EntityChangedEventArgs> EntityRemoved;

        public Layer CreateLayer(string name, Color color)
        {
            var layer = new Layer
            {
                LayerName = name,
                Color = color,
                RenderService = renderService
            };

            AddLayer(layer);
            return layer;
        }

        public void AddLayer(Layer layer)
        {
            if (layer == null)
                throw new ArgumentNullException(nameof(layer));

            if (layers.Any(existing => existing.Id == layer.Id))
                return;

            layer.RenderService = renderService;
            layers.Add(layer);
            LayerAdded?.Invoke(this, new LayerChangedEventArgs(layer));
        }

        public bool RemoveLayer(Guid layerId)
        {
            var layer = GetLayer(layerId);
            if (layer == null)
                return false;

            foreach (var entity in layer.Entities.ToList())
                RemoveEntityInternal(entity, true);

            layers.Remove(layer);
            LayerRemoved?.Invoke(this, new LayerChangedEventArgs(layer));
            return true;
        }

        public Layer GetLayer(Guid layerId)
        {
            return layers.FirstOrDefault(layer => layer.Id == layerId);
        }

        public Layer GetLayerForEntity(Entity entity)
        {
            if (entity == null)
                return null;

            Layer layer;
            return entityOwners.TryGetValue(entity.Id, out layer) ? layer : null;
        }

        public void AddEntity(Guid layerId, Entity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var layer = GetLayer(layerId);
            if (layer == null)
                throw new InvalidOperationException("Target layer was not found in the document.");

            Layer currentOwner;
            if (entityOwners.TryGetValue(entity.Id, out currentOwner))
            {
                if (currentOwner.Id == layerId)
                    return;

                if (!currentOwner.IsEditable)
                    throw new InvalidOperationException($"The source layer '{currentOwner.LayerName}' is locked or hidden.");
            }

            if (!layer.IsEditable)
                throw new InvalidOperationException($"The target layer '{layer.LayerName}' is locked or hidden.");

            if (entityOwners.TryGetValue(entity.Id, out currentOwner))
                currentOwner.Remove(entity);

            layer.Add(entity);
            entityOwners[entity.Id] = layer;
            EntityAdded?.Invoke(this, new EntityChangedEventArgs(entity, layer));
        }

        public void RemoveEntity(Entity entity)
        {
            RemoveEntityInternal(entity, false);
        }

        private void RemoveEntityInternal(Entity entity, bool bypassLayerPolicy)
        {
            if (entity == null)
                return;

            Layer owner;
            if (!entityOwners.TryGetValue(entity.Id, out owner))
                return;

            if (!bypassLayerPolicy && !owner.IsEditable)
                throw new InvalidOperationException($"The layer '{owner.LayerName}' is locked or hidden.");

            owner.Remove(entity);
            entityOwners.Remove(entity.Id);
            EntityRemoved?.Invoke(this, new EntityChangedEventArgs(entity, owner));
        }
    }
}

