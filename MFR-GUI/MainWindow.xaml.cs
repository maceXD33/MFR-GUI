using System.Windows;
using MFR_GUI.Pages;

namespace MFR_GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Startseite s = new Startseite();
            myframe.NavigationService.Navigate(s);
        }
    }
}