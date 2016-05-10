using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Comum
{
    public abstract class Trajetoria
    {
        public const double RAIO = 10000;

        protected Trajetoria(Vetor origem, Vetor destino)
        {
            this.Origem = origem;
            this.Destino = destino;
        }

        private Vetor velociade = Vetor.Zero;

        public Vetor Origem;
        public Vetor Destino;

        public abstract double Altitude { get; }
        public abstract double VelocidadeMedia { get; }

        public Vetor Velociade
        {
            get
            {
                if (velociade == Vetor.Zero)
                    this.velociade = (Destino - Origem).Unit() * VelocidadeMedia;

                return this.velociade;
            }
        }

        public Vetor PosicaoEm(double t)
        {
            return Origem + (t * Velociade);
        }
    }
}
