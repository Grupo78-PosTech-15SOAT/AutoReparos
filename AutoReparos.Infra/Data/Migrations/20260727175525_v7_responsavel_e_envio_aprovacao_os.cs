using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoReparos.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class v7_responsavel_e_envio_aprovacao_os : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EnvioAprovacaoEm",
                table: "OrdensServico",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponsavelId",
                table: "OrdensServico",
                type: "character varying(450)",
                maxLength: 450,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnvioAprovacaoEm",
                table: "OrdensServico");

            migrationBuilder.DropColumn(
                name: "ResponsavelId",
                table: "OrdensServico");
        }
    }
}
