using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Comum
{
    public class Aviao
    {
        private double tempoTroca;
        private bool vaiDesistir;

        private double tempoParaTrocarRota = -1;
        private readonly Vetor alvo = new Vetor(50000, 50000, 0);

        public Trajetoria Trajetoria;

        public void Iniciar()
        {
            var rnd = new Random();

            vaiDesistir = rnd.NextDouble() < 0.1;

            var anguloEntrada = rnd.NextDouble() * 2 * Math.PI;

            if (rnd.NextDouble() < 0.5)
            {
                Console.WriteLine("Trajetoria 1");
                Trajetoria = new Trajetoria1(alvo, anguloEntrada);
            }
            else
            {
                Console.WriteLine("Trajetoria 2");
                var anguloExtra = Util.Grau2Rad(Util.Rnd(rnd.NextDouble(), -75, -10, 10, 75));
                Trajetoria = new Trajetoria2(alvo, anguloEntrada, anguloExtra);
                tempoParaTrocarRota = Util.RndGauss(7.5, 1, rnd.NextDouble(), rnd.NextDouble());
            }
        }

        public Vetor PosicaoEm(double elapsed)
        {
            Vetor posicaoAtual = Trajetoria.PosicaoEm(elapsed - tempoTroca);

            if (vaiDesistir && (alvo - posicaoAtual).Mag() < 3000)
            {
                vaiDesistir = false;
                tempoTroca = elapsed;
                Console.WriteLine("Desistiu de atacar");
                Trajetoria = new Trajetoria3(posicaoAtual, alvo);
            }
            else if (tempoParaTrocarRota > -1 && elapsed > tempoParaTrocarRota)
            {
                tempoParaTrocarRota = -1;
                tempoTroca = elapsed;
                Console.WriteLine("Trajetoria 1");
                Trajetoria = new Trajetoria1(alvo, posicaoAtual);
            }

            return Trajetoria.PosicaoEm(elapsed - tempoTroca);
        }
    }
}
