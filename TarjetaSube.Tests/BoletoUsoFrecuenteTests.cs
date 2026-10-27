using System;
using NUnit.Framework;
using TarjetaSube;

namespace TarjetaSube.Tests
{
    /// <summary>
    /// Tests para el Boleto de Uso Frecuente.
    /// Reglas:
    ///   Viaje 1 al 29  del mes → tarifa normal (0% de descuento).
    ///   Viaje 30 al 79 del mes → 20% de descuento.
    ///   Viaje 80 en adelante   → 25% de descuento.
    ///
    /// El descuento corresponde al viaje que se va a realizar (aún no registrado).
    /// El beneficio aplica SOLO a tarjetas de tipo Normal.
    /// El conteo se resetea el primer día de cada mes.
    /// </summary>
    public class BoletoUsoFrecuenteTests
    {
        private RelojFalso _reloj = null!;
        private Colectivo _colectivo = null!;
        private TarjetaTipo _tipoNormal = null!;

        [SetUp]
        public void SetUp()
        {
            _reloj = new RelojFalso(new DateTime(2026, 10, 1, 10, 0, 0)); // Primer día del mes
            Tiempo.Actual = _reloj;

            _colectivo = new Colectivo("122 Verde", 2150);
            _tipoNormal = new TarjetaTipo("Normal", 0) { Id = TarjetaTipo.NormalId };
        }

        [TearDown]
        public void TearDown()
        {
            // Restaurar el reloj falso por defecto (Lunes 2026-10-19 10:00)
            // para no afectar otros tests que dependen del GlobalTestSetup
            Tiempo.Actual = new RelojFalso();
        }

        // ─── Helpers ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Registra <paramref name="cantidad"/> viajes en la tarjeta sin cobrar,
        /// avanzando 10 minutos entre viaje y viaje (todos en el mismo mes).
        /// </summary>
        private void RegistrarViajesPrevios(Tarjeta tarjeta, int cantidad)
        {
            for (int i = 0; i < cantidad; i++)
            {
                tarjeta.RegistrarViaje(_reloj.Ahora);
                _reloj.AvanzarMinutos(10);
            }
        }

        // ─── ContarViajesMes ──────────────────────────────────────────────────────

        [Test]
        public void ContarViajesMes_SinViajes_DevuelveCero()
        {
            var tarjeta = new Tarjeta(10000, _tipoNormal);

            Assert.That(tarjeta.ContarViajesMes(), Is.EqualTo(0));
        }

        [Test]
        public void ContarViajesMes_ConViajesEnMesActual_CuentaCorrectamente()
        {
            var tarjeta = new Tarjeta(10000, _tipoNormal);
            RegistrarViajesPrevios(tarjeta, 5);

            Assert.That(tarjeta.ContarViajesMes(), Is.EqualTo(5));
        }

        [Test]
        public void ContarViajesMes_ViajesDeOtroMesNoContabilizados()
        {
            var tarjeta = new Tarjeta(10000, _tipoNormal);

            // 3 viajes en septiembre
            _reloj.FijarFecha(new DateTime(2026, 9, 15, 10, 0, 0));
            RegistrarViajesPrevios(tarjeta, 3);

            // 2 viajes en octubre
            _reloj.FijarFecha(new DateTime(2026, 10, 5, 10, 0, 0));
            RegistrarViajesPrevios(tarjeta, 2);

            // Consultar desde octubre → solo 2
            _reloj.FijarFecha(new DateTime(2026, 10, 10, 10, 0, 0));
            Assert.That(tarjeta.ContarViajesMes(), Is.EqualTo(2));
        }

        // ─── CalcularDescuentoUsoFrecuente ────────────────────────────────────────
        // El método retorna el descuento para el PRÓXIMO viaje (el actual aún no está registrado).
        // Con N viajes registrados en el mes, el próximo viaje es el N+1.

        [Test]
        public void CalcularDescuento_Con0ViajesPrevios_ProximoEs1_RetornaCero()
        {
            var tarjeta = new Tarjeta(50000, _tipoNormal);
            // próximo = 1 → tramo 1–29 → 0%
            Assert.That(tarjeta.CalcularDescuentoUsoFrecuente(), Is.EqualTo(0));
        }

        [Test]
        public void CalcularDescuento_Con28ViajesPrevios_ProximoEs29_RetornaCero()
        {
            var tarjeta = new Tarjeta(50000, _tipoNormal);
            RegistrarViajesPrevios(tarjeta, 28);
            // próximo = 29 → tramo 1–29 → 0%
            Assert.That(tarjeta.CalcularDescuentoUsoFrecuente(), Is.EqualTo(0));
        }

        [Test]
        public void CalcularDescuento_Con29ViajesPrevios_ProximoEs30_Retorna20()
        {
            var tarjeta = new Tarjeta(50000, _tipoNormal);
            RegistrarViajesPrevios(tarjeta, 29);
            // próximo = 30 → primer viaje con 20%
            Assert.That(tarjeta.CalcularDescuentoUsoFrecuente(), Is.EqualTo(20));
        }

        [Test]
        public void CalcularDescuento_Con30ViajesPrevios_ProximoEs31_Retorna20()
        {
            var tarjeta = new Tarjeta(50000, _tipoNormal);
            RegistrarViajesPrevios(tarjeta, 30);
            // próximo = 31 → tramo 30–79 → 20%
            Assert.That(tarjeta.CalcularDescuentoUsoFrecuente(), Is.EqualTo(20));
        }

        [Test]
        public void CalcularDescuento_Con78ViajesPrevios_ProximoEs79_Retorna20()
        {
            var tarjeta = new Tarjeta(50000, _tipoNormal);
            RegistrarViajesPrevios(tarjeta, 78);
            // próximo = 79 → último viaje del tramo 30–79 → 20%
            Assert.That(tarjeta.CalcularDescuentoUsoFrecuente(), Is.EqualTo(20));
        }

        [Test]
        public void CalcularDescuento_Con79ViajesPrevios_ProximoEs80_Retorna25()
        {
            var tarjeta = new Tarjeta(50000, _tipoNormal);
            RegistrarViajesPrevios(tarjeta, 79);
            // próximo = 80 → primer viaje con 25%
            Assert.That(tarjeta.CalcularDescuentoUsoFrecuente(), Is.EqualTo(25));
        }

        [Test]
        public void CalcularDescuento_Con80ViajesPrevios_ProximoEs81_Retorna25()
        {
            var tarjeta = new Tarjeta(50000, _tipoNormal);
            RegistrarViajesPrevios(tarjeta, 80);
            // próximo = 81 → tramo 80+ → 25%
            Assert.That(tarjeta.CalcularDescuentoUsoFrecuente(), Is.EqualTo(25));
        }

        [Test]
        public void CalcularDescuento_ConMasDe80ViajesPrevios_Retorna25()
        {
            var tarjeta = new Tarjeta(50000, _tipoNormal);
            RegistrarViajesPrevios(tarjeta, 100);
            Assert.That(tarjeta.CalcularDescuentoUsoFrecuente(), Is.EqualTo(25));
        }

        // ─── CalcularPasaje con uso frecuente ─────────────────────────────────────

        [Test]
        public void CalcularPasaje_Normal_PrimerViajeDelMes_SinDescuento()
        {
            var tarjeta = new Tarjeta(50000, _tipoNormal);
            // Sin viajes previos → próximo es el 1 → 0%
            Assert.That(tarjeta.CalcularPasaje(2150), Is.EqualTo(2150));
        }

        [Test]
        public void CalcularPasaje_Normal_Con28ViajesPrevios_SinDescuento()
        {
            var tarjeta = new Tarjeta(50000, _tipoNormal);
            RegistrarViajesPrevios(tarjeta, 28);
            // próximo = 29 → 0%
            Assert.That(tarjeta.CalcularPasaje(2150), Is.EqualTo(2150));
        }

        [Test]
        public void CalcularPasaje_Normal_Con29ViajesPrevios_Descuento20Porciento()
        {
            var tarjeta = new Tarjeta(50000, _tipoNormal);
            RegistrarViajesPrevios(tarjeta, 29);
            // próximo = 30 → 20%
            Assert.That(tarjeta.CalcularPasaje(2150), Is.EqualTo(2150 * 0.80m));
        }

        [Test]
        public void CalcularPasaje_Normal_Con78ViajesPrevios_Descuento20Porciento()
        {
            var tarjeta = new Tarjeta(50000, _tipoNormal);
            RegistrarViajesPrevios(tarjeta, 78);
            // próximo = 79 → 20%
            Assert.That(tarjeta.CalcularPasaje(2150), Is.EqualTo(2150 * 0.80m));
        }

        [Test]
        public void CalcularPasaje_Normal_Con79ViajesPrevios_Descuento25Porciento()
        {
            var tarjeta = new Tarjeta(50000, _tipoNormal);
            RegistrarViajesPrevios(tarjeta, 79);
            // próximo = 80 → 25%
            Assert.That(tarjeta.CalcularPasaje(2150), Is.EqualTo(2150 * 0.75m));
        }

        [Test]
        public void CalcularPasaje_Normal_Con100ViajesPrevios_Descuento25Porciento()
        {
            var tarjeta = new Tarjeta(50000, _tipoNormal);
            RegistrarViajesPrevios(tarjeta, 100);
            // próximo = 101 → 25%
            Assert.That(tarjeta.CalcularPasaje(2150), Is.EqualTo(2150 * 0.75m));
        }

        // ─── El descuento NO aplica a tarjetas que no son Normal ─────────────────

        [Test]
        public void CalcularPasaje_MedioBoleto_Con80ViajesPrevios_SinDescuentoFrecuente()
        {
            var tipoMedio = new TarjetaTipo("Medio boleto estudiantil", 50) { Id = TarjetaTipo.MedioBoletoEstudiantilId };
            var tarjeta = new Tarjeta(50000, tipoMedio);

            // Registrar viajes en días distintos para no activar el límite de 2 viajes diarios
            for (int i = 0; i < 80; i++)
            {
                tarjeta.RegistrarViaje(_reloj.Ahora);
                _reloj.AvanzarDias(1);
            }

            // Volvemos al mes de octubre para consultar en ese mes
            var fechaOctubre = new DateTime(2026, 10, 1, 10, 0, 0);

            // El descuento de uso frecuente NO aplica al medio boleto;
            // solo aplica el 50% propio del tipo
            decimal pasaje = tarjeta.CalcularPasaje(2150, fechaOctubre);

            Assert.That(pasaje, Is.EqualTo(2150 * 0.50m));
        }

        [Test]
        public void CalcularPasaje_FranquiciaCompleta_Con80ViajesPrevios_SinDescuentoFrecuente()
        {
            var tipoFranquicia = new TarjetaTipo("Franquicia completa", 100) { Id = TarjetaTipo.FranquiciaCompletaId };
            var tarjeta = new Tarjeta(50000, tipoFranquicia);
            RegistrarViajesPrevios(tarjeta, 80);

            // La franquicia completa ya cobra 0; el uso frecuente no agrega nada
            decimal pasaje = tarjeta.CalcularPasaje(2150);

            Assert.That(pasaje, Is.EqualTo(0));
        }

        // ─── PagarCon integrado ───────────────────────────────────────────────────

        [Test]
        public void PagarCon_Normal_PrimerViajeDelMes_SinDescuento()
        {
            var tarjeta = new Tarjeta(10000, _tipoNormal);

            Boleto? boleto = _colectivo.PagarCon(tarjeta);

            Assert.That(boleto, Is.Not.Null);
            Assert.That(boleto!.Tarifa, Is.EqualTo(2150));
        }

        [Test]
        public void PagarCon_Normal_Viaje30_AplicaDescuento20Porciento()
        {
            decimal saldoInicial = 50000;
            var tarjeta = new Tarjeta(saldoInicial, _tipoNormal);
            // Con 29 viajes previos, el próximo (viaje 30) tendrá 20% de descuento
            RegistrarViajesPrevios(tarjeta, 29);

            Boleto? boleto = _colectivo.PagarCon(tarjeta);

            decimal esperado = 2150 * 0.80m;
            Assert.That(boleto, Is.Not.Null);
            Assert.That(boleto!.Tarifa, Is.EqualTo(esperado));
        }

        [Test]
        public void PagarCon_Normal_Viaje80_AplicaDescuento25Porciento()
        {
            decimal saldoInicial = 50000;
            var tarjeta = new Tarjeta(saldoInicial, _tipoNormal);
            // Con 79 viajes previos, el próximo (viaje 80) tendrá 25% de descuento
            RegistrarViajesPrevios(tarjeta, 79);

            Boleto? boleto = _colectivo.PagarCon(tarjeta);

            decimal esperado = 2150 * 0.75m;
            Assert.That(boleto, Is.Not.Null);
            Assert.That(boleto!.Tarifa, Is.EqualTo(esperado));
        }

        [Test]
        public void PagarCon_Normal_Viaje29_SinDescuento()
        {
            var tarjeta = new Tarjeta(50000, _tipoNormal);
            // Con 28 viajes previos, el próximo (viaje 29) no tiene descuento
            RegistrarViajesPrevios(tarjeta, 28);

            Boleto? boleto = _colectivo.PagarCon(tarjeta);

            Assert.That(boleto, Is.Not.Null);
            Assert.That(boleto!.Tarifa, Is.EqualTo(2150));
        }

        // ─── El conteo se reinicia con el cambio de mes ───────────────────────────

        [Test]
        public void DescuentoUsoFrecuente_ViajesDeOtroMesNoInfluyen()
        {
            var tarjeta = new Tarjeta(50000, _tipoNormal);

            // 50 viajes en octubre
            _reloj.FijarFecha(new DateTime(2026, 10, 5, 10, 0, 0));
            RegistrarViajesPrevios(tarjeta, 50);

            // Cambia el mes a noviembre → el contador vuelve a 0
            _reloj.FijarFecha(new DateTime(2026, 11, 1, 10, 0, 0));

            // En noviembre es el primer viaje (próximo = 1): sin descuento
            decimal pasaje = tarjeta.CalcularPasaje(2150, _reloj.Ahora);

            Assert.That(pasaje, Is.EqualTo(2150));
        }

        [Test]
        public void DescuentoUsoFrecuente_AlCambiarMesDescuentoSeReinicia()
        {
            var tarjeta = new Tarjeta(50000, _tipoNormal);

            // 29 viajes en octubre → próximo viaje en octubre tiene 20%
            _reloj.FijarFecha(new DateTime(2026, 10, 5, 10, 0, 0));
            RegistrarViajesPrevios(tarjeta, 29);

            Assert.That(tarjeta.CalcularDescuentoUsoFrecuente(new DateTime(2026, 10, 15)), Is.EqualTo(20));

            // Noviembre: 0 viajes → próximo es el 1 → sin descuento
            Assert.That(tarjeta.CalcularDescuentoUsoFrecuente(new DateTime(2026, 11, 1)), Is.EqualTo(0));
        }

        // ─── Transición exacta de tramos (tabla de verdad) ────────────────────────
        // viajesPrevios = viajes ya registrados en el mes.
        // descuentoEsperado = porcentaje para el PRÓXIMO viaje (viajesPrevios + 1).

        [TestCase(0,  0)]   // próximo = 1  → tramo 1–29
        [TestCase(1,  0)]   // próximo = 2  → tramo 1–29
        [TestCase(27, 0)]   // próximo = 28 → tramo 1–29
        [TestCase(28, 0)]   // próximo = 29 → tramo 1–29 (último sin descuento)
        [TestCase(29, 20)]  // próximo = 30 → tramo 30–79 (primer con 20%)
        [TestCase(50, 20)]  // próximo = 51 → tramo 30–79
        [TestCase(78, 20)]  // próximo = 79 → tramo 30–79 (último con 20%)
        [TestCase(79, 25)]  // próximo = 80 → tramo 80+  (primer con 25%)
        [TestCase(99, 25)]  // próximo = 100 → tramo 80+
        public void CalcularDescuento_SegunViajesPrevios_RetornaDescuentoDelProximoViaje(
            int viajesPrevios, decimal descuentoEsperado)
        {
            var tarjeta = new Tarjeta(50000, _tipoNormal);
            RegistrarViajesPrevios(tarjeta, viajesPrevios);

            Assert.That(tarjeta.CalcularDescuentoUsoFrecuente(), Is.EqualTo(descuentoEsperado));
        }
    }
}
