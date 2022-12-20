using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

            txt_NameSuchen.Focus();

            Uri u = new Uri(Globals.projectDirectory + "/TrainingFaces");
            explorer.Source = u;
        }
        
        private void btn_Zurueck2_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Menu());
        }

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if (Directory.Exists(Globals.projectDirectory + "/TrainingFaces/" + txt_NameSuchen.Text))
                {
                    Uri u = new Uri(Globals.projectDirectory + "/TrainingFaces/" + txt_NameSuchen.Text);
                    explorer.Source = u;
                }
                else
                {
                    l_Fehler.Content = "Person nicht gefunden!";
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