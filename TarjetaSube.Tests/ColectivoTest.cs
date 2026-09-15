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
        public void PagarCon_AliasConMinuscula_FuncionaIgual()
        {
            var colectivo = new Colectivo("115");
            var tarjeta = new Tarjeta(2000);
            Boleto? boleto = colectivo.pagarCon(tarjeta);
            Assert.That(boleto, Is.Not.Null);
            Assert.That(tarjeta.Saldo, Is.EqualTo(2000 - 1580));
        }
    }
}
