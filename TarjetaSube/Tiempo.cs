using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TarjetaSube
{
    public class TiempoSistema : ITiempo
    {
        public DateTime Ahora => DateTime.Now;
    }

    public static class Tiempo
    {
        public static ITiempo Actual { get; set; } = new TiempoSistema();
        public static DateTime Ahora => Actual.Ahora;
    }
}
