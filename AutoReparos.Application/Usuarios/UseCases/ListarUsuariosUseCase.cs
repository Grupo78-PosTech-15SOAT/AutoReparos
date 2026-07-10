using AutoReparos.Application.Shared;
using AutoReparos.Application.Usuarios.DTOs.Request;
using AutoReparos.Application.Usuarios.DTOs.Response;
using AutoReparos.Application.Usuarios.Mappers;
using AutoReparos.Application.Usuarios.UseCases.Interfaces;
using AutoReparos.Domain.Usuarios.Repositories;

namespace AutoReparos.Application.Usuarios.UseCases
{
    public class ListarUsuariosUseCase(IUsuarioRepository usuarioRepository) : IListarUsuariosUseCase
    {
        public async Task<PagedResult<UsuarioDto>> ExecuteAsync(UsuarioPagedRequest request)
        {
            var (usuarios, total) = await usuarioRepository.GetAllAsync(
                request.Nome,
                request.Skip,
                request.PageSize);

            var items = usuarios.Select(UsuarioMapper.ToDto);
            return new PagedResult<UsuarioDto>(items, total, request.PageNumber, request.PageSize);
        }
    }
}
