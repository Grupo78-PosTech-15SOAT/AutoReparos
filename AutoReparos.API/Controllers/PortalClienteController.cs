using AutoReparos.Application.Clientes.UseCases.Interfaces;
using AutoReparos.Application.OrdensServicos.UseCases.Interfaces;
using System.Security.Claims;

namespace AutoReparos.API.Controllers;

public class PortalClienteController(
    IObterMeusVeiculosUseCase obterMeusVeiculosUseCase,
    IObterMinhasOrdensServicoUseCase obterMinhasOrdensServicoUseCase)
{
    public async Task<IResult> GetMeusVeiculos(ClaimsPrincipal user, CancellationToken cancellationToken = default)
    {
        var clienteId = ObterClienteId(user);
        if (clienteId == null)
            return Results.Unauthorized();

        var veiculos = await obterMeusVeiculosUseCase.ExecuteAsync(clienteId.Value, cancellationToken);
        return Results.Ok(veiculos);
    }

    public async Task<IResult> GetMinhasOrdensServico(
        ClaimsPrincipal user,
        string? placa = null,
        CancellationToken cancellationToken = default)
    {
        var clienteId = ObterClienteId(user);
        if (clienteId == null)
            return Results.Unauthorized();

        var ordens = await obterMinhasOrdensServicoUseCase.ExecuteAsync(clienteId.Value, placa, cancellationToken);
        return Results.Ok(ordens);
    }

    private static Guid? ObterClienteId(ClaimsPrincipal user)
    {
        var idStr = user.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? user.FindFirstValue("sub");

        return Guid.TryParse(idStr, out var id) ? id : null;
    }
}
