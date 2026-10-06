using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using TarjetaSube;

namespace TarjetaSube.Tests
{
    public class ColectivoTest
    {
        [Test]
        public void PagarCon_SaldoSuficiente_EmiteBoletoYDescuentaTarifa()
        {
            var colectivo = new Colectivo("122 Verde");
            var tarjeta = new Tarjeta(5000);
            Boleto? boleto = colectivo.PagarCon(tarjeta);
            Assert.That(boleto, Is.Not.Null);
            Assert.That(boleto!.Tarifa, Is.EqualTo(1580));
            Assert.That(boleto.Linea, Is.EqualTo("122 Verde"));
            Assert.That(boleto.SaldoRestante, Is.EqualTo(5000 - 1580));
            Assert.That(tarjeta.Saldo, Is.EqualTo(3420));
        }
        [Test]
        public void PagarCon_SaldoInsuficiente_DevuelveNullYNoDescuentaSaldo()
        {
            var colectivo = new Colectivo("144 Negra");
            var tarjeta = new Tarjeta(1000);
            Boleto? boleto = colectivo.PagarCon(tarjeta);
            Assert.That(boleto, Is.Null);
            Assert.That(tarjeta.Saldo, Is.EqualTo(1000));
        }
        [Test]
        public void PagarCon_TarjetaNull_LanzaArgumentNullException()
        {
            var colectivo = new Colectivo("K");
            Assert.Throws<ArgumentNullException>(() => colectivo.PagarCon(null!));
        }
        [Test]
        public void pagarCon_SaldoSuficiente_DevuelveTrueYDescuentaTarifa()
        {
            var colectivo = new Colectivo("115");
            var tarjeta = new Tarjeta(2000);
            bool resultado = colectivo.pagarCon(tarjeta);
            Assert.That(resultado, Is.True);
            Assert.That(tarjeta.Saldo, Is.EqualTo(2000 - 1580));
        }
        [Test]
        public void pagarCon_SaldoInsuficiente_DevuelveFalseYNoDescuentaSaldo()
        {
            var colectivo = new Colectivo("115");
            var tarjeta = new Tarjeta(1000);
            bool resultado = colectivo.pagarCon(tarjeta);
            Assert.That(resultado, Is.False);
            Assert.That(tarjeta.Saldo, Is.EqualTo(1000));
        }
        [Test]
        public void pagarCon_TarjetaSeQuedaSinSaldo_DevuelveFalse()
        {
            var colectivo = new Colectivo("115");
            var tarjeta = new Tarjeta(1580);
            Assert.That(colectivo.pagarCon(tarjeta), Is.True);
            Assert.That(tarjeta.Saldo, Is.EqualTo(0));
            Assert.That(colectivo.pagarCon(tarjeta), Is.False);
            Assert.That(tarjeta.Saldo, Is.EqualTo(0));
        }
        [Test]
        public void PagarCon_VariosViajes_DescuentaCadaTarifa()
        {
            var colectivo = new Colectivo("K");
            var tarjeta = new Tarjeta(5000);
            colectivo.PagarCon(tarjeta);
            colectivo.PagarCon(tarjeta);
            colectivo.PagarCon(tarjeta);
            Assert.That(tarjeta.Saldo, Is.EqualTo(5000 - 3 * 1580));
            Assert.That(colectivo.PagarCon(tarjeta), Is.Null);
            Assert.That(tarjeta.Saldo, Is.EqualTo(5000 - 3 * 1580));
        }
    }
}
