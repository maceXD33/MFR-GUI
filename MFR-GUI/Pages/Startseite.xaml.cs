using Emgu.CV;
using System;
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
                faceDetector1.Init();
                faceDetector2.Init();

                // Set the detectorsLoaded to true so that they aren't initialized when returning from PasswortÄndern
                detectorsLoaded = true;
            }

            List<string> cameraNames = new List<string>();
            
            // 
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

            if (cameraNames.Count > 0)
            {
                // Assign default values in case the user doesn't select a camera
                kameraAuswahl.Text = cameraNames[0];
                cameraIndex = 0;
            }
            else
            {
                cameraIndex = -1;
            }
        }

        private void Btn_passwort_Click(object sender, RoutedEventArgs e)
        {
            PasswortÄndern p = new PasswortÄndern();
            this.NavigationService.Navigate(p);
        }

        private void Btn_speichern_Click(object sender, RoutedEventArgs e)
        {
            if (cameraIndex >= 0)
            {
                Task t = Task.Factory.StartNew(() =>
                {
                    // Acquire lock so other pages can't use the VideoCapture before it's initialized
                    lock (syncObj1)
                    {
                        // Initialize the capture device
                        videoCapture = new VideoCapture(cameraIndex, VideoCapture.API.DShow);
                    }
                });

                Menu m = new Menu();
                this.NavigationService.Navigate(m);
            }
            else
            {
                l_Fehler.Content = "Keine Kamera!";
            }
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cameraIndex = kameraAuswahl.SelectedIndex;
        }
    }
}