using AutoReparos.Domain.Shared;
using AutoReparos.Domain.Veiculos.Exceptions;
using AutoReparos.Domain.Veiculos.ValueObjects;

namespace AutoReparos.Domain.Veiculos.Entities
{
    public class Veiculo : Entity
    {
        public Guid ClienteId { get; private set; }

        public string Marca { get; private set; } = null!;
        public string Modelo { get; private set; } = null!;

        public int AnoFabricacao { get; private set; }
        public int AnoModelo { get; private set; }

        public Placa Placa { get; private set; } = null!;
        public Chassi Chassi { get; private set; } = null!;
        public Renavam Renavam { get; private set; } = null!;

        public DateTime CriadoEm { get; }
        public DateTime? AtualizadoEm { get; private set; }

        /// <summary>
        /// Construtor para uso do Entity Framework
        /// </summary>
        protected Veiculo() { }

        public Veiculo(
            Guid clienteId,
            string marca,
            string modelo,
            int anoFabricacao,
            int anoModelo,
            Placa placa,
            Chassi chassi,
            Renavam renavam)
        {
            Validar(clienteId, marca, modelo, anoFabricacao, anoModelo, placa, chassi, renavam);

            ClienteId = clienteId;
            Marca = marca;
            Modelo = modelo;
            AnoFabricacao = anoFabricacao;
            AnoModelo = anoModelo;
            Placa = placa;
            Chassi = chassi;
            Renavam = renavam;

            CriadoEm = DateTime.UtcNow;
        }

        public void Atualizar(
            string marca,
            string modelo,
            int anoFabricacao,
            int anoModelo)
        {
            ValidarAtualizacao(marca, modelo, anoFabricacao, anoModelo);

            Marca = marca;
            Modelo = modelo;
            AnoFabricacao = anoFabricacao;
            AnoModelo = anoModelo;

            AtualizadoEm = DateTime.UtcNow;
        }

        private static void Validar(
        Guid clienteId,
        string marca,
        string modelo,
        int anoFabricacao,
        int anoModelo,
        Placa placa,
        Chassi chassi,
        Renavam renavam)
        {
            if (clienteId == Guid.Empty)
                throw new InvalidVeiculoException("Cliente é obrigatório");

            ValidarAtualizacao(marca, modelo, anoFabricacao, anoModelo);

            if (placa is null)
                throw new InvalidVeiculoException("Placa é obrigatória");

            if (chassi is null)
                throw new InvalidVeiculoException("Chassi é obrigatório");

            if (renavam is null)
                throw new InvalidVeiculoException("Renavam é obrigatório");
        }

        private static void ValidarAtualizacao(
            string marca,
            string modelo,
            int anoFabricacao,
            int anoModelo)
        {
            if (string.IsNullOrWhiteSpace(marca))
                throw new InvalidVeiculoException("Marca é obrigatória");

            if (string.IsNullOrWhiteSpace(modelo))
                throw new InvalidVeiculoException("Modelo é obrigatório");

            if (anoFabricacao < 1900 || anoFabricacao > DateTime.UtcNow.Year + 1)
                throw new InvalidVeiculoException("Ano de fabricação inválido");

            if (anoModelo < anoFabricacao)
                throw new InvalidVeiculoException("Ano modelo não pode ser menor que fabricação");
        }
    }
}