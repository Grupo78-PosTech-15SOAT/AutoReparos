using AutoReparos.Domain.Clientes.Exceptions;
using AutoReparos.Domain.Clientes.ValueObjects;
using AutoReparos.Domain.Shared;
using AutoReparos.Domain.Shared.ValueObjects;

namespace AutoReparos.Domain.Clientes.Entities
{
    public class Cliente : Entity
    {
        /// <summary>
        /// Nome completo do cliente
        /// </summary>
        public string Nome { get; private set; } = null!;

        /// <summary>
        /// CPF/CNPJ do cliente
        /// </summary>
        public Documento Documento { get; private set; } = null!;

        /// <summary>
        /// Número de telefone do cliente
        /// </summary>
        public Telefone Telefone { get; private set; } = null!;

        /// <summary>
        /// E-mail do cliente
        /// </summary>
        public Email Email { get; private set; } = null!;

        /// <summary>
        /// Data de criação
        /// </summary>
        public DateTime CriadoEm { get; }

        /// <summary>
        /// Data da última atualização
        /// </summary>
        public DateTime AtualizadoEm { get; private set; }

        /// <summary>
        /// Construtor para uso do Entity Framework
        /// </summary>
        protected Cliente() { }

        /// <summary>
        /// Construtor da classe
        /// </summary>
        public Cliente(string nome, string documento, string telefone, string email) : base()
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new InvalidClienteException("Nome é obrigatório");

            if (nome.Length > 100)
                throw new InvalidClienteException("Nome muito longo, o nome deve ter no máximo 100 caracteres.");

            Nome = nome;
            Documento = Documento.Create(documento);
            Telefone = Telefone.Create(telefone);
            Email = Email.Create(email);
            CriadoEm = DateTime.UtcNow;
        }

        /// <summary>
        /// Método responsável por atualizar os dados do cliente
        /// </summary>
        /// <param name="nome">Nome do cliente</param>
        /// <param name="email">Endereço de e-mail do cliente</param>
        /// <param name="telefone">Número de telefone do cliente</param>
        public void Atualizar(string nome, string email, string telefone)
        {
            if (nome.Length > 100)
                throw new InvalidClienteException("Nome muito longo, o nome deve ter no máximo 100 caracteres.");

            Nome = nome;
            Email = Email.Create(email);
            Telefone = Telefone.Create(telefone);
            AtualizadoEm = DateTime.UtcNow;
        }
    }
}
