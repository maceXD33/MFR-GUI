using Emgu.CV;
using System;
using System.Collections.Generic;
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

            // Check if the data was already loaded
            // Data gets loaded the first time getting to the menu
            if (!dataLoaded)
            {
                // Check if the FaceRecognizer should be loaded from a file
                if (loadRecognizerFromFile)
                {
                    TrainingFacesLoader.LoadFaceRecognizer();
                }
                else
                {
                    // Load the images and label numbers for training
                    Tuple<List<Mat>, List<int>> tuple = LoadTrainingFacesForMenu(_logger);

                    List<Mat> trainingImagesMat = tuple.Item1;
                    List<int> labelNumbers = tuple.Item2;

                    // Train the recognizer with the images and label numbers
                    if (trainingImagesMat != null && labelNumbers != null)
                    {
                        recognizer.Train(trainingImagesMat.ToArray(), labelNumbers.ToArray());
                    }
                }

                // Set dataLoaded to true so the data doesn't get loaded again
                dataLoaded = true;
            }
        }

        /// <summary>
        /// Gets called when the btn_erfassen gets clicked and navigates to the corresponding page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_erfassen_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new PersonenErfassen());
        }

        /// <summary>
        /// Gets called when the btn_hinzufuegen gets clicked and navigates to the corresponding page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_hinzufuegen_Click(object sender, RoutedEventArgs e)
        {
            // Check if the user has already entered the password once
            if (passwordChecked)
            {
                // The password has already been entered
                // Navigate straight to the page BildHinzufügen
                BildHinzufuegen bh = new BildHinzufuegen();
                this.NavigationService.Navigate(bh);
            }
            else
            {
                // The password has never been entered
                // Navigate to the page to first enter the password
                Passwort_BildHinzufuegen pbh = new Passwort_BildHinzufuegen();
                this.NavigationService.Navigate(pbh);
            }
        }

        /// <summary>
        /// Gets called when the btn_anzeigen gets clicked and navigates to the corresponding page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_anzeigen_Click(object sender, RoutedEventArgs e)
        {
            // Check if the user has already entered the password once
            if (passwordChecked)
            {
                // The password has already been entered
                // Navigate straight to the page PersonenAnzeigen
                PersonenAnzeigen pa = new PersonenAnzeigen();
                this.NavigationService.Navigate(pa);
            }
            else
            {
                // The password has never been entered
                // Navigate to the page to first enter the password
                Passwort_PersonenAnzeigen ppa = new Passwort_PersonenAnzeigen();
                this.NavigationService.Navigate(ppa);
            }
        }

        /// <summary>
        /// Gets called when the btn_beenden gets clicked and the window gets closed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_beenden_Click(object sender, RoutedEventArgs e)
        {
            //Dispose of the objects if they are not null
            if (videoCapture != null)
            { 
                videoCapture.Dispose();
            }
            if (recognizer != null)
            {
                recognizer.Dispose();
            }

            // Get the window in which the sender is located
            Window parentWindow = Window.GetWindow((DependencyObject)sender);
            
            // Close the window if it isn't null
            if (parentWindow != null)
            {
                parentWindow.Close();
            }
        }
    }
}