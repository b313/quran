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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using ProtoBuf;
using Quran.Model;

namespace Quran.Engine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Constructor
        
        public MainWindow()
        {
            InitializeComponent();

            Engine.Run();

            GetInfo();
        }

        private void GetInfo()
        {
            infoTextBlock.Text = "رسم الخط: " + (QuranConfig.Current.IsUthmani ? "عثمانی" : "ساده");
            infoTextBlock.Text += "\tنسخه: " + QuranConfig.Current.Version;
        }
        
        #endregion

        #region Event Handlers

        private void buttonSliceMaker_Click(object sender, RoutedEventArgs e)
        {
            SlicerMain main = new SlicerMain();
            main.Show();
        }

        private void buttonAudioMapper_Click(object sender, RoutedEventArgs e)
        {
            new AudioMain().Show();
        }

        private void buttonSliceMakerTrans_Click(object sender, RoutedEventArgs e)
        {
            new TransMain().Show();
        }

        #endregion

        private void buttonUpdateMeta_Click(object sender, RoutedEventArgs e)
        {
            foreach (var qari in QuranConfig.Current.Meta.Qaris)
            {
                string audioPath = string.Format("../../Out/{0}.dat", qari.EnglishName);

                if (File.Exists(audioPath))
                {
                    AudioQuran audioQuran = null;
                    using (var file = File.OpenRead(audioPath))
                    {
                        using (CryptoStream cs = new CryptoStream(file, Crypto.AES.CreateDecryptor(), CryptoStreamMode.Read))
                        {
                            audioQuran = Serializer.Deserialize<Quran.Model.AudioQuran>(cs);
                        }
                    }

                    for (int i = 0; i < QuranConfig.Current.Meta.Suras.Length; i++)
                    {
                        if (audioQuran.AudioSuras[i].AudioSlices == null)
                            qari.Availability[i] = false;
                        else if (audioQuran.AudioSuras[i].AudioSlices[2].Start.TotalSeconds > 0)
                            qari.Availability[i] = true;
                    }
                }
            }

            Engine.SaveMeta(QuranConfig.Current.Meta);
        }

        private void buttonRemoveExtraBesm_Click(object sender, RoutedEventArgs e)
        {
            Quran.Model.Quran quran = null;

            using (var file = File.OpenRead(QuranConfig.Current.FilePath))
            {
                using (CryptoStream cs = new CryptoStream(file, Crypto.AES.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    quran = Serializer.Deserialize<Quran.Model.Quran>(cs);
                }
            }

            quran.Suras[95 - 1].Ayas[1].Slices[0].Text = quran.Suras[95 - 1].Ayas[1].Slices[0].Text.Replace("بِّسۡمِ ٱللَّهِ ٱلرَّحۡمَٰنِ ٱلرَّحِيمِ", "");
            quran.Suras[97 - 1].Ayas[1].Slices[0].Text = quran.Suras[97 - 1].Ayas[1].Slices[0].Text.Replace("بِّسۡمِ ٱللَّهِ ٱلرَّحۡمَٰنِ ٱلرَّحِيمِ", "");

            using (var file = File.Create(QuranConfig.Current.FilePath))
            {
                using (CryptoStream cs = new CryptoStream(file, Crypto.AES.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    Serializer.Serialize<Model.Quran>(cs, quran);
                }
            }

        }

        private void buttonGetMakaremTrans_Click(object sender, RoutedEventArgs e)
        {
            List<string> result = new List<string>();
            string[] lines = System.IO.File.ReadAllLines("../../In/Pre Trans/fa.makarem/Original.txt");

            int suraNo = 0;
            int ayaNo = -1;
            int j = -1;
            foreach (var line in lines)
            {
                ++j;

                string aya = line.TrimEnd().TrimStart();

                if (aya.Trim().Length == 0)
                    continue;


                if (aya == "بِسمِ اللّهِ الرَّحمانِ الرَّحيمِ" || aya == "بِسْمِ اللهِ الرَّحْمَنِ الرَّحِيمِ" || aya == "بِسمِ اللّهِ الرَّحمنِ الرَّحيمِ" || aya == "سمِ اللّهِ الرَّحمنِ الرَّحيمِ"
                    || aya == "به نام خداوند بخشنده مهربان" || aya == "بنام خداوند بخشنده مهربان")
                    continue;

                if (aya.StartsWith("▲"))
                {
                    ++suraNo;
                    int start = aya.IndexOf("[");
                    int end = aya.IndexOf("]");
                    if (Int32.Parse(aya.Substring(start + 1, end - start - 1)) != suraNo)
                        throw new ApplicationException();

                    if (suraNo == 1)
                    {
                        ayaNo = 2;
                        result.Add("1|1|بنام خداوند بخشنده مهربان");
                    }
                    else
                        ayaNo = 1;

                    continue;
                }

                string ayaTxt = aya;

                if (ayaTxt.StartsWith(ayaNo.ToString()))
                    continue;

                string endPart = string.Format("({0})", ayaNo);


                if (!ayaTxt.EndsWith(endPart))
                    throw new ApplicationException();

                ayaTxt = ayaTxt.Replace(endPart, "");

                result.Add(string.Format("{0}|{1}|{2}", suraNo, ayaNo, ayaTxt));

                ayaNo++;
            }

            int totalAyas = result.Count();

            string expected = "";
            int index = 0;
            foreach (var sura in QuranConfig.Current.Meta.Suras)
            {
                for (int ayaIndex = 1; ayaIndex <= sura.TotalAyas; ayaIndex++)
                {
                    expected = string.Format("{0}|{1}|", sura.SuraNo, ayaIndex);

                    if (!result[index].StartsWith(expected))
                        throw new InvalidDataException();

                    index++;
                }
            }
            result.Add( System.IO.File.ReadAllText( "../../In/Pre Trans/fa.makarem/footer.txt") );
            using ( var sw = new System.IO.StreamWriter("../../In/Pre Trans/fa.makarem/fa.makarem.txt") )
            {
                foreach (var item in result)
                {
                    sw.Write(item + "\n");
                }
            }

            string f = "finito";


        }

        private void buttonRepairTrans_Click(object sender, RoutedEventArgs e)
        {
            Model.TranslatedQuran trans = null;

            using (var file = File.OpenRead(QuranConfig.Current.TransPath))
            {
                using (CryptoStream cs = new CryptoStream(file, Crypto.AES.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    trans = Serializer.Deserialize<Quran.Model.TranslatedQuran>(cs);
                }
            }

            foreach (var sura in trans.Suras)
            {
                foreach (var aya in sura.Ayas)
                {
                    foreach (var slice in aya.Slices)
                    {
                        // 1:
                        // slice.Text = slice.Text.Replace(";", "؛");
                        
                        // 2:
                        //slice.Text = slice.Text.Replace("پديرفتند", "پذيرفتند");

                        // 3:
                        // slice.Text = slice.Text.Replace(" ـغير", " غير");

                        // 4:
                        // slice.Text = slice.Text.Replace("مىورزيد", "مى ورزيد");

                        // 5:
                        // slice.Text = slice.Text.Replace("ولىَّ", "ولىّ");

                        bool isExist = false;
                        if (slice.Text.Contains("مىو"))
                            isExist = true;
                    }
                    
                }

                foreach (var item in sura.AllSlices)
                {
                    // 1:
                    //item.Text = item.Text.Replace(";", "؛");
                    
                    // 2:
                    //item.Text = item.Text.Replace("پديرفتند", "پذيرفتند");

                    // 3:
                    // item.Text = item.Text.Replace(" ـغير", " غير");
                    
                    // 4:
                    // item.Text = item.Text.Replace("مىورزيد", "مى ورزيد");

                    // 5:
                    // item.Text = item.Text.Replace("ولىَّ", "ولىّ");


                    bool isExist = false;
                    if (item.Text.Contains("مىو"))
                        isExist = true;
                }
            }
            using (var file = File.Create(QuranConfig.Current.TransPath))
            {
                using (CryptoStream cs = new CryptoStream(file, Crypto.AES.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    Serializer.Serialize<Model.TranslatedQuran>(cs, trans);
                }
            }

 
        }

    }
}
