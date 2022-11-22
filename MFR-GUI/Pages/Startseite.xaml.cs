using Emgu.CV;
using System.Collections.Generic;
using System.Management;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static MFR_GUI.Pages.Globals;

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

            faceDetector.Init();

            if (!password_checked)
            {
                password_checked = false;
            }

            List<string> cameraNames = new List<string>();
            
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE (PNPClass = 'Image'OR PNPClass = 'Camera')"))
            {
                foreach (var device in searcher.Get())
                {
                    cameraNames.Add(device["Caption"].ToString());
                }
            }

            foreach (string cameraName in cameraNames)
            {
                kameraAuswahl.Items.Add(cameraName);
            }

            //Assign default-values in case the user doesn't select a camera
            kameraAuswahl.Text = cameraNames[0];
            cameraIndex = 0;
        }

        private void btn_passwort_Click(object sender, RoutedEventArgs e)
        {
            PasswortÄndern p = new PasswortÄndern();
            this.NavigationService.Navigate(p);
        }

        private void btn_speichern_Click(object sender, RoutedEventArgs e)
        {
            Task t = Task.Factory.StartNew(() =>
            {
                //Enter critical region
                lock (syncObj)
                {
                    //Initialize the capture device
                    grabber = new VideoCapture(cameraIndex, VideoCapture.API.DShow); //VideoCapture.API.DShow                
                }
            });
            
            Menu m = new Menu();
            this.NavigationService.Navigate(m);
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cameraIndex = kameraAuswahl.SelectedIndex;
        }
    }
}