using AutoReparos.Application.Clientes.DTOs.Request;
using AutoReparos.Application.Clientes.DTOs.Response;
using AutoReparos.Application.Clientes.Services.Interfaces;
using AutoReparos.Application.Shared;
using AutoReparos.Domain.Clientes.Entities;
using AutoReparos.Domain.Clientes.Repositories;
using AutoReparos.Domain.Clientes.ValueObjects;
using AutoReparos.Domain.Clientes.Exceptions;
using AutoReparos.Domain.Shared.Exceptions;

namespace AutoReparos.Application.Clientes.Services
{
    public class ClienteService : IClienteService
    {
        private readonly IClienteRepository _repository;

        public ClienteService(IClienteRepository repository)
        {
            _repository = repository;
        }

        public async Task<ClienteDTO> Create(ClienteCreateDTO dto)
        {
            var normalizedDocumento = Documento.Normalizar(dto.Documento);
            var normalizedEmail = dto.Email.Trim().ToLower();

            var clienteExiste = await _repository.GetByDocumentoOrEmail(normalizedDocumento, normalizedEmail);
            if (clienteExiste != null)
            {
                if (clienteExiste.Documento.Valor == normalizedDocumento)
                {
                    throw new InvalidDocumentoException("CPF ou CNPJ já cadastrado.");
                }
                throw new InvalidEmailException("E-mail já cadastrado.");
            }

            var cliente = new Cliente(dto.Nome, new Documento(dto.Documento), dto.Telefone, Email.Create(dto.Email));

            await _repository.Create(cliente);
            return ToDTO(cliente);
        }

        public async Task<ClienteDTO?> GetById(Guid id)
        {
            var cliente = await _repository.GetById(id);
            if (cliente == null)
                throw new NotFoundException("Cliente não encontrado.");
            return ToDTO(cliente);
        }


        public async Task<PagedResult<ClienteDTO>> GetAll(PagedRequest request)
        {
            var (clientes, total) = await _repository.GetAll(request.Nome, (request.PageNumber - 1) * request.PageSize, request.PageSize);
            var items = clientes.Select(ToDTO);
            return new PagedResult<ClienteDTO>(items, total, request.PageNumber, request.PageSize);
        }

        public async Task Update(Guid id, ClienteUpdateDTO dto)
        {
            var cliente = await _repository.GetById(id);
            if (cliente == null)
                throw new NotFoundException("Cliente não encontrado.");

            cliente.Atualizar(dto.Nome, Email.Create(dto.Email), dto.Telefone);
            await _repository.Update(cliente);
        }

        public async Task Delete(Guid id)
        {
            var cliente = await _repository.GetById(id);
            if (cliente == null)
                throw new NotFoundException("Cliente não encontrado.");

            await _repository.Delete(cliente);
        }

        private static ClienteDTO ToDTO(Cliente c) => new(
            c.Id,
            c.Nome,
            c.Documento.Valor,
            c.Documento.Tipo.ToString(),
            c.Telefone,
            c.Email
        );
    }
}
