using Emgu.CV.Face;
using Emgu.CV;
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
using static MFR_GUI.Pages.Globals;
using System.IO;

namespace MFR_GUI.Pages
{
    /// <summary>
    /// Interaktionslogik für Menu.xaml
    /// </summary>
    public partial class Menu : Page
    {
        public Menu()
        {
            InitializeComponent();

            //Get the working- and project-directory for further load
            Globals.workingDirectory = Environment.CurrentDirectory;
            Globals.projectDirectory = Directory.GetParent(Globals.workingDirectory).Parent.Parent.FullName;

            //Load haarcascades for face detection
            face = new CascadeClassifier(Globals.projectDirectory + "\\Haarcascade\\haarcascade_frontalface_alt.xml");

        }

        private void btn_erfassen_Click(object sender, RoutedEventArgs e)
        {
            PersonenErfassen pe = new PersonenErfassen();
            this.NavigationService.Navigate(pe);
        }

        private void btn_hinzufuegen_Click(object sender, RoutedEventArgs e)
        {
            BildHinzufuegen bh = new BildHinzufuegen();
            this.NavigationService.Navigate(bh);
        }

        private void btn_anzeigen_Click(object sender, RoutedEventArgs e)
        {
            PersonenAnzeigen pa = new PersonenAnzeigen();
            this.NavigationService.Navigate(pa);
        }

        private void btn_beenden_Click(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow((DependencyObject)sender);
            if (parentWindow != null)
            {
                parentWindow.Close();
            }
        }
    }
}
