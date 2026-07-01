namespace AutoReparos.Application.Shared.Interfaces
{
    public interface IAprovacaoTokenService
    {
        string GerarToken(Guid ordemServicoId);
        Guid ValidarToken(string token);
    }
}
