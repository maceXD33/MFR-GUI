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
using Emgu.CV.Structure;
using System.Windows.Forms;
using MessageBox = System.Windows.Forms.MessageBox;

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
            face = new CascadeClassifier(Globals.projectDirectory + "/Haarcascade/haarcascade_frontalface_alt.xml");
            
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!this.NavigationService.CanGoBack)
            {
                try
                {
                    //Load the file with the labels from previous trained faces and get the labels and the number of trained faces
                    string allLabels = File.ReadAllText(projectDirectory + "/TrainingFaces/TrainedLabels.txt");
                    string[] Labels = allLabels.Split('%');
                    trainingFacesCount = Convert.ToInt16(Labels[0]);

                    string[] distinctLabels = Labels.Distinct().ToArray();
                    
                    //Load the images from previous trained faces and add the images, labels and labelnumbers to Lists
                    int totalCount = 0;
                    foreach (string name in distinctLabels)
                    {
                        for (int i = 0; File.Exists(projectDirectory + "/TrainingFaces/" + name + "/" + name + i + ".bmp"); i++, totalCount++)
                        {
                            Image<Gray, byte> image = new Image<Gray, byte>(projectDirectory + "/TrainingFaces/" + name + "/" + name + i + ".bmp");
                            trainingImagesMat.Add(image.Mat);
                            labels.Add(Labels[totalCount + 1]);
                            labelNr.Add(totalCount);
                        }
                    }

                    //Train the facerecognizer with the images and labelnumbers
                    recognizer.Train(trainingImagesMat.ToArray(), labelNr.ToArray());
                }
                catch
                {
                    //Show a MessageBox if there was an exception
                    MessageBox.Show("Nothing in binary database, please add at least a face(Simply train the prototype with the Add Face Button).", "Triained faces load", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }

        private void btn_erfassen_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new PersonenErfassen());
        }

        private void btn_hinzufuegen_Click(object sender, RoutedEventArgs e)
        {
            Passwort_BildHinzufuegen pbh = new Passwort_BildHinzufuegen();
            this.NavigationService.Navigate(pbh);
        }

        private void btn_anzeigen_Click(object sender, RoutedEventArgs e)
        {
            Passwort_PersonenAnzeigen ppa = new Passwort_PersonenAnzeigen();
            this.NavigationService.Navigate(ppa);
        }

        private void btn_beenden_Click(object sender, RoutedEventArgs e)
        {
            if (grabber != null)
            { 
                grabber.Dispose();
            }
            if (face != null)
            {
                face.Dispose();
            }
            if (currentFrame != null)
            {
                currentFrame.Dispose();
            }
            if (gray != null)
            {
                gray.Dispose();
            }
            if (TrainingFace != null)
            {
                TrainingFace.Dispose();
            }
            if (result != null)
            {
                result.Dispose();
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