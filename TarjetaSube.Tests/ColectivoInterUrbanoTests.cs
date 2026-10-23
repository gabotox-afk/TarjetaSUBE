using System;
using System.Collections.Generic;
using System.Text;
using TarjetaSube;
using NUnit.Framework;

namespace TarjetaSube.Tests
{
    public class ColectivoInterUrbanoTests
    {
        [Test]
        public void ColectivoInterurbano_Tarifa_Es3000()
        {
            var colectivo = new ColectivoInterurbano("Rosario - Baigorria");

            Assert.That(colectivo.Tarifa, Is.EqualTo(3000));
            Assert.That(colectivo.Linea, Is.EqualTo("Rosario - Baigorria"));
        }

        [Test]
        public void PagarCon_TarjetaNormal_CobraTarifaInterUrbana3000()
        {
            var colectivo = new ColectivoInterurbano("Rosario - Baigorria");
            var tarjeta = new Tarjeta(3000);

            Boleto? boleto = colectivo.PagarCon(tarjeta);

            Assert.That(boleto, Is.Not.Null);
            Assert.That(boleto.Tarifa, Is.EqualTo(3000));
            Assert.That(boleto.SaldoRestante, Is.EqualTo(0));
            Assert.That(tarjeta.Saldo, Is.EqualTo(0));
        }

        [Test]
        public void PagarCon_MedioBoleto_CobraMitadDeTarifaInterurbana1500()
        {
            var colectivo = new ColectivoInterurbano("Rosario - Baigorria");
            var tipoMedio = new TarjetaTipo("Medio boleto estudiantil", 50);
            var tarjeta = new Tarjeta(5000, tipoMedio);

            Boleto? boleto = colectivo.PagarCon(tarjeta);

            Assert.That(boleto, Is.Not.Null);
            Assert.That(boleto!.Tarifa, Is.EqualTo(1500)); 
            Assert.That(tarjeta.Saldo, Is.EqualTo(5000 - 1500));
        }

        [Test]
        public void PagarCon_FranquiciaCompleta_ViajaGratis()
        {
            var colectivo = new ColectivoInterurbano("Rosario - Gálvez");
            var tipoFranquicia = new TarjetaTipo("Franquicia completa", 100);
            var tarjeta = new Tarjeta(0, tipoFranquicia);

            Boleto? boleto = colectivo.PagarCon(tarjeta);

            Assert.That(boleto, Is.Not.Null);
            Assert.That(boleto!.Tarifa, Is.EqualTo(0));
            Assert.That(tarjeta.Saldo, Is.EqualTo(0));
        }
        [Test]
        public void PagarCon_BoletoGratuitoEstudiantil_PrimerosDosGratisYTerceroCobraTarifaCompleta3000()
        {
            var colectivo = new ColectivoInterurbano("Rosario - Baigorria");
            var tipoGratuito = new TarjetaTipo("Boleto gratuito estudiantil", 100)
            {
                Id = TarjetaTipo.BoletoGratuitoEstudiantilId
            };
            var tarjeta = new Tarjeta(5000, tipoGratuito);

            Boleto? b1 = colectivo.PagarCon(tarjeta);
            Boleto? b2 = colectivo.PagarCon(tarjeta);

            Assert.That(b1!.Tarifa, Is.EqualTo(0));
            Assert.That(b2!.Tarifa, Is.EqualTo(0));

            Boleto? b3 = colectivo.PagarCon(tarjeta);

            Assert.That(b3, Is.Not.Null);
            Assert.That(b3!.Tarifa, Is.EqualTo(3000));
            Assert.That(tarjeta.Saldo, Is.EqualTo(5000 - 3000));
        }

        [Test]
        public void PagarCon_SaldoInsuficienteParaTarifaInterurbana_RebotaYNoDescuenta()
        {
            var colectivo = new ColectivoInterurbano("Rosario - Baigorria");
            var tarjeta = new Tarjeta(500);

            Boleto? boleto = colectivo.PagarCon(tarjeta);

            Assert.That(boleto, Is.Null);
            Assert.That(tarjeta.Saldo, Is.EqualTo(500));
        }
    }
}
