using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Threading;

namespace Spoustec {
    class Spustit {
        static Window1 mw = ((Window1)Application.Current.Windows[0]);
        static Process p = mw.p;

        #region spustit
        public static void PrikazSpustit() {
            mw.zastaveno = false;
            mw.pocitadlo = 0;
            mw.tStart.Content = "Stop";
            mw.seznam_vybranych_s.Clear();

            //seznam souboru	
            if (mw.seznam == 1) mw.seznam_vybranych_s.AddRange(mw.seznam_souboru);

            if (mw.seznam == 2) {//soubory - listview
                foreach (Polozky p in mw.lvSoubory.Items)
                    if (p.lvzaskrknuto) {
                        string lvpr = p.lvpripona == "" ? "" : "." + p.lvpripona;
                        mw.seznam_vybranych_s.Add(
                            (Directory.GetCurrentDirectory() + "\\" + p.lvnazev + lvpr).Replace("\\\\","\\"));
                    }
            }

            if (mw.seznam == 3) //vlastni seznam
                mw.seznam_vybranych_s.AddRange(
                    mw.tbVPolozky.Text.Split(new string[] { "\r\n" },
                                          StringSplitOptions.RemoveEmptyEntries));

            if (mw.menuSPPS.IsChecked) {
                mw.opakovat = mw.seznam_vybranych_s.Count();
                mw.tbOpakovat.Text = mw.opakovat.ToString();
            }
            else {
                mw.opakovat = Funkce.ynt(mw.tbOpakovat.Text);
            }

            if (mw.menuVkladat.IsChecked) {
                mw.seznam_prikazu.Clear();
                mw.seznam_prikazu.AddRange(
                    mw.tbPrikaz.Text.Split(new string[] { "\r\n" },
                                          StringSplitOptions.RemoveEmptyEntries));
            }


            if (mw.menuVkladat.IsChecked && mw.seznam_prikazu.Count > 0)
                Prikaz(mw.seznam_prikazu[0],0);
            else Prikaz(mw.cbPrikaz.Text,0);
        }

        public static bool Prikaz(string prikaz,int pocet) {
            mw.preruseno = false;
            string parametr = "";
            string program = "";
            string prikaz0 = "";
            string parametr0 = "";

            prikaz0 = prikaz;

            prikaz = prikaz
                .Replace("§i",mw.pocitadlo.ToString())
                .Replace("§cd",Directory.GetCurrentDirectory() + "\\");

            if (pocet < mw.seznam_vybranych_s.Count()) prikaz = Funkce.NactiSoubor(mw.seznam_vybranych_s[pocet],prikaz);

            Funkce.ZapisDoSouboru(mw.menuZapis.IsChecked,"\r\n" + Directory.GetCurrentDirectory() + ">" + prikaz + "\r\n");

            //najdi retezec v "zavorkach"
            Match najdi0 = Regex.Match(prikaz0,"(^\"[\\s\\S]*?\")");
            Match najdi = Regex.Match(prikaz,"(^\"[\\s\\S]*?\")");


            if (najdi0.Success) {//zadany parametr
                parametr0 = prikaz0.Remove(0,najdi0.Groups[0].Value.Length).TrimStart();

            }
            else {//jinak to rozdelit mezerou
                string[] rozdel = prikaz0.Split(' ');
                parametr0 = prikaz0.Remove(0,rozdel[0].Length).TrimStart();
            }

            if (najdi.Success) {//upraveny program a parametr
                parametr = prikaz.Remove(0,najdi.Groups[0].Value.Length).TrimStart();
                program = najdi.Groups[0].Value;

            }
            else {
                string[] rozdel = prikaz.Split(' ');
                parametr = prikaz.Remove(0,rozdel[0].Length).TrimStart();
                program = rozdel[0];
            }

            //zjisti cestu programu ze souboru		
            for (int i = 0;i <= mw.comboBox1.Items.Count - 1;i++) {
                string nazev = "";
                try { nazev = Path.GetFileNameWithoutExtension(program).ToLower(); } catch { };

                string c2 = mw.comboBox1.Items[i].ToString().ToLower();

                if (nazev == c2) {
                    program = "\"" + mw.seznam_programu[i] + "\"";
                    break;
                }
            }

            //prejde o slozku vys
            if (prikaz == "cd.." | prikaz == "cd ..") {
                try {
                    Directory.SetCurrentDirectory(
                        Directory.GetParent(
                            Directory.GetCurrentDirectory()).FullName);
                }
                catch { }

                mw.tbCesta.Text = Directory.GetCurrentDirectory();
                mw.Soubory(mw.tbCesta.Text);
                return true;
            }

            //prejde na jiny disk
            Match disk = Regex.Match(prikaz,"[a-zA-Z]:");
            if ((disk.Success && Directory.Exists(prikaz))) {
                Funkce.CD(prikaz);
                Funkce.CbZapsat(mw.cbPrikaz);
                return true;
            }

            //prejde na jinou slozku
            if (program == "cd") {
                if ((Directory.Exists(Directory.GetCurrentDirectory() + parametr))) {
                    Directory.SetCurrentDirectory(Directory.GetCurrentDirectory() + parametr);
                }
                else if ((Directory.Exists(parametr))) {
                    Directory.SetCurrentDirectory(parametr);
                }
                else {
                    Spust("cmd","/c cd " + parametr,parametr0);
                }
                mw.tbCesta.Text = Directory.GetCurrentDirectory();
                mw.Soubory(mw.tbCesta.Text);
                return true;
            }

            if (program.ToLower() == "zprava") {
                MessageBox.Show(parametr,"Zpráva",0,MessageBoxImage.Information);
                Funkce.VlozitDoHistorie(program,parametr);
                Ukonceno(null,null);
                return true;
            }

            if (program.ToLower() == "cmd") {
                Process proc = new Process();
                proc.StartInfo.FileName = "cmd";
                mw.tStart.Content = "Spustit";
                Funkce.VlozitDoHistorie("cmd","");
                try { proc.Start(); } catch { }
                return true;
            }

            string[] c_prikazy = {"dir","del","copy","move","md","rd","ren","mklink","erase","md","mkdir",
                "set","if","for","goto","start","echo","vol"};

            foreach (string c_prikaz in c_prikazy) {
                if (program.ToLower() == c_prikaz) {
                    program = "cmd";
                    parametr = "/c " + prikaz;
                    parametr0 = "/c " + prikaz0;
                    break;
                }
            }

            //dalsi specialni prikazy
            if (program.ToLower() == "exit" || program.ToLower() == "konec") {
                mw.Close();
                return true;
            }

            if (program.ToLower() == "napsat") {
                mw.tStart.Content = "Stop";
                mw.richTextBox1.Document.Blocks.Add(new Paragraph(new Run(parametr)));
                mw.richTextBox1.ScrollToEnd();
                Funkce.VlozitDoHistorie("napsat",parametr0);
                Ukonceno(null,null);
                return true;
            }

            if (program.ToLower() == "cls" || program.ToLower() == "clear") {
                mw.richTextBox1.Document.Blocks.Clear();
                Funkce.VlozitDoHistorie("cls","");
                Ukonceno(null,null);
                return true;
            }

            Spust(program,parametr,parametr0);
            return true;
        }

        public static void Vystup(object sender,DataReceivedEventArgs e) {
            mw.Dispatcher.BeginInvoke(DispatcherPriority.Background,(ThreadStart)delegate () {
                if (mw.zastaveno || mw.preruseno) return;
                mw.richTextBox1.Document.Blocks.Add(new Paragraph(new Run(e.Data)));
                Funkce.ZapisDoSouboru(mw.menuZapis.IsChecked,e.Data);
                mw.richTextBox1.ScrollToEnd();
            });
        }

        public static bool CSpustit(string program,string parametr) {
            try { p.Kill(); } catch { }

            p = null;
            p = new System.Diagnostics.Process();

            p.StartInfo = new ProcessStartInfo(program);
            p.StartInfo.Arguments = parametr;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardInput = true;
            p.EnableRaisingEvents = true;
            p.StartInfo.CreateNoWindow = true;
            p.OutputDataReceived += new DataReceivedEventHandler(Vystup);
            p.ErrorDataReceived += new DataReceivedEventHandler(Vystup);

            p.Exited += new EventHandler(Ukonceno);
            try { p.StartInfo.StandardOutputEncoding = Encoding.GetEncoding(mw.kodovani); }
            catch (Exception e) { MessageBox.Show(e.Message.ToString()); }

            try {
                p.Start();
                mw.psw = p.StandardInput;
                mw.psw.Flush();
                mw.psw.Close();
            }
            catch { return false; }

            mw.Dispatcher.BeginInvoke(DispatcherPriority.Send,(ThreadStart)delegate () {
                if (mw.menuPremazavat.IsChecked)
                    mw.richTextBox1.Document.Blocks.Clear();
            });

            try {
                p.BeginErrorReadLine();
                p.BeginOutputReadLine();
                p.WaitForExit();
            }
            catch { }

            return true;
        }

        public static bool CSpustitvCMD(string prikaz) {
            p = null;
            p = new System.Diagnostics.Process();
            p.StartInfo = new ProcessStartInfo("cmd");
            p.StartInfo.Arguments = "/k " + prikaz;
            p.StartInfo.UseShellExecute = false;
            p.Exited += new EventHandler(Ukonceno);
            try { p.Start(); } catch { return false; }
            mw.tStart.Content = "Spustit";
            return true;
        }

        public static void Spust(string program,string parametr,string parametr0) {
            bool vysledek = false;
            string prikaz0 = (program + " " + parametr0).Trim();
            string prikaz = (program + " " + parametr).Trim();

            if (mw.CMD) {
                CSpustitvCMD(prikaz);
                return;
            }

            mw.ukol = new Thread(() => {
                if (!CSpustit(program,parametr) && !CSpustit(prikaz,"")) {
                    string[] rozdel = prikaz.Split(' ');
                    string ccast = "";
                    string pm = "";
                    vysledek = false;

                    foreach (string cast in rozdel) {
                        ccast += cast + " ";
                        pm = prikaz.Substring((ccast.Trim()).Length).Trim();
                        if (CSpustit(ccast.Trim(),pm)) {
                            vysledek = true;
                            break;
                        }
                    }
                }
                else {
                    vysledek = true;
                }

                mw.Dispatcher.BeginInvoke(DispatcherPriority.Send,(ThreadStart)delegate () {
                    if (!vysledek) {
                        mw.richTextBox1.Document.Blocks.Add(
                                   new Paragraph(new Run("Tento příkaz neexistuje: " + prikaz)));
                        Funkce.CbZapsat(mw.cbPrikaz);
                        mw.richTextBox1.ScrollToEnd();
                        mw.tStart.Content = "Spustit";
                    }
                    else {
                        program = program.Replace("\"","");
                        Funkce.VlozitDoHistorie(program,parametr0);
                    }
                });
            });

            mw.ukol.IsBackground = true;
            mw.ukol.Start();
        }

        public static void Zastav() {
            mw.pocitadlo = 0;
            mw.prikazy_i = 0;
            mw.ukladat_historii = true;
            mw.tbOpakovat.Text = "1";
            mw.tStart.Content = "Spustit";
            Funkce.CbZapsat(mw.cbPrikaz);
        }

        private static void Ukonceno(object sender,System.EventArgs e) {
            mw.Dispatcher.BeginInvoke(DispatcherPriority.Send,(ThreadStart)delegate () {
                if (mw.zastaveno) {
                    Zastav();
                    return;
                }
                else {
                    if (mw.pocitadlo > 0)
                        mw.tbOpakovat.Text = (mw.opakovat - mw.pocitadlo).ToString();

                    int prikazy_pocet = mw.seznam_prikazu.Count - 1;

                    if (mw.menuVkladat.IsChecked && mw.seznam_prikazu.Count > 0) {
                        if (Funkce.ynt(mw.tbOpakovat.Text) <= 1 && mw.prikazy_i == prikazy_pocet) {
                            Zastav();
                            if (mw.tbHodiny.Text == "PU") Funkce.VypniPC();
                            return;
                        }
                    }

                    else if (Funkce.ynt(mw.tbOpakovat.Text) <= 1) {
                        Zastav();
                        if (mw.tbHodiny.Text == "PU") Funkce.VypniPC();
                        return;
                    }

                    if (mw.pocitadlo < mw.opakovat) {
                        mw.ukladat_historii = false;

                        if (mw.menuVkladat.IsChecked && mw.seznam_prikazu.Count > 0) {
                            if (mw.prikazy_i < prikazy_pocet) {
                                mw.prikazy_i++;
                                Prikaz(mw.seznam_prikazu[mw.prikazy_i],mw.pocitadlo);
                                return;
                            }

                            else if (mw.pocitadlo + 1 != mw.opakovat) {
                                mw.pocitadlo++;
                                mw.prikazy_i = 0;
                                Prikaz(mw.seznam_prikazu[0],mw.pocitadlo);
                            }
                        }
                        else {
                            mw.pocitadlo++;
                            Prikaz(mw.cbPrikaz.Text,mw.pocitadlo);
                        }
                    }
                }
            });
        }

        #endregion


    }
}
