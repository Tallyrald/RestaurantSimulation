using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HP_minta_2ZH
{
    public enum Allapot
    {
        [Description("Asztalra várakozik")]
        Varakozik,
        [Description("Rendelni szeretne")]
        Rendelne,
        [Description("Előételt fogyaszt")]
        Eloetel,
        [Description("Főételt fogyaszt")]
        Foetel,
        [Description("Fizetni szeretne")]
        Fizetne,
        [Description("Távozott")]
        Tavozott
    }

    class Vendeg
    {
        static Random R = new Random();
        int sorszam;
        int utolsoModositas;
        Allapot allapot;
        bool leptetheto;
        List<Etel> rendelesek;

        public bool Leptetheto
        {
            get { return leptetheto; }
            set { leptetheto = value; }
        }

        public Allapot Allapot
        {
            get { return allapot; }
            set { allapot = value; }
        }

        public int UtolsoModositas
        {
            get { return utolsoModositas; }
            set { utolsoModositas = value; }
        }

        public int Sorszam
        {
            get { return sorszam; }
            set { sorszam = value; }
        }

        internal List<Etel> Rendelesek
        {
            get { return rendelesek; }
            set { rendelesek = value; }
        }

        public Vendeg(int sorszam)
        {
            this.sorszam = sorszam;
            UtolsoModositas = 0;
            Allapot = Allapot.Varakozik;
            Leptetheto = true;
            rendelesek = new List<Etel>();
        }

        public bool KovAllapot()
        {
            if (Leptetheto)
            {
                Allapot++;
                Leptetheto = false;
                new Task(Gondolkodik).Start();
                return true;
            }
            else
            {
                return false;
            }
        }

        private void Gondolkodik()
        {
            if (Allapot != Allapot.Tavozott)
            {
                switch (Allapot)
                {
                    case Allapot.Rendelne:
                        Thread.Sleep(R.Next((int)Math.Ceiling(1000 * Menedzsment.GSP), (int)Math.Ceiling(2000 * Menedzsment.GSP)));
                        break;
                    case Allapot.Eloetel:
                        Rendel();
                        Thread.Sleep(R.Next((int)Math.Ceiling(4000 * Menedzsment.GSP), (int)Math.Ceiling(10000 * Menedzsment.GSP)));
                        break;
                    case Allapot.Foetel:
                        Rendel();
                        Thread.Sleep(R.Next((int)Math.Ceiling(10000 * Menedzsment.GSP), (int)Math.Ceiling(20000 * Menedzsment.GSP)));
                        break;
                    case Allapot.Fizetne:
                        Thread.Sleep(R.Next((int)Math.Ceiling(1000 * Menedzsment.GSP), (int)Math.Ceiling(12000 * Menedzsment.GSP)));
                        break;
                    default:
                        Thread.Sleep(R.Next((int)Math.Ceiling(1000 * Menedzsment.GSP), (int)Math.Ceiling(2000 * Menedzsment.GSP)));
                        break;
                }

                UtolsoModositas = 0;
                Leptetheto = true;
            }
        }

        public Etel Rendel()
        {
            switch (Allapot)
            {
                case Allapot.Eloetel:
                    rendelesek.Add(new Eloetel1((int)Math.Ceiling((R.Next(1, 2) /2 ) * Menedzsment.GSP)));
                    break;
                case Allapot.Foetel:
                    rendelesek.Add(new Foetel1((int)Math.Ceiling((R.Next(1, 2) / 2) * Menedzsment.GSP)));
                    break;
                default:
                    break;
            }

            return rendelesek.Last();
        }
    }
}
