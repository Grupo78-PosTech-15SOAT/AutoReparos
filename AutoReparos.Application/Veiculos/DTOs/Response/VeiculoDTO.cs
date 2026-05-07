namespace AutoReparos.Application.Veiculos.DTOs.Response
{
    public record VeiculoDto(
        Guid Id,
        Guid ClienteId,
        string Marca,
        string Modelo,
        int AnoFabricacao,
        int AnoModelo,
        string Placa,
        string Chassi,
        string Renavam
    );
}
