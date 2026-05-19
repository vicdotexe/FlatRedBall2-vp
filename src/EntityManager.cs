using System.Collections.Generic;

namespace FlatRedBall2;

/// <summary>
/// Owns the list of active entities for a Screen. Screen iterates this list each frame
/// for physics, activity, and cleanup.
/// </summary>
internal sealed class EntityManager
{
    private readonly List<Entity> _entities = new();

    /// <summary>All entities currently registered with this screen.</summary>
    internal IReadOnlyList<Entity> Entities => _entities;

    internal void Register(Entity entity) => _entities.Add(entity);

    internal void Unregister(Entity entity) => _entities.Remove(entity);

    internal void Clear() => _entities.Clear();
}
