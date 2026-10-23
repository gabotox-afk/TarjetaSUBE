using System;
using System.Collections.Generic;
using System.Text;
using TarjetaSube;
using NUnit.Framework;
using Microsoft.Extensions.DependencyModel;

namespace TarjetaSube.Tests
{
    public class FranquiciaHorarioTests
    {
        private RelojFalso _reloj = null!;
        private Colectivo _colectivo = null!;

        [SetUp]
        public void Setup()
        {
            _reloj = new RelojFalso();
            Tiempo.Actual = _reloj;
            _colectivo = new Colectivo("122 Verde");
        }

        [TearDown]
        public void TearDown()
        {
            Tiempo.Actual = new RelojFalso(); // Restaurar el reloj por defecto
        }

        [Test]
        public void FranquiciaPuedePagar_EnHorarioPermitido_LunesAViernes6A22()
        {
            var tipoMedio = new TarjetaTipo("Medio boleto estudiantil", 50);
            var tarjeta = new Tarjeta(10000, tipoMedio);

            // Lunes a las 06:00 (apertura)
            _reloj.FijarFecha(new DateTime(2026, 10, 19, 6, 0, 0));
            Assert.That(_colectivo.PagarCon(tarjeta), Is.Not.Null);

            // Miércoles a las 14:00 (en el medio de la franja)
            _reloj.FijarFecha(new DateTime(2026, 10, 21, 14, 0, 0));
            Assert.That(_colectivo.PagarCon(tarjeta), Is.Not.Null);

            // Viernes a las 22:00 (cierre)
            _reloj.FijarFecha(new DateTime(2026, 10, 23, 22, 0, 0));
            Assert.That(_colectivo.PagarCon(tarjeta), Is.Not.Null);
        }

        [Test]
        public void Franquicia_AntesDeLas6_RebotaYNoDescuenta()
        {
            var tipoFranquicia = new TarjetaTipo("Franquicia Completa", 50);
            var tarjeta = new Tarjeta(5000, tipoFranquicia);

            // lunes a las 05:59 :O
            _reloj.FijarFecha(new DateTime(2026, 10, 19, 5, 59, 0));

            Boleto? boleto = _colectivo.PagarCon(tarjeta);

            Assert.That(boleto, Is.Null);
            Assert.That(tarjeta.Saldo, Is.EqualTo(5000));
        }

        [Test]
        public void Franquicia_DespuesDeLas22_RebotaYNoDescuenta()
        {
            var tipoFranquicia = new TarjetaTipo("Franquicia Completa", 50);
            var tarjeta = new Tarjeta(5000, tipoFranquicia);

            // lunes a las 22:01 :O
            _reloj.FijarFecha(new DateTime(2026, 10, 19, 22, 01, 0));

            Boleto? boleto = _colectivo.PagarCon(tarjeta);

            Assert.That(boleto, Is.Null);
            Assert.That(tarjeta.Saldo, Is.EqualTo(5000));
        }

        [Test]
        public void Franquicia_FinDeSemana_Sabado_RebotaYNoDescuenta()
        {
            var tipoFranquicia = new TarjetaTipo("Franquicia Completa", 50);
            var tarjeta = new Tarjeta(5000, tipoFranquicia);

            // Sabado en horario permitido :O
            _reloj.FijarFecha(new DateTime(2026, 10, 24, 19, 01, 0));

            Boleto? boleto = _colectivo.PagarCon(tarjeta);

            Assert.That(boleto, Is.Null);
            Assert.That(tarjeta.Saldo, Is.EqualTo(5000));
        }

        [Test]
        public void Franquicia_FinDeSemana_Domingo_RebotaYNoDescuenta()
        {
            var tipoFranquicia = new TarjetaTipo("Franquicia Completa", 50);
            var tarjeta = new Tarjeta(5000, tipoFranquicia);

            // Sabado en horario permitido :O
            _reloj.FijarFecha(new DateTime(2026, 10, 25, 19, 01, 0));

            Boleto? boleto = _colectivo.PagarCon(tarjeta);

            Assert.That(boleto, Is.Null);
            Assert.That(tarjeta.Saldo, Is.EqualTo(5000));
        }

        [Test]
        public void TarjetaNormal_FinDeSemana_FueraDeHorario_SiemprePuedePagar()
        {
            var tarjeta = new Tarjeta(5000);

            // Sabado fuera de horario permitido :O
            _reloj.FijarFecha(new DateTime(2026, 10, 25, 23, 0, 0));

            Boleto? boleto = _colectivo.PagarCon(tarjeta);

            Assert.That(boleto, Is.Not.Null);
            Assert.That(tarjeta.Saldo, Is.EqualTo(5000 - 2150));
        }

        [Test]
        public void TodasLasFranquicias_AplicanLaMismaRestriccion()
        {
            // Domingo a las 10:00 de la mañana
            _reloj.FijarFecha(new DateTime(2026, 10, 25, 10, 0, 0));

            var t1 = new Tarjeta(5000, new TarjetaTipo("Medio boleto estudiantil", 50));
            var t2 = new Tarjeta(5000, new TarjetaTipo("Boleto gratuito estudiantil", 100));
            var t3 = new Tarjeta(5000, new TarjetaTipo("Franquicia completa", 100));
            var t4 = new Tarjeta(5000, new TarjetaTipo("Jubilados", 100)); // Franquicia personalizada

            Assert.That(_colectivo.PagarCon(t1), Is.Null, "Medio boleto no debe viajar en domingo");
            Assert.That(_colectivo.PagarCon(t2), Is.Null, "Boleto gratuito no debe viajar en domingo");
            Assert.That(_colectivo.PagarCon(t3), Is.Null, "Franquicia completa no debe viajar en domingo");
            Assert.That(_colectivo.PagarCon(t4), Is.Null, "Cualquier franquicia no debe viajar en domingo");
        }
    }
}
