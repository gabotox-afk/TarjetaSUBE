using System;
using TarjetaSube;

namespace TarjetaSube.Tests
{
    public class RelojFalso : ITiempo
    {
        public DateTime FechaActual { get; set; } = new DateTime(2026, 10, 19, 10, 0, 0); // Lunes 10:00 AM (hábil y en horario)
        public DateTime Ahora => FechaActual;

        public RelojFalso() { }

        public RelojFalso(DateTime fechaInicial)
        {
            FechaActual = fechaInicial;
        }

        public void FijarFecha(DateTime fecha) => FechaActual = fecha;
        public void AvanzarMinutos(int minutos) => FechaActual = FechaActual.AddMinutes(minutos);
        public void AvanzarDias(int dias) => FechaActual = FechaActual.AddDays(dias);
        public void AvanzarHoras(int horas) => FechaActual = FechaActual.AddHours(horas);
    }
}
