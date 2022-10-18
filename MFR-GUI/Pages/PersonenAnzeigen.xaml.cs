using System;
using System.Collections.Generic;
using System.IO;
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

            Uri u = new Uri(Globals.projectDirectory +  "\\gespeicherte Personen");
            explorer.Source = u;
        }

        private void btn_Zurueck2_Click(object sender, RoutedEventArgs e)
        {
            Menu m = new Menu();
            this.NavigationService.Navigate(m);
        }
        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if (Directory.Exists(Globals.projectDirectory + "\\gespeicherte Personen\\" + txt_NameSuchen.Text))
                {
                    Uri u = new Uri(Globals.projectDirectory + "\\gespeicherte Personen\\" + txt_NameSuchen.Text);
                    explorer.Source = u;
                }
            }
        }

        private void btn_Back_Click(object sender, RoutedEventArgs e)
        {
            if(explorer.CanGoBack)
            {
                explorer.GoBack();
            }
        }

        private void btn_Forward_Click(object sender, RoutedEventArgs e)
        {
            if(explorer.CanGoForward)
            {
                explorer.GoForward();
            }
        }
    }
}