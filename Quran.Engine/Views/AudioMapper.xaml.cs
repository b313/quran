using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;
using ProtoBuf;
using Quran.Model;

namespace Quran.Engine
{
    /// <summary>
    /// Interaction logic for AudioMapper.xaml
    /// </summary>
    public partial class AudioMapper : Window
    {
        private bool mediaPlayerIsPlaying = false;
        private bool userIsDraggingSlider = false;

        SuraMeta suraMeta = null;
        Qari qari = null;

        AudioQuran audioQuran = null;

        public AudioMapper( SuraMeta suraMeta, Qari qari )
        {
            InitializeComponent();

            this.suraMeta = suraMeta;
            this.qari = qari;

            suraTextBlock.Text = suraMeta.SuraNo + " " + suraMeta.NameArabic;
            qariTextBlock.Text = qari.Name;

            DispatcherTimer timer = new DispatcherTimer();
                        timer.Interval = TimeSpan.FromMilliseconds(100);
                        timer.Tick += timer_Tick;
                        timer.Start();

            LoadSlices(suraMeta);

            LoadAudioQuran();

        }

        #region Properties

        ObservableCollection<TempAudioSlice> slices = new ObservableCollection<TempAudioSlice>();
        public ObservableCollection<TempAudioSlice> Slices
        {
            get
            {
                return slices;
            }
        }

        private TempAudioSlice CurrentAudioSlice
        {
            get
            {
                return listBox.SelectedItem as TempAudioSlice;
            }
        }

        private AudioSura AudioSura
        {
            get
            {
                return audioQuran.AudioSuras[suraMeta.SuraNo - 1];
            }
        }

        private TimeSpan ResyncValue
        {
            get
            {
                switch (resyncComboBox.SelectedIndex)
                {
                    case 0:
                        return TimeSpan.FromMilliseconds(100);
                    case 1:
                        return TimeSpan.FromMilliseconds(250);
                    case 2:
                        return TimeSpan.FromMilliseconds(500);
                    case 3:
                        return TimeSpan.FromMilliseconds(1000);
                }

                return TimeSpan.FromMilliseconds(0);
            }
        }

        public string AudioPath
        {
            get
            {
                return string.Format("../../Out/{0}.dat", qari.EnglishName);
            }
        }

        #endregion

        #region Event Handlers

        private void loadButton_Click(object sender, RoutedEventArgs e)
        {
            //Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            //dlg.DefaultExt = ".mp3";
            //dlg.Filter = "MP3 Files (*.mp3)|*.mp3";

            //Nullable<bool> result = dlg.ShowDialog();

            //if (result == true)
            //{
            //    mediaElement.Source = new Uri( dlg.FileName );
            //}
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if ((mePlayer.Source != null) && (mePlayer.NaturalDuration.HasTimeSpan) && (!userIsDraggingSlider))
            {
                sliProgress.Minimum = 0;
                sliProgress.Maximum = mePlayer.NaturalDuration.TimeSpan.TotalSeconds;
                sliProgress.Value = mePlayer.Position.TotalSeconds;
            }
        }

        private void Open_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Media files (*.mp3;*.mpg;*.mpeg)|*.mp3;*.mpg;*.mpeg|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
                mePlayer.Source = new Uri(openFileDialog.FileName);
        }

        private void Play_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (mePlayer != null) && (mePlayer.Source != null);
        }

        private void Play_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Play();
        }

        private void Pause_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = mediaPlayerIsPlaying;
        }

        private void Pause_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            mePlayer.Pause();
        }

        private void Stop_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = mediaPlayerIsPlaying;
        }

        private void Stop_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            mePlayer.Stop();
            mediaPlayerIsPlaying = false;
        }

        private void sliProgress_DragStarted(object sender, DragStartedEventArgs e)
        {
            userIsDraggingSlider = true;
        }

        private void sliProgress_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            userIsDraggingSlider = false;
            mePlayer.Position = TimeSpan.FromSeconds(sliProgress.Value);
        }

        private void sliProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            lblProgressStatus.Text = TimeSpan.FromSeconds(sliProgress.Value).ToString(@"hh\:mm\:ss\.fff");
        }

        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            mePlayer.Volume += (e.Delta > 0) ? 0.1 : -0.1;
        }

        private void listBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            RePlay();
        }

        private void resyncBackButton_Click(object sender, RoutedEventArgs e)
        {
            Backward();
        }

        private void resyncForwardButton_Click(object sender, RoutedEventArgs e)
        {
            Forward();
        }

        private void breakPointButton_Click(object sender, RoutedEventArgs e)
        {
            BreakPoint();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                BreakPoint();
            }
            else if ( e.KeyboardDevice.Modifiers == ModifierKeys.Control )
            {
                if (e.Key == Key.Subtract)
                {
                    e.Handled = true;
                    BackwardAll();
                }
                else if (e.Key == Key.Add)
                {
                    e.Handled = true;
                    ForwardAll();
                }
            }
            else if (e.Key == Key.Subtract)
            {
                e.Handled = true;
                Backward();
            }
            else if (e.Key == Key.Add)
            {
                e.Handled = true;
                Forward();
            }
            else if (e.Key == Key.F)
            {
                e.Handled = true;
                SpeedUp();
            }
            else if (e.Key == Key.B)
            {
                e.Handled = true;
                SpeedDown();
            }
            else if (e.Key == Key.Space)
            {
                e.Handled = true;
                SpeedNormal();
            }
            else if (e.Key == Key.R)
            {
                e.Handled = true;
                RePlay();
            }
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void verseByVerseLoad_Click(object sender, RoutedEventArgs e)
        {
            var dlg =new Microsoft.Win32.OpenFileDialog();
            if ( dlg.ShowDialog() == true) 
            {

                string audioPath = dlg.FileName.Replace("\\Audio Timing", "");
                audioPath = System.IO.Path.ChangeExtension( audioPath, "mp3" );

                mePlayer.Source = new Uri(audioPath);
                var lines = System.IO.File.ReadAllLines(dlg.FileName);

                int lineNo = 0;
                for (int i = 0; i < listBox.Items.Count-1; i++)
                {
                    TempAudioSlice tas = listBox.Items[i+1] as TempAudioSlice;
                    if ( tas.SliceNo == 1 )
                        tas.Position = new TimeSpan(Convert.ToInt64(lines[lineNo++] ) * 10000);
                }

                Play();
            }
        }

        private void backwardAllButton_Click(object sender, RoutedEventArgs e)
        {
            BackwardAll();
        }

        private void forwardAllButton_Click(object sender, RoutedEventArgs e)
        {
            ForwardAll();
        }

        private void speedDownButton_Click(object sender, RoutedEventArgs e)
        {
            SpeedDown();
        }

        private void speedNormalButton_Click(object sender, RoutedEventArgs e)
        {
            SpeedNormal();
        }

        private void speedUpButton_Click(object sender, RoutedEventArgs e)
        {
            SpeedUp();
        }




        #endregion

        #region Methods

        private void BreakPoint()
        {
            var position = mePlayer.Position;
            if (listBox.SelectedIndex >= listBox.Items.Count - 1)
                return;

            listBox.SelectedIndex++;
            
            //TimeSpan extra = new TimeSpan(0, 0, 0, 0, 500);

            CurrentAudioSlice.Position = position;//extra.Add(position);
        }

        private void Backward()
        {
            mePlayer.Pause();
            CurrentAudioSlice.Position = CurrentAudioSlice.Position.Subtract(ResyncValue);
            mePlayer.Position = CurrentAudioSlice.Position;
            mePlayer.Play();
        }

        private void Forward()
        {
            mePlayer.Pause();
            CurrentAudioSlice.Position = CurrentAudioSlice.Position.Add(ResyncValue);
            mePlayer.Position = CurrentAudioSlice.Position;
            mePlayer.Play();
        }

        private void BackwardAll()
        {
            mePlayer.Pause();

            int shift = Int32.Parse(shiftTextBox.Text) * -1;
            for (int i = listBox.SelectedIndex; i < listBox.Items.Count; i++)
            {
                var item = listBox.Items[i] as TempAudioSlice;
                item.Position = item.Position.Add(TimeSpan.FromMilliseconds(shift));
            }
            
            mePlayer.Position = CurrentAudioSlice.Position;
            mePlayer.Play();
        }

        private void ForwardAll()
        {
            mePlayer.Pause();

            int shift = Int32.Parse(shiftTextBox.Text);
            for (int i = listBox.SelectedIndex; i < listBox.Items.Count; i++)
            {
                var item = listBox.Items[i] as TempAudioSlice;
                item.Position = item.Position.Add(TimeSpan.FromMilliseconds(shift));
            }

            mePlayer.Position = CurrentAudioSlice.Position;
            mePlayer.Play();

        }


        private void LoadSlices(SuraMeta suraMeta)
        {
            Model.Sura sura = null;

            using (var file = File.OpenRead( QuranConfig.Current.FilePath ))
            {
                using (CryptoStream cs = new CryptoStream(file, Crypto.AES.CreateDecryptor(), CryptoStreamMode.Read))
                {

                    sura = Serializer.Deserialize<Quran.Model.Quran>(cs).Suras[suraMeta.SuraNo - 1];
                }
            }

            int index = 0;

            //if (suraMeta.SuraNo != 9 && suraMeta.SuraNo != 1)
            //{
            //    slices.Insert(index, new TempAudioSlice() { SliceID = suraMeta.SuraNo * 1000000 , Slice = "بِسۡمِ ٱللَّهِ ٱلرَّحۡمَٰنِ ٱلرَّحِيمِ", SliceIndex = index, Position = new TimeSpan(0) });
            //    ++index;
            //}

            foreach (var aya in sura.Ayas)
            {
                foreach (Slice slice in aya.Slices)
                {
                    string text = slice.Text;
                    
                    TempAudioSlice tempAudioSlice = new TempAudioSlice();
                    tempAudioSlice.SliceID = slice.SliceID;
                    tempAudioSlice.Slice = text;
                    tempAudioSlice.SliceIndex = index;
                    tempAudioSlice.Position = new TimeSpan(0);
                    tempAudioSlice.HasMultiSlices = (aya.Slices.Length > 1);

                    slices.Add(tempAudioSlice);
                    ++index;
                }
            }


            listBox.ItemsSource = Slices;
            listBox.SelectedIndex = 0;
        }

        private void LoadAudioQuran()
        {
            if (!File.Exists(AudioPath))
            {
                audioQuran = new AudioQuran() { Qari = qari, Album = "Default", LastUpdate = DateTime.Now.ToShortDateString() };
                audioQuran.AudioSuras = new AudioSura[114];
                for (int i = 0; i < 114; i++)
                {
                    audioQuran.AudioSuras[i] = new AudioSura();
                }
            }
            else
            {
                using (var file = File.OpenRead(AudioPath))
                {
                    using (CryptoStream cs = new CryptoStream(file, Crypto.AES.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        audioQuran = Serializer.Deserialize<Quran.Model.AudioQuran>(cs);
                    }
                }

                if (AudioSura.AudioSlices != null)
                {
                    for (int i = 0; i < listBox.Items.Count; i++)
                    {
                        (listBox.Items[i] as TempAudioSlice).Position = AudioSura.AudioSlices[i].Start;
                    }
                }
            }
        }

        private void Play()
        {
            mePlayer.Play();
            mediaPlayerIsPlaying = true;
        }

        private void Save()
        {
            AudioSura.AudioSlices = new AudioSlice[listBox.Items.Count];

            TimeSpan finish;
            for (int i = 0; i < listBox.Items.Count; i++)
            {
                TempAudioSlice tas = listBox.Items[i] as TempAudioSlice;
                if (i < listBox.Items.Count - 1)
                    finish = (listBox.Items[i + 1] as TempAudioSlice).Position;
                else
                    finish = mePlayer.NaturalDuration.TimeSpan;
                AudioSura.AudioSlices[i] = new AudioSlice() {  SliceID = tas.SliceID, Start = tas.Position, Finish = finish };
            }
            
            using (var file = File.Create(AudioPath))
            {
                using (CryptoStream cs = new CryptoStream(file, Crypto.AES.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    Serializer.Serialize<Model.AudioQuran>(cs, audioQuran);
                }
            }

            QuranConfig.Current.Meta.Qaris[qari.ID].Availability[suraMeta.SuraNo-1] = true;

            Engine.SaveMeta(QuranConfig.Current.Meta);
        }

        private void SpeedDown()
        {
            mePlayer.SpeedRatio = mePlayer.SpeedRatio - 1;
            SpeedRatio.Text = mePlayer.SpeedRatio + "X";
        }

        private void SpeedNormal()
        {
            mePlayer.SpeedRatio = 1;
            SpeedRatio.Text = mePlayer.SpeedRatio + "X";
        }

        private void SpeedUp()
        {
            mePlayer.SpeedRatio = mePlayer.SpeedRatio + 1;
            SpeedRatio.Text = mePlayer.SpeedRatio + "X";
        }

        private void RePlay()
        {
            mePlayer.Position = CurrentAudioSlice.Position;
        }

        #endregion

    }

    public class TempAudioSlice : INotifyPropertyChanged
    {
        public int SliceID { get; set; }

        public int SliceIndex { get; set; }

        public int SliceNo
        {
            get
            {
                return SliceID % 1000;
            }
        }

        public string SliceHint
        {
            get
            {
                if (!HasMultiSlices)
                    return "";
                return new string('*', SliceNo);
            }
        }

        public bool HasMultiSlices { get; set; }

        public string Slice { get; set; }

        private TimeSpan position = new TimeSpan(0);
        public TimeSpan Position {
            get
            {
                return position;
            }
            set
            {
                position = value;

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Position"));
                    PropertyChanged(this, new PropertyChangedEventArgs("Time"));
                }

            }
        }

        public string Time
        {
            get
            {
                return Position.ToString(@"hh\:mm\:ss\.fff");
            }
        }

        public string Display
        {
            get
            {
                return string.Format("{0}   {1}   {2}", SliceIndex.ToString().PadLeft(3, '0'), Time, Slice);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
