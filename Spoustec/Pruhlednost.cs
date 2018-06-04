using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace Spoustec {
    class Pruhlednost {
        [StructLayout(LayoutKind.Sequential)]
        private struct Margins {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyBottomHeight;
        }

        [DllImport("DwmApi.dll")]
        private static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd,ref Margins pMarInset);

        public static void Pruhledne(Window okno) {
            try {
                var mainWindowPtr = new WindowInteropHelper(okno).Handle;
                var mainWindowSrc = HwndSource.FromHwnd(mainWindowPtr);
                if (mainWindowSrc != null)
                    if (mainWindowSrc.CompositionTarget != null)
                        mainWindowSrc.CompositionTarget.BackgroundColor = System.Windows.Media.Color.FromArgb(0,0,0,0);

                var margins = new Margins {
                    cxLeftWidth = Convert.ToInt32(okno.Width) * Convert.ToInt32(okno.Width),
                    cxRightWidth = 0,
                    cyTopHeight = Convert.ToInt32(okno.Height) * Convert.ToInt32(okno.Height),
                    cyBottomHeight = 0
                };

                if (mainWindowSrc != null) DwmExtendFrameIntoClientArea(mainWindowSrc.Handle,ref margins);
            }
            catch (DllNotFoundException) {
                Application.Current.MainWindow.Background = Brushes.White;
            }
        }
    }
}
