using System;
using System.Windows;
using System.Windows.Controls;
using static MFR_GUI.Pages.Globals;
using static MFR_GUI.Pages.TrainingFacesLoader;

namespace MFR_GUI.Pages
{
    /// <summary>
    /// Interaktionslogik für Menu.xaml
    /// </summary>
    public partial class Menu : Page
    {
        private Logger _logger;

        public Menu()
        {
            InitializeComponent();

            _logger = new Logger();

            if (!dataLoaded)
            {
                LoadTrainingFaces(_logger);

                dataLoaded = true;
            }
        }

        private void btn_erfassen_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new PersonenErfassen());
        }

        private void btn_hinzufuegen_Click(object sender, RoutedEventArgs e)
        {
            if (password_checked)
            {
                BildHinzufuegen bh = new BildHinzufuegen();
                this.NavigationService.Navigate(bh);
            }
            else
            {
                Passwort_BildHinzufuegen pbh = new Passwort_BildHinzufuegen();
                this.NavigationService.Navigate(pbh);
            }
        }

        private void btn_anzeigen_Click(object sender, RoutedEventArgs e)
        {
            if (password_checked)
            {
                PersonenAnzeigen pa = new PersonenAnzeigen();
                this.NavigationService.Navigate(pa);
            }
            else
            {
                Passwort_PersonenAnzeigen ppa = new Passwort_PersonenAnzeigen();
                this.NavigationService.Navigate(ppa);
            }
        }

        private void btn_beenden_Click(object sender, RoutedEventArgs e)
        {
            if (grabber != null)
            { 
                grabber.Dispose();
            }
            if (recognizer != null)
            {
                recognizer.Dispose();
            }

            Window parentWindow = Window.GetWindow((DependencyObject)sender);
            if (parentWindow != null)
            {
                parentWindow.Close();
            }
        }
    }
}