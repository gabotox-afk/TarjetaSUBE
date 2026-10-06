using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;

namespace TarjetaSube
{
    public class Tarjeta
    {
        public int Id { get; set; }
        public decimal Saldo { get; private set; }
        public const decimal LimiteSaldo = 40000;
        private static readonly List<decimal> CargasValidas = new List<decimal>() { 2000, 3000, 4000, 5000, 8000, 10000, 15000, 20000, 25000, 30000 };

        public int TarjetaTipoId { get; set; } = TarjetaTipo.NormalId;
        public TarjetaTipo? TarjetaTipo { get; set; }


        public Tarjeta()
        {
            Saldo = 0;
        }


        public Tarjeta(decimal saldoInicial)
        {
            if (saldoInicial < 0 || saldoInicial > LimiteSaldo)
                throw new ArgumentException("El saldo inicial no es válido.");

            Saldo = saldoInicial;
        }

        public Tarjeta(decimal saldoInicial, TarjetaTipo tipo) : this(saldoInicial)
        {
            TarjetaTipo = tipo ?? throw new ArgumentNullException(nameof(tipo));
            TarjetaTipoId = tipo.Id;
        }

        public decimal CalcularPasaje(decimal tarifa)
        {
            if (TarjetaTipo == null)
                return tarifa;

            return TarjetaTipo.CalcularPrecio(tarifa);
        }

        public void Cargar(decimal monto)
        {
            if (!CargasValidas.Contains(monto))
            {
                return;
            }
            
            if((Saldo + monto)> LimiteSaldo)
            {
                return;
            }
            Saldo += monto;
        
        }

        public bool Descontar(decimal monto)
        {
            if (monto < 0)
            {
                return false;
            }
            
            if(Saldo < monto)
            {
                return false;
            }

            Saldo -= monto;
            return true;
        }
    }
}
