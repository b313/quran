using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
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
using Quran.Model;

namespace Quran.Engine
{
    /// <summary>
    /// Interaction logic for SliceMaker.xaml
    /// </summary>
    public partial class SliceMaker : Window
    {
        SuraMeta suraMeta = null;

        Model.Quran quran = null;

        private readonly int ayaMinLength = 120;
        private readonly int sliceMinLength = 80;
        private readonly int sliceMaxLength = 200;

        public SliceMaker(SuraMeta suraMeta)
        {
            InitializeComponent();

            this.suraMeta = suraMeta;
            this.suraName.Text = suraMeta.NameArabic;

            LoadQuran();

            List<AyaViewModel> list = new List<AyaViewModel>();
            foreach (var aya in CurrentSura.Ayas)
            {
                list.Add(new AyaViewModel() { AyaNo = aya.AyaNo, Text = aya.AyaNo.ToString().PadLeft(3, '0') + " " + aya.ToString() });
            }

            lbAyas.ItemsSource = list;

            lbAyas.SelectedIndex = 0;
        }

        #region Property

        public Sura CurrentSura
        {
            get
            {
                return quran.Suras[suraMeta.SuraNo - 1];
            }
        }
        public Slice[] CurrentSlices
        {
            get
            {
                return quran.Suras[suraMeta.SuraNo - 1].Ayas[lbAyas.SelectedIndex].Slices;
            }
        }

        public string CurrentAyaText { get; set; }

        private AyaViewModel PreviousAyaItem
        {
            get;
            set;
        }
        private AyaViewModel CurrentAyaItem
        {
            get
            {
                return (lbAyas.SelectedItem as AyaViewModel);
            }
        }
        #endregion

        #region Event Handlers

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            SaveQuran();
        }

        private void buttonAutoSlice_Click(object sender, RoutedEventArgs e)
        {
            AutoSlice(CurrentAyaItem.AyaNo, CurrentAyaText);
        }

        private void buttonAutoSliceAll_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < suraMeta.TotalAyas; i++)
            {
                int ayaNo = i;
                if (suraMeta.SuraNo == 1 || suraMeta.SuraNo == 9)
                    ++ayaNo;

                AutoSlice(ayaNo, CurrentSura.Ayas[i].ToString());
            }

            SliceChecker();
        }

        private void buttonSliceLength_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Total: " + CurrentAyaText.Length);
            foreach (Slice item in CurrentSlices)
            {
                sb.AppendLine(item.Text.Length.ToString());
            }

            MessageBox.Show(sb.ToString());
        }

        private void buttonLetters_Click(object sender, RoutedEventArgs e)
        {
            int length = 0;
            foreach (var aya in CurrentSura.Ayas)
            {
                foreach (var slice in aya.Slices)
                {
                    length += slice.Text.Length;
                }
            }

            MessageBox.Show("No of Letters: " + length);
        }

        private void lbAyas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                richTextBox.TextChanged -= richTextBox_TextChanged;

                if (PreviousAyaItem != null )
                    SaveSlices(PreviousAyaItem);

                CurrentAyaText = "";
                flowDoc.Blocks.Clear();
                foreach (Slice slice in CurrentSlices)
                {
                    CurrentAyaText += slice.Text;
                    flowDoc.Blocks.Add(new Paragraph(new Run(slice.Text)));
                }

                PreviousAyaItem = CurrentAyaItem;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                richTextBox.TextChanged += richTextBox_TextChanged;
            }

        }

        private void richTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (lbAyas.SelectedItem != null)
            {
                AyaViewModel item = lbAyas.SelectedItem as AyaViewModel;
                item.IsDirty = true;
                lbAyas.Items.Refresh();
            }
        }

        private void richTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                richTextBox.Selection.Select(richTextBox.CaretPosition, richTextBox.CaretPosition);
                if (e.Key != Key.Enter && e.Key != Key.Back &&
                    e.Key != Key.Left && e.Key != Key.Right &&
                    e.Key != Key.Up && e.Key != Key.Down &&
                    e.Key != Key.Home && e.Key != Key.End)
                {
                    e.Handled = true;
                }


                if (!richTextBox.CaretPosition.IsAtLineStartPosition && e.Key == Key.Back)
                    e.Handled = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void richTextBox_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            richTextBox.Selection.Select(richTextBox.CaretPosition, richTextBox.CaretPosition);
        }

        #endregion

        #region Methods

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

        private void SaveQuran()
        {

            try
            {
                SaveSlices(CurrentAyaItem);

                int sliceIndex = 0;
                foreach (var aya in quran.Suras[suraMeta.SuraNo - 1].Ayas)
                {
                    foreach (var slice in aya.Slices)
                    {
                        slice.SliceIndex = sliceIndex++;
                    }
                }

                using (var file = File.Create(QuranConfig.Current.FilePath))
                {
                    using (CryptoStream cs = new CryptoStream(file, Crypto.AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        Serializer.Serialize<Model.Quran>(cs, quran);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private void SaveSlices(AyaViewModel ayaItem)
        {
            try
            {
                if (ayaItem.IsDirty)
                {
                    List<Slice> slices = new List<Slice>();
                    int sliceNo = 1;
                    foreach (Paragraph item in flowDoc.Blocks)
                    {
                        string text = (item.Inlines.FirstInline as Run).Text;
                        if (text.Trim().Length > 0)
                        {
                            slices.Add(new Slice() { SliceID = suraMeta.SuraNo * 1000000 + ayaItem.AyaNo * 1000 + sliceNo, AyaNo = ayaItem.AyaNo, Text = text });
                            ++sliceNo;
                        }
                    }

                    if ( suraMeta.SuraNo == 1 || suraMeta.SuraNo == 9 )
                        quran.Suras[suraMeta.SuraNo - 1].Ayas[ayaItem.AyaNo-1].Slices = slices.ToArray();
                    else
                        quran.Suras[suraMeta.SuraNo - 1].Ayas[ayaItem.AyaNo].Slices = slices.ToArray();

                    lbAyas.Items.Refresh();
                }

                ayaItem.IsDirty = false;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void buttonSliceChecker_Click(object sender, RoutedEventArgs e)
        {
            SliceChecker();
        }

        private void SliceChecker()
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder extra = new StringBuilder();

            int totalSlices = 0;

            int sliceIndex = 0;
            foreach (var aya in quran.Suras[suraMeta.SuraNo - 1].Ayas)
            {
                sb.Append("AyaNo: " + aya.AyaNo + " Slices: ");

                if (aya.Slices.Length > 1)
                    extra.Append(aya.AyaNo + "\t");

                foreach (var slice in aya.Slices)
                {
                    ++totalSlices;
                    sb.Append(" - " + slice.SliceID);
                    slice.SliceIndex = sliceIndex++;
                }
                sb.AppendLine();
            }

            string info = string.Format("Ayas: {0} \t Slices: {1}\r\n", quran.Suras[suraMeta.SuraNo - 1].Ayas.Length, totalSlices);
            info += "-------------------------------\r\n";
            info += extra.ToString() + "\r\n";
            info += "-------------------------------\r\n";
            sb.Insert(0, info);

            string filename = suraMeta.SuraNo.ToString().PadLeft(3, '0') + "-Slices.txt";
            File.WriteAllText(filename, sb.ToString());

            System.Diagnostics.Process.Start(filename);
        }


        private void AutoSlice(int ayaNo, string ayaText)
        {
            try
            {

                string[] slices = new string[1] { ayaText };

                if (ayaText.Length > ayaMinLength)
                {
                    slices = ayaText.Replace(QuranConfig.stop_Mim.ToString(), QuranConfig.stop_Mim.ToString() + " @@").
                                                Replace(QuranConfig.stop_Gholi.ToString(), QuranConfig.stop_Gholi.ToString() + " ##").
                                                Split(new string[] { "@@ ", "## " }, StringSplitOptions.None);
                }

                string[] newSlices = slices;
                do
                {
                    slices = newSlices;
                    newSlices = SliceBy(slices, QuranConfig.stop_Jim);
                } while (newSlices.Length != slices.Length);

                do
                {
                    slices = newSlices;
                    newSlices = SliceBy(slices, QuranConfig.stop_Sali);
                } while (newSlices.Length != slices.Length);


                Slice[] result = new Slice[newSlices.Length];
                for (int i = 0; i < result.Length; i++)
                {
                    int sliceNo = i + 1;
                    if (ayaNo == 0)
                        result[i] = new Slice() { SliceID = suraMeta.SuraNo * 1000000, AyaNo = ayaNo, Text = newSlices[i] };
                    else
                        result[i] = new Slice() { SliceID = suraMeta.SuraNo * 1000000 + ayaNo * 1000 + sliceNo, AyaNo = ayaNo, Text = newSlices[i] };
                }

                if ( suraMeta.SuraNo == 1 || suraMeta.SuraNo == 9 )
                    CurrentSura.Ayas[ayaNo-1].Slices = result;
                else
                    CurrentSura.Ayas[ayaNo].Slices = result;
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        private string[] SliceBy(string[] slices, char slicerChar)
        {
            try
            {

                List<string> orginal = slices.ToList();
                List<string> result = slices.ToList();

                int sliceIndex = 0;
                bool stop = false;
                foreach (string slice in orginal)
                {
                    if (slice.Length >= sliceMaxLength && slice.Contains(slicerChar))
                    {
                        int slicerIndex = slice.IndexOf(slicerChar);

                        do
                        {
                            if (slicerIndex + 1 == slice.Length || slicerIndex + 2 == slice.Length)   // It is not the last slicerChar ( with & without Space )
                                break;

                            if (slicerIndex > sliceMinLength)
                            {
                                string part1 = slice.Substring(0, slicerIndex + 2);
                                string part2 = slice.Substring(slicerIndex + 2);
                                if (part1[part1.Length - 1] != ' ')
                                    throw new ApplicationException("There is no space after Slicer Char!");
                                result[sliceIndex] = part1;
                                result.Insert(sliceIndex + 1, part2);
                                stop = true;
                                break;
                            }

                            slicerIndex = slice.IndexOf(slicerChar, slicerIndex + 1);

                        } while (slicerIndex != -1);

                        if (stop)
                            break;
                    }
                    ++sliceIndex;
                }

                return result.ToArray();

            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        #endregion

        private class AyaViewModel
        {
            public int AyaNo { get; set; }

            public string Text { get; set; }

            public bool IsDirty { get; set; }

            public SolidColorBrush Color
            {
                get
                {
                    if (IsDirty)
                        return Brushes.Red;
                    return Brushes.Black;
                }
            }

            public override string ToString()
            {
                return  Text ;
            }
        }

    }
}
