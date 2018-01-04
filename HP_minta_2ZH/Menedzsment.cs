using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HP_minta_2ZH
{
    class Menedzsment
    {
        const int ASZTALSZAM = 20;
        const int MAX_NAPIVENDEG = 100;
        const int PINCER_SZAM = 2;
        const int PINCER_MAXINTERVALLUM = 3000;
        const int VENDEGERKEZES_MAXINTERVALLUM = 8000;
        const int PERC_INTERVALLUM = 1000;
        public const double GSP = 0.3; // Globális szimuláció sebesség (Global Simulation Pace)
        
        int napiVendegek;
        int napiBevetel;
        int teljesBevetel;

        bool ettermiNapVege;
        bool erkezoSzimulacioVege;

        DateTime SzimulacioIdo;
        static Random R;

        Queue<Vendeg> Erkezok;
        Vendeg[] asztalok;
        List<Pincer> pincerek;
        List<Task> pincerMunkak;

        private object asztalLock = new object();
        private object queueLock = new object();
        private object varakozoLock = new object();

        Task percSzamlalo;
        Task vevoSpawner;
        Task konzolKezelo;
        
        public Menedzsment()
        {
            asztalok = new Vendeg[ASZTALSZAM];
            ettermiNapVege = false;
            Erkezok = new Queue<Vendeg>();
            erkezoSzimulacioVege = false;
            konzolKezelo = new Task(KonzolKiiras, TaskCreationOptions.LongRunning);
            percSzamlalo = new Task(PercSzimulalas, TaskCreationOptions.LongRunning);
            napiVendegek = 0;
            pincerek = new List<Pincer>();
            pincerMunkak = new List<Task>();
            R = new Random();
            SzimulacioIdo = new DateTime(2017, 01, 01, 10, 00, 00);
            vevoSpawner = new Task(Spawner, TaskCreationOptions.LongRunning);

            for (int i = 0; i < PINCER_SZAM; i++)
            {
                pincerek.Add(new Pincer());
            }
        }

        public void StartSimulation()
        {
            vevoSpawner.Start();

            for (int i = 0; i < PINCER_SZAM; i++)
            {
                int counter = i;
                pincerMunkak.Add(new Task((x => Pincer(pincerek[counter])), TaskCreationOptions.LongRunning));
                pincerMunkak[i].Start();
            }

            percSzamlalo.Start();

            new Task(KonzolKiiras, TaskCreationOptions.LongRunning).Start();
            Console.ReadLine();
        }

        private void PercSzimulalas()
        {
            while(true)
            {
                Thread.Sleep((int)Math.Ceiling(PERC_INTERVALLUM * GSP));
                lock (asztalLock)
                {
                    Parallel.For(0, ASZTALSZAM, i =>
                    {
                        if (asztalok[i] != null && asztalok[i].GetType() == typeof(Vendeg))
                        {
                            if (asztalok[i].Allapot == Allapot.Tavozott)
                            {
                                asztalok[i] = null;
                            }
                            else
                            {
                                asztalok[i].UtolsoModositas++;
                            }
                        }
                    });
                }

                SzimulacioIdo = SzimulacioIdo.AddMinutes(1);
            }
        }

        private void Pincer(Pincer pincer)
        {
            while (!ettermiNapVege)
            {
                Thread.Sleep(R.Next((int)Math.Ceiling(1000 * GSP), (int)Math.Ceiling(PINCER_MAXINTERVALLUM * pincer.IdoFaktor * GSP)));

                lock (queueLock)
                {
                    if (Erkezok.Count > 0)
                    {
                        try
                        {
                            lock (asztalLock)
                            {
                                int uresAsztalIdx = UresAsztal();
                                asztalok[uresAsztalIdx] = Erkezok.Dequeue();
                                if (Cselekszik(asztalok[uresAsztalIdx]))
                                {
                                    pincer.XPSzerzes(Allapot.Varakozik);
                                }
                            }

                            continue;
                        }
                        catch (NincsUresAsztalException)
                        {
                        }
                    }
                }

                if (asztalok.Any())
                {
                    try
                    {
                        lock (varakozoLock)
                        {
                            int legregebbi = RegenVarakozo();
                            lock (asztalLock)
                            {
                                Allapot megoldandoAllapot = asztalok[legregebbi].Allapot;
                                if (Cselekszik(asztalok[legregebbi]))
                                {
                                    if (megoldandoAllapot == Allapot.Fizetne)
                                    {
                                        napiBevetel += asztalok[legregebbi].Rendelesek.Sum(x => x.Ertek);
                                    }
                                    pincer.XPSzerzes(megoldandoAllapot);
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {

                    }
                }

                if (erkezoSzimulacioVege && !asztalok.Any() && Erkezok.Count == 0)
                {
                    ettermiNapVege = true;
                }
            }
        }

        private int RegenVarakozo()
        {
            int max = -1;
            int idx = -1;
            lock (asztalLock)
            {
                for (int i = 0; i < ASZTALSZAM; i++)
                {
                    if (asztalok[i] != null && asztalok[i].GetType() == typeof(Vendeg) && asztalok[i].Leptetheto && asztalok[i].Allapot != Allapot.Tavozott && asztalok[i].UtolsoModositas > max)
                    {
                        max = asztalok[i].UtolsoModositas;
                        idx = i;
                    }
                }
            }

            return idx;
        }

        private void Spawner()
        {
            while (SzimulacioIdo.Hour < 22)
            {
                lock (queueLock)
                {
                    Erkezok.Enqueue(new Vendeg(napiVendegek));
                    Interlocked.Add(ref napiVendegek, 1);
                }
                Thread.Sleep(R.Next((int)Math.Ceiling(1000 * GSP), (int)Math.Ceiling(VENDEGERKEZES_MAXINTERVALLUM * GSP)));
            }

            Erkezok = new Queue<Vendeg>();
            erkezoSzimulacioVege = true;
        }

        private bool Cselekszik(Vendeg vendeg)
        {
            return vendeg.KovAllapot();
        }

        private int UresAsztal()
        {
            int i = 0;
            while (i < ASZTALSZAM && asztalok[i] != null)
            {
                i++;
            }

            if (i < ASZTALSZAM)
            {
                return i;
            }
            else
            {
                throw new NincsUresAsztalException();
            }
        }

        private void KonzolKiiras()
        {
            while (true)
            {
                Thread.Sleep(100);
                Console.CursorVisible = false;
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.SetCursorPosition(0, 0);
                ClearCurrentConsoleLine();
                Console.Write("Jelenlegi idő: " + SzimulacioIdo.ToShortTimeString());
                Console.SetCursorPosition(0, 1);
                ClearCurrentConsoleLine();
                Console.Write("Várakozó vendégek: {0}", Erkezok.Count());
                Console.SetCursorPosition(0, 2);
                ClearCurrentConsoleLine();
                Console.Write("Pincérek: {0}", pincerMunkak.Count());
                Console.SetCursorPosition(0, 3);
                ClearCurrentConsoleLine();
                Console.Write("Napi bevétel: {0} Ft", napiBevetel);

                for (int i = 0; i < PINCER_SZAM; i++)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.SetCursorPosition((int)Math.Floor(Console.WindowWidth * 0.6), i);
                    Console.Write("{0}. pincér: Szint: {1}, Szerzett XP: {2}", i+1, pincerek[i].Lvl, pincerek[i].Xp);
                }

                lock (asztalLock)
                {
                    for (int i = 0; i < ASZTALSZAM; i++)
                    {
                        Console.SetCursorPosition(0, i + 4);
                        ClearCurrentConsoleLine();
                        string segedszam = string.Empty;

                        if (i < 9)
                        {
                            segedszam = "00";
                        }
                        else if (i < 99)
                        {
                            segedszam = "0";
                        }

                        if (asztalok[i] != null && asztalok[i].GetType() == typeof(Vendeg))
                        {
                            string elfoglaltsag = string.Empty;
                            if (asztalok[i].Leptetheto)
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                elfoglaltsag = "Pincérre vár " + asztalok[i].UtolsoModositas + " perce";
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                elfoglaltsag = "Elfoglalt";
                            }



                            Console.WriteLine("{0}{1}: #{2} ({3}), {4}", segedszam, i+1, asztalok[i].Sorszam, GetDescription(asztalok[i].Allapot), elfoglaltsag);
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("{0}{1}: Szabad asztal", segedszam, i+1);
                        }
                    }
                }
            }
        }

        public static void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }

        static string GetDescription(Enum en)
        {
            Type type = en.GetType();
            MemberInfo[] memInfo = type.GetMember(en.ToString());

            if (memInfo != null && memInfo.Length > 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attrs != null && attrs.Length > 0)
                {
                    return ((DescriptionAttribute)attrs[0]).Text;
                }
            }

            return en.ToString();
        }
    }
}
