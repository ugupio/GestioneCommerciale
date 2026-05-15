using Microsoft.EntityFrameworkCore;
using SerramentiConfigurator.Models;
using System.Reflection.Emit;

namespace SerramentiConfigurator.Data
{
    public class AppDbContext : DbContext
    {
        // Il costruttore riceve la stringa di connessione definita in appsettings.json
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // =========================================================================
        // TABELLE ANAGRAFICHE E CANTIERI B2B
        // =========================================================================
        public DbSet<Cliente> Clienti { get; set; }
        public DbSet<Cantiere> Cantieri { get; set; }
        public DbSet<Commessa> Commesse { get; set; }
        public DbSet<ElementoInfisso> ElementiInfissi { get; set; }

        // =========================================================================
        // TABELLE ARCHIVIO TECNICO (STRUTTURA SISTEMI STILE FP PRO)
        // =========================================================================
        public DbSet<SerieSerramento> SerieSerramenti { get; set; }
        public DbSet<ProfiloCatalogo> ProfiliCatalogo { get; set; }
        public DbSet<AccessorioCatalogo> AccessoriCatalogo { get; set; }
        public DbSet<RegolaFerramenta> MatriceRegole { get; set; }

        // =========================================================================
        // TABELLE DI CONFIGURAZIONE E PARAMETRI COMMERCIALI
        // =========================================================================
        public DbSet<ImpiantoVerniciatura> ImpiantiVerniciatura { get; set; }
        public DbSet<FinituraPrezzo> FiniturePrezzi { get; set; }




        // Configurazione avanzata delle tabelle, precisione e vincoli di integrità
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Configurazione precisione per i prezzi commerciali B2B (4 cifre decimali industriali)
            modelBuilder.Entity<ProfiloCatalogo>()
                .Property(p => p.PrezzoAlMetro)
                .HasPrecision(18, 4);

            modelBuilder.Entity<AccessorioCatalogo>()
                .Property(a => a.PrezzoUnitario)
                .HasPrecision(18, 4);

            // 2. Configurazione vincoli e relazioni a cascata (Evita blocchi su SQL Server)
            modelBuilder.Entity<Cantiere>()
                .HasOne<Cliente>()
                .WithMany(c => c.Cantieri)
                .HasForeignKey(c => c.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Commessa>()
                .HasOne<Cantiere>()
                .WithMany(c => c.Commesse)
                .HasForeignKey(c => c.CantiereId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ElementoInfisso>()
                .HasOne<Commessa>()
                .WithMany(c => c.Elementi)
                .HasForeignKey(c => c.CommessaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProfiloCatalogo>()
                .HasOne<SerieSerramento>()
                .WithMany(s => s.Profili)
                .HasForeignKey(p => p.SerieSerramentoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SerieSerramento>()
                .Property(s => s.Id)
                .ValueGeneratedNever();

            modelBuilder.Entity<Cliente>()
                .Property(c => c.ScontoStandardProfili)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Cliente>()
                .Property(c => c.ScontoStandardAccessori)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Commessa>()
                .Property(c => c.ScontoProfiliApplicato)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Commessa>()
                .Property(c => c.ScontoAccessoriApplicato)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<FinituraPrezzo>()
                .Property(f => f.SovrapprezzoVerniciaturaAlKg)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<ImpiantoVerniciatura>().HasData(
                new ImpiantoVerniciatura { Id = 1, Nome = "Impianto Interno", Descrizione = "Impianto aziendale per tinte RAL standard" },
                new ImpiantoVerniciatura { Id = 2, Nome = "Impianto Esterno", Descrizione = "Subfornitore specializzato in effetti legno ed anodizzazioni" }
            );

            modelBuilder.Entity<FinituraPrezzo>().HasData(
                new FinituraPrezzo { Id = 1, CodiceFinitura = "RAL9010", Descrizione = "Bianco Opaco", Sottogruppo = "Colori Standard", FasciaPrezzo = "ITALVER", CodiceEsadecimale = "#F4F4F4", SovrapprezzoVerniciaturaAlKg = 1.20m, ImpiantoVerniciaturaId = 1 },
                new FinituraPrezzo { Id = 2, CodiceFinitura = "RAL7016", Descrizione = "Antracite", Sottogruppo = "Colori Standard", FasciaPrezzo = "D", CodiceEsadecimale = "#383E42", SovrapprezzoVerniciaturaAlKg = 1.40m, ImpiantoVerniciaturaId = 1 },
                new FinituraPrezzo { Id = 3, CodiceFinitura = "LEGNO_ROVERE", Descrizione = "Sublimato Rovere", Sottogruppo = "Sublimati", FasciaPrezzo = "SUBLIMATI", CodiceEsadecimale = "#B07C41", SovrapprezzoVerniciaturaAlKg = 3.50m, ImpiantoVerniciaturaId = 2 }
            );

            // All'interno di protected override void OnModelCreating(ModelBuilder modelBuilder)

            modelBuilder.Entity<ProfiloCatalogo>()
                .HasOne(p => p.SerieSerramento)         // Il profilo ha una serie padre
                .WithMany(s => s.Profili)                // La serie ha molti profili
                .HasForeignKey(p => p.SerieSerramentoId) // La chiave esterna REALE è questa qui
                .OnDelete(DeleteBehavior.Restrict);      // Evita i cicli di eliminazione a cascata



        }
    }
}
