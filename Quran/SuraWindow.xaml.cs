using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using Quran.Code;
using Quran.Model;

namespace Quran
{
    /// <summary>
    /// Interaction logic for SuraWindow.xaml
    /// </summary>
    public partial class SuraWindow : Window
    {
        private enum SliceType { SingleLine, MultiLine }

        private static Brush activeForeground = null;
        private static Brush normalForeground = null;

        private static ToolTip tooltip = new ToolTip();

        private static Run run = new Run();

        private static double lineHeight = 70; // mainParagraph.LineHeight

        ScrollViewer scrollViewer = null;
        DispatcherTimer timer = null;

        DateTime waitStarted = DateTime.MinValue;
        int repeat = 1;


        private Point cursorPos = new Point();
        private Cursor previousCursor;

        //[DllImport("User32.dll")]
        //private static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };
        public static Point GetMousePosition()
        {
            Win32Point w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }

        #region Constructor

        public SuraWindow()
        {
            InitializeComponent();

            activeForeground = (Brush)new BrushConverter().ConvertFrom("#358995");
            normalForeground = (Brush)new BrushConverter().ConvertFrom("#404040");

            int defaultSuraNo = 59;
            suraListBox.SelectedIndex = defaultSuraNo - 1;

            toggleButtonPlayPause.Visibility = PlayPauseShown;

            LoadSura(defaultSuraNo);

            LoadMeta();

            timer = new DispatcherTimer(DispatcherPriority.Normal);
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += timer_Tick;

            timer.Start();
        }


        #endregion

        #region Properties

        private SuraMeta Sura
        {
            get;
            set;
        }

        private string SuraInfo
        {
            get
            {
                return string.Format("{0} آيه", Sura.TotalAyas);

                //if (suraMeta.IsMeccan)
                //    return string.Format("مکیّة({0} آيه)", suraMeta.TotalAyas);
                //else
                //    return string.Format("مدنيه({0} آيه)", suraMeta.TotalAyas);
            }
        }
        public string TotalAyas
        {
            get
            {
                return Sura.TotalAyas.ToString();
            }
        }

        public Aya CurrentAya
        {
            get
            {
                int index = SliceInfo.AyaNo;
                if (Sura.SuraNo == 1 || Sura.SuraNo == 9)
                    --index;

                return QuranProvider.Quran.Suras[Sura.SuraNo - 1].Ayas[index];
            }
        }
        private SliceInfo SliceInfo { get; set; }

        private AudioSlice CurrentAudioSlice
        {
            get
            {
                return QuranProvider.audio[SliceInfo.SliceID];
            }
        }

        private MediaState playerState = MediaState.Play;
        private MediaState PlayerState
        {
            get
            {
                return playerState;
            }
            set
            {
                playerState = value;
                if (value == MediaState.Play)
                    toggleButtonPlayPause.IsChecked = true;
                else
                    toggleButtonPlayPause.IsChecked = false;
            }
        }

        public Visibility PlayPauseShown
        {
            get
            {
                if (AppSettings.HasAudio)
                    return Visibility.Visible;
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }

        #endregion

        #region Load Methods

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPagePadding();

            DependencyObject firstChild = VisualTreeHelper.GetChild(flowDocSV, 0);
            Decorator border = VisualTreeHelper.GetChild(firstChild, 0) as Decorator;
            scrollViewer = border.Child as ScrollViewer;
            //scrollViewer.ScrollChanged += scrollViewer_ScrollChanged;

            StartSura();

        }

        private void LoadSura(int suraNo)
        {
            Paragraph paragraph = new Paragraph();
            QuranProvider.InitProvider(suraNo, paragraph);

            SliceInfo = QuranProvider.runs[suraNo][0].Tag as SliceInfo;
            Sura = QuranProvider.Meta.Suras[suraNo - 1];
            introAyaSpan.Inlines.Clear();
            mainParagraph.Inlines.Clear();

            QuranProvider.InitProvider(suraNo, mainParagraph);

            if (suraNo != 9)
                introAyaSpan.Inlines.Add(QuranProvider.GetIntroAya(suraNo));

            suraNameContentControl.Content = string.Format("سوره {0}", QuranProvider.GetSuraName(suraNo));
            //suraInfoTextBlock.Text = SuraInfo;

            if (suraNo == 1)
                mainParagraph.Inlines.Remove(mainParagraph.Inlines.FirstInline);

            this.UpdateLayout();

            PlayAudio(suraNo);

            LoadAyas();

            qariNameContentControl.Content = QuranProvider.GetQariName();

            if (suraNo < 114)
            {
                NextSura.Visibility = System.Windows.Visibility.Visible;
                NextSura.Content = QuranProvider.Meta.Suras[suraNo].NameArabic;
            }
            else
                NextSura.Visibility = System.Windows.Visibility.Hidden;

        }

        private void ApplyPagePadding()
        {
            PresentationSource source = PresentationSource.FromVisual(this);

            Matrix m = PresentationSource.FromVisual(Application.Current.MainWindow).CompositionTarget.TransformToDevice;
            double dx = m.M11;
            double dy = m.M22;

            double dpiX = 0;
            double dpiY = 0;

            if (source != null)
            {
                dpiX = 96.0 * source.CompositionTarget.TransformToDevice.M11;
                dpiY = 96.0 * source.CompositionTarget.TransformToDevice.M22;
            }

            double width = dpiX / 96 * System.Windows.SystemParameters.PrimaryScreenWidth;
            double height = dpiY / 96 * System.Windows.SystemParameters.PrimaryScreenHeight;

            if (Sura.TotalAyas < 10)
            {
                if (width >= 1920)
                    flowDoc.PagePadding = new Thickness(500, 0, 500, 0);
                else if (width >= 1600)
                    flowDoc.PagePadding = new Thickness(350, 0, 350, 0);
                else if (width >= 1280)
                    flowDoc.PagePadding = new Thickness(200, 0, 200, 0);
            }
            else
            {
                if (width >= 1920)
                    flowDoc.PagePadding = new Thickness(350, 0, 350, 0);
                else if (width >= 1600)
                    flowDoc.PagePadding = new Thickness(200, 0, 200, 0);
                else if (width >= 1280)
                    flowDoc.PagePadding = new Thickness(50, 0, 50, 0);
            }
        }

        private void LoadMeta()
        {
            foreach (var sura in Quran.Code.QuranProvider.Meta.Suras)
            {
                suraListBox.Items.Add(new ListBoxItem() { Content = sura.FullNameArabic });
            }
        }

        private void LoadAyas()
        {
            try
            {
                ayaComboBox.SelectionChanged -= ayaComboBox_SelectionChanged;

                ayaComboBox.Items.Clear();

                for (int i = 1; i <= Sura.TotalAyas; i++)
                {
                    ayaComboBox.Items.Add(new ComboBoxItem() { Content = i.ToString() });
                }
            }
            finally
            {
                ayaComboBox.SelectionChanged += ayaComboBox_SelectionChanged;
            }

        }

        #endregion

        #region Audio Section

        private void PlayAudio()
        {
            PlayAudio(false);
        }

        private void PlayAudio(bool newQari)
        {
            if (!AppSettings.HasAudio)
                return;

            if (player.Source != null && !newQari)
            {
                player.Position = CurrentAudioSlice.Start;

                if (PlayerState == MediaState.Pause)
                {
                    PlayerState = MediaState.Play;
                    player.Play();
                }
            }
            else
            {
                player.Source = new Uri(QuranProvider.GetAudioPath(Sura.SuraNo), UriKind.Relative);
                player.Position = CurrentAudioSlice.Start;
                player.Play();
                PlayerState = MediaState.Play;
            }
        }

        private void PlayAudio(int suraNo)
        {
            if (!AppSettings.HasAudio)
            {
                player.Source = null;
                return;
            }

            //player.Volume = 0.1;

            player.Source = new Uri(QuranProvider.GetAudioPath(suraNo), UriKind.Relative);
            player.Play();
            PlayerState = MediaState.Play;
            repeat = AppSettings.Repeat;
        }

        private void StopAudio()
        {
            if (!AppSettings.HasAudio)
                return;

            player.Stop();
            PlayerState = MediaState.Pause;
            SliceInfo = QuranProvider.runs[Sura.SuraNo][0].Tag as SliceInfo;
        }

        private void PlayPause()
        {
            if (!AppSettings.HasAudio)
                return;

            if (PlayerState == MediaState.Play)
                Pause();
            else
                Play();
        }

        private void Pause()
        {
            if (!AppSettings.HasAudio)
                return;

            player.Pause();
            PlayerState = MediaState.Pause;
        }

        private void Play()
        {
            if (!AppSettings.HasAudio)
                return;

            player.Play();
            PlayerState = MediaState.Play;
        }

        void timer_Tick(object sender, EventArgs e)
        {
            try
            {
                //timer.Tick -= timer_Tick;
                timer.Stop();

                if (AppSettings.Qari == 0)
                    return;

                if (player.Position <= CurrentAudioSlice.Finish)
                    return;

                if (SliceInfo.SliceIndex >= QuranProvider.runs[Sura.SuraNo].Length - 1)
                    return;

                if (AppSettings.Wait > 0)
                {
                    if (waitStarted == DateTime.MinValue)
                    {
                        waitStarted = DateTime.Now;
                        Pause();
                    }
                    else if (DateTime.Now.Subtract(waitStarted).TotalSeconds > AppSettings.Wait)
                    {
                        waitStarted = DateTime.MinValue;

                        PreparingNextSlice();

                        Play();
                    }
                }
                else
                {
                    PreparingNextSlice();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //timer.Tick += timer_Tick;
                timer.Start();
            }

        }

        private void PreparingNextSlice()
        {
            if (repeat > 1)
            {
                --repeat;
                player.Position = CurrentAudioSlice.Start;
            }
            else
            {
                NextHandler(this, null);
                repeat = AppSettings.Repeat;
            }
        }

        public async void RePlay()
        {
            await System.Threading.Tasks.Task.Delay(2000);
            PlayPause();
            System.Threading.Tasks.Task.WaitAll();
        }

        private void toggleButtonPlayPause_Click(object sender, RoutedEventArgs e)
        {
            PlayPause();
        }

        private void PausePlayHandler(object sender, ExecutedRoutedEventArgs e)
        {
            PlayPause();
        }

        private void player_MediaEnded(object sender, RoutedEventArgs e)
        {
            StopAudio();
        }

        #endregion

        #region Translation Section

        //void scrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        //{
        //    ShowToolTip(run);
        //}

        private void Run_MouseDown(object sender, EventArgs e)
        {
            run.Foreground = normalForeground;
            //run.Background = normalBackground;

            run.ToolTip = null;

            run = sender as Run;

            SelectRun(run);

            bool isAutoMode = (e == null);

            if (isAutoMode)
            {
                SetTooltip();

                ShowToolTip(run);
            }
            else
            {
                SliceInfo = (SliceInfo)run.Tag;

                SetTooltip();

                ShowToolTip(run);

                PlayAudio();
            }

            try
            {
                ayaComboBox.SelectionChanged -= ayaComboBox_SelectionChanged;

                if (SliceInfo.AyaNo > 0)
                    ayaComboBox.SelectedIndex = SliceInfo.AyaNo - 1;
                else
                    ayaComboBox.SelectedIndex = -1;
            }
            finally
            {
                ayaComboBox.SelectionChanged += ayaComboBox_SelectionChanged;
            }


            juzTextBlock.Text = string.Format("الجزء {0}", CurrentAya.Juz.ToString());

        }


        private void SelectRun(Run run)
        {
            run.Foreground = activeForeground;
            //run.Background = activeBackground;

            Rect rect = run.ContentStart.GetCharacterRect(LogicalDirection.Forward);

            if (rect.Top > this.Height * 2 / 3 - 100)
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + rect.Y - rect.Height);

                this.UpdateLayout();
            }
            else if (rect.Top < 0)
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + rect.Y - this.Height / 3);

                this.UpdateLayout();
            }
        }

        private void SetTooltip()
        {
            if (!AppSettings.HasTranslation)
                return;

            tooltip.IsOpen = false;
            tooltip = new ToolTip();
            tooltip.Content = QuranProvider.trans[SliceInfo.SliceID];
            tooltip.HasDropShadow = false;
            tooltip.VerticalOffset = 60;
            tooltip.Style = this.FindResource("tooltipStyle") as Style;
        }

        private void ShowToolTip(Run run)
        {
            if (cursorPos != new Point() && cursorPos == GetMousePosition())
            {
                //Mouse.OverrideCursor = Cursors.None;
                previousCursor = Cursor;
                Cursor = Cursors.None;
                //SetCursorPos((int)this.ActualWidth, 100);
            }
            else
                cursorPos = GetMousePosition();

            Quran.Code.QuranProvider.SajdaType sajdaType = QuranProvider.GetSajdaInfo(Sura.SuraNo, SliceInfo.AyaNo);

            if (sajdaType == QuranProvider.SajdaType.Mostahab)
            {
                infoPopup.IsOpen = false;
                infoPopup.IsOpen = true;
            }
            else if (sajdaType == QuranProvider.SajdaType.Vajeb)
            {
                infoTextBlock.Text = "سجده واجب";
                infoPopup.IsOpen = false;
                infoPopup.IsOpen = true;
            }

            if (!this.IsActive)
                return;

            if (!AppSettings.HasTranslation)
                return;

            SliceType sliceType = SliceType.SingleLine;
            tooltip.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));


            var rectStart = run.ContentStart.GetCharacterRect(LogicalDirection.Forward);
            var rectEnd = run.ContentEnd.GetInsertionPosition(LogicalDirection.Forward).GetCharacterRect(LogicalDirection.Forward);
            var startPoint = flowDocSV.PointToScreen(new Point(rectStart.X - tooltip.DesiredSize.Width, rectStart.Y));
            var endPoint = flowDocSV.PointToScreen(new Point(rectEnd.X, rectEnd.Y));

            if (endPoint.Y < 100 || rectEnd.Y > flowDocSV.ActualHeight)
            {
                tooltip.IsOpen = false;
                return;
            }


            if (rectEnd.Y > rectStart.Y)
                sliceType = SliceType.MultiLine;

            double freeSpace = 0;
            bool canCoverAtStart = true;

            if (sliceType == SliceType.SingleLine)
            {
                freeSpace = rectStart.X;

                if (freeSpace < tooltip.DesiredSize.Width)
                    canCoverAtStart = false;
            }
            else
            {
                freeSpace = rectEnd.X - flowDoc.PagePadding.Left;

                if (freeSpace < tooltip.DesiredSize.Width)
                    canCoverAtStart = false;
            }

            // Show Tooltip
            tooltip.StaysOpen = true;
            tooltip.IsOpen = true;
            tooltip.Placement = System.Windows.Controls.Primitives.PlacementMode.Absolute;
            run.ToolTip = tooltip;

            if (sliceType == SliceType.SingleLine)
            {
                if (canCoverAtStart)
                    tooltip.PlacementRectangle = new Rect(startPoint.X, startPoint.Y, 0, 0);
                else
                    tooltip.PlacementRectangle = new Rect(endPoint.X, startPoint.Y, 0, 0);
            }
            else
            {
                if (canCoverAtStart)
                {
                    double extraSpace = 50;
                    double extraHeight = rectEnd.Height + (lineHeight - rectEnd.Height) / 2 - 2;
                    Point tooltipPoint = flowDocSV.PointToScreen(new Point(rectEnd.X - tooltip.DesiredSize.Width - extraSpace, rectEnd.Y - extraHeight));
                    tooltip.PlacementRectangle = new Rect(tooltipPoint.X, tooltipPoint.Y, 0, 0);
                }
                else
                {
                    //Rect rect = run.ContentEnd.GetLineStartPosition(1).GetCharacterRect(LogicalDirection.Forward);
                    Rect rect = run.ContentEnd.GetLineStartPosition(0).GetCharacterRect(LogicalDirection.Forward);
                    Point tooltipPoint = flowDocSV.PointToScreen(new Point(rect.X - tooltip.DesiredSize.Width, rectEnd.Y));
                    tooltip.PlacementRectangle = new Rect(tooltipPoint.X, tooltipPoint.Y, 0, 0);
                }
            }
        }

        private void Run_MouseEnter(object sender, EventArgs e)
        {
            Cursor = Cursors.Hand;
        }

        private void Run_MouseLeave(object sender, EventArgs e)
        {
            Cursor = Cursors.Arrow;
        }

        #endregion

        #region Toolbar Section

        private void ayaComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Run run = null;
            if (Sura.SuraNo == 1 && ayaComboBox.SelectedIndex == 0)
                run = (introAyaSpan.Inlines.FirstInline as Span).Inlines.FirstInline as Run;
            else
                run = QuranProvider.GetRun(Sura.SuraNo, ayaComboBox.SelectedIndex + 1);
            SliceInfo = run.Tag as SliceInfo;
            Run_MouseDown(run, e);

            this.Focus();
        }

        private void ayaComboBox_DropDownOpened(object sender, EventArgs e)
        {
            //timer.Tick -= timer_Tick;
            timer.Stop();
            player.Pause();
        }

        private void ayaComboBox_DropDownClosed(object sender, EventArgs e)
        {
            player.Play();
            //timer.Tick += timer_Tick;
            timer.Start();
        }

        #endregion

        #region Commands Section

        private void CloseCommandHandler(object sender, ExecutedRoutedEventArgs e)
        {
            if (!suraPopup.IsOpen)
                this.Close();
            else
            {
                ClosePopupSura();
                ShowToolTip(run);
                PausePlayHandler(this, null);
            }
        }

        private void PreviousHandler(object sender, ExecutedRoutedEventArgs e)
        {
            Run previousRun = QuranProvider.runs[Sura.SuraNo][SliceInfo.SliceIndex - 1];
            SliceInfo = previousRun.Tag as SliceInfo;
            Run_MouseDown(previousRun, e);
        }

        private void NextHandler(object sender, ExecutedRoutedEventArgs e)
        {
            Run nextRun = QuranProvider.runs[Sura.SuraNo][SliceInfo.SliceIndex + 1];
            SliceInfo = nextRun.Tag as SliceInfo;
            Run_MouseDown(nextRun, e);
        }

        private void CanExecuteNext(object sender, CanExecuteRoutedEventArgs e)
        {
            if (SliceInfo.SliceIndex == QuranProvider.runs[Sura.SuraNo].Length - 1)
                e.CanExecute = false;
            else
                e.CanExecute = true;
        }

        private void CanExecutePrevious(object sender, CanExecuteRoutedEventArgs e)
        {
            if (SliceInfo.SliceIndex == 0)
                e.CanExecute = false;
            else
                e.CanExecute = true;
        }

        #endregion

        #region Sura Section

        private void StartSura()
        {
            SliceInfo = QuranProvider.runs[Sura.SuraNo][0].Tag as SliceInfo;
            Run firstRun = QuranProvider.runs[Sura.SuraNo][SliceInfo.SliceIndex];
            Run_MouseDown(firstRun, null);
        }

        private void suraNameInnerTextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OpenPopupSura();
        }

        private void suraListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SuraSelected();

            if (AppSettings.HasAudio)
                timer.Start();
        }

        private void suraListBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SuraSelected();

                if (AppSettings.HasAudio)
                    timer.Start();
            }
        }

        private void SuraSelected()
        {
            ClosePopupSura();

            if (Sura.SuraNo != suraListBox.SelectedIndex + 1)
            {
                int suraNo = suraListBox.SelectedIndex + 1;
                int prevQari = AppSettings.Qari;
                QuranProvider.ChangeQari(suraNo);
                scrollViewer.ScrollToVerticalOffset(0);
                toggleButtonPlayPause.Visibility = PlayPauseShown;
                LoadSura(suraNo);
                ApplyPagePadding();

                if (prevQari != AppSettings.Qari)
                    QuranProvider.InitAudio();

                StartSura();
                
            }
            else
            {
                ShowToolTip(run);
                PausePlayHandler(this, null);
            }

        }

        private void OpenPopupSura()
        {
            //timer.Tick -= timer_Tick;
            timer.Stop();
            Pause();
            tooltip.IsOpen = false;
            BlurEffect objBlur = new BlurEffect();
            objBlur.KernelType = KernelType.Gaussian;
            objBlur.RenderingBias = RenderingBias.Performance;
            objBlur.Radius = 8;
            this.Effect = objBlur;
            this.IsHitTestVisible = false;
            suraPopup.IsOpen = true;
            FocusListBox();
        }

        private void ClosePopupSura()
        {
            suraPopup.IsOpen = false;
            this.IsHitTestVisible = true;
            this.Effect = null;
        }

        void FocusListBox()
        {
            var i = suraListBox.ItemContainerGenerator.ContainerFromIndex(suraListBox.SelectedIndex) as ListBoxItem;
            if (i != null)
                Keyboard.Focus(i);

            Dispatcher.BeginInvoke(new Action(FocusListBox), DispatcherPriority.Input);
        }

        #endregion

        #region Qari Section

        private void qariNameInnerTextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OpenPopupQari();
        }

        private void OpenPopupQari()
        {
            try
            {
                timer.Stop();

                Pause();

                tooltip.IsOpen = false;

                BlurEffect objBlur = new BlurEffect();
                objBlur.KernelType = KernelType.Gaussian;
                objBlur.RenderingBias = RenderingBias.Performance;
                objBlur.Radius = 8;
                this.Effect = objBlur;

                int prevQari = AppSettings.Qari;

                bool preAudioFileExist = QuranProvider.IsAudioFileExist(Sura.SuraNo);

                new QariSelector(Sura.SuraNo).ShowDialog();

                if (prevQari != AppSettings.Qari)
                {
                    QuranProvider.InitAudio();
                    PlayAudio(true);
                    if (AppSettings.HasAudio)
                        timer.Start();
                }
                else if (preAudioFileExist == false && QuranProvider.IsAudioFileExist(Sura.SuraNo))
                {
                    SuraSelected();
                }

                suraPopup.IsOpen = false;
                this.Effect = null;

                qariNameContentControl.Content = QuranProvider.GetQariName();

                PlayAudio();
                ShowToolTip(run);

                toggleButtonPlayPause.Visibility = PlayPauseShown;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region Settings Section

        private void OpenPopupSettings()
        {
            Pause();

            tooltip.IsOpen = false;

            BlurEffect objBlur = new BlurEffect();
            objBlur.KernelType = KernelType.Gaussian;
            objBlur.RenderingBias = RenderingBias.Performance;
            objBlur.Radius = 8;
            this.Effect = objBlur;

            new Settings().ShowDialog();

            suraPopup.IsOpen = false;
            this.Effect = null;

            PlayAudio();
            ShowToolTip(run);
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            OpenPopupSettings();

            repeat = AppSettings.Repeat;
        }

        #endregion

        private void NextSura_Click(object sender, RoutedEventArgs e)
        {
            StopAudio();
            suraListBox.SelectedIndex++;
            SuraSelected();

        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if ( Cursor == Cursors.None)
                Cursor = previousCursor;
        }
    }
}
