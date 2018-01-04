using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HP_minta_2ZH
{
    class Foetel1 : Etel
    {
        private int ertek;
        private double homerseklet;
        static Random R;

        public int Ertek
        {
            get { return ertek; }
        }

        public double Homerseklet
        {
            get { return homerseklet; }
            set { homerseklet = value; }
        }

        public Foetel1(double homerseklet)
        {
            R = new Random();
            ertek = R.Next(1000, 2100);
            this.homerseklet = homerseklet;
        }
    }
}
