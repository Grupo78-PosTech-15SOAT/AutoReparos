using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoReparos.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class v8_cliente_inativo_e_os_diagnostico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DiagnosticoIniciadoEm",
                table: "OrdensServico",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InativoEm",
                table: "Clientes",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiagnosticoIniciadoEm",
                table: "OrdensServico");

            migrationBuilder.DropColumn(
                name: "InativoEm",
                table: "Clientes");
        }
    }
}
