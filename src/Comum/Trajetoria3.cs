using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Comum
{
    public class Trajetoria3 : Trajetoria
    {
        public const double ALTITUDE = 1200;
        public const double VELOCIDADEMEDIA = 750 * 3.6;

        public Trajetoria3(Vetor posicaoEntrada, Vetor posicaoSaida)
            : base(posicaoEntrada.SetZ(ALTITUDE), posicaoSaida.SetZ(ALTITUDE))
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
