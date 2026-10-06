using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using TarjetaSube;

namespace TarjetaSube.Tests
{
    public class TarjetaTipoTests
    {
        [TestCase(0, 1580)]
        [TestCase(50, 790)]
        [TestCase(100, 0)]
        public void CalcularPrecio_AplicaElPorcentajeDeDescuento(decimal porcentaje, decimal precioEsperado)
        {
            var tipo = new TarjetaTipo("Tipo de prueba", porcentaje);
            Assert.That(tipo.CalcularPrecio(1580), Is.EqualTo(precioEsperado));
        }

        [Test]
        public void CalcularPrecio_TarifaNegativa_LanzaArgumentException()
        {
            var tipo = new TarjetaTipo("Normal", 0);
            Assert.Throws<ArgumentException>(() => tipo.CalcularPrecio(-1));
        }

        [TestCase(-1)]
        [TestCase(101)]
        public void Constructor_PorcentajeFueraDeRango_LanzaArgumentException(decimal porcentaje)
        {
            Assert.Throws<ArgumentException>(() => new TarjetaTipo("Invalido", porcentaje));
        }

        [TestCase("")]
        [TestCase("   ")]
        public void Constructor_NombreVacio_LanzaArgumentException(string nombre)
        {
            Assert.Throws<ArgumentException>(() => new TarjetaTipo(nombre, 0));
        }

        [Test]
        public void NuevoTipo_SeCreaSinNecesitarUnaClaseNueva()
        {
            var universitario = new TarjetaTipo("Medio boleto universitario", 50);
            var tarjeta = new Tarjeta(5000, universitario);
            Assert.That(tarjeta.CalcularPasaje(1580), Is.EqualTo(790));
        }
    }
}
