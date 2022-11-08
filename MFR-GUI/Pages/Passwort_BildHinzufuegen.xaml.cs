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
    /// Interaktionslogik für Passwort_BildHinzufuegen.xaml
    /// </summary>
    public partial class Passwort_BildHinzufuegen : Page
    {
        public Passwort_BildHinzufuegen()
        {
            InitializeComponent();
        }

        private void btn_anmelden_Click(object sender, RoutedEventArgs e)
        {
            BildHinzufuegen bh = new BildHinzufuegen();
            this.NavigationService.Navigate(bh);
        }

        private void btn_zurück_Click(object sender, RoutedEventArgs e)
        {
            Menu m = new Menu();
            this.NavigationService.Navigate(m);
        }
    }
}
