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
    public partial class SlicerMain : Window
    {
        public SlicerMain()
        {
            InitializeComponent();

            LoadSura();
        }

        #region Event Handlers
        
        private void buttonLock_Click(object sender, RoutedEventArgs e)
        {
            LockSura();
        }

        private void buttonUnLock_Click(object sender, RoutedEventArgs e)
        {
            UnLockSura();
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        #endregion

        #region Methods

        private void LoadSura()
        {
            int si = lbSuras.SelectedIndex;

            if (si == -1)
                si = 0;

            lbSuras.Items.Clear();

            BitmapImage lockBitmap = new BitmapImage();
            lockBitmap.BeginInit();
            lockBitmap.UriSource = new Uri("../../content/padlock.png", UriKind.RelativeOrAbsolute);
            lockBitmap.EndInit();

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
                Image lockImg = new Image();
                lockImg.Source = lockBitmap;
                lockImg.Height = 16;

                Image hasSlicesImg = new Image();
                hasSlicesImg.Source = hasSlicesBitmap;
                hasSlicesImg.Height = 16;

                string text = sura.SuraNo.ToString().PadLeft(3, '0') + "   " + sura.NameArabic.ToString();
                TextBlock tb = new TextBlock();
                if (QuranConfig.Current.EngineeInfo.LockedSuras[sura.SuraNo - 1])
                {
                    tb.Inlines.Add(lockImg);
                    tb.Inlines.Add("   ");
                    tb.Foreground = Brushes.LightGray;
                }
                else
                    tb.Inlines.Add("     ");

                bool hasSlices = false;
                foreach (var aya in quran.Suras[sura.SuraNo - 1].Ayas)
                {
                    if ( aya.Slices.Count() > 1 )
                    {
                        hasSlices = true;
                        break;
                    }
                }

                if (hasSlices)
                {
                    tb.Inlines.Add(hasSlicesImg);
                    tb.Inlines.Add("   ");
                    tb.Foreground = Brushes.LightGray;
                }
                else
                    tb.Inlines.Add("     ");

                tb.Inlines.Add(text);

                lbSuras.Items.Add(tb);
            }

            lbSuras.SelectedIndex = si;
        }

        private void LockSura()
        {
            QuranConfig.Current.EngineeInfo.LockedSuras[lbSuras.SelectedIndex] = true;
            LoadSura();
        }

        private void UnLockSura()
        {
            QuranConfig.Current.EngineeInfo.LockedSuras[lbSuras.SelectedIndex] = false;
            LoadSura();
        }

        private void Save()
        {
            using (var file = File.Create(QuranConfig.Current.EngineeInfoPath))
            {
                    Serializer.Serialize<EngineeInfo>(file, QuranConfig.Current.EngineeInfo);
            }
        }

        #endregion

        private void lbSuras_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int si = lbSuras.SelectedIndex;
            if (!QuranConfig.Current.EngineeInfo.LockedSuras[si])
                new SliceMaker(QuranConfig.Current.Meta.Suras[si]).ShowDialog();
            else
                MessageBox.Show("This Sura is Locked!");
        }

    }
}
