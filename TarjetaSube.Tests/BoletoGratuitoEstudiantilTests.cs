using System;
using NUnit.Framework;
using TarjetaSube;

namespace TarjetaSube.Tests
{
    public class BoletoGratuitoEstudiantilTests
    {
        private Colectivo _colectivo = null!;
        private TarjetaTipo _tipoBoletoGratuito = null!;

        [SetUp]
        public void SetUp()
        {
            _colectivo = new Colectivo("122 Verde", 1580);
            _tipoBoletoGratuito = new TarjetaTipo("Boleto gratuito estudiantil", 100)
            {
                Id = TarjetaTipo.BoletoGratuitoEstudiantilId
            };
        }

        [Test]
        public void PagarCon_BoletoGratuitoEstudiantil_NoPermiteMasDeDosViajesGratuitosPorDia()
        {
            var tarjeta = new Tarjeta(5000, _tipoBoletoGratuito);

            // Primer viaje: gratuito
            Boleto? boleto1 = _colectivo.PagarCon(tarjeta);
            Assert.That(boleto1, Is.Not.Null);
            Assert.That(boleto1!.Tarifa, Is.EqualTo(0));

            // Segundo viaje: gratuito
            Boleto? boleto2 = _colectivo.PagarCon(tarjeta);
            Assert.That(boleto2, Is.Not.Null);
            Assert.That(boleto2!.Tarifa, Is.EqualTo(0));

            // Tercer viaje: ya superó el límite, NO es gratuito
            Boleto? boleto3 = _colectivo.PagarCon(tarjeta);
            Assert.That(boleto3, Is.Not.Null);
            Assert.That(boleto3!.Tarifa, Is.Not.EqualTo(0),
                "El tercer viaje no debe ser gratuito, se superó el límite de 2 viajes gratuitos por día.");
        }

        [Test]
        public void PagarCon_BoletoGratuitoEstudiantil_ViajesPosterioresAlSegundoSeCobraATarifaCompleta()
        {
            var tarjeta = new Tarjeta(10000, _tipoBoletoGratuito);

            // Consumir los 2 viajes gratuitos del día
            _colectivo.PagarCon(tarjeta);
            _colectivo.PagarCon(tarjeta);

            decimal saldoAntesDelTercerViaje = tarjeta.Saldo;

            // Tercer viaje: debe cobrarse a tarifa completa
            Boleto? boleto3 = _colectivo.PagarCon(tarjeta);
            Assert.That(boleto3, Is.Not.Null);
            Assert.That(boleto3!.Tarifa, Is.EqualTo(_colectivo.Tarifa),
                "El tercer viaje debe cobrarse con la tarifa completa.");
            Assert.That(tarjeta.Saldo, Is.EqualTo(saldoAntesDelTercerViaje - _colectivo.Tarifa),
                "El saldo debe descontarse con la tarifa completa a partir del tercer viaje.");

            // Cuarto viaje: también a tarifa completa
            decimal saldoAntesCuartoViaje = tarjeta.Saldo;
            Boleto? boleto4 = _colectivo.PagarCon(tarjeta);
            Assert.That(boleto4, Is.Not.Null);
            Assert.That(boleto4!.Tarifa, Is.EqualTo(_colectivo.Tarifa),
                "El cuarto viaje también debe cobrarse con la tarifa completa.");
            Assert.That(tarjeta.Saldo, Is.EqualTo(saldoAntesCuartoViaje - _colectivo.Tarifa));
        }
    }
}
