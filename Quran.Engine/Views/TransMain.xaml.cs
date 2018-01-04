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

namespace Quran.Engine
{
    /// <summary>
    /// Interaction logic for MainSlicer.xaml
    /// </summary>
    public partial class TransMain : Window
    {
        Model.Quran quran = null;
        Model.TranslatedQuran trans = null;

        public TransMain()
        {
            InitializeComponent();

            LoadQuran();
            
            LoadTrans();

            LoadSuras();
        }

        #region Event Handlers

        private void lbSuras_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int si = lbSuras.SelectedIndex;
            if (!QuranConfig.Current.EngineeInfo.LockedSuras[si])
            {
                MessageBox.Show("Slicing has not been applied!");
                return;
            }

            var sura = QuranConfig.Current.Meta.Suras[si];
            TransMapper mapper = new TransMapper(quran, trans, sura);
            mapper.ShowDialog();

            LoadTrans();

            LoadSuras();
        }

        #endregion

        #region Methods

        private void LoadSuras()
        {
            int si = lbSuras.SelectedIndex;

            if (si == -1)
                si = 0;

            lbSuras.Items.Clear();

            BitmapImage hasSlicesBitmap = new BitmapImage();
            hasSlicesBitmap.BeginInit();
            hasSlicesBitmap.UriSource = new Uri("../../content/HasSlices.png", UriKind.RelativeOrAbsolute);
            hasSlicesBitmap.EndInit();

            // Load Quran
            Quran.Model.Quran quran = null;
            using (var file = File.OpenRead(QuranConfig.Current.FilePath))
            {
                using (CryptoStream cs = new CryptoStream(file, Crypto.AES.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    quran = Serializer.Deserialize<Quran.Model.Quran>(cs);
                }
            }

            foreach (var sura in QuranConfig.Current.Meta.Suras)
            {
                Image hasSlicesImg = new Image();
                hasSlicesImg.Source = hasSlicesBitmap;
                hasSlicesImg.Height = 16;

                bool hasSlices = false;
                int sliceNo = 0;
                foreach (var aya in quran.Suras[sura.SuraNo - 1].Ayas)
                {
                    sliceNo += aya.Slices.Count();
                    if (aya.Slices.Count() > 1)
                    {
                        hasSlices = true;
                    }
                }

                string text = sura.SuraNo.ToString().PadLeft(3, '0') + "   " + sura.NameArabic.ToString();
                TextBlock tb = new TextBlock();

                if (hasSlices)
                {
                    tb.Inlines.Add(hasSlicesImg);
                    tb.Inlines.Add("   ");
                    tb.Foreground = Brushes.LightGray;
                }
                else
                    tb.Inlines.Add("     ");


                if (!QuranConfig.Current.EngineeInfo.LockedSuras[sura.SuraNo - 1])
                    tb.Foreground = Brushes.LightGray;
                else
                    tb.Foreground = Brushes.Black;

                tb.Inlines.Add(text);

                if ( trans.Suras[sura.SuraNo-1].AllSlices.Count() != sliceNo )
                    tb.Inlines.Add(" *");
                
                lbSuras.Items.Add(tb);
            }

            lbSuras.SelectedIndex = si;
        }

        private void LoadQuran()
        {
            using (var file = File.OpenRead(QuranConfig.Current.FilePath))
            {
                using (CryptoStream cs = new CryptoStream(file, Crypto.AES.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    quran = Serializer.Deserialize<Quran.Model.Quran>(cs);
                }
            }
        }

        private void LoadTrans()
        {
            using (var file = File.OpenRead(QuranConfig.Current.TransPath))
            {
                using (CryptoStream cs = new CryptoStream(file, Crypto.AES.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    trans = Serializer.Deserialize<Quran.Model.TranslatedQuran>(cs);
                }
            }
        }

        #endregion


    }
}
