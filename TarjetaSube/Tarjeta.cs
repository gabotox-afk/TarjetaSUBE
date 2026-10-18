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
        public const decimal SaldoNegativoPermitido = 2000;
        private static readonly List<decimal> CargasValidas = new List<decimal>() { 2000, 3000, 4000, 5000, 8000, 10000, 15000, 20000, 25000, 30000 };

        public int TarjetaTipoId { get; set; } = TarjetaTipo.NormalId;
        public TarjetaTipo? TarjetaTipo { get; set; }

        public const int LimiteViajesGratuitosPorDia = 2;
        private readonly Dictionary<DateTime, int> _viajesGratuitosPorDia = new Dictionary<DateTime, int>();

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

        public bool EsBoletoGratuitoEstudiantil()
        {
            return TarjetaTipoId == TarjetaTipo.BoletoGratuitoEstudiantilId
                || (TarjetaTipo != null && TarjetaTipo.Id == TarjetaTipo.BoletoGratuitoEstudiantilId);
        }

        public int ContarViajesGratuitosHoy()
        {
            var hoy = DateTime.Today;
            return _viajesGratuitosPorDia.TryGetValue(hoy, out int count) ? count : 0;
        }

        public void RegistrarViajeGratuito()
        {
            var hoy = DateTime.Today;
            if (_viajesGratuitosPorDia.ContainsKey(hoy))
                _viajesGratuitosPorDia[hoy]++;
            else
                _viajesGratuitosPorDia[hoy] = 1;
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

            if (monto == 0)
            {
                return true;
            }
            
            if(Saldo - monto < -SaldoNegativoPermitido)
            {
                return false;
            }

            Saldo -= monto;
            return true;
        }
    }
}
