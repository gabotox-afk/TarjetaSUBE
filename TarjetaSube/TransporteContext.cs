using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TarjetaSube
{
    public class TransporteContext : DbContext
    {
        public TransporteContext() { }
        public TransporteContext(DbContextOptions<TransporteContext> options) : base(options) { }
        public DbSet<Tarjeta> Tarjetas => Set<Tarjeta>();
        public DbSet<Colectivo> Colectivos => Set<Colectivo>();
        public DbSet<Boleto> Boletos => Set<Boleto>();
        public DbSet<TarjetaTipo> TarjetaTipos => Set<TarjetaTipo>();
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=transporte.db");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tarjeta>()
                .HasOne(t => t.TarjetaTipo)
                .WithMany(tt => tt.Tarjetas)
                .HasForeignKey(t => t.TarjetaTipoId);

            modelBuilder.Entity<TarjetaTipo>().HasData(
                new TarjetaTipo { Id = TarjetaTipo.NormalId, Nombre = "Normal", PorcentajeDescuento = 0 },
                new TarjetaTipo { Id = TarjetaTipo.MedioBoletoEstudiantilId, Nombre = "Medio boleto estudiantil", PorcentajeDescuento = 50 },
                new TarjetaTipo { Id = TarjetaTipo.BoletoGratuitoEstudiantilId, Nombre = "Boleto gratuito estudiantil", PorcentajeDescuento = 100 },
                new TarjetaTipo { Id = TarjetaTipo.FranquiciaCompletaId, Nombre = "Franquicia completa", PorcentajeDescuento = 100 }
            );
        }
    }
}
