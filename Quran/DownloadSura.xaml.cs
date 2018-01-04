using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Quran.Code;

namespace Quran
{
    /// <summary>
    /// Interaction logic for DownloadSura.xaml
    /// </summary>
    public partial class DownloadSura : Window
    {
        int suraNo = -1;
        string url = "";
        string destPath = "";

        WebClient webClient = new WebClient();

        public DownloadSura( QariViewModel qari, int suraNo )
        {
            InitializeComponent();

            this.suraNo = suraNo;


            destPath = string.Format("Sounds//{0}", qari.Qari.EnglishName );

            GetDownloadURL( qari );

            DownlaodSura();
        }

        public bool HasError { get; set; }

        public bool IsDownloaded { get; set; }

        public string FileName
        {
            get
            {
                return suraNo.ToString().PadLeft(3, '0') + ".mp3";
            }
        }

        private void GetDownloadURL(QariViewModel qari )
        {
            using (var file = Assembly.GetExecutingAssembly().GetManifestResourceStream( "Quran.Data." + qari.Qari.EnglishName + ".lst"))
            {
                using (var tr = new StreamReader(file))
                {
                    int lineNo = 0;
                    while (lineNo < suraNo - 1)
                    {
                        tr.ReadLine();
                        ++lineNo;
                    }

                    url = tr.ReadLine();
                }
            }
        }

        private void DownlaodSura()
        {
            Directory.CreateDirectory(destPath);

            string path = System.IO.Path.Combine(destPath, "_" + FileName);


            if (File.Exists(path))
                File.Delete(path);

            webClient.DownloadFileCompleted += client_DownloadFileCompleted;
            webClient.DownloadProgressChanged += client_DownloadProgressChanged;
            webClient.DownloadFileAsync(new Uri(url), path);
        }

        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressbar.Value = e.ProgressPercentage;
        }

        void client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            string tempPath = System.IO.Path.Combine(destPath, "_" + FileName);

            if (e.Error != null )
            {
                HasError = true;
                IsDownloaded = false;
                this.Close();
            }
            else if (!e.Cancelled  )
            {
                string path = System.IO.Path.Combine(destPath, FileName);
                System.IO.File.Move(tempPath, path);
                IsDownloaded = true;
                this.Close();
            }
            else
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        private void CloseCommandHandler(object sender, ExecutedRoutedEventArgs e)
        {
            webClient.CancelAsync();
            IsDownloaded = false;
            this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            webClient.Dispose();

            base.OnClosed(e);
        }

    }
}
