using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Quran.Util;

namespace Quran
{
    public partial class App : Application
    {
        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        private readonly SingleInstance SingleInstance = new SingleInstance(new Guid("15D910B2-1F72-44BA-85A1-AB7BC2655FFE"));

        [STAThread()]
        static void Main()
        {
            try
            {

                SplashScreen splash = new SplashScreen("Splash.png");

                splash.Show(false);
                Quran.Code.QuranProvider.Init(false);
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("ar-SA");
                System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("ar-SA");
                splash.Close(TimeSpan.FromSeconds(1));

                new App();
            }
            catch (ApplicationException)
            {
                MessageBox.Show("متاسفانه اجرای برنامه با مشکل مواجه شد", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                Quran.Code.ErrorManager.Handle("Unknown", ex);
            }

        }

        public App()
        {
            StartupUri = new System.Uri("SuraWindow.xaml", UriKind.Relative);

            Run();
        }

        protected override void OnStartup(StartupEventArgs e)
        {

            if (!SingleInstance.IsFirstInstance)
            {
                var procs = System.Diagnostics.Process.GetProcessesByName("Quran");
                foreach (var p in procs.Where(p => p.MainWindowHandle != IntPtr.Zero))
                {
                    SetForegroundWindow(p.MainWindowHandle);
                }

                Environment.Exit(0);
            }
            else
                base.OnStartup(e);
        }
    }
}
