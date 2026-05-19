using System.Collections;
using System.Collections.Generic;

namespace FlatRedBall2;

/// <summary>
/// Generic ordered registry for engine-managed lists within a Screen.
/// Provides consistent Register/Unregister/Clear semantics with read-only external access.
/// </summary>
internal sealed class ScreenRegistry<T> : IEnumerable<T>
{
    private readonly List<T> _items = new();

    internal IReadOnlyList<T> Items => _items;

    internal void Register(T item) => _items.Add(item);
    internal void Unregister(T item) => _items.Remove(item);
    internal void Clear() => _items.Clear();
    internal bool Contains(T item) => _items.Contains(item);
    internal int Count => _items.Count;
    internal T this[int index] => _items[index];

    public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
}
