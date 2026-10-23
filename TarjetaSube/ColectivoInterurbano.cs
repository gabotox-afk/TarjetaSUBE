using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TarjetaSube
{
    public class ColectivoInterurbano : Colectivo
    {
        public const decimal tarifaInterurbana = 3000;

        public ColectivoInterurbano()
        {
            Tarifa = tarifaInterurbana;
        }

        public ColectivoInterurbano(string linea, decimal tarifa = tarifaInterurbana) : base(linea, tarifa)
        {

        }
    }
}
