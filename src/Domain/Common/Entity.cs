namespace Domain.Common;

public abstract class Entity<TId> : IEntity<TId>
{
    public TId Id { get; set; }

    protected Entity() { }

    protected Entity(TId id)
    {
        Id = id;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> other)
        {
            return false;
        }

        if (!ReferenceEquals(this, other))
        {
            return false;
        }

        if (Id!.Equals(default) || other.Id!.Equals(default))
        {
            return false;
        }

        return Id.Equals(other.Id);
    }

    //public static bool operator ==(Entity<TId> a, Entity<TId> b) => a.Equals(b);

    //public static bool operator !=(Entity<TId> a, Entity<TId> b) => !(a == b);

    public override int GetHashCode() => throw new NotImplementedException();
}
