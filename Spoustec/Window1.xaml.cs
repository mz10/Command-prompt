using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;

namespace Spoustec{
	public partial class Window1 : Window{
		public Window1(){
			InitializeComponent();			
			ikonaUAC.ImageSource = Ikony.Ikona("user32.dll",-106,true);
			ikonaUloz.ImageSource = Ikony.Ikona("shell32.dll",258,true);
			ikonaNapoveda.ImageSource = Ikony.Ikona("shell32.dll",277,true);
			ikonaNastaveni.ImageSource = Ikony.Ikona("imageres.dll",109,true);	
			tbCesta.Text = vychozi_cesta;
			Directory.SetCurrentDirectory(vychozi_cesta);
            Funkce.CD(vychozi_cesta);
			richTextBox1.Document.Blocks.Add(
				new Paragraph(new Run(Environment.OSVersion.ToString() + "\r\n"
			)));
			
			if(Funkce.Admin()) 
				Title+= " - Admin práva";
			
			DispatcherTimer dt = new DispatcherTimer();
			dt.Tick += new EventHandler(Hodiny);
			dt.Interval = TimeSpan.FromSeconds(10);
			dt.Start();
            Predvolby.NactiNastaveni();
            Predvolby.NactiProgramy();
            Predvolby.NactiHistorii();
            Funkce.ZapisDatum();
            Funkce.CbZapsat(cbPrikaz);
        }		


		#region hlavni promenne
		//nastaveni
		public string cesta_zapis = "";
		public int kodovani = 852;
		public int historie = 20;		
		public string vychozi_cesta = profil;
			
		//program
		public bool ukonci = false;
		public bool zastaveno = false;	
		public bool preruseno = false;
		public bool zobrazit_soubory = false;
		public bool nacitat_soubory = true;
		public bool ukladat_historii = true;
		public bool disk = false;
		public bool CMD = false;		
		public int opakovat = 0;
		public int pocitadlo = 0;
		public int seznam = 1;
		public int prikazy_i = 0;
		public static string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Spoustec\\";		
		public static string profil = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
		public string dokumenty = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\vystup.txt";	



		public string prog_cesta = appdata + "programy.txt";
		public string prog_nastaveni = appdata + "nastaveni.txt";
		public string prog_historie = appdata + "historie.txt";

        //seznamy
        public List<string> seznam_programu = new List<string>();
        public List<string> seznam_slozek = new List<string>();
        public List<string> seznam_souboru = new List<string>();
        public List<string> seznam_vybranych_s = new List<string>();
        public List<string> seznam_prikazu = new List<string>();


        public ObservableCollection<string> seznam_historie = new ObservableCollection<string>();

		public Process p;	
		public Process Proc = new System.Diagnostics.Process();		
		public Thread ukol;
		public StreamWriter psw;

        #endregion

        public void Soubory(string cesta) {
            string filtr = tbFiltr.Text;

            Thread u = new Thread(() => {
                try {
                    seznam_souboru.Clear();
                    seznam_slozek.Clear();
                    ImageSource ikSlozka = null;

                    Dispatcher.BeginInvoke(DispatcherPriority.Background,(ThreadStart)delegate () {
                        lvSoubory.Items.Clear();
                        lvSlozky.Items.Clear();
                        ikSlozka = Ikony.Ikona("shell32.dll",3,true);
                    });

                    foreach (string s in Directory.GetFiles(cesta)) {
                        if (filtr == "") seznam_souboru.Add(s);
                        else {
                            foreach (string p in filtr.Split(' ')) {
                                if (Path.GetExtension(s).ToLower() == "." + p)
                                    seznam_souboru.Add(s);
                            }
                        }
                    }

                    foreach (string p in Directory.GetDirectories(cesta)) {
                        seznam_slozek.Add(p);
                    }

                    foreach (string p in Directory.GetDirectories(cesta)) {
                        seznam_slozek.Add(p);
                    }

                    Dispatcher.BeginInvoke(DispatcherPriority.Background,(ThreadStart)delegate () {
                        lvSlozky.Items.Add(new Polozky { lvnazev = ".." });
                    });

                    foreach (string p in seznam_slozek.ToList()) {
                        Dispatcher.BeginInvoke(DispatcherPriority.Background,(ThreadStart)delegate () {
                            lvSlozky.Items.Add(new Polozky { lvikona = ikSlozka,lvnazev = Path.GetFileName(p) });
                        });
                    }

                    seznam = 1;
                    if (nacitat_soubory) {

                        foreach (string p in seznam_souboru.ToList()) {
                            Dispatcher.BeginInvoke(DispatcherPriority.Background,(ThreadStart)delegate () {
                                lvSoubory.Items.Add(
                                    new Polozky {
                                        lvzaskrknuto = true,
                                        lvikona = Ikony.Ikona2(Path.GetFullPath(p)),
                                        lvnazev = Path.GetFileNameWithoutExtension(p),
                                        lvpripona = Path.GetExtension(p).Replace(".","")
                                    });
                            });
                        }
                        seznam = 2;
                    }
                }
                catch (Exception e) {
                    if (e is DirectoryNotFoundException) cesta = "1";
                    else MessageBox.Show(e.Message.ToString(),"Upozornění",0,MessageBoxImage.Warning);

                    Dispatcher.BeginInvoke(DispatcherPriority.Background,(ThreadStart)delegate () {
                        Funkce.CD("..");
                    });
                }
            });
            u.IsBackground = true;
            u.Start();
        }

        #region udalosti - menu
        void mZapis_Click(object sender, RoutedEventArgs e) {
			if (cesta_zapis == "")
				cesta_zapis = Environment.GetFolderPath(
					Environment.SpecialFolder.MyDocuments) + "\\vystup.txt";		
		}		
		
		void mNastaveni_Click(object sender, RoutedEventArgs e) {
			var ns = new Nastaveni();
			ns.Show();
		}
		
		void mNVC_Click(object sender, RoutedEventArgs e) {
			vychozi_cesta = Directory.GetCurrentDirectory();
		}
		
		void mCesty_Click(object sender, RoutedEventArgs e) {
            Funkce.ZobrazitSoubory(true);
			lvSoubory.Visibility = Visibility.Hidden;
			tbVPolozky.Visibility = Visibility.Visible;
			tbVPolozky.Text = "";
			foreach (string prog in seznam_programu) {
				tbVPolozky.Text += prog + "\r\n";
			}
			tVSeznam.Content = "Uložit";
			menuSoubory.IsChecked = true;
            Funkce.ZobrazitPruzkumnik(true);
		}
				
		void mVycistit_Click(object sender, RoutedEventArgs e) {
			richTextBox1.Document.Blocks.Clear();
		}
			
		void mHistorie_Click(object sender, RoutedEventArgs e) {
			seznam_historie.Clear();
			cbPrikaz.ItemsSource = seznam_historie;
		}
		
		void mUlozit_Click(object sender, RoutedEventArgs e) {
			SaveFileDialog sd = new SaveFileDialog();
			sd.Title = "Uložit výstup...";
			sd.InitialDirectory = "previousPath";
			sd.Filter = "TXT|*.txt";
			sd.ValidateNames = true;

			if (sd.ShowDialog() == true) {			
				TextRange text = new TextRange(richTextBox1.Document.ContentStart, richTextBox1.Document.ContentEnd);
				FileStream fs = new FileStream(sd.FileName, FileMode.Create);
				text.Save(fs, DataFormats.Text);
				fs.Close();
			}
		}

		void mNapoveda_Click(object sender, RoutedEventArgs e) {
			Napoveda n = new Napoveda();
			richTextBox1.Document.Blocks.Clear();
			richTextBox1.Document.Blocks.Add(new Paragraph(new Run(n.nap)));
			richTextBox1.ScrollToEnd();
		}
			
		void mPruzkumnik_Click(object sender, RoutedEventArgs e) {
            Funkce.ZobrazitPruzkumnik(menuPruzkumnik.IsChecked);
		}
		
		void mOpravneni_Click(object sender, RoutedEventArgs e) {
			if(Environment.OSVersion.Version.Major >= 6) {
	        	Process pr = new Process(); 
	        	pr.StartInfo = new ProcessStartInfo(Process.GetCurrentProcess().MainModule.FileName);
	        	pr.StartInfo.UseShellExecute = true;
	            pr.StartInfo.Verb = "runas";
	            try{pr.Start();} catch{return;}
            	Close();
			}
		}

		void mSoubory_Click(object sender, RoutedEventArgs e) {
            Funkce.ZobrazitSoubory(menuSoubory.IsChecked);
		}
		
		void mVPolozky_Click(object sender, RoutedEventArgs e) {
            Funkce.ZobrazitVPolozky(menuVPolozky.IsChecked);
		}
		
		void mVkladat_Click(object sender, RoutedEventArgs e) {
			if (menuVkladat.IsChecked) {
				cbPrikaz.Visibility = Visibility.Hidden;
				tbPrikaz.Visibility = Visibility.Visible;
				tbPrikaz.Text = cbPrikaz.Text;
			}
			else {
				cbPrikaz.Visibility = Visibility.Visible;
				tbPrikaz.Visibility = Visibility.Hidden;
				tbPrikaz.Text = "";
			}
		}
		
		void mCMD_Click(object sender, RoutedEventArgs e) {
			Process proc = new Process();
			proc.StartInfo.FileName = "cmd";
			try{proc.Start();} catch{}
		}						
		
		void mPrerusit_Click(object sender, RoutedEventArgs e) {
			preruseno = true;
			try{p.Kill();} catch{}
			
			if(tStart.Content.ToString() == "Stop") {
				richTextBox1.Document.Blocks.Add(new Paragraph(new Run("\nProces byl přerušen")));
                Funkce.ZapisDoSouboru(menuZapis.IsChecked, "\r\nProces byl přerušen.\n");
				richTextBox1.ScrollToEnd();
			}
		}

		void mKonec_Click(object sender, RoutedEventArgs e) {
			Close();
		}
		#endregion
		
		#region udalosti - menu2
		void m2Otevrit_Click(object sender, RoutedEventArgs e) {
			Funkce.OtevritSoubor((Polozky)lvSoubory.SelectedItem);
		}

		void m2Pridat_Click(object sender, RoutedEventArgs e) {
			var p = (Polozky)lvSoubory.SelectedItem;
			
			if(p != null) {
				if(p.lvpripona == "")
					tbVPolozky.Text += p.lvnazev + "\r\n";
				else
					tbVPolozky.Text += p.lvnazev + "." + p.lvpripona + "\r\n";
			}
			
			lvSoubory.Visibility = Visibility.Hidden;
			tbVPolozky.Visibility = Visibility.Visible;
			menuSoubory.IsChecked = false;
			menuVPolozky.IsChecked = true;
			tVSeznam.Content = "Soubory";
		}
	
		void m2Pridat_DP_Click(object sender, RoutedEventArgs e) {			
			var polozka = (Polozky)lvSoubory.SelectedItem;
			
			if(polozka != null) {
				cbPrikaz.Text = cbPrikaz.Text.TrimEnd() + " \"" + polozka.lvnazev + "." + polozka.lvpripona + "\"";
			}			
		}
		
		void m2Nezobrazovat_S_Click(object sender, RoutedEventArgs e) {
            Funkce.ZobrazitSoubory(false);
		}

		void mSL_Zobrazovat_S_Click(object sender, RoutedEventArgs e) {
            Funkce.ZobrazitSoubory(mSL_Zobrazovat_S.IsChecked);
		}	
		#endregion		
			
		#region udalosti - tlacitka			
		void tZkopirovat_Click(object sender, RoutedEventArgs e){			
			Clipboard.SetText(tbCesta.Text + ">" + cbPrikaz.Text + "\n" +
				new TextRange(
					richTextBox1.Document.ContentStart,
					richTextBox1.Document.ContentEnd).Text,
					TextDataFormat.UnicodeText);
		}
	
		void tStart_Click(object sender, RoutedEventArgs e){
            Funkce.CbZapsat(cbPrikaz);
			CMD = false;
			if (cbPrikaz.Text == "" && !menuVkladat.IsChecked) return;
			if (tbPrikaz.Text == "" && menuVkladat.IsChecked) return;
			
			if(tStart.Content.ToString() == "Spustit")
                Spustit.PrikazSpustit();		
			else {				
				zastaveno = true;
				try{p.Kill();} catch{}
				richTextBox1.Document.Blocks.Add(new Paragraph(new Run("\nProces byl zastaven")));
                Funkce.ZapisDoSouboru(menuZapis.IsChecked, "\r\nProces byl zastaven.\n");
				richTextBox1.ScrollToEnd();
				tStart.Content = "Spustit";
			}			
		}

		void tStartCMD_Click(object sender, RoutedEventArgs e){
			CMD = true;
			if (cbPrikaz.Text == "") return;					
			if(tStart.Content.ToString() == "Spustit") {
                Spustit.PrikazSpustit();
            }
		}

		void tCD_Click(object sender, RoutedEventArgs e){
			try {Directory.SetCurrentDirectory(
				Directory.GetParent(
					Directory.GetCurrentDirectory()).FullName);} catch {}
			tbCesta.Text = Directory.GetCurrentDirectory();
            Soubory(tbCesta.Text);
		}				
		
		void tZmenit_Click(object sender, RoutedEventArgs e){
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Filter = "Složky|*.";
			ofd.Title = "Vyber cestu...";
			ofd.FileName = "Vyber složku";
			ofd.InitialDirectory = "previousPath";			
			ofd.RestoreDirectory = true;
			ofd.ValidateNames = false;
			ofd.CheckFileExists = false;
			ofd.CheckPathExists = false;

			if (ofd.ShowDialog() == true) {
                Funkce.CD(Path.GetDirectoryName(ofd.FileName));
			}			
		}
		
		void tVSeznam_Click(object sender, System.Windows.RoutedEventArgs e){
			if(tVSeznam.Content.ToString() == "Uložit") {
				tVSeznam.Content = "V. seznam";
				lvSoubory.Visibility = Visibility.Visible;
				File.WriteAllText(prog_cesta, tbVPolozky.Text); 
				tbVPolozky.Text = "";
                Predvolby.NactiProgramy();
				seznam=2;
				return;
			}

			if(tVSeznam.Content.ToString() == "V. seznam") Funkce.ZobrazitVPolozky(true);
			else Funkce.ZobrazitVPolozky(false);				
		}
		
		void tVymazat_Click(object sender, System.Windows.RoutedEventArgs e){
				tbVPolozky.Text = "";					
		}
		
		void tPridat_Click(object sender, System.Windows.RoutedEventArgs e){
			if(lvSoubory.Visibility == Visibility.Visible) {
				foreach(Polozky p in lvSoubory.Items)
					if(p.lvzaskrknuto) {
						if(p.lvpripona == "")
							tbVPolozky.Text += p.lvnazev + "\r\n";
						else
							tbVPolozky.Text += p.lvnazev + "." + p.lvpripona + "\r\n";
				}
				lvSoubory.Visibility = Visibility.Hidden;
				tbVPolozky.Visibility = Visibility.Visible;
				menuSoubory.IsChecked = false;
				menuVPolozky.IsChecked = true;
				tVSeznam.Content = "Soubory";
				seznam=3;
			}
			else {
				OpenFileDialog ofd = new OpenFileDialog();
				ofd.Title = "Přidat soubor...";
				ofd.InitialDirectory = Directory.GetCurrentDirectory();
				ofd.Multiselect = true;
	
				if (ofd.ShowDialog() == true) {
					foreach (string s in ofd.FileNames) {
						tbVPolozky.Text += s + "\r\n";
					}
				}	
			}
		}

		void tSkryt_Click(object sender, System.Windows.RoutedEventArgs e){
			if(tSkryt.Content.ToString() == "<") Funkce.ZobrazitPruzkumnik(true);
			else Funkce.ZobrazitPruzkumnik(false);			
		}
		
		#endregion
		
		#region udalosti - textboxy/bloky
		void cbPrikaz_KeyDown(object sender, KeyEventArgs e){
			if (e.Key == Key.Enter) {
				if (cbPrikaz.Text == "") return;
				
				if(tStart.Content.ToString() == "Spustit")
                    Spustit.PrikazSpustit();
                else {				
					zastaveno = true;
					try{p.Kill();} catch{}
					richTextBox1.Document.Blocks.Add(new Paragraph(new Run("\nProces byl zastaven")));
                    Funkce.ZapisDoSouboru(menuZapis.IsChecked, "\r\nProces byl zastaven.\n");
					richTextBox1.ScrollToEnd();
				}

				e.Handled = true;
			};		
		}

		void cbPrikaz_KeyUp(object sender, KeyEventArgs e){
			if (e.Key == Key.Up || e.Key == Key.Down) {		
				e.Handled = true;
			}
			
			if (e.Key == Key.LeftCtrl) {
				cbPrikaz.IsDropDownOpen = !cbPrikaz.IsDropDownOpen;			
				e.Handled = true;
			}
			
			if (e.Key == Key.RightCtrl) {
				if(cbPrikaz.Text == "")
					zobrazit_soubory = false;
				
				zobrazit_soubory = !zobrazit_soubory;
				cbPrikaz.ItemsSource = null;
				
				if (zobrazit_soubory) cbPrikaz.ItemsSource = seznam_souboru;	
				else cbPrikaz.ItemsSource = seznam_historie.Reverse();
				
				e.Handled = true;
			}			
		}

		void tbCesta_KeyUp(object sender, KeyEventArgs e){
			if (e.Key == Key.Enter) {
                Funkce.CD(tbCesta.Text);
				e.Handled = true;
			}	
		}
		
		void cbPrikazPozadi_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e){
            Funkce.CbZapsat(cbPrikaz);
		}
        #endregion

        #region udalosti - ostatni
        //pruhlednost okna   
        public void pruhlednost(object sender,RoutedEventArgs e) {
            Pruhlednost.Pruhledne(this);
        }

        public void Hodiny(object sender,EventArgs e) {
            if (tbHodiny.Text == (DateTime.Now.Hour.ToString() + ":" + DateTime.Now.Minute.ToString("D2"))) {
                Funkce.VypniPC();
                tbHodiny.Text = "";
            }
        }

        void comboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e){			
			cbPrikaz.Text = comboBox1.SelectedValue.ToString();
			cbPrikaz.Focus();
		}		
		
		void window1_Closing(object sender, EventArgs e){
			try{p.Kill();} catch{}
			Predvolby.UlozNastaveni();
            Predvolby.UlozHistorii();
		}
		
		void lvSlozky_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e){
			var polozka = (Polozky)lvSlozky.SelectedItem;
			
			if(polozka != null) {
                Funkce.CD(polozka.lvnazev);
			}
		}
		
		void lvSlozky_KeyUp(object sender, System.Windows.Input.KeyEventArgs e){
			if (e.Key == Key.Enter) {
				var polozka = (Polozky)lvSlozky.SelectedItem;				
				if(polozka != null) Funkce.CD(polozka.lvnazev);				
				e.Handled = true;
			}	
		}
		
		void lvSoubory_KeyUp(object sender, System.Windows.Input.KeyEventArgs e){
			if (e.Key == Key.Enter) {
				Funkce.OtevritSoubor((Polozky)lvSoubory.SelectedItem);
				e.Handled = true;
			}				
		}
		
		void lvSoubory_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e){
            Funkce.OtevritSoubor((Polozky)lvSoubory.SelectedItem);
		}		
		
		void chb_listview_Checked(object sender, RoutedEventArgs e){
			foreach(Polozky p in lvSoubory.Items)
				p.lvzaskrknuto = chb_listview.IsChecked==true;
			
			lvSoubory.Items.Refresh();
		}		
		
		void GridSplitter_Pohyb(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e) {
			if(Funkce.ynt(Grid1.ColumnDefinitions[4].Width.ToString()) < 40)
                Funkce.ZobrazitPruzkumnik(false);
		}
		
		void tbFiltr_KeyUp(object sender, System.Windows.Input.KeyEventArgs e){
			if (e.Key == Key.Enter) {
                Funkce.CD(tbCesta.Text);
				e.Handled = true;
			}			
		}
		#endregion		
	}	
}