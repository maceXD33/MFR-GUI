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

namespace MFR_GUI.Pages
{
    /// <summary>
    /// Interaktionslogik für PasswortÄndern.xaml
    /// </summary>
    public partial class PasswortÄndern : Page
    {
        public PasswortÄndern()
        {
            InitializeComponent();
        }

        private void btn_speichern_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btn_zurück_Click(object sender, RoutedEventArgs e)
        {
            Startseite s = new Startseite();
            this.NavigationService.Navigate(s);
        }
    }
}
