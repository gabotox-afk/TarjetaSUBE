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
        public const decimal MultaPorDemora = 1250m;
        public static readonly TimeSpan TiempoMaximoUso = TimeSpan.FromHours(1);
        
        public bool RetirarBici(Tarjeta tarjeta)
        {
            if (tarjeta == null || tarjeta.TieneBiciRetirada) return false;

            DateTime fechaActual = Tiempo.Ahora;
            bool yaPagoHoy = tarjeta.TieneServicioBiciPagado(fechaActual);
            decimal montoAPagar = (yaPagoHoy ? 0 : TarifaServicio) + tarjeta.MultasBiciPendientes;

            if (tarjeta.Saldo < montoAPagar || !tarjeta.Descontar(montoAPagar))
            {
                return false;
            }

            tarjeta.RegistrarPagoServicioBici(fechaActual);
            tarjeta.SaldarMultasBici();
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
            if (fechaActual - tarjeta.InicioUsoBici!.Value > TiempoMaximoUso)
            {
                tarjeta.RegistrarMultaBici(MultaPorDemora);
            }

            tarjeta.RegistrarDevolucionBici(fechaActual);
            return true;
        }

        public bool DevolverBicicleta(Tarjeta tarjeta)
        {
            return DevolverBici(tarjeta);
        }
    }
}
