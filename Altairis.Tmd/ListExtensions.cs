namespace Altairis.Tmd;

/// <summary>
/// Provides extension methods for <see cref="IList{T}"/> to manipulate list items.
/// </summary>
public static class ListExtensions {

    /// <summary>
    /// Moves an item within the list from one index to another.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="list">The list to operate on.</param>
    /// <param name="oldIndex">The zero-based index of the item to move.</param>
    /// <param name="newIndex">The zero-based index to move the item to.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the list is empty, or if <paramref name="oldIndex"/> or <paramref name="newIndex"/> is out of range.
    /// </exception>
    public static void Move<T>(this IList<T> list, int oldIndex, int newIndex) {
        if (list.Count == 0) throw new ArgumentOutOfRangeException(nameof(newIndex), "List is empty");
        if (oldIndex < 0 || oldIndex >= list.Count) throw new ArgumentOutOfRangeException(nameof(oldIndex));
        if (newIndex < 0 || newIndex >= list.Count) throw new ArgumentOutOfRangeException(nameof(newIndex));
        if (oldIndex == newIndex) return; // No need to move if the index is the same

        var item = list[oldIndex];
        list.RemoveAt(oldIndex);
        if (newIndex > oldIndex) newIndex--; // The actual index could have shifted due to the removal
        list.Insert(newIndex, item);
    }

    /// <summary>
    /// Moves a specified item within the list to a new index.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="list">The list to operate on.</param>
    /// <param name="item">The item to move.</param>
    /// <param name="newIndex">The zero-based index to move the item to.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the list is empty, or if <paramref name="item"/> is not found in the list, or if <paramref name="newIndex"/> is out of range.
    /// </exception>
    public static void Move<T>(this IList<T> list, T item, int newIndex) {
        if (list.Count == 0) throw new ArgumentOutOfRangeException(nameof(newIndex), "List is empty");
        var oldIndex = list.IndexOf(item);
        if (oldIndex < 0) throw new ArgumentOutOfRangeException(nameof(item), "Item not found in list");
        if (newIndex < 0 || newIndex >= list.Count) throw new ArgumentOutOfRangeException(nameof(newIndex));
        if (oldIndex == newIndex) return; // No need to move if the index is the same

        list.RemoveAt(oldIndex);
        if (newIndex > oldIndex) newIndex--; // The actual index could have shifted due to the removal
        list.Insert(newIndex, item);
    }

}
