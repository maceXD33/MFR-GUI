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

            // Initialize the Detectors if they haven't been already
            if (!detectorsLoaded)
            {
                facemarkDetector.Init();
                faceDetector.Init();
                faceDetector1.Init();
                faceDetector2.Init();

                // Set the detectorsLoaded to true so that they aren't initialized when returning from PasswortÄndern
                detectorsLoaded = true;
            }

            List<string> cameraNames = new List<string>();
            
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE (PNPClass = 'Image' OR PNPClass = 'Camera')"))
            {
                foreach (ManagementBaseObject device in searcher.Get())
                {
                    cameraNames.Add(device["Caption"].ToString());
                }
            }

            foreach (string cameraName in cameraNames)
            {
                kameraAuswahl.Items.Add(cameraName);
            }

            // Assign default values in case the user doesn't select a camera
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
                // Acquire lock so other pages can't use the VideoCapture before it's initialized
                lock (syncObj)
                {
                    // Initialize the capture device
                    videoCapture = new VideoCapture(cameraIndex, VideoCapture.API.DShow);              
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