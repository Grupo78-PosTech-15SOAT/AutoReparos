using AutoReparos.Application.Clientes.DTOs.Request;
using AutoReparos.Application.Clientes.DTOs.Response;
using AutoReparos.Application.Clientes.Mappers;
using AutoReparos.Application.Clientes.UseCases.Interfaces;
using AutoReparos.Domain.Clientes.Entities;
using AutoReparos.Domain.Clientes.Exceptions;
using AutoReparos.Domain.Clientes.Repositories;
using AutoReparos.Domain.Clientes.ValueObjects;
using AutoReparos.Domain.Shared.Exceptions;

namespace AutoReparos.Application.Clientes.UseCases
{
    public class CriarClienteUseCase(IClienteRepository repository) : ICriarClienteUseCase
    {
        public async Task<ClienteDto> ExecuteAsync(ClienteCreateDto dto)
        {
            var normalizedDocumento = Documento.Normalizar(dto.Documento);
            var normalizedEmail = dto.Email.Trim().ToLower();

            var clienteExiste = await repository.GetByDocumentoOrEmail(normalizedDocumento, normalizedEmail);
            if (clienteExiste != null)
            {
                if (clienteExiste.Documento.Valor == normalizedDocumento)
                {
                    throw new InvalidDocumentoException("CPF ou CNPJ já cadastrado.");
                }
                throw new InvalidEmailException("E-mail já cadastrado.");
            }

            var cliente = new Cliente(dto.Nome, dto.Documento, dto.Telefone, dto.Email);

            await repository.Create(cliente);
            return ClienteMapper.ToDto(cliente);
        }
    }
}
