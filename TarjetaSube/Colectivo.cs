using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TarjetaSube
{
    public class Colectivo
    {
        public int Id { get; set; }
        public string Linea { get; set; } = string.Empty;
        public decimal Tarifa { get; set; } = 2150;

        public Colectivo() { }

        public Colectivo( string linea, decimal tarifa = 2150)
        {
            Linea = linea;
            Tarifa = tarifa;
        }

        public Boleto? PagarCon(Tarjeta tarjeta)
        {

            if (tarjeta == null)
            {
                throw new ArgumentNullException(nameof(tarjeta));
            }

            DateTime fechaActual = Tiempo.Ahora;

            if (!tarjeta.PuedeViajar(fechaActual))
            {
                return null;    
            }

            bool esBoletoGratuito = tarjeta.EsBoletoGratuitoEstudiantil();
            bool superaLimite = esBoletoGratuito
                && tarjeta.ContarViajesGratuitosHoy() >= Tarjeta.LimiteViajesGratuitosPorDia;

            decimal montoAPagar = superaLimite ? Tarifa : tarjeta.CalcularPasaje(Tarifa, fechaActual);

            decimal saldoPrevio = tarjeta.Saldo;
            decimal totalAbonado = montoAPagar;
            if (saldoPrevio < 0)
            {
                totalAbonado = montoAPagar + Math.Abs(saldoPrevio);
            }

            if (!tarjeta.Descontar(montoAPagar))
            {

                return null;
            }

            tarjeta.RegistrarViaje(fechaActual);

            if (esBoletoGratuito && !superaLimite)
            {
                tarjeta.RegistrarViajeGratuito();
            }

            return new Boleto(this, tarjeta, montoAPagar, tarjeta.Saldo, totalAbonado);

        }
        public bool pagarCon(Tarjeta tarjeta) => PagarCon(tarjeta) != null;
    }
}
