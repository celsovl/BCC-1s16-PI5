using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Comum
{
    public class Trajetoria1 : Trajetoria
    {
        public const double ALTITUDE = 200;
        public const double VELOCIDADEMEDIA = 240 / 3.6;

        public double AnguloEntrada;

        public Trajetoria1(Vetor centro, double anguloEntrada)
            : this(centro, centro.To(anguloEntrada, RAIO))
        {
            this.AnguloEntrada = anguloEntrada;
        }

        public Trajetoria1(Vetor centro, Vetor posicaoEntrada)
            : base(posicaoEntrada.SetZ(ALTITUDE), centro.SetZ(ALTITUDE))
        { }

        public override double Altitude
        {
            get { return ALTITUDE; }
        }

        public override double VelocidadeMedia
        {
            get { return VELOCIDADEMEDIA; }
        }
    }
}
