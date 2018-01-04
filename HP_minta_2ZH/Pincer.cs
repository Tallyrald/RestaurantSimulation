using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HP_minta_2ZH
{
    class Pincer
    {
        int lvl;
        int xp;

        public double IdoFaktor
        {
            get { return 1 - ((Lvl - 1) * 0.02); }
        }

        public int Xp
        {
            get { return xp; }
            set { xp = value; }
        }

        public int Lvl
        {
            get { return lvl; }
            set { lvl = value; }
        }

        public Pincer()
        {
            Lvl = 1;
            Xp = 0;
        }

        public void XPSzerzes(Allapot megoldottAllapot)
        {
            int elotteXP = xp;
            switch (megoldottAllapot)
            {
                case Allapot.Varakozik:
                    xp++;
                    break;
                case Allapot.Rendelne:
                    xp += 3;
                    break;
                case Allapot.Eloetel:
                    xp++;
                    break;
                case Allapot.Foetel:
                    xp++;
                    break;
                case Allapot.Fizetne:
                    xp += 2;
                    break;
                default:
                    break;

            }

            int utanaXP = xp;
            Szintlepes(elotteXP, utanaXP);
        }

        private void Szintlepes(int elotte, int utana)
        {
            if (elotte / 100 < utana / 100)
            {
                lvl++;
            }
        }
    }
}
