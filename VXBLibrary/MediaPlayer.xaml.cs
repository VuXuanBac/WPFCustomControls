using System;
using System.Windows;
using System.Windows.Controls;
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
        public static readonly DependencyProperty ControlButtonOpacityProperty;
        public static readonly DependencyProperty SourceProperty;
        
        public static readonly RoutedEvent MediaLoadedEvent;
        public static readonly RoutedEvent MediaEndedEvent;
        public static readonly RoutedEvent MediaFailedEvent;

        public bool AutoHideControlButtons { get { return (bool)GetValue(AutoHideControlButtonsProperty); } set { SetValue(AutoHideControlButtonsProperty, value); } }
        public bool CanPause { get { return (bool)GetValue(CanPauseProperty); } set { SetValue(CanPauseProperty, value); } }
        public bool CanZoom { get { return (bool)GetValue(CanZoomProperty); } set { SetValue(CanZoomProperty, value); } }
        public bool CanHide { get { return (bool)GetValue(CanHideProperty); } set { SetValue(CanHideProperty, value); } }
        public bool CanRestart { get { return (bool)GetValue(CanRestartProperty); } set { SetValue(CanRestartProperty, value); } }
        public double ControlButtonWidth { get { return (double)GetValue(ControlButtonWidthProperty); } set { SetValue(ControlButtonWidthProperty, value); } }
        public double ControlButtonHeight { get { return (double)GetValue(ControlButtonHeightProperty); } set { SetValue(ControlButtonHeightProperty, value); } }
        public double ControlButtonOpacity { get { return (double)GetValue(ControlButtonOpacityProperty); } set { SetValue(ControlButtonOpacityProperty, value); } }
        public Uri Source { get { return (Uri)GetValue(SourceProperty); } set { SetValue(SourceProperty, value); } }

        public event RoutedEventHandler MediaLoaded { add { AddHandler(MediaLoadedEvent, value); } remove { RemoveHandler(MediaLoadedEvent, value); } }
        public event RoutedEventHandler MediaEnded { add { AddHandler(MediaEndedEvent, value); } remove { RemoveHandler(MediaEndedEvent, value); } }
        public event RoutedEventHandler MediaFailed { add { AddHandler(MediaFailedEvent, value); } remove { RemoveHandler(MediaFailedEvent, value); } }
        #endregion

        #region Constant and Private Variables
        private const double LOW_VOLUME = 0.3;
        private const double MEDIUM_VOLUME = 0.5;

        private bool isEnd = false;
        #endregion

        #region Properties

        public int SizeStep { get; set; } = 50;
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
            ControlButtonOpacityProperty = DependencyProperty.Register("ControlButtonOpacity", typeof(double), typeof(MediaPlayer), new PropertyMetadata(1.0));
            SourceProperty = DependencyProperty.Register("Source", typeof(Uri), typeof(MediaPlayer), new PropertyMetadata(null, OnSourceChanged));
            
            MediaLoadedEvent = EventManager.RegisterRoutedEvent("MediaLoaded", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MediaPlayer));
            MediaEndedEvent = EventManager.RegisterRoutedEvent("MediaEnded", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MediaPlayer));
            MediaFailedEvent = MediaElement.MediaFailedEvent.AddOwner(typeof(MediaPlayer));
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
            (d as MediaPlayer).pnlControl.Tag = (bool)e.NewValue ? "AutoHide" : "ShowAlways";
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (Source != null && player.NaturalDuration.HasTimeSpan)
            {
                lblStatus.Content = String.Format("{0:hh\\:mm\\:ss} / {1:hh\\:mm\\:ss}", player.Position, player.NaturalDuration.TimeSpan);
            }
        }

        private void ContentLoaded(object sender, RoutedEventArgs e)
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
            lblStatus.Content = "Loaded Successfully";

            var args = new RoutedEventArgs(MediaLoadedEvent, sender);
            this.RaiseEvent(args);
        }

        private void ContentEnded(object sender, RoutedEventArgs e)
        {
            btnPlay.Tag = "Play";
            btnPlay.IsEnabled = CanRestart;

            isEnd = true;
            IsPlaying = false;

            var args = new RoutedEventArgs(MediaEndedEvent, sender);
            this.RaiseEvent(args);
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
            if ((!double.IsInfinity(this.MaxWidth) && this.Width + SizeStep >= this.MaxWidth)
                || (!double.IsInfinity(this.MaxHeight) && this.Height + SizeStep >= this.MaxHeight))
            {
                btnZoomIn.IsEnabled = false;
            }
        }

        private void ZoomOut(object sender, RoutedEventArgs e)
        {
            btnZoomIn.IsEnabled = true;
            this.Width -= SizeStep;
            if(this.Width <= ControlButtonWidth - SizeStep)
            {
                btnZoomOut.IsEnabled = false;
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
