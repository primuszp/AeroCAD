using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Primusz.AeroCAD.Core.Documents;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Spatial
{
    public class SpatialQueryService : ISpatialQueryService
    {
        private readonly IEntityBoundsService boundsService;
        private readonly Dictionary<Guid, Entity> entitiesById = new Dictionary<Guid, Entity>();
        private readonly Dictionary<Guid, Rect> boundsByEntityId = new Dictionary<Guid, Rect>();
        private readonly Dictionary<Guid, HashSet<long>> cellsByEntityId = new Dictionary<Guid, HashSet<long>>();
        private readonly Dictionary<long, HashSet<Guid>> entitiesByCell = new Dictionary<long, HashSet<Guid>>();
        private readonly double cellSize;

        public SpatialQueryService(ICadDocumentService document, IEntityBoundsService boundsService, double cellSize = 100.0d)
        {
            this.boundsService = boundsService ?? throw new ArgumentNullException(nameof(boundsService));
            this.cellSize = cellSize > 0 ? cellSize : throw new ArgumentOutOfRangeException(nameof(cellSize));

            if (document == null)
                throw new ArgumentNullException(nameof(document));

            document.EntityAdded += OnEntityAdded;
            document.EntityRemoved += OnEntityRemoved;

            foreach (var entity in document.Entities)
                Register(entity);
        }

        public IReadOnlyCollection<Entity> QueryNearby(Point point, double radius)
        {
            var rect = new Rect(point.X - radius, point.Y - radius, radius * 2.0d, radius * 2.0d);
            return QueryIntersecting(rect);
        }

        public IReadOnlyCollection<Entity> QueryIntersecting(Rect rect)
        {
            if (rect.IsEmpty)
                return Array.Empty<Entity>();

            rect = NormalizeRect(rect);
            var ids = new HashSet<Guid>();

            foreach (var key in GetCellKeys(rect))
            {
                HashSet<Guid> cellEntities;
                if (!entitiesByCell.TryGetValue(key, out cellEntities))
                    continue;

                foreach (var entityId in cellEntities)
                    ids.Add(entityId);
            }

            var result = new List<Entity>();
            foreach (var entityId in ids)
            {
                Rect bounds;
                Entity entity;
                if (!boundsByEntityId.TryGetValue(entityId, out bounds) || !entitiesById.TryGetValue(entityId, out entity))
                    continue;

                if (bounds.IntersectsWith(rect) || rect.Contains(bounds))
                    result.Add(entity);
            }

            return result;
        }

        private void OnEntityAdded(object sender, EntityChangedEventArgs e)
        {
            Register(e.Entity);
        }

        private void OnEntityRemoved(object sender, EntityChangedEventArgs e)
        {
            Unregister(e.Entity);
        }

        private void Register(Entity entity)
        {
            if (entity == null || entitiesById.ContainsKey(entity.Id))
                return;

            entitiesById[entity.Id] = entity;
            entity.GeometryChanged += OnEntityGeometryChanged;
            Reindex(entity);
        }

        private void Unregister(Entity entity)
        {
            if (entity == null)
                return;

            entity.GeometryChanged -= OnEntityGeometryChanged;
            RemoveFromCells(entity.Id);
            boundsByEntityId.Remove(entity.Id);
            entitiesById.Remove(entity.Id);
        }

        private void OnEntityGeometryChanged(object sender, EventArgs e)
        {
            var entity = sender as Entity;
            if (entity != null)
                Reindex(entity);
        }

        private void Reindex(Entity entity)
        {
            RemoveFromCells(entity.Id);

            Rect bounds;
            if (!boundsService.TryGetBounds(entity, out bounds))
            {
                boundsByEntityId.Remove(entity.Id);
                return;
            }

            bounds = NormalizeRect(bounds);
            boundsByEntityId[entity.Id] = bounds;

            var keys = new HashSet<long>(GetCellKeys(bounds));
            cellsByEntityId[entity.Id] = keys;

            foreach (var key in keys)
            {
                HashSet<Guid> entities;
                if (!entitiesByCell.TryGetValue(key, out entities))
                {
                    entities = new HashSet<Guid>();
                    entitiesByCell[key] = entities;
                }

                entities.Add(entity.Id);
            }
        }

        private void RemoveFromCells(Guid entityId)
        {
            HashSet<long> keys;
            if (!cellsByEntityId.TryGetValue(entityId, out keys))
                return;

            foreach (var key in keys)
            {
                HashSet<Guid> entities;
                if (!entitiesByCell.TryGetValue(key, out entities))
                    continue;

                entities.Remove(entityId);
                if (entities.Count == 0)
                    entitiesByCell.Remove(key);
            }

            cellsByEntityId.Remove(entityId);
        }

        private IEnumerable<long> GetCellKeys(Rect rect)
        {
            int minX = (int)Math.Floor(rect.Left / cellSize);
            int maxX = (int)Math.Floor(rect.Right / cellSize);
            int minY = (int)Math.Floor(rect.Top / cellSize);
            int maxY = (int)Math.Floor(rect.Bottom / cellSize);

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                    yield return ComposeCellKey(x, y);
            }
        }

        private static long ComposeCellKey(int x, int y)
        {
            return ((long)x << 32) ^ (uint)y;
        }

        private static Rect NormalizeRect(Rect rect)
        {
            double left = Math.Min(rect.Left, rect.Right);
            double top = Math.Min(rect.Top, rect.Bottom);
            double right = Math.Max(rect.Left, rect.Right);
            double bottom = Math.Max(rect.Top, rect.Bottom);
            return new Rect(new Point(left, top), new Point(right, bottom));
        }
    }
}

