using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Threading;

namespace VXBLibrary
{
    /// <summary>
    /// Interaction logic for MediaPlayer.xaml
    /// </summary>
    public partial class MediaPlayer : UserControl
    {
        #region Dependency Properties
        public static readonly DependencyProperty AutoHideControlButtonsProperty;
        public static readonly DependencyProperty CanPauseProperty;
        public static readonly DependencyProperty CanZoomProperty;
        public static readonly DependencyProperty CanHideProperty;
        public static readonly DependencyProperty CanRestartProperty;
        public static readonly DependencyProperty ControlButtonWidthProperty;
        public static readonly DependencyProperty ControlButtonHeightProperty;
        public static readonly DependencyProperty SourceProperty;
        
        public static readonly RoutedEvent ContentLoadedEvent;

        public bool AutoHideControlButtons { get { return (bool)GetValue(AutoHideControlButtonsProperty); } set { SetValue(AutoHideControlButtonsProperty, value); } }
        public bool CanPause { get { return (bool)GetValue(CanPauseProperty); } set { SetValue(CanPauseProperty, value); } }
        public bool CanZoom { get { return (bool)GetValue(CanZoomProperty); } set { SetValue(CanZoomProperty, value); } }
        public bool CanHide { get { return (bool)GetValue(CanHideProperty); } set { SetValue(CanHideProperty, value); } }
        public bool CanRestart { get { return (bool)GetValue(CanRestartProperty); } set { SetValue(CanRestartProperty, value); } }
        public double ControlButtonWidth { get { return (double)GetValue(ControlButtonWidthProperty); } set { SetValue(ControlButtonWidthProperty, value); } }
        public double ControlButtonHeight { get { return (double)GetValue(ControlButtonHeightProperty); } set { SetValue(ControlButtonHeightProperty, value); } }
        public Uri Source { get { return (Uri)GetValue(SourceProperty); } set { SetValue(SourceProperty, value); } }

        public event RoutedEventHandler ContentLoaded { add { AddHandler(ContentLoadedEvent, value); } remove { RemoveHandler(ContentLoadedEvent, value); } }
        #endregion

        #region Constant and Private Variables
        private const double LOW_VOLUME = 0.3;
        private const double MEDIUM_VOLUME = 0.5;
        private const string CONTROL_BUTTONS_STYLE_KEY = "ControlPanelStyle";

        private bool isEnd = false;
        #endregion

        #region Properties

        public int SizeStep { get; set; } = 50;
        public double ControlButtonsOpacity { get; set; } = 1.0;
        public bool HasAudio { get => player.HasAudio; }
        public bool HasVideo { get => player.HasVideo; }
        public bool IsContentLoaded
        {
            get; private set;
        } = false;
        public bool IsPlaying
        {
            get; private set;
        } = false;
        #endregion

        public MediaPlayer()
        {
            InitializeComponent();
            this.DataContext = this;
            DispatcherTimer timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Start();
            ResetButtons();
        }
        private void ResetButtons()
        {
            btnZoomIn.Visibility = Visibility.Collapsed;
            btnZoomOut.Visibility = Visibility.Collapsed;
            btnHide.Visibility = Visibility.Collapsed;
            btnStop.IsEnabled = false;
            btnVolume.IsEnabled = false;
            btnPlay.IsEnabled = false;
            btnPlay.Tag = "Play";
        }

        private void ResetStatus()
        {
            isEnd = false;
            IsPlaying = false;
            IsContentLoaded = false;
        }
        static MediaPlayer()
        {
            AutoHideControlButtonsProperty = DependencyProperty.Register("AutoHideControlButtons", typeof(bool), typeof(MediaPlayer), new PropertyMetadata(true, OnAutoHideChanged));
            CanPauseProperty = DependencyProperty.Register("CanPause", typeof(bool), typeof(MediaPlayer), new PropertyMetadata(true));
            CanRestartProperty = DependencyProperty.Register("CanRestart", typeof(bool), typeof(MediaPlayer), new PropertyMetadata(true, OnCanRestartChanged));
            CanZoomProperty = DependencyProperty.Register("CanZoom", typeof(bool), typeof(MediaPlayer), new PropertyMetadata(true, OnCanZoomChanged));
            CanHideProperty = DependencyProperty.Register("CanHide", typeof(bool), typeof(MediaPlayer), new PropertyMetadata(true, OnCanHideChanged));
            ControlButtonWidthProperty = DependencyProperty.Register("ControlButtonWidth", typeof(double), typeof(MediaPlayer), new PropertyMetadata(48.0));
            ControlButtonHeightProperty = DependencyProperty.Register("ControlButtonHeight", typeof(double), typeof(MediaPlayer), new PropertyMetadata(32.0));
            SourceProperty = DependencyProperty.Register("Source", typeof(Uri), typeof(MediaPlayer), new PropertyMetadata(null, OnSourceChanged));
            
            ContentLoadedEvent = EventManager.RegisterRoutedEvent("ContentLoaded", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MediaPlayer));
        }

        private static void OnCanRestartChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
                (d as MediaPlayer).btnPlay.IsEnabled = true;
        }

        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as MediaPlayer;
            Uri uri = e.NewValue as Uri;
            control.player.Source = uri;
            control.ResetStatus();
            control.ResetButtons();
            if (uri != null && !String.IsNullOrEmpty(uri.OriginalString))
                control.btnPlay.IsEnabled = true;
        }

        private static void OnCanZoomChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as MediaPlayer;
            if (!control.HasVideo)
                return;
            var v = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
            control.btnZoomIn.Visibility = v;
            control.btnZoomOut.Visibility = v;
         }

        private static void OnCanHideChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as MediaPlayer;
            if (!control.HasVideo)
                return;
            control.btnHide.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
        }

        private static void OnAutoHideChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as MediaPlayer;
            if ((bool)e.NewValue)
            {
                control.pnlControl.Style = control.FindResource(CONTROL_BUTTONS_STYLE_KEY) as Style;
            }
            else
            {
                control.pnlControl.Style = null;
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (Source != null && player.NaturalDuration.HasTimeSpan)
            {
                lblStatus.Content = String.Format("{0:hh\\:mm\\:ss} / {1:hh\\:mm\\:ss}", player.Position, player.NaturalDuration.TimeSpan);
            }
            else
                lblStatus.Content = "";
        }

        private void MediaLoaded(object sender, RoutedEventArgs e)
        {
            if (player.HasVideo)
            {
                if(CanHide)
                    btnHide.Visibility = Visibility.Visible;
                if (CanZoom)
                {
                    btnZoomIn.Visibility = Visibility.Visible;
                    btnZoomOut.Visibility = Visibility.Visible;
                }
            }
            if (player.HasAudio)
                btnVolume.IsEnabled = true;
            else
                btnVolume.Visibility = Visibility.Collapsed;

            IsContentLoaded = true;

            var args = new RoutedEventArgs(ContentLoadedEvent, sender);
            this.RaiseEvent(args);
        }

        private void MediaEnded(object sender, RoutedEventArgs e)
        {
            isEnd = true;
            IsPlaying = false;
            btnPlay.Tag = "Play";
            btnPlay.IsEnabled = CanRestart;
        }

        private void Stop(object sender, RoutedEventArgs e)
        {
            if (IsPlaying)
            {
                player.Stop();
                IsPlaying = false;
                isEnd = true;
                btnPlay.Tag = "Play";
                btnPlay.IsEnabled = CanRestart;
            }
            btnStop.IsEnabled = false;
        }

        private void VolumeChange(object sender, RoutedEventArgs e)
        {
            double currentVolume = player.Volume;
            if (currentVolume == 0)
            {
                player.Volume = LOW_VOLUME;
                btnVolume.Tag = "Low";
            }
            else if (currentVolume == LOW_VOLUME)
            {
                player.Volume = MEDIUM_VOLUME;
                btnVolume.Tag = "Medium";
            }
            else if (currentVolume == MEDIUM_VOLUME)
            {
                player.Volume = 1;
                btnVolume.Tag = "High";
            }
            else
            {
                player.Volume = 0;
                btnVolume.Tag = "Mute";
            }
        }

        private void Play_Pause(object sender, RoutedEventArgs e)
        {
            if(IsPlaying)
            {
                if (CanPause)
                {
                    player.Pause();
                    IsPlaying = false;
                    btnPlay.Tag = "Play";
                }
            }
            else
            {
                if (!isEnd || CanRestart)
                {
                    player.Play();
                    btnPlay.Tag = "Pause";
                    IsPlaying = true;
                    btnStop.IsEnabled = true;
                }
            }
        }

        private void ZoomIn(object sender, RoutedEventArgs e)
        {
            btnZoomOut.IsEnabled = true;
            this.Width += SizeStep;
            if ((!double.IsInfinity(this.MaxWidth) && this.Width >= this.MaxWidth)
                || (!double.IsInfinity(this.MaxHeight) && this.Height >= this.MaxHeight))
            {
                btnZoomIn.IsEnabled = false;
                this.Width -= SizeStep;
            }
        }

        private void ZoomOut(object sender, RoutedEventArgs e)
        {
            btnZoomIn.IsEnabled = true;
            this.Width -= SizeStep;
            if(this.Width <= ControlButtonWidth)
            {
                btnZoomOut.IsEnabled = false;
                this.Width += SizeStep;
            }
        }

        private void Hide_Show(object sender, RoutedEventArgs e)
        {
            if (player.Visibility == Visibility.Visible)
            {
                player.Visibility = Visibility.Collapsed;
                btnHide.Tag = "Show";
            }
            else
            {
                player.Visibility = Visibility.Visible;
                btnHide.Tag = "Hide";
            }
        }

    }
}
