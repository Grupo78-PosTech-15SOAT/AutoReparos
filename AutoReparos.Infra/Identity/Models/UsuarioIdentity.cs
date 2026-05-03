using AutoReparos.Domain.Usuarios.Entities;
using AutoReparos.Domain.Usuarios.Enums;
using Microsoft.AspNetCore.Identity;

namespace AutoReparos.Infra.Identity.Models
{
    public class UsuarioIdentity : IdentityUser<Guid>
    {
        public string NomeCompleto { get; set; } = string.Empty;
        public ETipoUsuario Tipo { get; set; }
        public DateTime CriadoEm { get; set; }
        public DateTime? AtualizadoEm { get; set; }

        /// <summary>
        /// Construtor para uso do EF/Identity
        /// </summary>
        public UsuarioIdentity() { }

        /// <summary>
        /// Construtor da classe UsuarioIdentity
        /// </summary>
        public UsuarioIdentity(Guid id, string nomeCompleto, string email, ETipoUsuario tipo, DateTime criadoEm)
        {
            Id = id;
            NomeCompleto = nomeCompleto;
            Email = email;
            UserName = email;
            Tipo = tipo;
            CriadoEm = criadoEm;
            NormalizedEmail = email.ToUpperInvariant();
            NormalizedUserName = email.ToUpperInvariant();
        }

        public Usuario ToDomain()
        {
            return Usuario.Load(
                Id,
                NomeCompleto,
                Email!,
                Tipo,
                CriadoEm,
                AtualizadoEm);
        }
    }
}
