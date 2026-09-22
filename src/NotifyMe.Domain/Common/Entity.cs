namespace NotifyMe.Domain.Common;

/// <summary>
/// Base type for domain entities: identity-based equality keyed on <see cref="Id"/> plus the
/// runtime type, so entities of different kinds never compare equal even if a Guid collided.
/// </summary>
public abstract class Entity
{
    public Guid Id { get; protected set; }

    protected Entity(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Entity id cannot be empty.", nameof(id));
        }

        Id = id;
    }

    /// <summary>
    /// Reserved for ORM materialization (Phase 05); not for direct use in application code.
    /// </summary>
    protected Entity()
    {
    }

    public override bool Equals(object? obj)
    {
        return obj is Entity other && other.GetType() == GetType() && other.Id == Id;
    }

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public static bool operator ==(Entity? left, Entity? right) => Equals(left, right);

    public static bool operator !=(Entity? left, Entity? right) => !Equals(left, right);
}
