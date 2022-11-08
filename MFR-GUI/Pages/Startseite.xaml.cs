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
    /// Interaktionslogik für Startseite.xaml
    /// </summary>
    public partial class Startseite : Page
    {
        public Startseite()
        {
            InitializeComponent();
            kameraAuswahl.Items.Add("TestObjekt");
        }

        private void btn_passwort_Click(object sender, RoutedEventArgs e)
        {
            PasswortÄndern p = new PasswortÄndern();
            this.NavigationService.Navigate(p);
        }

        private void btn_speichern_Click(object sender, RoutedEventArgs e)
        {
            Menu m = new Menu();
            this.NavigationService.Navigate(m);
        }
    }
}
