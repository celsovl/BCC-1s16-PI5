using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comum
{
    public class GaussianRandom
    {
        private double mu;
        private double sigma;
        private Random rnd = new Random();

        public GaussianRandom(double mu, double sigma)
        {
            this.mu = mu;
            this.sigma = sigma;
        }

        public double Next()
        {
            const double doisPi = 2 * Math.PI;
            double u1 = rnd.NextDouble();
            double u2 = rnd.NextDouble();

            double z = Math.Sqrt(-2 * Math.Log(u1)) * Math.Cos(doisPi * u2);
            return z * sigma + mu;
        }
    }
}
