using Emgu.CV;
using Emgu.CV.Structure;
using System.Threading;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static MFR_GUI.Pages.Globals;
using System.Drawing;
using Color = System.Drawing.Color;
using Emgu.CV.UI;
using System.Windows.Forms;
using System.IO;
using MessageBox = System.Windows.Forms.MessageBox;
using System.Collections.Generic;
using Emgu.CV.Models;
using Timer = System.Threading.Timer;

namespace MFR_GUI.Pages
{
    /// <summary>
    /// Interaktionslogik für BildHinzufuegen.xaml
    /// </summary>
    public partial class BildHinzufuegen : Page
    {
        //List<DetectedObject> fullFaceRegions = new List<DetectedObject>();
        //List<DetectedObject> partialFaceRegions = new List<DetectedObject>();
        ImageBox imgBoxKamera;
        Timer timer;

        public BildHinzufuegen()
        {
            InitializeComponent();

            Task t = Task.Factory.StartNew(() =>
            {
                Thread.Sleep(100);

                timer = new Timer(FrameGrabber, null, 100, 50);
            });
        }

        public void FrameGrabber(object state)
        {
            List<DetectedObject> fullFaceRegions = new List<DetectedObject>();
            List<DetectedObject> partialFaceRegions = new List<DetectedObject>();
            Image<Bgr, Byte> currentFrame;

            //Get the current frame from capture device
            currentFrame = grabber.QueryFrame().ToImage<Bgr, Byte>().Resize(320, 240, Emgu.CV.CvEnum.Inter.Cubic);

            if (Monitor.TryEnter(syncObj))
            {
                //Detect rectangular regions which contain a face
                faceDetector.Detect(currentFrame, fullFaceRegions, partialFaceRegions);

                foreach (DetectedObject d in fullFaceRegions)
                {
                    currentFrame.Draw(d.Region, new Bgr(Color.Red), 1);
                }

                //Show the image with the drawn face
                imgBoxKamera.Image = currentFrame;

                Monitor.Exit(syncObj);
            }
        }

        public void FrameGrabber2(Object stateInfo)
        {
            List<DetectedObject> fullFaceRegions = new List<DetectedObject>();
            List<DetectedObject> partialFaceRegions = new List<DetectedObject>();
            Image<Bgr, Byte>? currentFrame;
            //Logger.LogInfo("FrameGrabber", "Event ThreadIdle was raised");

            //Try entering critical region
            if (Monitor.TryEnter(syncObj))
            {
                Logger.LogInfo("FrameGrabber", "Lock on syncObj aquired");

                //Get the current frame from capture device
                currentFrame = grabber.QueryFrame().ToImage<Bgr, Byte>().Resize(320, 240, Emgu.CV.CvEnum.Inter.Cubic);
            
                //Detect rectangular regions which contain a face
                faceDetector.Detect(currentFrame, fullFaceRegions, partialFaceRegions);

                foreach (DetectedObject d in fullFaceRegions)
                {
                    currentFrame.Draw(d.Region, new Bgr(Color.Red), 1);
                }

                Logger.LogInfo("FrameGrabber", "Setting new image");
                //Show the image with the drawn face
                imgBoxKamera.Image = currentFrame;
                //Empty the lists for face-dedection
                fullFaceRegions = new List<DetectedObject>();
                partialFaceRegions = new List<DetectedObject>();

                Monitor.Exit(syncObj);

                Logger.LogInfo("FrameGrabber", "End of Method");
            }
            else
            {
                Logger.LogInfo("FrameGrabber", "Lock on syncObj couldn't be aquired");
            }
        }

        private void btn_speichern_Click(object sender, RoutedEventArgs e)
        {
            //Create and start a Task
            Task t = Task.Factory.StartNew(() =>
            {
                List<DetectedObject> fullFaceRegions = new List<DetectedObject>();
                List<DetectedObject> partialFaceRegions = new List<DetectedObject>();
                Image<Bgr, Byte> currentFrame;

                string name = get_txt_Name();
                
                try
                {
                    //Get the current frame from capture device
                    currentFrame = grabber.QueryFrame().ToImage<Bgr, Byte>().Resize(320, 240, Emgu.CV.CvEnum.Inter.Cubic);

                    gray = currentFrame.Convert<Gray, Byte>();

                    List<Rectangle> dedectedFaces = new List<Rectangle>();

                    //Enter critical region
                    lock (syncObj)
                    {
                        //Detect rectangular regions which contain a face
                        faceDetector.Detect(currentFrame, fullFaceRegions, partialFaceRegions);
                    }

                    if (fullFaceRegions.Count != 0)
                    {
                        //Take the first region as the training face
                        TrainingFace = gray.Copy(fullFaceRegions[0].Region);
                    
                        //Resize the image of the detected face and add the image and label to the lists for training
                        TrainingFace = TrainingFace.Resize(320, 240, Emgu.CV.CvEnum.Inter.Cubic);
                        trainingImagesMat.Add(TrainingFace.Mat);

                        string trainingFacesDirectory = projectDirectory + "/TrainingFaces/";

                        if (!labels.Contains(name))
                        {
                            labels.Add(name);
                            labelNr.Add(savedNamesCount++);
                            File.AppendAllText(trainingFacesDirectory + "TrainedLabels.txt", "%" + name);
                        }
                        else
                        {
                            labelNr.Add(labels.IndexOf(name));
                        }

                        //Train the recognizer with all Images and Labels
                        recognizer.Train(trainingImagesMat.ToArray(), labelNr.ToArray());

                        //save the images as bitmap-file
                        if (!Directory.Exists(trainingFacesDirectory + name + "/"))
                        {
                            Directory.CreateDirectory(trainingFacesDirectory + name + "/");
                        }

                        int i;
                        for(i = 0; File.Exists(trainingFacesDirectory + name + "/" + name + i + ".bmp"); i++);

                        trainingImagesMat[trainingImagesMat.Count - 1].Save(trainingFacesDirectory + name + "/" + name + i + ".bmp");

                        fullFaceRegions = new List<DetectedObject>();
                        partialFaceRegions = new List<DetectedObject>();

                        //Show a MessageBox for confirmation of successful training
                        l_Fehler.Foreground = System.Windows.Media.Brushes.Green;
                        l_Fehler.Content = "Gesicht wurde erkannt";                        
                    }
                    else
                    {
                        l_Fehler.Foreground = System.Windows.Media.Brushes.Red;
                        l_Fehler.Content = "Kein Gesicht erkannt";                       
                    }
                }
                catch (Exception ex)
                {
                    //Show a MessageBox if there was an exception
                    MessageBox.Show("Enable the face detection first", "Training Fail", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            });
        }

        private void btn_Zurueck1_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Menu());
        }

        private void i_Kamera_Loaded(object sender, RoutedEventArgs e)
        {
            //Create the interop host control.
            System.Windows.Forms.Integration.WindowsFormsHost host = new System.Windows.Forms.Integration.WindowsFormsHost();

            //Create the ImageBox control.
            imgBoxKamera = new ImageBox();
            imgBoxKamera.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            imgBoxKamera.Enabled = false;

            Grid.SetColumn(host, 3);
            Grid.SetRow(host, 1);
            Grid.SetColumnSpan(host, 2);
            Grid.SetRowSpan(host, 6);

            // Assign the ImageBox control as the host control's child.
            host.Child = imgBoxKamera;
            //Add the interop host control to the Grid
            //control's collection of child controls.
            this.grid2.Children.Add(host);

            this.SizeChanged += hideScrollbars;
        }

        /// <summary>
        /// Hides the vertical and horizontal scrollbar of imgBoxKamera
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void hideScrollbars(object sender, RoutedEventArgs e)
        {
            imgBoxKamera.HorizontalScrollBar.Hide();
            imgBoxKamera.VerticalScrollBar.Hide();
        }

        //Threadsafe method
        /// <summary>
        /// Gets the Name Property of the TextView txt_Name.
        /// This function can be called in a thread outside of the Main-Thread.
        /// </summary>
        private string get_txt_Name()
        {
            if (this.txt_Name.Dispatcher.CheckAccess())
            {
                //We are on the thread that owns the control
                return txt_Name.Text;
            }
            else
            {
                //We are on a different thread, that's why we need to call Invoke to execute the method on the thread onwing the control
                return (String) this.Dispatcher.Invoke(new GetTextFromTextBoxDelegate(this.get_txt_Name));
            }
        }
    }
}