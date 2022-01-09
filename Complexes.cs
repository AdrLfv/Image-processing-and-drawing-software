using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEST_lire_ecrire_image
{
    class Complexes
    {
        public double a;    //partie réelle
        public double b;    //partie imaginaire

        public Complexes(double a, double b)
        {
            this.a = a;
            this.b = b;
        }

        public void Square()
        {
            double temp = (a * a) - (b * b);
            b = 2.0 * a * b;
            a = temp;
        }
        public double Module()
        {
            return Math.Sqrt((a * a) + (b * b));
        }
        public void Inverse()
        {
            a = a / Module();
            b = -b / Module();
        }
        public void Sub(Complexes c)
        {
            a -= c.a;
            b -= c.b;
        }
        public void Add(Complexes c)
        {
            a += c.a;
            b += c.b;
        }
        public void Conjugue()
        {
            b = -b;
        }
        public double[] Tan()
        {
            a = Math.Sin(2 * a) / (Math.Cos(2 * a) + Math.Cosh(2 * b));
            b = Math.Sinh(2 * b) / (Math.Cos(2 * a) + Math.Cosh(2 * b));
            double[] tan = { a, b };
            return tan;
        }
    }
}
