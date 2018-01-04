using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Quran.Util;

namespace Quran
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();

            LoadSettings();
        }

        private void LoadSettings()
        {


            comboBoxTranslator.Items.Add(Model.Translator.NoTranslator);
            foreach (var translator in Quran.Code.QuranProvider.Meta.Translators)
            {
                comboBoxTranslator.Items.Add(translator);
            }

            KeyValuePair<int,string> selectedTextZoom = AppSettings.TextZooms.First();
            foreach (var textZoom in AppSettings.TextZooms)
            {
                comboBoxZoom.Items.Add(textZoom);

                if (textZoom.Key == AppSettings.TextZoom)
                    selectedTextZoom = textZoom;
            }
            
            comboBoxRepeat.SelectedIndex = AppSettings.Repeat - 1;
            comboBoxWait.SelectedIndex = AppSettings.Wait / 5;
            comboBoxTranslator.SelectedIndex = AppSettings.Translator;
            comboBoxZoom.SelectedItem = selectedTextZoom;
        }

        private void CloseCommandHandler(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        private void SaveCommandHandler(object sender, ExecutedRoutedEventArgs e)
        {
            AppSettings.Repeat = comboBoxRepeat.SelectedIndex + 1;
            AppSettings.Wait = comboBoxWait.SelectedIndex * 5;
            AppSettings.Translator = comboBoxTranslator.SelectedIndex;
            AppSettings.TextZoom = ((KeyValuePair<int, string>)comboBoxZoom.SelectedValue).Key;

            AppSettings.Save();

            this.Close();
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            this.Close();

            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
