using AutoReparos.Domain.Shared;
using AutoReparos.Domain.Shared.ValueObjects;
using AutoReparos.Domain.Usuarios.Enums;

namespace AutoReparos.Domain.Usuarios.Entities
{
    /// <summary>
    /// Representa um usuário do sistema (Mecânico, Atendente ou Administrador)
    /// </summary>
    public class Usuario : Entity
    {
        /// <summary>
        /// Nome completo do usuário
        /// </summary>
        public string NomeCompleto { get; private set; } = null!;

        /// <summary>
        /// E-mail do usuário
        /// </summary>
        public Email Email { get; private set; } = null!;

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
        /// <param name="email">E-mail do usuário</param>
        /// <param name="tipo">Perfil de acesso do usuário</param>
        public Usuario(string nomeCompleto, string email, ETipoUsuario tipo) : base()
        {
            if (string.IsNullOrWhiteSpace(nomeCompleto))
                throw new ArgumentException("Nome completo é obrigatório", nameof(nomeCompleto));

            if (nomeCompleto.Length > 150)
                throw new ArgumentException("Nome completo muito longo, o nome deve ter no máximo 150 caracteres.", nameof(nomeCompleto));

            NomeCompleto = nomeCompleto;
            Email = Email.Create(email);
            Tipo = tipo;
            CriadoEm = DateTime.UtcNow;
        }

        private Usuario(Guid id, string nomeCompleto, Email email, ETipoUsuario tipo, DateTime criadoEm, DateTime? atualizadoEm) : base(id)
        {
            NomeCompleto = nomeCompleto;
            Email = email;
            Tipo = tipo;
            CriadoEm = criadoEm;
            AtualizadoEm = atualizadoEm;
        }

        /// <summary>
        /// Factory para carregar um usuário existente da persistência
        /// </summary>
        public static Usuario Load(Guid id, string nomeCompleto, string email, ETipoUsuario tipo, DateTime criadoEm, DateTime? atualizadoEm)
        {
            return new Usuario(id, nomeCompleto, Email.Create(email), tipo, criadoEm, atualizadoEm);
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
