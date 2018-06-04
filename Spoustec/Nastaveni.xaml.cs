using System;
using System.Drawing;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;

namespace Spoustec{
	public partial class Nastaveni : Window{
		Window1 mw = ((Window1) System.Windows.Application.Current.Windows[0]);
		System.Windows.Media.Color barva;
		
		public Nastaveni(){
			InitializeComponent();			
			System.Windows.Media.Brush rbPozadi = mw.richTextBox1.Background;						
			byte[] b = Ikony.Barvy(mw.richTextBox1.Background);			
			barva = System.Windows.Media.Color.FromArgb(b[0], b[1], b[2], b[3]);			
			slider1.Value = (double)b[0];
			tbKodovani.Text = mw.kodovani.ToString();
			tbCesta.Text = mw.cesta_zapis;
			tbHistorie.Text = mw.historie.ToString();
			tbVCesta.Text = mw.vychozi_cesta;
			
			if(mw.tbHodiny.Text == "PU") {rv2.IsChecked = true;}
			else tbCas.Text = mw.tbHodiny.Text;
			
			ru3.IsChecked = mw.menuZapis.IsChecked == true;
			ru2.IsChecked = mw.cesta_zapis == mw.dokumenty;
			ru1.IsChecked = mw.menuZapis.IsChecked == false;	
		}
				
		System.Windows.Media.Color barvy2(System.Windows.Media.Brush stetec) {
			System.Windows.Media.Color br = System.Windows.Media.Color.FromArgb(0,0,0,0);
			
			byte[] b = Ikony.Barvy(stetec);
			ColorDialog cd = new ColorDialog();
			cd.Color = System.Drawing.Color.FromArgb(b[0], b[1], b[2], b[3]);
			cd.FullOpen = true;
			
			if (cd.ShowDialog() != System.Windows.Forms.DialogResult.Cancel) {
				br =  System.Windows.Media.Color.FromArgb(255, cd.Color.R, cd.Color.G, cd.Color.B);
				return br;
			}
			return br;			
		}	
			
		public int ynt(string text, int vhodnota){
			try {vhodnota = Int32.Parse(text);} catch {}
			return vhodnota;
		}
			
		bool zkontrolovat() {
			if(ru1.IsChecked == true) {
				mw.menuZapis.IsChecked = false;
			}
			else if(ru2.IsChecked == true) {
				mw.menuZapis.IsChecked = true;
				mw.cesta_zapis = mw.dokumenty;
			}
			else if(ru3.IsChecked == true) {
				if(tbCesta.Text != "") {
					mw.menuZapis.IsChecked = true;
					mw.cesta_zapis = tbCesta.Text;
				}
				else {
					mw.menuZapis.IsChecked = false;
					ru1.IsChecked = true;
				}
			}			
			
			if(rv1.IsChecked == true) {
				Match najdi = Regex.Match(tbCas.Text, "(\\d+):(\\d+)");
				bool casy = najdi.Success && ynt(najdi.Groups[1].Value,-1000) < 24 && ynt(najdi.Groups[2].Value,-1000) < 60;
				bool minuty = ynt(tbCas.Text,-1000) >= 0 && ynt(tbCas.Text,-1000) <= 10000;
				bool prazny = tbCas.Text == "";
				
				if (casy) {
					tOK.IsEnabled = true;
					mw.tbHodiny.Text = mw.tbHodiny.Text = najdi.Groups[1].Value + ":" + ynt(najdi.Groups[2].Value,-1000).ToString("D2");
				} 
				else if (minuty) {
					tOK.IsEnabled = true;
					int hm = DateTime.Now.Hour * 60 + DateTime.Now.Minute + ynt(tbCas.Text,-1000);
					int hodina = hm / 60;
					int minuta = hm - (60 * hodina);
					if (hodina >= 24) {
						hodina = hodina - 24;
					}
					mw.tbHodiny.Text = mw.tbHodiny.Text = hodina.ToString() + ":" + minuta.ToString("D2");
				} 
				else if (prazny) {
					tOK.IsEnabled = true;
					mw.tbHodiny.Text = "";
				} 
				else {
					tOK.IsEnabled = false;
				}
	
			}
			if(rv2.IsChecked == true) {
				mw.tbHodiny.Text = "PU";
			}
			
			
			mw.kodovani = ynt(tbKodovani.Text,852);
			mw.historie = ynt(tbHistorie.Text,20);
			string profil = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
			mw.vychozi_cesta = tbVCesta.Text == "" ? profil : tbVCesta.Text;
			
			return true;
		}
			
		void tOK_Click(object sender, RoutedEventArgs e){
			zkontrolovat();
			Close();
		}	

		void slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e){			
			mw.richTextBox1.Background = new SolidColorBrush(
				System.Windows.Media.Color.FromArgb((byte)slider1.Value, barva.R, barva.G, barva.B));
		}
		
		void tBPozadi_Click(object sender, RoutedEventArgs e){
			System.Windows.Media.Color br = barvy2(mw.richTextBox1.Background);
			mw.richTextBox1.Background = new SolidColorBrush(
				System.Windows.Media.Color.FromArgb((byte)slider1.Value, br.R, br.G, br.B));
			
			//mw.richTextBox1.Background = new SolidColorBrush(barva);
			
			barva = br;
		}
		
		void tBPisma_Click(object sender, RoutedEventArgs e){
			System.Windows.Media.Color br = barvy2(mw.richTextBox1.Foreground);
			mw.richTextBox1.Foreground = new SolidColorBrush(br);
		}	
		
		void tVybrat_Click(object sender, RoutedEventArgs e){
			SaveFileDialog fd = new SaveFileDialog();
			fd.Title = "Vyber soubor...";
			fd.InitialDirectory = "previousPath";
			fd.FileName = "vystup.txt";
			fd.Filter = "Textové soubory (*.txt)|*.txt";
			if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
				tbCesta.Text = fd.FileName;
			}
		}
		
		void tbCesta_TextChanged(object sender, TextChangedEventArgs e){
			ru3.IsChecked = true;
		}
			
		void tbCas_KeyUp(object sender, System.Windows.Input.KeyEventArgs e){
			rv1.IsChecked = true;
			zkontrolovat();
		}
	
		void tbCas_TextChanged(object sender, TextChangedEventArgs e){
			rv1.IsChecked = true;
			zkontrolovat();
		}
		
		void tPismo_Click(object sender, RoutedEventArgs e){
			int dpiX = (int)typeof(SystemParameters)
				.GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static)
				.GetValue(null, null);
			
			var fd = new FontDialog();
			fd.ShowApply = true;
			fd.ShowEffects = false;
			float fs = (float)mw.richTextBox1.FontSize / dpiX * 72;
			
			fd.Font = new Font(new System.Drawing.FontFamily(mw.richTextBox1.FontFamily.ToString()), fs);
			
			if (fd.ShowDialog() != System.Windows.Forms.DialogResult.Cancel){
				mw.richTextBox1.FontFamily = new System.Windows.Media.FontFamily(fd.Font.Name);
				mw.richTextBox1.FontSize = fd.Font.Size * dpiX / 72;
			    mw.richTextBox1.FontWeight = fd.Font.Bold ? FontWeights.Bold : FontWeights.Regular;
			    mw.richTextBox1.FontStyle = fd.Font.Italic ? FontStyles.Italic : FontStyles.Normal;
			    			    
			    TextDecorationCollection tdc = new TextDecorationCollection();
			    if (fd.Font.Underline) tdc.Add(TextDecorations.Underline);
			    if (fd.Font.Strikeout) tdc.Add(TextDecorations.Strikethrough);
			}
		}

	}
}