using AutoReparos.Application.Clientes.DTOs.Response;
using AutoReparos.Domain.Clientes.Entities;

namespace AutoReparos.Application.Clientes.Mappers
{
    public static class ClienteMapper
    {
        public static ClienteDto ToDto(Cliente cliente) => new(
            cliente.Id,
            cliente.Nome,
            cliente.Documento.Valor,
            cliente.Documento.Tipo.ToString(),
            cliente.Telefone,
            cliente.Email
        );
    }
}
