using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TarjetaSube.Tests
{
    internal class EstacionTests
    {
        private Estacion estacion = null!;
        private RelojFalso reloj = null!;

        [SetUp]
        public void SetUp()
        {
            estacion = new Estacion();
            reloj = new RelojFalso(new DateTime(2026, 11, 2, 8, 0, 0));
            Tiempo.Actual = reloj;
        }

        [TearDown]
        public void TearDown()
        {
            Tiempo.Actual = new RelojFalso();
        }

        [Test]
        public void RetirarBici_TarjetaNormal_CobraTarifaDiariaExactaYPermiteRetiro()
        {
            var tarjeta = new Tarjeta(5000);

            bool retirar = estacion.RetirarBici(tarjeta);

            Assert.That(retirar, Is.True);
            Assert.That(tarjeta.Saldo, Is.EqualTo(5000 - Estacion.TarifaServicio));
            Assert.That(tarjeta.TieneBiciRetirada, Is.True);
        }

        [Test]
        public void RetirarBici_FranquiciasNoTienenDescuento_CobranTarifaCompleta()
        {
            var tarjetaMedio = new Tarjeta(5000, new TarjetaTipo("Medio boleto estudiantil", 50));
            var tarjetaGratuito = new Tarjeta(5000, new TarjetaTipo("Boleto gratuito estudiantil", 100));
            bool r1 = estacion.RetirarBici(tarjetaMedio);
            bool r2 = estacion.RetirarBici(tarjetaGratuito);
            Assert.That(r1, Is.True);
            Assert.That(tarjetaMedio.Saldo, Is.EqualTo(5000 - Estacion.TarifaServicio));
            Assert.That(r2, Is.True);
            Assert.That(tarjetaGratuito.Saldo, Is.EqualTo(5000 - Estacion.TarifaServicio));
        }

        [Test]
        public void RetirarBici_SegundoRetiroEnElMismoDia_NoVuelveACobrarLaTarifa()
        {
            var tarjeta = new Tarjeta(5000);           

            Assert.That(estacion.RetirarBici(tarjeta), Is.True);
            Assert.That(tarjeta.Saldo, Is.EqualTo(5000 - Estacion.TarifaServicio));

            reloj.AvanzarMinutos(30);

            Assert.That(estacion.DevolverBici(tarjeta), Is.True);
            Assert.That(tarjeta.TieneBiciRetirada, Is.False);

            reloj.AvanzarMinutos(30);

            Assert.That(estacion.RetirarBici(tarjeta), Is.True);
            Assert.That(tarjeta.Saldo, Is.EqualTo(5000 - Estacion.TarifaServicio));
        }

        [Test]
        public void RetirarBici_AlDiaSiguiente_VuelveACobrarTarifaDiaria()
        {
            var tarjeta = new Tarjeta(5000);           

            Assert.That(estacion.RetirarBici(tarjeta), Is.True);
            Assert.That(tarjeta.Saldo, Is.EqualTo(5000 - Estacion.TarifaServicio));

            reloj.AvanzarDias(1);

            Assert.That(estacion.RetirarBici(tarjeta), Is.True);
            Assert.That(tarjeta.Saldo, Is.EqualTo(5000 - 2 * Estacion.TarifaServicio));
        }
        [Test]
        public void DevolverBici_SinHaberRetirado_DevuelveFalse()
        {
            var tarjeta = new Tarjeta(5000);

            Assert.That(estacion.DevolverBici(tarjeta), Is.False);
        }

        [Test]
        public void RetirarBici_TarjetaNull_DevuelveFalse()
        {
            Assert.That(estacion.RetirarBici(null!), Is.False);
        }

    }
}
