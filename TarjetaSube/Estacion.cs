using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TarjetaSube
{
    public class Estacion
    {
        public int Id { get; set; }
        public const decimal TarifaServicio = 2418.75m;
        
        public bool RetirarBici(Tarjeta tarjeta)
        {
            if (tarjeta == null) return false;

            DateTime fechaActual = Tiempo.Ahora;
            bool yaPagoHoy = tarjeta.TieneServicioBiciPagado(fechaActual);
            decimal MontoAPagar = yaPagoHoy ? 0 : TarifaServicio;

            if (MontoAPagar > 0)
            {
                if (!tarjeta.Descontar(MontoAPagar))
                {
                    return false;
                }
            }

            tarjeta.RegistrarPagoServicioBici(fechaActual);
            tarjeta.RegistrarRetiroBici(fechaActual);
            return true;
        }

        public bool DevolverBici(Tarjeta tarjeta)
        {
            if (tarjeta == null) return false;
            if (!tarjeta.TieneBiciRetirada)
            {
                return false;
            }

            DateTime fechaActual = Tiempo.Ahora;
            tarjeta.RegistrarDevolucionBici(fechaActual);
            return true;
        }
    }
}
