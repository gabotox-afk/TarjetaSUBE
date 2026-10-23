using System;
using NUnit.Framework;
using TarjetaSube;

namespace TarjetaSube.Tests
{
    [SetUpFixture]
    public class GlobalTestSetup
    {
        [OneTimeSetUp]
        public void Setup()
        {
            Tiempo.Actual = new RelojFalso();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            Tiempo.Actual = new TiempoSistema();
        }
    }
}
