using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Spoustec {
    class Funkce {
        static Window1 mw = ((Window1)Application.Current.Windows[0]);

        public static string NactiSoubor(string ccesta,string prikaz) {
            StringBuilder sbPrikaz = new StringBuilder();

            try {
                string dsk = "";
                string slozka = "";

                try {
                    slozka = Path.GetDirectoryName(ccesta);
                    dsk = ccesta.Substring(0,1);
                }
                catch { }

                if (slozka == "") {
                    slozka = Directory.GetCurrentDirectory();
                    ccesta = slozka + "\\" + ccesta;
                    dsk = slozka.Substring(0,1);
                }

                string cnazev = Path.GetFileName(ccesta);
                string nazev = Path.GetFileNameWithoutExtension(ccesta);
                string pripona = Path.GetExtension(ccesta).Replace(".","");
                string cd = System.AppDomain.CurrentDomain.BaseDirectory;

                sbPrikaz = new StringBuilder(prikaz);

                sbPrikaz = sbPrikaz
                    .Replace("§cn",slozka + "\\" + nazev)
                    .Replace("§sl",slozka + "\\")
                    .Replace("§c",ccesta)
                    .Replace("§p",pripona)
                    .Replace("§n",nazev)
                    .Replace("§d",dsk)
                    .Replace("§s",cnazev)
                    .Replace("\\\\","\\");
            }
            catch { }
            return sbPrikaz.ToString();
        }

        public static void OtevritSoubor(Polozky polozka) {
            if (polozka != null) {
                string cesta = Directory.GetCurrentDirectory() + "\\" + polozka.lvnazev + "." + polozka.lvpripona;
                Process proc = new Process();
                proc.StartInfo.FileName = cesta;
                proc.StartInfo.UseShellExecute = true;
                try { proc.Start(); } catch { }
            }
        }

        public static void ZapisDatum() {
            string den = DateTime.Now.Day.ToString();
            string mes = DateTime.Now.Month.ToString();
            string rok = DateTime.Now.Year.ToString();
            string hod = DateTime.Now.Hour.ToString();
            string min = DateTime.Now.Minute.ToString("D2");

            ZapisDoSouboru(mw.menuZapis.IsChecked,"Spusteno: " + den + "." + mes + "." + rok + " " + hod + ":" + min);
        }

        public static void CbZapsat(ComboBox cb) {
            cb.Dispatcher.BeginInvoke((Action)(() => { cb.Focus(); }),DispatcherPriority.Render);
        }

        public static void VlozitDoHistorie(string program,string parametr) {
            if (mw.ukladat_historii) {
                if (mw.seznam_programu.Contains(program)) {
                    program = Path.GetFileNameWithoutExtension(program);
                }

                string vlozit = (program + " " + parametr).Replace("cmd /c ","").Trim();

                if (!mw.seznam_historie.Contains(vlozit)) {
                    if (mw.seznam_historie.Count >= mw.historie)
                        mw.seznam_historie.RemoveAt(0);

                    mw.seznam_historie.Add(vlozit);
                    mw.cbPrikaz.ItemsSource = mw.seznam_historie;
                }
            }
        }

        public static void ZobrazitPruzkumnik(bool jn) {
            if (jn) {
                mw.Grid1.ColumnDefinitions[4].Width = new GridLength(0,GridUnitType.Auto);
                mw.tSkryt.Content = ">";
                mw.menuPruzkumnik.IsChecked = true;
            }

            else {
                mw.Grid1.ColumnDefinitions[4].Width = new GridLength(0);
                mw.tSkryt.Content = "<";
                mw.menuPruzkumnik.IsChecked = false;
            }

        }

        public static void ZobrazitSoubory(bool jn) {
            mw.menuSoubory.IsChecked = jn;
            mw.mSL_Zobrazovat_S.IsChecked = jn;
            mw.nacitat_soubory = jn;

            if (jn) {
                mw.lvSoubory.Visibility = Visibility.Visible;
                mw.tbVPolozky.Visibility = Visibility.Hidden;
                mw.lvSlozky.SetValue(Grid.RowSpanProperty,4);
                mw.seznam = 2;
                mw.menuVPolozky.IsChecked = false;
                mw.tVSeznam.Content = "V. seznam";
                if (mw.lvSoubory.Items.Count == 0) Funkce.CD(mw.tbCesta.Text);
            }
            else {
                mw.lvSoubory.Visibility = Visibility.Hidden;
                mw.lvSlozky.SetValue(Grid.RowSpanProperty,5);
                mw.seznam = 1;
            }
        }

        public static void ZobrazitVPolozky(bool jn) {
            mw.menuVPolozky.IsChecked = jn;
            mw.nacitat_soubory = jn;

            if (jn) {
                mw.lvSoubory.Visibility = Visibility.Hidden;
                mw.tbVPolozky.Visibility = Visibility.Visible;
                mw.lvSlozky.SetValue(Grid.RowSpanProperty,4);
                mw.seznam = 3;
                mw.menuSoubory.IsChecked = false;
                mw.tVSeznam.Content = "Soubory";
            }
            else {
                ZobrazitSoubory(true);
            }
        }

        public static void CD(string cesta) {
            if (Regex.Match(mw.tbCesta.Text,"^[a-zA-Z]:\\\\$").Success && cesta == "..") {
                mw.disk = true;
                mw.lvSlozky.Items.Clear();

                foreach (var d in DriveInfo.GetDrives()) {
                    string druh = d.DriveType.ToString();
                    ImageSource ik = null;
                    if (druh == "CDRom") ik = Ikony.Ikona("shell32.dll",11,true);
                    else if (druh == "Fixed") ik = Ikony.Ikona("shell32.dll",8,true);
                    else ik = Ikony.Ikona("shell32.dll",8,true);

                    mw.lvSlozky.Items.Add(new Polozky { lvikona = ik,lvnazev = d.Name.ToString() });
                }

                mw.lvSlozky.Items.Add(new Polozky { lvikona = Ikony.Ikona("shell32.dll",158,true),lvnazev = "Profil" });
                mw.lvSlozky.Items.Add(new Polozky { lvikona = Ikony.Ikona("imageres.dll",105,true),lvnazev = "Plocha" });
                mw.lvSlozky.Items.Add(new Polozky { lvikona = Ikony.Ikona("shell32.dll",3,true),lvnazev = "Windows" });
                return;
            }

            if (mw.disk) {
                if (cesta == "Windows")
                    cesta = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                if (cesta == "Profil")
                    cesta = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                if (cesta == "Plocha")
                    cesta = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                mw.disk = false;
            }

            string cesta2 = "";

            if (Regex.Match(cesta,"[a-zA-Z]:").Success) cesta2 = cesta;
            else cesta2 = Directory.GetCurrentDirectory() + "\\" + cesta;

            try { Directory.SetCurrentDirectory(cesta2); } catch { }
            mw.tbCesta.Text = Directory.GetCurrentDirectory();
            mw.Soubory(cesta2);
        }

        public static bool Admin() {
            WindowsPrincipal p = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            return p.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static void VypniPC() {
            var vyp = new Vypnout();
            vyp.UkoncitWindows(1,16);
        }

        //prevod textu na int
        public static int ynt(string text,int vHodnota = 0) {
            try { vHodnota = Int32.Parse(text); } catch { }
            return vHodnota;
        }

        public static bool ZapisDoSouboru(bool povoleno,string text) {
            if (!povoleno) return true;

            try {
                using (StreamWriter sw = new StreamWriter(mw.cesta_zapis,true)) {
                    sw.WriteLine(text);
                    sw.Flush();
                }
            }

            catch (Exception e) {
                if (e is UnauthorizedAccessException || e is DirectoryNotFoundException) return false;
                else MessageBox.Show(e.Message.ToString(),"Chyba při zápisu do souboru",0,MessageBoxImage.Error); return false;
            }

            return true;
        }
    }
}