namespace SampleRack.SharedKernel.Exceptions;

/// <summary>
/// Thrown when a requested entity cannot be found.
/// </summary>
public sealed class NotFoundException : DomainException
{
    public string EntityName { get; }
    public object? EntityId { get; }

    public NotFoundException(string entityName, object? entityId)
        : base($"Entity '{entityName}' with identifier '{entityId}' was not found.")
    {
        EntityName = entityName;
        EntityId = entityId;
    }

    public NotFoundException(string message)
        : base(message)
    {
        EntityName = string.Empty;
    }

    public NotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
        EntityName = string.Empty;
    }

    /// <summary>
    /// Creates a <see cref="NotFoundException"/> using the entity type's name and the given identifier.
    /// </summary>
    public static NotFoundException For<T>(object? id) => new(typeof(T).Name, id);
}
