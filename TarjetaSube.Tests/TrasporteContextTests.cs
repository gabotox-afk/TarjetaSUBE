using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using TarjetaSube;

namespace TarjetaSube.Tests
{
    public class TrasporteContextTests
    {
        private TransporteContext _db = null!;
        [SetUp]
        public void Setup()
        {
            var opciones = new DbContextOptionsBuilder<TransporteContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _db = new TransporteContext(opciones);
        }
        [TearDown]
        public void TearDown()
        {
            _db.Dispose();
        }
        [Test]
        public void GuardarYRecuperarTarjeta_PersisteCorrectamente()
        {
            var tarjeta = new Tarjeta();
            tarjeta.Cargar(5000);
            _db.Tarjetas.Add(tarjeta);
            _db.SaveChanges();
            var tarjetaGuardada = _db.Tarjetas.Find(tarjeta.Id);
            Assert.That(tarjetaGuardada, Is.Not.Null);
            Assert.That(tarjetaGuardada!.Saldo, Is.EqualTo(5000));
        }
        [Test]
        public void GuardarYRecuperarColectivo_PersisteCorrectamente()
        {
            var colectivo = new Colectivo("144 Negra", 1580);
            _db.Colectivos.Add(colectivo);
            _db.SaveChanges();
            var colectivoGuardado = _db.Colectivos.Find(colectivo.Id);
            Assert.That(colectivoGuardado, Is.Not.Null);
            Assert.That(colectivoGuardado!.Linea, Is.EqualTo("144 Negra"));
            Assert.That(colectivoGuardado.Tarifa, Is.EqualTo(1580));
        }
        [Test]
        public void GuardarYRecuperarBoleto_PersisteCorrectamente()
        {
            var colectivo = new Colectivo("122 Verde", 1580);
            var tarjeta = new Tarjeta(5000);
            Boleto? boleto = colectivo.PagarCon(tarjeta);
            _db.Boletos.Add(boleto!);
            _db.SaveChanges();
            var boletoGuardado = _db.Boletos.Find(boleto!.Id);
            Assert.That(boletoGuardado, Is.Not.Null);
            Assert.That(boletoGuardado!.Linea, Is.EqualTo("122 Verde"));
            Assert.That(boletoGuardado.Tarifa, Is.EqualTo(1580));
            Assert.That(boletoGuardado.SaldoRestante, Is.EqualTo(3420));
        }
    }
}
