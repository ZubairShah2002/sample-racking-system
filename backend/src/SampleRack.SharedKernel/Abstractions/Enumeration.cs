using System.Reflection;

namespace SampleRack.SharedKernel.Abstractions;

/// <summary>
/// Smart enum base class. Provides a type-safe enumeration pattern
/// with display name support and lookup methods.
/// </summary>
public abstract class Enumeration : IComparable<Enumeration>, IEquatable<Enumeration>
{
    public int Id { get; }
    public string Name { get; }

    protected Enumeration(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public override string ToString() => Name;

    /// <summary>
    /// Returns all defined members of the specified enumeration type.
    /// </summary>
    public static IEnumerable<T> GetAll<T>() where T : Enumeration
    {
        return typeof(T)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(f => f.FieldType == typeof(T))
            .Select(f => (T)f.GetValue(null)!)
            .OrderBy(e => e.Id);
    }

    /// <summary>
    /// Looks up an enumeration member by its integer id.
    /// </summary>
    public static T FromId<T>(int id) where T : Enumeration
    {
        return GetAll<T>().FirstOrDefault(e => e.Id == id)
            ?? throw new InvalidOperationException(
                $"'{id}' is not a valid id in {typeof(T).Name}.");
    }

    /// <summary>
    /// Looks up an enumeration member by its name (case-insensitive).
    /// </summary>
    public static T FromName<T>(string name) where T : Enumeration
    {
        return GetAll<T>().FirstOrDefault(e =>
                string.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException(
                $"'{name}' is not a valid name in {typeof(T).Name}.");
    }

    /// <summary>
    /// Attempts to look up an enumeration member by id.
    /// </summary>
    public static bool TryFromId<T>(int id, out T? result) where T : Enumeration
    {
        result = GetAll<T>().FirstOrDefault(e => e.Id == id);
        return result is not null;
    }

    /// <summary>
    /// Attempts to look up an enumeration member by name (case-insensitive).
    /// </summary>
    public static bool TryFromName<T>(string name, out T? result) where T : Enumeration
    {
        result = GetAll<T>().FirstOrDefault(e =>
            string.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase));
        return result is not null;
    }

    public int CompareTo(Enumeration? other) => other is null ? 1 : Id.CompareTo(other.Id);

    public override bool Equals(object? obj)
    {
        if (obj is not Enumeration other)
            return false;

        return Equals(other);
    }

    public bool Equals(Enumeration? other)
    {
        if (other is null)
            return false;

        return GetType() == other.GetType() && Id == other.Id;
    }

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public static bool operator ==(Enumeration? left, Enumeration? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(Enumeration? left, Enumeration? right)
    {
        return !(left == right);
    }
}
