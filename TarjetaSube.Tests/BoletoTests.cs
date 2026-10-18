using System;
using NUnit.Framework;
using TarjetaSube;

namespace TarjetaSube.Tests
{
    public class BoletoTests
    {
        [Test]
        public void Boleto_TarjetaNormal_InformaDatosCorrectamente()
        {
            var colectivo = new Colectivo("122 Verde");
            var tarjeta = new Tarjeta(5000); 
            tarjeta.Id = 42;

            Boleto? boleto = colectivo.PagarCon(tarjeta);

            Assert.That(boleto, Is.Not.Null);
            Assert.That(boleto!.ObtenerFecha(), Is.EqualTo(DateTime.Now).Within(TimeSpan.FromSeconds(5)));
            Assert.That(boleto.ObtenerTipoTarjeta(), Is.EqualTo("Normal"));
            Assert.That(boleto.ObtenerLinea(), Is.EqualTo("122 Verde"));
            Assert.That(boleto.ObtenerTotalAbonado(), Is.EqualTo(1580));
            Assert.That(boleto.ObtenerSaldo(), Is.EqualTo(5000 - 1580));
            Assert.That(boleto.ObtenerTarjetaId(), Is.EqualTo(42));
        }

        [Test]
        public void Boleto_MedioBoletoEstudiantil_InformaTipoYTotalAbonadoMitad()
        {
            var colectivo = new Colectivo("K");
            var tipoMedio = new TarjetaTipo("Medio boleto estudiantil", 50);
            var tarjeta = new Tarjeta(3000, tipoMedio);

            Boleto? boleto = colectivo.PagarCon(tarjeta);

            Assert.That(boleto, Is.Not.Null);
            Assert.That(boleto!.ObtenerTipoTarjeta(), Is.EqualTo("Medio boleto estudiantil"));
            Assert.That(boleto.ObtenerTotalAbonado(), Is.EqualTo(790));
            Assert.That(boleto.ObtenerSaldo(), Is.EqualTo(3000 - 790));
        }

        [Test]
        public void Boleto_FranquiciaCompleta_InformaTotalAbonadoCero()
        {
            var colectivo = new Colectivo("115");
            var tipoFranquicia = new TarjetaTipo("Franquicia completa", 100);
            var tarjeta = new Tarjeta(0, tipoFranquicia);

            Boleto? boleto = colectivo.PagarCon(tarjeta);

            Assert.That(boleto, Is.Not.Null);
            Assert.That(boleto!.ObtenerTipoTarjeta(), Is.EqualTo("Franquicia completa"));
            Assert.That(boleto.ObtenerTotalAbonado(), Is.EqualTo(0));
            Assert.That(boleto.ObtenerSaldo(), Is.EqualTo(0));
        }

        [Test]
        public void Boleto_BoletoGratuitoEstudiantil_InformaTotalAbonadoCero()
        {
            var colectivo = new Colectivo("144 Negra");
            var tipoGratuito = new TarjetaTipo("Boleto gratuito estudiantil", 100);
            var tarjeta = new Tarjeta(0, tipoGratuito);

            Boleto? boleto = colectivo.PagarCon(tarjeta);

            Assert.That(boleto, Is.Not.Null);
            Assert.That(boleto!.ObtenerTipoTarjeta(), Is.EqualTo("Boleto gratuito estudiantil"));
            Assert.That(boleto.ObtenerTotalAbonado(), Is.EqualTo(0));
        }

        [Test]
        public void Boleto_ConSaldoNegativoPrevio_InformaTotalAbonadoSuperiorATarifaNormal()
        {
            var colectivo = new Colectivo("122 Verde");
            var tarjeta = new Tarjeta(); 

            colectivo.PagarCon(tarjeta);
            Assert.That(tarjeta.Saldo, Is.EqualTo(-1580));


            tarjeta.Cargar(2000);
            Assert.That(tarjeta.Saldo, Is.EqualTo(420));

            var tarjetaConDeuda = new Tarjeta(1000);
       
            colectivo.PagarCon(tarjetaConDeuda);
            Assert.That(tarjetaConDeuda.Saldo, Is.EqualTo(-580));


            var tarjetaConDeudaChica = new Tarjeta(1380);
            colectivo.PagarCon(tarjetaConDeudaChica); 
            Assert.That(tarjetaConDeudaChica.Saldo, Is.EqualTo(-200));

            Boleto? segundoBoleto = colectivo.PagarCon(tarjetaConDeudaChica);

            Assert.That(segundoBoleto, Is.Not.Null);
            Assert.That(segundoBoleto!.ObtenerTotalAbonado(), Is.EqualTo(1580 + 200));
            Assert.That(segundoBoleto.ObtenerTotalAbonado(), Is.GreaterThan(1580));
        }
    }
}