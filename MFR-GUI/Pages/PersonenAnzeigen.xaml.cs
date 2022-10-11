using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
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
    /// Interaktionslogik für PersonenAnzeigen.xaml
    /// </summary>
    public partial class PersonenAnzeigen : Page
    {
        public PersonenAnzeigen()
        {
            InitializeComponent();
        }

        private void btn_suchen_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btn_Zurueck2_Click(object sender, RoutedEventArgs e)
        {
            Menu m = new Menu();
            this.NavigationService.Navigate(m);
        }
        private void txt_Name_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
