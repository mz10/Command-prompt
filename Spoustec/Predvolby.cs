using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Linq;

namespace Spoustec {
    class Predvolby {
        static Window1 mw = ((Window1)Application.Current.Windows[0]);

        public static bool NactiProgramy() {
            try {
                mw.comboBox1.Items.Clear();
                mw.seznam_programu.Clear();
                foreach (string radek in File.ReadAllLines(mw.prog_cesta)) {
                    string nazev = Path.GetFileNameWithoutExtension(radek);
                    mw.comboBox1.Items.Add(nazev);
                    mw.seznam_programu.Add(radek);
                }
            }
            catch { return false; }
            return true;
        }

        public static bool NactiNastaveni() {
            mw.tbPrikaz.Visibility = Visibility.Hidden;

            try { Directory.CreateDirectory(Window1.appdata); } catch { }

            try {
                using (StreamReader sr = new StreamReader(mw.prog_nastaveni)) {
                    string vc = sr.ReadLine();
                    mw.vychozi_cesta = vc == "" ? Window1.profil : vc;
                    mw.richTextBox1.FontFamily = new FontFamily(sr.ReadLine());
                    mw.richTextBox1.FontSize = Funkce.ynt(sr.ReadLine(),14);
                    mw.richTextBox1.FontWeight = sr.ReadLine() == "Bold" ? FontWeights.Bold : FontWeights.Normal;
                    mw.richTextBox1.FontStyle = sr.ReadLine() == "Italic" ? FontStyles.Italic : FontStyles.Normal;
                    mw.kodovani = Funkce.ynt(sr.ReadLine(),mw.kodovani);
                    mw.historie = Funkce.ynt(sr.ReadLine(),20);
                    string[] bpoz = sr.ReadLine().Split(',');
                    string[] bpis = sr.ReadLine().Split(',');
                    mw.cesta_zapis = sr.ReadLine();
                    mw.menuZapis.IsChecked = sr.ReadLine() == "True" ? true : false;
                    mw.menuPremazavat.IsChecked = sr.ReadLine() == "True" ? true : false;
                    if (sr.ReadLine() == "False") Funkce.ZobrazitPruzkumnik(false);
                    if (sr.ReadLine() == "False") Funkce.ZobrazitSoubory(false);
                    mw.Width = Funkce.ynt(sr.ReadLine(),500);
                    mw.Height = Funkce.ynt(sr.ReadLine(),500);
                    if (sr.ReadLine() == "max") mw.WindowState = WindowState.Maximized;

                    mw.richTextBox1.Foreground = new SolidColorBrush(Color.FromArgb(
                        (byte)Funkce.ynt(bpis[0],255),
                        (byte)Funkce.ynt(bpis[1]),
                        (byte)Funkce.ynt(bpis[2]),
                        (byte)Funkce.ynt(bpis[3])));

                    mw.richTextBox1.Background = new SolidColorBrush(Color.FromArgb(
                        (byte)Funkce.ynt(bpoz[0],255),
                        (byte)Funkce.ynt(bpoz[1],255),
                        (byte)Funkce.ynt(bpoz[2],255),
                        (byte)Funkce.ynt(bpoz[3],255)));
                }
                Funkce.CD(mw.vychozi_cesta);
            }
            catch { return false; }

            return true;
        }

        public static bool UlozNastaveni() {
            try {
                byte[] bpozadi = Ikony.Barvy(mw.richTextBox1.Background);
                byte[] bpisma = Ikony.Barvy(mw.richTextBox1.Foreground);

                using (StreamWriter sw = new StreamWriter(mw.prog_nastaveni,false)) {
                    sw.WriteLine(mw.vychozi_cesta);
                    sw.WriteLine(mw.richTextBox1.FontFamily.ToString());
                    sw.WriteLine(((int)mw.richTextBox1.FontSize).ToString());
                    sw.WriteLine(mw.richTextBox1.FontWeight.ToString());
                    sw.WriteLine(mw.richTextBox1.FontStyle.ToString());
                    sw.WriteLine(mw.kodovani.ToString());
                    sw.WriteLine(mw.historie.ToString());
                    sw.WriteLine(bpozadi[0].ToString() + "," +
                                 bpozadi[1].ToString() + "," +
                                 bpozadi[2].ToString() + "," +
                                 bpozadi[3].ToString());
                    sw.WriteLine(bpisma[0].ToString() + "," +
                                 bpisma[1].ToString() + "," +
                                 bpisma[2].ToString() + "," +
                                 bpisma[3].ToString());
                    sw.WriteLine(mw.cesta_zapis);
                    sw.WriteLine(mw.menuZapis.IsChecked);
                    sw.WriteLine(mw.menuPremazavat.IsChecked);
                    sw.WriteLine(mw.menuPruzkumnik.IsChecked);
                    sw.WriteLine(mw.menuSoubory.IsChecked || mw.menuVPolozky.IsChecked);
                    sw.WriteLine(mw.Width.ToString());
                    sw.WriteLine(mw.Height.ToString());

                    if (mw.WindowState == WindowState.Maximized)
                        sw.WriteLine("max");
                    else sw.WriteLine("0");
                    sw.Flush();
                }
            }

            catch (Exception e) {
                if (e is UnauthorizedAccessException || e is DirectoryNotFoundException) return false;
                else MessageBox.Show(e.Message.ToString(),"Chyba při ukládání nastavení",0,MessageBoxImage.Error);
            }

            return true;
        }

        public static bool NactiHistorii() {
            try {
                foreach (string radek in File.ReadAllLines(mw.prog_historie)) {
                    mw.seznam_historie.Add(radek);
                }
                mw.cbPrikaz.ItemsSource = mw.seznam_historie.Reverse();
            }
            catch { return false; }
            return true;
        }

        public static bool UlozHistorii() {
            try {
                using (StreamWriter sw = new StreamWriter(mw.prog_historie,false)) {
                    foreach (string prikaz in mw.seznam_historie) {
                        sw.WriteLine(prikaz);
                    }
                    sw.Flush();
                }
            }

            catch (Exception e) {
                if (e is UnauthorizedAccessException || e is DirectoryNotFoundException) return false;
                else MessageBox.Show(e.Message.ToString(),"Chyba při ukládání historie",0,MessageBoxImage.Error);
            }

            return true;
        }
    }
}
