using System;
using NUnit.Framework;
using TarjetaSube;

namespace TarjetaSube.Tests
{
    public class SaldoPendienteTests
    {
        [Test]
        public void Cargar_MontoQueSupereaElMaximo_AcreditaHastaElLimiteYDejaExcedenteComoSaldoPendiente()
        {
            // Tarjeta con saldo en 50000, límite es 56000 → espacio disponible = 6000
            var tarjeta = new Tarjeta(50000);

            // Cargamos 10000: solo caben 6000 → saldo queda en 56000, pendiente = 4000
            tarjeta.Cargar(10000);

            Assert.That(tarjeta.Saldo, Is.EqualTo(Tarjeta.LimiteSaldo),
                "El saldo debe alcanzar el máximo permitido (56000).");
            Assert.That(tarjeta.SaldoPendiente, Is.EqualTo(4000),
                "El excedente (4000) debe quedar pendiente de acreditación.");
        }

        [Test]
        public void Descontar_ConSaldoPendiente_AcreditaElPendienteLuegoDelViaje()
        {
            // Tarjeta con saldo en 50000, límite es 56000 → espacio disponible = 6000
            var tarjeta = new Tarjeta(50000);

            // Cargamos 10000: quedan 56000 en saldo y 4000 pendientes
            tarjeta.Cargar(10000);
            Assert.That(tarjeta.Saldo, Is.EqualTo(56000));
            Assert.That(tarjeta.SaldoPendiente, Is.EqualTo(4000));

            // Realizamos un viaje (tarifa = 1580): el saldo baja a 54420,
            // liberando 1580 de espacio → se acreditan 1580 del pendiente
            var colectivo = new Colectivo("122 Verde", 1580);
            colectivo.PagarCon(tarjeta);

            // Después del viaje: se acreditaron los 1580 que cupieron del pendiente
            // Saldo: 54420 + 1580 = 56000 (vuelve al máximo)
            // SaldoPendiente: 4000 - 1580 = 2420
            Assert.That(tarjeta.Saldo, Is.EqualTo(56000),
                "Tras el viaje, el saldo pendiente debe acreditarse y el saldo volver al máximo.");
            Assert.That(tarjeta.SaldoPendiente, Is.EqualTo(2420),
                "El saldo pendiente restante debe ser 2420 (4000 - 1580).");
        }
    }
}
