using Microsoft.AspNetCore.Identity;
using AutoReparos.Domain.Clientes.ValueObjects;
using AutoReparos.Domain.Usuarios.Enums;

namespace AutoReparos.Domain.Usuarios.Entities
{
    /// <summary>
    /// Representa um usuário do sistema (Mecânico, Atendente ou Administrador)
    /// </summary>
    public class Usuario : IdentityUser<Guid>
    {
        /// <summary>
        /// Nome completo do usuário
        /// </summary>
        public string NomeCompleto { get; private set; }

        /// <summary>
        /// Tipo de perfil do usuário no sistema
        /// </summary>
        public ETipoUsuario Tipo { get; private set; }

        /// <summary>
        /// Data de criação do usuário
        /// </summary>
        public DateTime CriadoEm { get; }

        /// <summary>
        /// Data da última atualização dos dados do usuário
        /// </summary>
        public DateTime? AtualizadoEm { get; private set; }

        /// <summary>
        /// Construtor para uso do Entity Framework
        /// </summary>
        protected Usuario() { }
        /// <summary>
        /// Construtor da classe Usuario
        /// </summary>
        /// <param name="nomeCompleto">Nome completo do usuário</param>
        /// <param name="email">E-mail do usuário (usado para validação via Value Object)</param>
        /// <param name="tipo">Perfil de acesso do usuário</param>
        public Usuario(string nomeCompleto, Email email, ETipoUsuario tipo) : base()
        {
            if (string.IsNullOrWhiteSpace(nomeCompleto))
                throw new ArgumentException("Nome completo é obrigatório", nameof(nomeCompleto));

            if (nomeCompleto.Length > 150)
                throw new ArgumentException("Nome completo muito longo, o nome deve ter no máximo 150 caracteres.", nameof(nomeCompleto));

            Id = Guid.NewGuid();
            NomeCompleto = nomeCompleto;
            Email = email.Address;
            UserName = email.Address;
            Tipo = tipo;
            NormalizedEmail = email.Address.ToUpperInvariant();
            NormalizedUserName = email.Address.ToUpperInvariant();
            CriadoEm = DateTime.UtcNow;
        }

        /// <summary>
        /// Atualiza os dados básicos do usuário
        /// </summary>
        /// <param name="nomeCompleto">Novo nome completo</param>
        /// <param name="tipo">Novo perfil de acesso</param>
        public void Atualizar(string nomeCompleto, ETipoUsuario tipo)
        {
            if (string.IsNullOrWhiteSpace(nomeCompleto))
                throw new ArgumentException("Nome completo é obrigatório", nameof(nomeCompleto));

            NomeCompleto = nomeCompleto;
            Tipo = tipo;
            AtualizadoEm = DateTime.UtcNow;
        }
    }
}
