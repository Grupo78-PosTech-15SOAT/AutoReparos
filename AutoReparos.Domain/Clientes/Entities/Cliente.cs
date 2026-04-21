using AutoReparos.Domain.Clientes.Exceptions;
using AutoReparos.Domain.Clientes.ValueObjects;
using AutoReparos.Domain.Shared;

namespace AutoReparos.Domain.Clientes.Entities
{
    public class Cliente : Entity
    {
        /// <summary>
        /// Nome completo do cliente
        /// </summary>
        public string Nome { get; private set; }

        /// <summary>
        /// CPF/CNPJ do cliente
        /// </summary>
        public Documento Documento { get; }

        /// <summary>
        /// Número de telefone do cliente
        /// </summary>
        public string Telefone { get; private set; }

        /// <summary>
        /// E-mail do cliente
        /// </summary>
        public Email Email { get; private set; }

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
        public Cliente(string nome, Documento documento, string telefone, Email email) : base()
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new InvalidClienteException("Nome é obrigatório");
            if (string.IsNullOrWhiteSpace(telefone))
                throw new InvalidClienteException("Telefone é obrigatório");

            if (nome.Length > 100)
                throw new InvalidClienteException("Nome muito longo, o nome deve ter no máximo 100 caracteres.");

            var digitsPhone = new string(telefone.Where(char.IsDigit).ToArray());
            if (digitsPhone.Length != 9)
                throw new InvalidClienteException("Telefone inválido. O número deve conter 9 dígitos");
            else if (digitsPhone.FirstOrDefault() != '9')
                throw new InvalidClienteException("Telefone inválido. O número deve começar com 9");

            Nome = nome;
            Documento = documento;
            Telefone = telefone;
            Email = email;
            CriadoEm = DateTime.UtcNow;
        }

        /// <summary>
        /// Método responsável por atualizar os dados do cliente
        /// </summary>
        /// <param name="nome">Nome do cliente</param>
        /// <param name="email">Endereço de e-mail do cliente</param>
        /// <param name="telefone">Número de telefone do cliente</param>
        public void Atualizar(string nome, Email email, string telefone)
        {
            Nome = nome;
            Email = email;
            Telefone = telefone;
            AtualizadoEm = DateTime.UtcNow;
        }
    }
}
