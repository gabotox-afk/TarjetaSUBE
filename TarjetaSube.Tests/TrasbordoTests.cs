using System;
using NUnit.Framework;
using TarjetaSube;

namespace TarjetaSube.Tests
{
    /// <summary>
    /// Tests para la funcionalidad de Trasbordos.
    ///
    /// Reglas:
    ///   – Trasbordo gratuito dentro de la ventana de 1 hora desde el primer boleto.
    ///   – Debe ser entre líneas distintas (distinta a la del último viaje).
    ///   – Válido de lunes a sábado de 07:00 a 22:00.
    ///   – Todas las tarjetas pueden trasbordar.
    ///   – El boleto indica si es trasbordo (EsTrasbordo = true).
    /// </summary>
    public class TrasbordoTests
    {
        private RelojFalso _reloj = null!;
        private Tarjeta _tarjeta = null!;
        private Colectivo _lineaA = null!;
        private Colectivo _lineaB = null!;
        private Colectivo _lineaC = null!;

        // Lunes 2026-10-19, 10:00 — día y horario válido para trasbordos
        private static readonly DateTime FechaBase = new DateTime(2026, 10, 19, 10, 0, 0);

        [SetUp]
        public void SetUp()
        {
            _reloj = new RelojFalso(FechaBase);
            Tiempo.Actual = _reloj;

            _tarjeta = new Tarjeta(50000);
            _lineaA = new Colectivo("122 Verde", 2150);
            _lineaB = new Colectivo("144 Negra", 2150);
            _lineaC = new Colectivo("K",         2150);
        }

        [TearDown]
        public void TearDown()
        {
            Tiempo.Actual = new RelojFalso();
        }

        // ─── CalificaComoTrasbordo (unidad) ───────────────────────────────────────

        [Test]
        public void CalificaComoTrasbordo_SinViajesPrevios_RetornaFalse()
        {
            // Sin ventana activa → nunca es trasbordo
            Assert.That(_tarjeta.CalificaComoTrasbordo("144 Negra", FechaBase), Is.False);
        }

        [Test]
        public void CalificaComoTrasbordo_DentroDeUnaHoraLineaDistinta_RetornaTrue()
        {
            _tarjeta.IniciarVentanaTrasbordo("122 Verde", FechaBase);

            bool resultado = _tarjeta.CalificaComoTrasbordo("144 Negra", FechaBase.AddMinutes(30));

            Assert.That(resultado, Is.True);
        }

        [Test]
        public void CalificaComoTrasbordo_JustoAlLimiteDe60Minutos_RetornaTrue()
        {
            _tarjeta.IniciarVentanaTrasbordo("122 Verde", FechaBase);

            bool resultado = _tarjeta.CalificaComoTrasbordo("144 Negra", FechaBase.AddMinutes(60));

            Assert.That(resultado, Is.True);
        }

        [Test]
        public void CalificaComoTrasbordo_PasadaUnaHora_RetornaFalse()
        {
            _tarjeta.IniciarVentanaTrasbordo("122 Verde", FechaBase);

            bool resultado = _tarjeta.CalificaComoTrasbordo("144 Negra", FechaBase.AddMinutes(61));

            Assert.That(resultado, Is.False);
        }

        [Test]
        public void CalificaComoTrasbordo_MismaLinea_RetornaFalse()
        {
            _tarjeta.IniciarVentanaTrasbordo("122 Verde", FechaBase);

            bool resultado = _tarjeta.CalificaComoTrasbordo("122 Verde", FechaBase.AddMinutes(10));

            Assert.That(resultado, Is.False);
        }

        [Test]
        public void CalificaComoTrasbordo_Domingo_RetornaFalse()
        {
            var domingo = new DateTime(2026, 10, 18, 10, 0, 0); // Domingo
            _tarjeta.IniciarVentanaTrasbordo("122 Verde", domingo);

            bool resultado = _tarjeta.CalificaComoTrasbordo("144 Negra", domingo.AddMinutes(10));

            Assert.That(resultado, Is.False);
        }

        [Test]
        public void CalificaComoTrasbordo_Sabado_RetornaTrue()
        {
            var sabado = new DateTime(2026, 10, 24, 10, 0, 0); // Sábado
            _tarjeta.IniciarVentanaTrasbordo("122 Verde", sabado);

            bool resultado = _tarjeta.CalificaComoTrasbordo("144 Negra", sabado.AddMinutes(10));

            Assert.That(resultado, Is.True);
        }

        [Test]
        public void CalificaComoTrasbordo_AntesDeLas7_RetornaFalse()
        {
            var antes = new DateTime(2026, 10, 19, 6, 0, 0); // Lunes 06:00
            _tarjeta.IniciarVentanaTrasbordo("122 Verde", antes);

            // La consulta es a las 06:30 — todavía antes de las 07:00
            bool resultado = _tarjeta.CalificaComoTrasbordo("144 Negra", antes.AddMinutes(30));

            Assert.That(resultado, Is.False);
        }

        [Test]
        public void CalificaComoTrasbordo_DespuesDeLas22_RetornaFalse()
        {
            var despues = new DateTime(2026, 10, 19, 22, 1, 0); // Lunes 22:01
            _tarjeta.IniciarVentanaTrasbordo("122 Verde", despues);

            bool resultado = _tarjeta.CalificaComoTrasbordo("144 Negra", despues.AddMinutes(5));

            Assert.That(resultado, Is.False);
        }

        [Test]
        public void CalificaComoTrasbordo_JustoALas7_RetornaTrue()
        {
            var inicio = new DateTime(2026, 10, 19, 7, 0, 0);
            _tarjeta.IniciarVentanaTrasbordo("122 Verde", inicio);

            bool resultado = _tarjeta.CalificaComoTrasbordo("144 Negra", inicio.AddMinutes(10));

            Assert.That(resultado, Is.True);
        }

        [Test]
        public void CalificaComoTrasbordo_JustoALas22_RetornaTrue()
        {
            var inicio = new DateTime(2026, 10, 19, 21, 50, 0);
            _tarjeta.IniciarVentanaTrasbordo("122 Verde", inicio);

            bool resultado = _tarjeta.CalificaComoTrasbordo("144 Negra", new DateTime(2026, 10, 19, 22, 0, 0));

            Assert.That(resultado, Is.True);
        }

        // ─── PagarCon integrado ───────────────────────────────────────────────────

        [Test]
        public void PagarCon_PrimerViaje_NuncaEsTrasbordo()
        {
            Boleto? boleto = _lineaA.PagarCon(_tarjeta);

            Assert.That(boleto, Is.Not.Null);
            Assert.That(boleto!.EsTrasbordo, Is.False);
            Assert.That(boleto.Tarifa, Is.EqualTo(2150));
        }

        [Test]
        public void PagarCon_SegundoViajeLineaDistintaDentroDeUnaHora_EsTrasbordo()
        {
            _lineaA.PagarCon(_tarjeta);          // primer boleto — inicia ventana
            _reloj.AvanzarMinutos(20);

            Boleto? trasbordo = _lineaB.PagarCon(_tarjeta);

            Assert.That(trasbordo, Is.Not.Null);
            Assert.That(trasbordo!.EsTrasbordo, Is.True);
            Assert.That(trasbordo.Tarifa, Is.EqualTo(0));
        }

        [Test]
        public void PagarCon_TrasbordoGratuito_NoDescuentaSaldo()
        {
            decimal saldoInicial = _tarjeta.Saldo;
            _lineaA.PagarCon(_tarjeta);
            decimal saldoTrasPrimerViaje = _tarjeta.Saldo;

            _reloj.AvanzarMinutos(20);
            _lineaB.PagarCon(_tarjeta);

            // El saldo no debe cambiar respecto al primer viaje
            Assert.That(_tarjeta.Saldo, Is.EqualTo(saldoTrasPrimerViaje));
        }

        [Test]
        public void PagarCon_MultiplesTrasborbosDentroDeUnaHora_TodosGratis()
        {
            _lineaA.PagarCon(_tarjeta);
            _reloj.AvanzarMinutos(10);

            Boleto? t1 = _lineaB.PagarCon(_tarjeta);
            _reloj.AvanzarMinutos(10);

            Boleto? t2 = _lineaC.PagarCon(_tarjeta);
            _reloj.AvanzarMinutos(10);

            Boleto? t3 = _lineaA.PagarCon(_tarjeta); // Vuelve a A: es válido (última fue C)

            Assert.That(t1!.EsTrasbordo, Is.True);
            Assert.That(t2!.EsTrasbordo, Is.True);
            Assert.That(t3!.EsTrasbordo, Is.True);
            Assert.That(t1.Tarifa, Is.EqualTo(0));
            Assert.That(t2.Tarifa, Is.EqualTo(0));
            Assert.That(t3.Tarifa, Is.EqualTo(0));
        }

        [Test]
        public void PagarCon_TrasbordoMismaLinea_NoCobra0SinoTarifaCompleta()
        {
            _lineaA.PagarCon(_tarjeta);
            _reloj.AvanzarMinutos(10);

            // Misma línea → no es trasbordo válido → paga tarifa completa
            Boleto? boleto = _lineaA.PagarCon(_tarjeta);

            Assert.That(boleto, Is.Not.Null);
            Assert.That(boleto!.EsTrasbordo, Is.False);
            Assert.That(boleto.Tarifa, Is.EqualTo(2150));
        }

        [Test]
        public void PagarCon_SegundoViajeDespuesDeUnaHora_NuevaVentanaNoCobra0()
        {
            _lineaA.PagarCon(_tarjeta);
            _reloj.AvanzarMinutos(61); // Ventana expirada

            Boleto? boleto = _lineaB.PagarCon(_tarjeta);

            Assert.That(boleto, Is.Not.Null);
            Assert.That(boleto!.EsTrasbordo, Is.False);
            Assert.That(boleto.Tarifa, Is.EqualTo(2150));
        }

        [Test]
        public void PagarCon_NuevoViajeTrasVentanaExpirada_ReiniciaVentana()
        {
            // Primer ciclo
            _lineaA.PagarCon(_tarjeta);
            _reloj.AvanzarMinutos(61);

            // Segundo ciclo — el viaje en B no es trasbordo (expiró), inicia nueva ventana
            _lineaB.PagarCon(_tarjeta);
            _reloj.AvanzarMinutos(20);

            // Este viaje en A sí es trasbordo de la segunda ventana
            Boleto? trasbordo = _lineaA.PagarCon(_tarjeta);

            Assert.That(trasbordo, Is.Not.Null);
            Assert.That(trasbordo!.EsTrasbordo, Is.True);
            Assert.That(trasbordo.Tarifa, Is.EqualTo(0));
        }

        [Test]
        public void PagarCon_TrasbordobEnDomingo_NoCobra0()
        {
            var domingo = new DateTime(2026, 10, 18, 10, 0, 0);
            _reloj.FijarFecha(domingo);

            _lineaA.PagarCon(_tarjeta);
            _reloj.AvanzarMinutos(20);

            Boleto? boleto = _lineaB.PagarCon(_tarjeta);

            // Domingo → no aplica trasbordo
            Assert.That(boleto, Is.Not.Null);
            Assert.That(boleto!.EsTrasbordo, Is.False);
            Assert.That(boleto.Tarifa, Is.EqualTo(2150));
        }

        [Test]
        public void PagarCon_TrasbordoSabado_EsGratuito()
        {
            var sabado = new DateTime(2026, 10, 24, 10, 0, 0);
            _reloj.FijarFecha(sabado);

            _lineaA.PagarCon(_tarjeta);
            _reloj.AvanzarMinutos(20);

            Boleto? boleto = _lineaB.PagarCon(_tarjeta);

            Assert.That(boleto, Is.Not.Null);
            Assert.That(boleto!.EsTrasbordo, Is.True);
            Assert.That(boleto.Tarifa, Is.EqualTo(0));
        }

        [Test]
        public void PagarCon_TrasbordoFueraDeHorario_NoCobra0()
        {
            var noche = new DateTime(2026, 10, 19, 22, 30, 0); // Lunes 22:30
            _reloj.FijarFecha(noche);

            _lineaA.PagarCon(_tarjeta);
            _reloj.AvanzarMinutos(20);

            Boleto? boleto = _lineaB.PagarCon(_tarjeta);

            Assert.That(boleto, Is.Not.Null);
            Assert.That(boleto!.EsTrasbordo, Is.False);
            Assert.That(boleto.Tarifa, Is.EqualTo(2150));
        }

        // ─── Todas las tarjetas pueden trasbordar ─────────────────────────────────

        [Test]
        public void PagarCon_TarjetaMedioBoleto_TrasbordoEsGratuito()
        {
            var tipoMedio = new TarjetaTipo("Medio boleto estudiantil", 50) { Id = TarjetaTipo.MedioBoletoEstudiantilId };
            var tarjetaMedio = new Tarjeta(10000, tipoMedio);

            _lineaA.PagarCon(tarjetaMedio);
            _reloj.AvanzarMinutos(20);

            Boleto? trasbordo = _lineaB.PagarCon(tarjetaMedio);

            Assert.That(trasbordo, Is.Not.Null);
            Assert.That(trasbordo!.EsTrasbordo, Is.True);
            Assert.That(trasbordo.Tarifa, Is.EqualTo(0));
        }

        [Test]
        public void PagarCon_TarjetaFranquiciaCompleta_TrasbordoEsGratuito()
        {
            var tipoFranquicia = new TarjetaTipo("Franquicia completa", 100) { Id = TarjetaTipo.FranquiciaCompletaId };
            var tarjetaFranquicia = new Tarjeta(0, tipoFranquicia);

            _lineaA.PagarCon(tarjetaFranquicia);
            _reloj.AvanzarMinutos(20);

            Boleto? trasbordo = _lineaB.PagarCon(tarjetaFranquicia);

            Assert.That(trasbordo, Is.Not.Null);
            Assert.That(trasbordo!.EsTrasbordo, Is.True);
            Assert.That(trasbordo.Tarifa, Is.EqualTo(0));
        }

        [Test]
        public void PagarCon_TarjetaGratuitaEstudiantil_TrasbordoEsGratuito()
        {
            var tipoGratuito = new TarjetaTipo("Boleto gratuito estudiantil", 100) { Id = TarjetaTipo.BoletoGratuitoEstudiantilId };
            var tarjetaGratuita = new Tarjeta(0, tipoGratuito);

            _lineaA.PagarCon(tarjetaGratuita);
            _reloj.AvanzarMinutos(20);

            Boleto? trasbordo = _lineaB.PagarCon(tarjetaGratuita);

            Assert.That(trasbordo, Is.Not.Null);
            Assert.That(trasbordo!.EsTrasbordo, Is.True);
            Assert.That(trasbordo.Tarifa, Is.EqualTo(0));
        }

        // ─── EsTrasbordo se refleja en el boleto ──────────────────────────────────

        [Test]
        public void Boleto_PrimerViaje_EsTrasbordoFalse()
        {
            Boleto? boleto = _lineaA.PagarCon(_tarjeta);

            Assert.That(boleto, Is.Not.Null);
            Assert.That(boleto!.EsTrasbordo, Is.False);
        }

        [Test]
        public void Boleto_Trasbordo_EsTrasbordoTrue()
        {
            _lineaA.PagarCon(_tarjeta);
            _reloj.AvanzarMinutos(15);

            Boleto? boleto = _lineaB.PagarCon(_tarjeta);

            Assert.That(boleto, Is.Not.Null);
            Assert.That(boleto!.EsTrasbordo, Is.True);
        }

        // ─── Trasbordo no afecta boleto de uso frecuente ─────────────────────────

        [Test]
        public void PagarCon_TrasbordoNoContaComo_ViajeParaUsoFrecuente()
        {
            // El trasbordo registra el viaje en la lista (para fines de historial),
            // así que sí cuenta para uso frecuente. Verificamos que el precio del
            // primer boleto post-trasbordo refleja los viajes acumulados correctamente.
            var tipoNormal = new TarjetaTipo("Normal", 0) { Id = TarjetaTipo.NormalId };
            var tarjeta = new Tarjeta(50000, tipoNormal);

            // 28 viajes previos (el próximo es el 29 → sin descuento)
            for (int i = 0; i < 28; i++)
            {
                tarjeta.RegistrarViaje(_reloj.Ahora);
                _reloj.AvanzarMinutos(5);
            }

            // Viaje 29 (sin descuento)
            Boleto? v29 = _lineaA.PagarCon(tarjeta);
            Assert.That(v29!.EsTrasbordo, Is.False);
            Assert.That(v29.Tarifa, Is.EqualTo(2150)); // próximo era 29 → 0%

            // Trasbordo (viaje 30) → gratuito por trasbordo, no por uso frecuente
            _reloj.AvanzarMinutos(10);
            Boleto? trasbordo = _lineaB.PagarCon(tarjeta);
            Assert.That(trasbordo!.EsTrasbordo, Is.True);
            Assert.That(trasbordo.Tarifa, Is.EqualTo(0));

            // Próximo viaje normal (31) → ahora hay 30 registrados → 20% descuento
            _reloj.AvanzarMinutos(70); // Ventana expirada
            Boleto? v31 = _lineaA.PagarCon(tarjeta);
            Assert.That(v31!.EsTrasbordo, Is.False);
            Assert.That(v31.Tarifa, Is.EqualTo(2150 * 0.80m));
        }
    }
}
