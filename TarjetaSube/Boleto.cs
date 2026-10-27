using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TarjetaSube
{
    public class Boleto
    {
        public int Id { get; set; }
        public decimal Tarifa { get; set; }
        public DateTime Fecha { get; set; }
        public decimal SaldoRestante { get; set; }
        public string Linea { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public decimal Abonado { get; set; }

        public int TarjetaId { get; set; }
        public Tarjeta? Tarjeta { get; set; }
        public int ColectivoId { get; set; }
        public Colectivo? Colectivo { get; set; }
        public bool EsTrasbordo { get; set; } = false;

        public DateTime ObtenerFecha() { 
            return Fecha;
        }

        public string ObtenerTipoTarjeta() {
            return Tipo;
        }

        public string ObtenerLinea()
        {
            return Linea;
        }
        public decimal ObtenerTotalAbonado()
        {
            return Abonado;
        }
        public decimal ObtenerSaldo()
        {
            return SaldoRestante;
        }
        public int ObtenerTarjetaId()
        {
            return TarjetaId;
        }

        public Boleto()
        {

        }

        public Boleto (Colectivo colectivo, Tarjeta tarjeta, decimal tarifa, decimal saldoRestante, decimal? totalAbonado = null, bool esTrasbordo = false)
        {
            Colectivo = colectivo;
            Linea = colectivo.Linea;
            ColectivoId = colectivo.Id;

            Tarjeta = tarjeta;
            TarjetaId = tarjeta.Id;

            Tarifa = tarifa;
            SaldoRestante = saldoRestante;
            Fecha = DateTime.Now;

            Tipo = tarjeta.TarjetaTipo?.Nombre ?? "Normal";

            Abonado = totalAbonado ?? tarifa;
            EsTrasbordo = esTrasbordo;
        }
    }
}
