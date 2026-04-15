namespace AutoReparos.Domain.Shared
{
    public abstract class Entity(Guid? id = null)
    {
        public Guid Id { get; } = id ?? Guid.NewGuid();

        public bool Equals(Guid other) => Id.Equals(other);
        public override int GetHashCode() => Id.GetHashCode();
    }
}
