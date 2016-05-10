using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Comum
{
    public class Util
    {
        private static Random random = new Random();

        public static double Grau2Rad(double graus)
        {
            return graus * Math.PI / 180;
        }

        public static double Rnd(double rnd, params double[] limits)
        {
            double ranges = limits.Length / 2;
            int rangeSelected = (int)Math.Floor(rnd * ranges);
            return limits[rangeSelected * 2] +
                (limits[rangeSelected * 2 + 1] - limits[rangeSelected * 2]) *
                (rnd * ranges - rangeSelected);
        }

        public static double RndGauss(double mu, double sigma)
        {
            return RndGauss(mu, sigma, random.NextDouble(), random.NextDouble());
        }

        public static double RndGauss(double mu, double sigma, double rndU1, double rndU2)
        {
            double z0 = Math.Sqrt(-2.0 * Math.Log(rndU1)) * Math.Cos(2 * Math.PI * rndU2);
            return z0 * sigma + mu;
        }
    }
}
