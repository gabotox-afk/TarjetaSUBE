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
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=transporte.db");
            }
        }
    }
}
