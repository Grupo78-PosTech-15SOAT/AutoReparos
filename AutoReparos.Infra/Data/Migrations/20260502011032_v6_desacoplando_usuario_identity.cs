using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoReparos.Infra.Migrations
{
    /// <inheritdoc />
    public partial class v6_decouple_identity_usuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Intentionally empty.
            // This migration only updates the EF Core model snapshot after
            // decoupling UsuarioIdentity from the domain entity.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally empty.
            // No database changes were generated for this migration.
        }
    }
}