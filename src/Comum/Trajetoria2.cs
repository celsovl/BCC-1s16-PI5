using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Comum
{
    public class Trajetoria2 : Trajetoria
    {
        public const double ALTITUDE = 500;
        public const double VELOCIDADEMEDIA = 400 * 3.6;

        public double AnguloEntrada;
        public double AnguloExtra;

        public Trajetoria2(Vetor centro, double anguloEntrada, double anguloExtra)
            : this(centro.To(anguloEntrada, RAIO), centro + (centro - centro.To(anguloEntrada, RAIO)).RotateZ(anguloExtra))
        {
            this.AnguloEntrada = anguloEntrada;
            this.AnguloExtra = anguloExtra;
        }

        public Trajetoria2(Vetor posicaoEntrada, Vetor posicaoSaida)
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
