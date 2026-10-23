using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Storage;

namespace TarjetaSube
{
    public class Tarjeta
    {
        public int Id { get; set; }
        public decimal Saldo { get; private set; }
        public const decimal LimiteSaldo = 56000;
        public const decimal SaldoNegativoPermitido = 2000;
        public decimal SaldoPendiente { get; private set; } = 0;
        private static readonly List<decimal> CargasValidas = new List<decimal>() { 2000, 3000, 4000, 5000, 8000, 10000, 15000, 20000, 25000, 30000 };
        public List<DateTime> Viajes { get; set; } = new();
        public bool EsMedioBoleto => TarjetaTipo?.Id == TarjetaTipo.MedioBoletoEstudiantilId || (TarjetaTipo != null && TarjetaTipo.PorcentajeDescuento == 50);

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

        public decimal CalcularPasaje(decimal tarifa, DateTime? fecha = null)
        {
            DateTime f = fecha ?? Tiempo.Ahora;

            if (TarjetaTipo == null)
                return tarifa;

            if (EsMedioBoleto)
            {
                int viajesHoy = Viajes.Count(v => v.Date == f.Date);
                if (viajesHoy >= 2)
                {
                    return tarifa;
                }
            }

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

        /// <summary>
        /// Acredita en el saldo todo lo que quepa hasta el límite máximo.
        /// El excedente queda almacenado en SaldoPendiente para acreditarse
        /// a medida que se libere espacio con el uso de la tarjeta.
        /// </summary>
        public void AcreditarCarga(decimal monto)
        {
            decimal espacioDisponible = LimiteSaldo - Saldo;

            if (espacioDisponible <= 0)
            {
                SaldoPendiente += monto;
                return;
            }

            if (monto <= espacioDisponible)
            {
                Saldo += monto;
            }
            else
            {
                Saldo += espacioDisponible;
                SaldoPendiente += monto - espacioDisponible;
            }
        }

        public void Cargar(decimal monto)
        {
            if (!CargasValidas.Contains(monto))
            {
                return;
            }

            AcreditarCarga(monto);
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

            // Si hay saldo pendiente, acreditar lo que quepa tras el descuento
            if (SaldoPendiente > 0)
            {
                decimal espacioDisponible = LimiteSaldo - Saldo;
                decimal aAcreditar = Math.Min(SaldoPendiente, espacioDisponible);
                if (aAcreditar > 0)
                {
                    Saldo += aAcreditar;
                    SaldoPendiente -= aAcreditar;
                }
            }

            return true;
        }

        public bool PuedeViajar(DateTime fecha)
        {
            if (EsMedioBoleto && Viajes.Count > 0)
            {
                DateTime ultimoViaje = Viajes.Last();
                if ((fecha - ultimoViaje).TotalMinutes < 5)
                {
                    return false;
                }
            }

            if (EsFranquicia)
            {
                bool esDiaHabil = fecha.DayOfWeek >= DayOfWeek.Monday && fecha.DayOfWeek <= DayOfWeek.Friday;
                bool esHorarioValido = fecha.TimeOfDay >= new TimeSpan(6, 0, 0) && fecha.TimeOfDay <= new TimeSpan(22, 0, 0);

                if (!esDiaHabil || !esHorarioValido)
                {
                    return false;
                }
            }

            return true;
        }

        public void RegistrarViaje(DateTime fecha)
        {
            Viajes.Add(fecha);
        }

        public bool EsFranquicia => TarjetaTipo != null && TarjetaTipo.Id != TarjetaTipo.NormalId && !string.Equals(TarjetaTipo.Nombre, "Normal", StringComparison.OrdinalIgnoreCase);

    }
}
