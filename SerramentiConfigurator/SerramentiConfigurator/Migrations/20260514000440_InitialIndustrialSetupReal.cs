using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SerramentiConfigurator.Migrations
{
    /// <inheritdoc />
    public partial class InitialIndustrialSetupReal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccessoriCatalogo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodiceArticolo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descrizione = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UnitaMisura = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrezzoUnitario = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessoriCatalogo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Clienti",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RagioneSociale = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PartitaIva = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clienti", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MatriceRegole",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SerieProfilo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TipoAperturaFerramenta = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CodiceAccessorioCorrelato = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LogicaCalcolo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuantitaMoltiplicatore = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatriceRegole", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SerieSerramenti",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    CodiceSerie = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descrizione = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TipoSistema = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LunghezzaBarraDefault = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SerieSerramenti", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cantieri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Localita = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Stato = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsPinned = table.Column<bool>(type: "bit", nullable: false),
                    DataCreazione = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cantieri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cantieri_Clienti_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clienti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProfiliCatalogo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SerieSerramentoId = table.Column<int>(type: "int", nullable: false),
                    CodiceArticolo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descrizione = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TipoProfilo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrezzoAlMetro = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PesoAlMetro = table.Column<double>(type: "float", nullable: false),
                    LunghezzaCommercialeBarra = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfiliCatalogo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProfiliCatalogo_SerieSerramenti_SerieSerramentoId",
                        column: x => x.SerieSerramentoId,
                        principalTable: "SerieSerramenti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Commesse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CantiereId = table.Column<int>(type: "int", nullable: false),
                    TitoloCommessa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Commesse", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Commesse_Cantieri_CantiereId",
                        column: x => x.CantiereId,
                        principalTable: "Cantieri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ElementiInfissi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommessaId = table.Column<int>(type: "int", nullable: false),
                    EtichettaVano = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TipologiaApertura = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SerieProfilo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Colore = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Larghezza = table.Column<int>(type: "int", nullable: false),
                    Altezza = table.Column<int>(type: "int", nullable: false),
                    Quantita = table.Column<int>(type: "int", nullable: false),
                    TipoSoglia = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TipoManiglia = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TipoVetro = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TelaioSuperiore = table.Column<bool>(type: "bit", nullable: false),
                    TelaioInferiore = table.Column<bool>(type: "bit", nullable: false),
                    TelaioSinistro = table.Column<bool>(type: "bit", nullable: false),
                    TelaioDestro = table.Column<bool>(type: "bit", nullable: false),
                    NumeroAnte = table.Column<int>(type: "int", nullable: false),
                    AntaHaZoccoloInferiore = table.Column<bool>(type: "bit", nullable: false),
                    AltezzaZoccolo = table.Column<int>(type: "int", nullable: false),
                    HaSopraluce = table.Column<bool>(type: "bit", nullable: false),
                    AltezzaSopraluce = table.Column<int>(type: "int", nullable: false),
                    HaFiancoluce = table.Column<bool>(type: "bit", nullable: false),
                    LarghezzaFiancoluce = table.Column<int>(type: "int", nullable: false),
                    TipoAperturaFerramenta = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VersoApertura = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumeroCerniere = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElementiInfissi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ElementiInfissi_Commesse_CommessaId",
                        column: x => x.CommessaId,
                        principalTable: "Commesse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cantieri_ClienteId",
                table: "Cantieri",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Commesse_CantiereId",
                table: "Commesse",
                column: "CantiereId");

            migrationBuilder.CreateIndex(
                name: "IX_ElementiInfissi_CommessaId",
                table: "ElementiInfissi",
                column: "CommessaId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfiliCatalogo_SerieSerramentoId",
                table: "ProfiliCatalogo",
                column: "SerieSerramentoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccessoriCatalogo");

            migrationBuilder.DropTable(
                name: "ElementiInfissi");

            migrationBuilder.DropTable(
                name: "MatriceRegole");

            migrationBuilder.DropTable(
                name: "ProfiliCatalogo");

            migrationBuilder.DropTable(
                name: "Commesse");

            migrationBuilder.DropTable(
                name: "SerieSerramenti");

            migrationBuilder.DropTable(
                name: "Cantieri");

            migrationBuilder.DropTable(
                name: "Clienti");
        }
    }
}
