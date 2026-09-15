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
        public decimal Tarifa { get; set; } = 1580;

        public Colectivo() { }

        public Colectivo( string linea, decimal tarifa = 1580)
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

            if (!tarjeta.Descontar(Tarifa))
            {

                return null;
            }

            return new Boleto(this, tarjeta, Tarifa, tarjeta.Saldo);

        }
        public Boleto? pagarCon(Tarjeta tarjeta) => PagarCon(tarjeta);
    }
}
