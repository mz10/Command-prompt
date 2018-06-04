using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Spoustec {
    class Ikony {

        //ikony z dll
        [DllImport("shell32.dll")]
        static extern IntPtr ExtractAssociatedIcon(IntPtr hInst,StringBuilder lpIconPath,out ushort lpiIcon);

        [DllImport("shell32.dll",EntryPoint = "ExtractIconEx")]
        private static extern int ExtractIconExA(string lpszFile,int nIconIndex,ref IntPtr phiconLarge,ref IntPtr phiconSmall,int nIcons);

        [DllImport("user32.dll",SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        public static ImageSource Ikona(string soubor,int index,bool s) {
            IntPtr largeIcon = IntPtr.Zero;
            IntPtr smallIcon = IntPtr.Zero;

            if (s) ExtractIconExA(Environment.SystemDirectory + "\\" + soubor,index,ref largeIcon,ref smallIcon,1);
            else ExtractIconExA(soubor,index,ref largeIcon,ref smallIcon,1);

            System.Drawing.Icon ikona = null;
            if (smallIcon != IntPtr.Zero) {
                ikona = System.Drawing.Icon.FromHandle(smallIcon);

                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                                ikona.Handle,
                                System.Windows.Int32Rect.Empty,
                                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            }
            return null;
        }

        public static ImageSource Ikona2(string soubor) {
            StringBuilder sbcesta = new StringBuilder(soubor);
            ushort c = (ushort)0;

            System.Drawing.Icon ikona = System.Drawing.Icon.FromHandle(ExtractAssociatedIcon(IntPtr.Zero,sbcesta,out c));

            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                            ikona.Handle,
                            System.Windows.Int32Rect.Empty,
                            System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
        }


        public static byte[] Barvy(System.Windows.Media.Brush stetec) {
            byte A = ((System.Windows.Media.Color)stetec
                      .GetValue(SolidColorBrush.ColorProperty)).A;
            byte R = ((System.Windows.Media.Color)stetec
                      .GetValue(SolidColorBrush.ColorProperty)).R;
            byte G = ((System.Windows.Media.Color)stetec
                      .GetValue(SolidColorBrush.ColorProperty)).G;
            byte B = ((System.Windows.Media.Color)stetec
                      .GetValue(SolidColorBrush.ColorProperty)).B;

            byte[] vysledek = { A,R,G,B };

            return vysledek;
        }
    }
}
