using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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
using ProtoBuf;
using Quran.Code;
using Quran.Model;
using Quran.Util;

namespace Quran
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class QariSelector : Window
    {
        private int suraNo;
        public QariSelector( int suraNo )
        {
            InitializeComponent();

            this.suraNo = suraNo;

            LoadQaris( suraNo );
        }

        private void LoadQaris( int suraNo )
        {

            List<QariViewModel> list = new List<QariViewModel>();

            foreach (var qari in Quran.Code.QuranProvider.Meta.Qaris)
            {

                QariViewModel qariItem = new QariViewModel();
                qariItem.ID = qari.ID;
                qariItem.Name = qari.Name;
                qariItem.Qari = qari;
                qariItem.ImageSource = string.Format("./images/qaris/{0}.png", qari.ID);
                qariItem.IsAvailable = qari.ID == 0 || qari.Availability[suraNo - 1];

                if (qariItem.IsAvailable)
                {
                    if (qari.ID != 0)
                    {
                        string audioFile = string.Format("Sounds//{0}//{1}.mp3", qari.EnglishName, suraNo.ToString().PadLeft(3, '0'));
                        if (System.IO.File.Exists(audioFile))
                            qariItem.IsExist = true;
                    }
                    else
                        qariItem.IsExist = true;
                }

                list.Add(qariItem);
            }

            listBox.ItemsSource = list;

            listBox.SelectedIndex = AppSettings.Qari;
        }

        private void CloseCommandHandler(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        private void OpenCommandHandler(object sender, ExecutedRoutedEventArgs e)
        {
            SelectQari();
        }

        private void listBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SelectQari();

        }

        private void SelectQari()
        {
            var selected = listBox.SelectedValue as QariViewModel;

            if (!selected.IsAvailable)
                return;


            if (!selected.IsExist)
            {
                var downloadSura = new DownloadSura(selected, suraNo);

                downloadSura.ShowDialog();

                if (downloadSura.IsDownloaded)
                {
                    AppSettings.Qari = listBox.SelectedIndex;
                    this.Close();
                }

                if ( downloadSura.HasError )
                    MessageBox.Show("خطا در دانلود، لطفاً از اتصال به اینترنت مطمئن شوید", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                AppSettings.Qari = listBox.SelectedIndex;
                this.Close();
            }
        }
    }

    public class QariViewModel
    {
        static SolidColorBrush color = new SolidColorBrush( Color.FromRgb(0x0D, 0x73,0x82) );
        public int ID { get; set; }

        public string Name { get; set; }

        public Qari Qari { get; set; }

        public string ImageSource { get; set; }

        public bool IsAvailable { get; set; }

        public bool IsExist { get; set; }

        public string DownloadURL { get; set; }

        public Brush TextColor
        {
            get
            {
                if (IsAvailable)
                    return Brushes.Black;
                else
                    return Brushes.LightGray;
            }
        }

        public UIElement Icon
        {
            get
            {
                Ellipse ellipse = new Ellipse();

                if (IsExist)
                {
                    ellipse.Height = 10;
                    ellipse.Width = 10;
                    ellipse.StrokeThickness = 5;
                    ellipse.Stroke = color;
                }

                return ellipse;
            }
        }
    }
}
