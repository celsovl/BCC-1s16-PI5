using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Comum
{
    public struct Vetor
    {
        public static readonly Vetor Zero = new Vetor(0, 0, 0);

        public double X;
        public double Y;
        public double Z;

        public Vetor(double x, double y, double z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public static Vetor operator -(Vetor p1)
        {
            return new Vetor(-p1.X, -p1.Y, -p1.Z);
        }

        public static Vetor operator +(Vetor p1, Vetor p2)
        {
            return new Vetor(p1.X + p2.X, p1.Y + p2.Y, p1.Z + p2.Z);
        }

        public static Vetor operator -(Vetor p1, Vetor p2)
        {
            return new Vetor(p1.X - p2.X, p1.Y - p2.Y, p1.Z - p2.Z);
        }

        public static Vetor operator *(double scale, Vetor p1)
        {
            return new Vetor(scale * p1.X, scale * p1.Y, scale * p1.Z);
        }

        public static Vetor operator *(Vetor p1, double scale)
        {
            return new Vetor(scale * p1.X, scale * p1.Y, scale * p1.Z);
        }

        public static Vetor operator /(Vetor p1, double scale)
        {
            return new Vetor(p1.X / scale, p1.Y / scale, p1.Z / scale);
        }

        public static double operator *(Vetor p1, Vetor p2)
        {
            return p1.X * p2.X + p1.Y * p2.Y + p1.Z * p2.Z;
        }

        public static bool operator ==(Vetor p1, Vetor p2)
        {
            return p1.X == p2.X &&
                p1.Y == p2.Y &&
                p1.Z == p2.Z;
        }

        public static bool operator !=(Vetor p1, Vetor p2)
        {
            return !(p1 == p2);
        }

        public override bool Equals(Object obj)
        {
            return obj is Vetor && this == (Vetor)obj;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
        }

        public double Mag()
        {
            return Math.Sqrt(this * this);
        }

        public Vetor Unit()
        {
            return this / this.Mag();
        }

        public Vetor To(double angulo, double raio)
        {
            return this + new Vetor(
                raio * Math.Cos(angulo),
                raio * Math.Sin(angulo),
                0);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "({0,10:f3}, {1,10:f3}, {2,10:f3})", X, Y, Z);
        }

        public Vetor SetX(double v)
        {
            return new Vetor(v, this.Y, this.Z);
        }

        public Vetor SetY(double v)
        {
            return new Vetor(this.X, v, this.Z);
        }

        public Vetor SetZ(double v)
        {
            return new Vetor(this.X, this.Y, v);
        }

        public Vetor RotateZ(double angle, Vetor center)
        {
            return (this - center).RotateZ(angle) + center;
        }

        public Vetor RotateZ(double angle)
        {
            return new Vetor(
                this.X * Math.Cos(angle) - this.Y * Math.Sin(angle),
                this.X * Math.Sin(angle) + this.Y * Math.Cos(angle),
                this.Z);
        }
    }
}
