using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TarjetaSube
{
    public class TarjetaTipo
    {
        public const int NormalId = 1;
        public const int MedioBoletoEstudiantilId = 2;
        public const int BoletoGratuitoEstudiantilId = 3;
        public const int FranquiciaCompletaId = 4;

        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;

        // Valor entre 0 y 100: 0 = paga tarifa completa, 100 = viaja gratis.
        public decimal PorcentajeDescuento { get; set; }

        public List<Tarjeta> Tarjetas { get; set; } = new List<Tarjeta>();

        public TarjetaTipo() { }

        public TarjetaTipo(string nombre, decimal porcentajeDescuento)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre del tipo de tarjeta no es válido.");

            if (porcentajeDescuento < 0 || porcentajeDescuento > 100)
                throw new ArgumentException("El porcentaje de descuento debe estar entre 0 y 100.");

            Nombre = nombre;
            PorcentajeDescuento = porcentajeDescuento;
        }

        public decimal CalcularPrecio(decimal tarifa)
        {
            if (tarifa < 0)
                throw new ArgumentException("La tarifa no puede ser negativa.");

            return tarifa * (100 - PorcentajeDescuento) / 100;
        }
    }
}
