using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Comum;
using System.Globalization;

namespace Testes
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestAviao()
        {
            Aviao aviao = new Aviao();
            aviao.Iniciar();

            double t = 0;
            while (t < 15)
            {
                Console.WriteLine(
                    "{0:f2} ==> Posicao: {1}, {2}",
                    t,
                    aviao.PosicaoEm(t),
                    aviao.Trajetoria.GetType().Name);

                t += 0.5;
            }
        }

        [TestMethod]
        public void TestAviaoTrajetoria1()
        {
            var posicaoRadar = new Vetor(50000, 50000, 0);

            var aviao = new Trajetoria1(posicaoRadar, Math.PI * 4 / 4);
            Assert.IsTrue(10000 - (aviao.Origem - posicaoRadar).SetZ(0).Mag() < 0.000001);

            Console.WriteLine("Angulo: {0}", aviao.AnguloEntrada * 180 / Math.PI);

            for (double i = 10; i < 12; i += 0.01)
                Console.WriteLine("Posicao em {0,10:f2}: {1,10:f2}  ::: {2,10:f2}", i, aviao.PosicaoEm(i), (aviao.PosicaoEm(i).SetZ(0) - posicaoRadar).Mag());
        }

        [TestMethod]
        public void TestAviaoTrajetoria2()
        {
            var posicaoRadar = new Vetor(50000, 50000, 0);

            double anguloEntrada = Math.PI * 2 / 4;
            double anguloExtra = 10 * Math.PI / 180d;

            var aviao = new Trajetoria2(posicaoRadar, anguloEntrada, anguloExtra);
            Assert.IsTrue(10000 - (aviao.Origem - posicaoRadar).SetZ(0).Mag() < 0.000001);

            Console.WriteLine("Angulo: {0}", aviao.AnguloEntrada * 180 / Math.PI);
            Console.WriteLine("Angulo Extra: {0}", aviao.AnguloExtra * 180 / Math.PI);

            for (double i = 0; i < 12; i += 0.5)
                Console.WriteLine("Posicao em {0,10:f2}: {1,10:f2}  ::: {2,10:f2}", i, aviao.PosicaoEm(i), (aviao.PosicaoEm(i).SetZ(0) - posicaoRadar).Mag());
        }

        [TestMethod]
        public void TestAviaoTrajetoria2_2()
        {
            var posicaoRadar = new Vetor(50000, 50000, 0);

            double anguloEntrada = Math.PI * 2 / 4;
            double extra = Util.Rnd(new Random().NextDouble(), -75, -10, 10, 75);

            var aviao = new Trajetoria2(posicaoRadar, anguloEntrada, Util.Grau2Rad(extra));
            Assert.IsTrue(10000 - (aviao.Origem - posicaoRadar).SetZ(0).Mag() < 0.000001);

            Console.WriteLine("Angulo: {0}", aviao.AnguloEntrada * 180 / Math.PI);
            Console.WriteLine("Angulo Extra: {0}", aviao.AnguloExtra * 180 / Math.PI);

            for (double i = 0; i < 12; i += 0.5)
                Console.WriteLine("Posicao em {0,10:f2}: {1,10:f2}  ::: {2,10:f2}", i, aviao.PosicaoEm(i), (aviao.PosicaoEm(i).SetZ(0) - posicaoRadar).Mag());
        }

        [TestMethod]
        public void TestUtilRnd()
        {
            Assert.AreEqual(-75, Util.Rnd(0, -75, -10, 10, 75));
            Assert.AreEqual(-42.5, Util.Rnd(0.25, -75, -10, 10, 75));
            Assert.AreEqual(10, Util.Rnd(0.5, -75, -10, 10, 75));
            Assert.AreEqual(42.5, Util.Rnd(0.75, -75, -10, 10, 75));
            Assert.IsTrue(75 - Util.Rnd(0.9999999999, -75, -10, 10, 75) < 0.0001);
        }

        [TestMethod]
        public void TestUtilGauss()
        {
            List<double> vals = new List<double>();
            for (int i = 0; i < 30; i++)
            {
                vals.Add(Util.RndGauss(7.5, 1));
            }

            vals.Sort();
            Console.WriteLine(string.Join(", ", vals.Select(x => x.ToString("f3", CultureInfo.InvariantCulture)).ToArray()));
        }
    }
}
