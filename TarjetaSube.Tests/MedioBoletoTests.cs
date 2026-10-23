using System;
using NUnit.Framework;
using TarjetaSube;

namespace TarjetaSube.Tests
{
    // Reloj simulado para avanzar el tiempo en los tests
    public class RelojFalso : ITiempo
    {
        public DateTime FechaActual { get; set; } = new DateTime(2026, 10, 18, 8, 0, 0);
        public DateTime Ahora => FechaActual;

        public void AvanzarMinutos(int minutos) => FechaActual = FechaActual.AddMinutes(minutos);
        public void AvanzarDias(int dias) => FechaActual = FechaActual.AddDays(dias);
    }

    public class MedioBoletoTests
    {
        private RelojFalso _reloj = null!;

        [SetUp]
        public void Setup()
        {
            _reloj = new RelojFalso();
            Tiempo.Actual = _reloj; // Usamos el reloj falso para los tests
        }

        [TearDown]
        public void TearDown()
        {
            Tiempo.Actual = new TiempoSistema(); // Restauramos el reloj real al terminar
        }

        // Test 1: No permitir viajar si pasaron menos de 5 minutos
        [Test]
        public void PagarCon_MedioBoleto_MenosDe5Minutos_RechazaElViaje()
        {
            var colectivo = new Colectivo("122 Verde");
            var tipoMedio = new TarjetaTipo("Medio boleto estudiantil", 50);
            var tarjeta = new Tarjeta(5000, tipoMedio);

            // 1er viaje a las 8:00 -> Exitoso
            Boleto? primerViaje = colectivo.PagarCon(tarjeta);
            Assert.That(primerViaje, Is.Not.Null);
            Assert.That(primerViaje!.Tarifa, Is.EqualTo(790));

            // Adelantamos 3 minutos (8:03)
            _reloj.AvanzarMinutos(3);

            // 2do viaje -> Debe rebotar porque no pasaron 5 minutos
            Boleto? intentoRechazado = colectivo.PagarCon(tarjeta);
            Assert.That(intentoRechazado, Is.Null);
            Assert.That(tarjeta.Saldo, Is.EqualTo(5000 - 790)); // El saldo no se tocó
        }

        // Test 2: Permitir viajar si ya pasaron 5 minutos o más
        [Test]
        public void PagarCon_MedioBoleto_Pasados5Minutos_PermiteElViaje()
        {
            var colectivo = new Colectivo("122 Verde");
            var tipoMedio = new TarjetaTipo("Medio boleto estudiantil", 50);
            var tarjeta = new Tarjeta(5000, tipoMedio);

            // 1er viaje a las 8:00
            colectivo.PagarCon(tarjeta);

            // Adelantamos 5 minutos exactos (8:05)
            _reloj.AvanzarMinutos(5);

            // 2do viaje -> Debe permitirse
            Boleto? segundoViaje = colectivo.PagarCon(tarjeta);
            Assert.That(segundoViaje, Is.Not.Null);
            Assert.That(segundoViaje!.Tarifa, Is.EqualTo(790));
            Assert.That(tarjeta.Saldo, Is.EqualTo(5000 - 790 * 2));
        }

        // Test 3: Máximo 2 viajes por día con descuento, el 3ro cobra tarifa normal
        [Test]
        public void PagarCon_MedioBoleto_TercerViajeEnElMismoDia_CobraTarifaNormal()
        {
            var colectivo = new Colectivo("122 Verde");
            var tipoMedio = new TarjetaTipo("Medio boleto estudiantil", 50);
            var tarjeta = new Tarjeta(10000, tipoMedio);

            // 1er viaje (8:00): $790
            Boleto? b1 = colectivo.PagarCon(tarjeta);
            Assert.That(b1!.Tarifa, Is.EqualTo(790));

            // 2do viaje (8:10): $790
            _reloj.AvanzarMinutos(10);
            Boleto? b2 = colectivo.PagarCon(tarjeta);
            Assert.That(b2!.Tarifa, Is.EqualTo(790));

            // 3er viaje (8:30): ya no hay descuento, cobra tarifa normal $1580
            _reloj.AvanzarMinutos(20);
            Boleto? b3 = colectivo.PagarCon(tarjeta);
            Assert.That(b3, Is.Not.Null);
            Assert.That(b3!.Tarifa, Is.EqualTo(1580)); // Tarifa normal
        }

        // Test 4: Al día siguiente vuelve a tener 2 viajes con medio boleto
        [Test]
        public void PagarCon_MedioBoleto_AlDiaSiguiente_VuelveATenerDescuento()
        {
            var colectivo = new Colectivo("122 Verde");
            var tipoMedio = new TarjetaTipo("Medio boleto estudiantil", 50);
            var tarjeta = new Tarjeta(10000, tipoMedio);

            // Hacemos 2 viajes hoy
            colectivo.PagarCon(tarjeta);
            _reloj.AvanzarMinutos(10);
            colectivo.PagarCon(tarjeta);

            // Avanzamos al día siguiente
            _reloj.AvanzarDias(1);

            // 1er viaje del nuevo día: vuelve a costar la mitad ($790)
            Boleto? bNuevoDia = colectivo.PagarCon(tarjeta);
            Assert.That(bNuevoDia!.Tarifa, Is.EqualTo(790));
        }
    }
}