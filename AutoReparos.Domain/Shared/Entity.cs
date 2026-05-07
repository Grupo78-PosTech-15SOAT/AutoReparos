namespace AutoReparos.Domain.Shared
{
    public abstract class Entity(Guid? id = null)
    {
        public Guid Id { get; } = id ?? Guid.NewGuid();
    }
}
