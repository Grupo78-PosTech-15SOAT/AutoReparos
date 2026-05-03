namespace AutoReparos.Domain.Shared
{
    public abstract class Entity(Guid? id = null)
    {
        public Guid Id { get; } = id ?? Guid.NewGuid();

        public override bool Equals(object? obj)
        {
            if (obj is not Entity other)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return Id.Equals(other.Id);
        }

        public bool Equals(Guid other) => Id.Equals(other);
        public override int GetHashCode() => Id.GetHashCode();
    }
}
