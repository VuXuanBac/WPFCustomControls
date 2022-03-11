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

namespace VXBLibrary
{
    /// <summary>
    /// Interaction logic for PhotoViewer.xaml
    /// </summary>
    public partial class PhotoViewer : UserControl
    {
        /// <summary>
        /// The behavior of the control when change current item within a list.
        /// </summary>
        public enum ChangeCurrentItemBehavior
        {
            /// <summary>
            /// Insert the new item in the position of current item.
            /// </summary>
            Insert = 0,
            /// <summary>
            /// Replace old current item with the new one.
            /// </summary>
            Replace = 1
        }
        #region Dependency Properties
        public static readonly DependencyProperty AutoHideControlButtonsProperty;
        public static readonly DependencyProperty CanZoomProperty;
        public static readonly DependencyProperty CanHideProperty;
        public static readonly DependencyProperty ControlButtonWidthProperty;
        public static readonly DependencyProperty ControlButtonHeightProperty;
        public static readonly DependencyProperty ControlButtonOpacityProperty;
        public static readonly DependencyProperty CurrentImageProperty;
        public static readonly DependencyProperty CurrentIndexProperty;
        public static readonly DependencyProperty ImagesProperty;

        public static readonly RoutedEvent ImageLoadedEvent;

        public bool AutoHideControlButtons { get { return (bool)GetValue(AutoHideControlButtonsProperty); } set { SetValue(AutoHideControlButtonsProperty, value); } }
        public bool CanZoom { get { return (bool)GetValue(CanZoomProperty); } set { SetValue(CanZoomProperty, value); } }
        public bool CanHide { get { return (bool)GetValue(CanHideProperty); } set { SetValue(CanHideProperty, value); } }
        public double ControlButtonWidth { get { return (double)GetValue(ControlButtonWidthProperty); } set { SetValue(ControlButtonWidthProperty, value); } }
        public double ControlButtonHeight { get { return (double)GetValue(ControlButtonHeightProperty); } set { SetValue(ControlButtonHeightProperty, value); } }
        public double ControlButtonOpacity { get { return (double)GetValue(ControlButtonOpacityProperty); } set { SetValue(ControlButtonOpacityProperty, value); } }
        public Uri CurrentImage { get { return (Uri)GetValue(CurrentImageProperty); } set { SetValue(CurrentImageProperty, value); } }
        public int CurrentIndex { get { return (int)GetValue(CurrentIndexProperty); } set { SetValue(CurrentIndexProperty, value); } }
        public IList<Uri> Images { get { return (IList<Uri>)GetValue(ImagesProperty); } set { SetValue(ImagesProperty, value); } }

        public event RoutedEventHandler ImageLoaded { add { AddHandler(ImageLoadedEvent, value); } remove { RemoveHandler(ImageLoadedEvent, value); } }
        #endregion

        #region Properties

        public int SizeStep { get; set; } = 50;
        public bool IsContentLoaded
        {
            get; private set;
        } = false;
        public ChangeCurrentItemBehavior ChangeCurrentImageBehavior { get; set; } = ChangeCurrentItemBehavior.Replace;
        #endregion

        private bool is_internal_change = false;

        public PhotoViewer()
        {
            InitializeComponent();
            this.DataContext = this;
            //ResetButtons();
        }
        private void ResetButtons()
        {
            btnZoomIn.Visibility = Visibility.Visible;
            btnZoomOut.Visibility = Visibility.Visible;
            btnHide.Visibility = Visibility.Visible;
            btnPrevious.Visibility = Visibility.Collapsed;
            btnNext.Visibility = Visibility.Collapsed;
            btnZoomIn.IsEnabled = false;
            btnZoomOut.IsEnabled = false;
            btnHide.IsEnabled = false;
        }

        static PhotoViewer()
        {
            AutoHideControlButtonsProperty = DependencyProperty.Register("AutoHideControlButtons", typeof(bool), typeof(PhotoViewer), new PropertyMetadata(true, OnAutoHideChanged));
            CanZoomProperty = DependencyProperty.Register("CanZoom", typeof(bool), typeof(PhotoViewer), new PropertyMetadata(true, OnCanZoomChanged));
            CanHideProperty = DependencyProperty.Register("CanHide", typeof(bool), typeof(PhotoViewer), new PropertyMetadata(true, OnCanHideChanged));
            ControlButtonWidthProperty = DependencyProperty.Register("ControlButtonWidth", typeof(double), typeof(PhotoViewer), new PropertyMetadata(48.0));
            ControlButtonHeightProperty = DependencyProperty.Register("ControlButtonHeight", typeof(double), typeof(PhotoViewer), new PropertyMetadata(32.0));
            ControlButtonOpacityProperty = DependencyProperty.Register("ControlButtonOpacity", typeof(double), typeof(PhotoViewer), new PropertyMetadata(1.0));
            CurrentImageProperty = DependencyProperty.Register("CurrentImage", typeof(Uri), typeof(PhotoViewer), new PropertyMetadata(null, OnCurrentImageChanged));
            CurrentIndexProperty = DependencyProperty.Register("CurrentIndex", typeof(int), typeof(PhotoViewer), new PropertyMetadata(0, OnCurrentIndexChanged, CoerceCurrentIndex) );
            ImagesProperty = DependencyProperty.Register("Images", typeof(IList<Uri>), typeof(PhotoViewer), new PropertyMetadata(null, OnImagesChanged));

            ImageLoadedEvent = EventManager.RegisterRoutedEvent("ImageLoaded", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PhotoViewer));
        }

        private static object CoerceCurrentIndex(DependencyObject d, object baseValue)
        {
            var control = d as PhotoViewer;
            if (control.is_internal_change)
                return baseValue;

            int index = (int)baseValue;
            var images = control.Images;
            // handled invalid index.
            // if the list null, use default index is 0.
            if (images == null || index >= images.Count)
                return 0;
            else if (index < 0)
                return images.Count - 1;

            return index;
        }

        private static void OnCurrentIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as PhotoViewer;
            /* if set check condition for index in Coerce, then can apply change here */
            /* else set check condition beforing set */

            // Prevent circular changing
            if (control.is_internal_change)
                return;
            // If have set List, change the current image in it.
            // Else, keep everything not change: CurrentIndex is 0 too.
            if (control.Images != null)
            {
                control.is_internal_change = true;
                control.CurrentImage = control.Images[(int)e.NewValue];
                control.is_internal_change = false;
            }
        }

        private static void OnImagesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as PhotoViewer;
            control.is_internal_change = true;
            control.CurrentIndex = 0;
            control.CurrentImage = (e.NewValue as List<Uri>)[0];
            control.is_internal_change = false;
        }

        private static void OnCurrentImageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as PhotoViewer;
            // Prevent circular changing
            if (control.is_internal_change)
                return;
            // If have set List, change the current image in it.
            // Else, keep everything not change: CurrentIndex is 0 too.
            if (control.Images != null)
            {
                control.is_internal_change = true;
                if (control.ChangeCurrentImageBehavior == ChangeCurrentItemBehavior.Insert)
                {
                    control.Images.Insert(control.CurrentIndex, e.NewValue as Uri);
                }
                else
                {
                    control.Images[control.CurrentIndex] = e.NewValue as Uri;
                }
                control.is_internal_change = false;
            }
        }

        private static void OnCanZoomChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as PhotoViewer;
            var v = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
            control.btnZoomIn.Visibility = v;
            control.btnZoomOut.Visibility = v;
        }

        private static void OnCanHideChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as PhotoViewer;
            control.btnHide.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
        }

        private static void OnAutoHideChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as PhotoViewer).pnlControl.Tag = (bool)e.NewValue ? "AutoHide" : "ShowAlways";
        }
        
        //TODO: ContentLoaded
        private void ContentLoaded(object sender, RoutedEventArgs e)
        {
            IsContentLoaded = true;

            var args = new RoutedEventArgs(ImageLoadedEvent, sender);
            this.RaiseEvent(args);
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
            if (this.Width <= ControlButtonWidth - SizeStep)
            {
                btnZoomOut.IsEnabled = false;
            }
        }

        private void Hide_Show(object sender, RoutedEventArgs e)
        {
            if (image.Visibility == Visibility.Visible)
            {
                image.Visibility = Visibility.Collapsed;
                btnHide.Tag = "Show";
            }
            else
            {
                image.Visibility = Visibility.Visible;
                btnHide.Tag = "Hide";
            }
        }

        private void GoPrevious(object sender, RoutedEventArgs e)
        {
            // the value will be handled in Coerce
            CurrentIndex--;
        }

        private void GoNext(object sender, RoutedEventArgs e)
        {
            // the value will be handled in Coerce
            CurrentIndex++;
        }

    }
}
