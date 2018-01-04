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
    public partial class TransMapper : Window
    {
        SuraMeta suraMeta = null;
        Model.Quran quran = null;
        Model.TranslatedQuran trans = null;

        private readonly int ayaMinLength = 120;
        private readonly int sliceMinLength = 80;
        private readonly int sliceMaxLength = 200;
        private int currentAya = -1;

        public TransMapper(Model.Quran quran, Model.TranslatedQuran trans, SuraMeta suraMeta)
        {
            InitializeComponent();

            this.quran = quran;
            this.trans = trans;
            this.suraMeta = suraMeta;
            this.suraName.Text = suraMeta.NameArabic;

            LoadAyas();
        }


        #region Property


        public Sura CurrentSura
        {
            get
            {
                return quran.Suras[suraMeta.SuraNo-1];
            }
        }

        public Slice[] CurrentSlices
        {
            get
            {
                int i = (int)cbAyas.SelectedValue;
                
                if (suraMeta.SuraNo == 1 || suraMeta.SuraNo == 9 )
                    return quran.Suras[suraMeta.SuraNo - 1].Ayas[ i - 1].Slices;

                return quran.Suras[suraMeta.SuraNo - 1].Ayas[i].Slices;
            }
        }

        public TranslatedSlice[] CurrentTranslatedSlices
        {
            get
            {
                int i = (int)cbAyas.SelectedValue;

                if (suraMeta.SuraNo == 1 || suraMeta.SuraNo == 9)
                    return trans.Suras[suraMeta.SuraNo - 1].Ayas[i - 1].Slices;

                return trans.Suras[suraMeta.SuraNo - 1].Ayas[i].Slices;
            }
        }

        public string CurrentAyaText { get; set; }


        #endregion

        #region Event Handlers

        private void cbAyas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SaveSlices();

            Save();

            CurrentAyaText = "";
            flowDoc.Blocks.Clear();
            flowDocTrans.Blocks.Clear();

            foreach (Slice slice in CurrentSlices)
            {
                CurrentAyaText += slice.Text;
                flowDoc.Blocks.Add(new Paragraph(new Run(slice.Text)));
            }

            foreach (TranslatedSlice slice in CurrentTranslatedSlices)
            {
                flowDocTrans.Blocks.Add(new Paragraph(new Run(slice.Text)));
            }

            currentAya = (int)cbAyas.SelectedValue;
        }

        private void SaveSlices()
        {
            if (currentAya < 0)
                return;

            int sliceNo = 1;

            List<TranslatedSlice> slices = new List<TranslatedSlice>();
            foreach (Paragraph item in flowDocTrans.Blocks)
            {
                string text = new TextRange(item.ContentStart, item.ContentEnd).Text;
                if (text.Trim().Length > 0)
                {
                    slices.Add(new TranslatedSlice() { SliceID = suraMeta.SuraNo * 1000000 + currentAya * 1000 + sliceNo, Text = text });
                }
                
                sliceNo++;
            }

            if ( suraMeta.SuraNo == 1 || suraMeta.SuraNo == 9  )
                trans.Suras[suraMeta.SuraNo - 1].Ayas[currentAya-1].Slices = slices.ToArray();
            else
                trans.Suras[suraMeta.SuraNo - 1].Ayas[currentAya].Slices = slices.ToArray();

            slices = new List<TranslatedSlice>();
            foreach (var aya in trans.Suras[suraMeta.SuraNo - 1].Ayas)
            {
                foreach (var slice in aya.Slices)
                {
                    slices.Add(slice);
                }
            }
            trans.Suras[suraMeta.SuraNo - 1].AllSlices = slices.ToArray();

        }

        private void checkBoxFilter_Checked(object sender, RoutedEventArgs e)
        {
            LoadAyas();
        }

        private void richTextBoxTrans_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            richTextBoxTrans.Selection.Select(richTextBoxTrans.CaretPosition, richTextBoxTrans.CaretPosition);
            if (e.Key != Key.Enter && e.Key != Key.Back &&
                e.Key != Key.Left && e.Key != Key.Right &&
                e.Key != Key.Up && e.Key != Key.Down &&
                e.Key != Key.Home && e.Key != Key.End)
            {
                e.Handled = true;
            }


            if (!richTextBoxTrans.CaretPosition.IsAtLineStartPosition && e.Key == Key.Back)
                e.Handled = true;
        }

        private void richTextBoxTrans_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            richTextBoxTrans.Selection.Select(richTextBoxTrans.CaretPosition, richTextBoxTrans.CaretPosition);
        }



        #endregion

        #region Methods

        private void LoadAyas()
        {
            foreach (Aya aya in CurrentSura.Ayas)
            {
                if ( aya.Slices.Length > 1)
                    cbAyas.Items.Add(aya.AyaNo);
            }
        }

        private void Save()
        {
            using (var file = File.Create(QuranConfig.Current.TransPath))
            {
                using (CryptoStream cs = new CryptoStream(file, Crypto.AES.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    Serializer.Serialize<Model.TranslatedQuran>(cs, trans);
                }
            }
        }

        #endregion

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            SaveSlices();

            Save();

        }

        private void buttonAllowEdit_Click(object sender, RoutedEventArgs e)
        {
            richTextBoxTrans.PreviewKeyDown -= richTextBoxTrans_PreviewKeyDown;
            richTextBoxTrans.PreviewMouseUp -= richTextBoxTrans_PreviewMouseUp;
            richTextBoxTrans.Background = Brushes.Orange;
        }

    }
}
