using AutoReparos.Domain.Shared;
using AutoReparos.Domain.Veiculos.ValueObjects;
using AutoReparos.Domain.Veiculos.ValueObjects.Exceptions;

namespace AutoReparos.Domain.Veiculos.Entities
{
    public class Veiculo : Entity
    {
        public Guid ClienteId { get; private set; }

        public string Marca { get; private set; }
        public string Modelo { get; private set; }

        public int AnoFabricacao { get; private set; }
        public int AnoModelo { get; private set; }

        public Placa Placa { get; private set; }
        public Chassi Chassi { get; private set; }
        public Renavam Renavam { get; private set; }

        public DateTime CriadoEm { get; }
        public DateTime AtualizadoEm { get; private set; }

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
            if (clienteId == Guid.Empty)
                throw new InvalidVeiculoException("Cliente é obrigatório");

            if (string.IsNullOrWhiteSpace(marca))
                throw new InvalidVeiculoException("Marca é obrigatória");

            if (string.IsNullOrWhiteSpace(modelo))
                throw new InvalidVeiculoException("Modelo é obrigatório");

            if (anoFabricacao < 1900 || anoFabricacao > DateTime.UtcNow.Year + 1)
                throw new InvalidVeiculoException("Ano de fabricação inválido");

            if (anoModelo < anoFabricacao)
                throw new InvalidVeiculoException("Ano modelo não pode ser menor que fabricação");

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
            Marca = marca;
            Modelo = modelo;
            AnoFabricacao = anoFabricacao;
            AnoModelo = anoModelo;

            AtualizadoEm = DateTime.UtcNow;
        }
    }
}