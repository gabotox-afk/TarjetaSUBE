using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using TarjetaSube;

namespace TarjetaSube.Tests
{
    public class TarjetaTests
    {
        [TestCase(2000)]
        [TestCase(3000)]
        [TestCase(4000)]
        [TestCase(5000)]
        [TestCase(8000)]
        [TestCase(10000)]
        [TestCase(15000)]
        [TestCase(20000)]
        [TestCase(25000)]
        [TestCase(30000)]

        public void Cargar_MontosValidos_AcreditaElSaldoCorrectamente(decimal monto)
        {
            var tarjeta = new Tarjeta();

            tarjeta.Cargar(monto);

            Assert.That(tarjeta.Saldo, Is.EqualTo(monto));
        }

        [TestCase(500)]
        [TestCase(1000)]
        [TestCase(1234)]
        [TestCase(-2000)]

        public void Cargar_MontosInvalidos_NoModificaElSaldo(decimal montoInvalido)
        {
            var tarjeta = new Tarjeta();
            tarjeta.Cargar(montoInvalido);
            Assert.That(tarjeta.Saldo, Is.EqualTo(0));
        }

        [Test]

        public void CargaInvalida_SuperaElLimiteMaximo_NoSeAcredita()
        {
            var tarjeta = new Tarjeta();
            tarjeta.Cargar(30000);
            tarjeta.Cargar(15000);
            Assert.That(tarjeta.Saldo, Is.EqualTo(30000));
        }

        [Test]

        public void Descontar_SaldoSuficiente_DescuenteYDevuelveTrue()
        {
            var tarjeta = new Tarjeta(5000);
            bool resultado = tarjeta.Descontar(1580);
            Assert.That(resultado, Is.True);
            Assert.That(tarjeta.Saldo, Is.EqualTo(3420));

        }

        [Test]

        public void Descontar_SaldoInsuficiente_NoDescuentaYDevuelveFalse()
        {
            var tarjeta = new Tarjeta(1000);
            bool resultado = tarjeta.Descontar(1580);
            Assert.That(resultado, Is.False);
            Assert.That(tarjeta.Saldo, Is.EqualTo(1000));
        }
    }
}
