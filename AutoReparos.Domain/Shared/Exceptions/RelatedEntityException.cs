namespace AutoReparos.Domain.Shared.Exceptions
{
    public sealed class RelatedEntityException(string entidadePrincipal, string entidadeRelacionada)
        : DomainException($"Não é possível excluir '{entidadePrincipal}' pois existem '{entidadeRelacionada}' vinculados.");
}
