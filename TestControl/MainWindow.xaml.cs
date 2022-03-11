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

namespace TestControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            image.Images = new List<Uri>()
           {
               new Uri("/hinh2.jpg", UriKind.RelativeOrAbsolute),
               new Uri("/hinhdep.jpg", UriKind.RelativeOrAbsolute),
               new Uri("/hinh1.jpg", UriKind.RelativeOrAbsolute)
           };
        }

        private void Button_Click1(object sender, RoutedEventArgs e)
        {
            image.CurrentImage = new Uri("/happyface.jpg", UriKind.RelativeOrAbsolute);
        }

        private void Button_Click2(object sender, RoutedEventArgs e)
        {
            image.CurrentIndex = 2;
        }
    }
}
