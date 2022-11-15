using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Face;
using Emgu.CV.Structure;
using System.Threading;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static MFR_GUI.Pages.Globals;
using System.Drawing;
using Point = System.Drawing.Point;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;
using Emgu.CV.UI;
using Size = System.Drawing.Size;
using System.Windows.Interop;
using System.Windows.Forms;
using System.Diagnostics;
using System.Linq;
using Emgu.CV.Models;
using System.Collections.Generic;
using Brushes = System.Windows.Media.Brushes;
using ColorConverter = System.Drawing.ColorConverter;
using Brush = System.Windows.Media.Brush;

namespace MFR_GUI.Pages
{
    /// <summary>
    /// Interaktionslogik für PersonenErfassen.xaml
    /// </summary>
    public partial class PersonenErfassen : Page
    {
        ImageBox imgBoxKamera;
        List<DetectedObject> fullFaceRegions = new List<DetectedObject>();
        List<DetectedObject> partialFaceRegions = new List<DetectedObject>();

        public PersonenErfassen()
        {
            InitializeComponent();
            Label1.Content = "Status";
            Label2.Content = "Name";

            //Create and start a Task
            Task t = Task.Factory.StartNew(() =>
            {
                //Initialize the capture device
                this.AddFrameGrabberEvent();
            });
        }

        private void btn_Zurueck3_Click(object sender, RoutedEventArgs e)
        {
            ComponentDispatcher.ThreadIdle -= FrameGrabber;
            i_Kamera.Loaded -= i_Kamera_Loaded;
            this.SizeChanged -= hideScrollbars;
            imgBoxKamera.Dispose();
            fullFaceRegions.Clear();
            partialFaceRegions.Clear();

            this.NavigationService.Navigate(new Menu());
        }

        void FrameGrabber(object sender, EventArgs e)
        {
            string status = "nicht erkannt";
            string recognizedNames = "";

            if(Monitor.TryEnter(syncObj))
            {
                //Get the current frame from capture device
                currentFrame = grabber.QueryFrame().ToImage<Bgr, Byte>().Resize(1920, 1080, Emgu.CV.CvEnum.Inter.Cubic);

                //Detect rectangular regions which contain a face
                faceDetector.Detect(currentFrame, fullFaceRegions, partialFaceRegions);

                //Action for each region detected
                foreach (DetectedObject d in fullFaceRegions)
                {
                    //Get the rectangular region out of the whole image
                    result = currentFrame.Copy(d.Region).Convert<Gray, Byte>().Resize(1080, 1080, Emgu.CV.CvEnum.Inter.Cubic);

                    //Draw a rectangle around the region
                    currentFrame.Draw(d.Region, new Bgr(Color.Red), 3);

                    //Check if there are any trained faces
                    if (savedNamesCount != 0)
                    {
                        //Get the result of the prediction from the recognizer
                        FaceRecognizer.PredictionResult res = recognizer.Predict(result);

                        //res.Distance < n determs how familiar the faces must look
                        if (res.Distance <= 20)
                        {
                            //Draw the label for the detected face
                            currentFrame.Draw(labels[res.Label], new Point(d.Region.X - 5, d.Region.Y - 5), FontFace.HersheyTriplex, 3.0d, new Bgr(Color.LightGreen), thickness: 5);

                            //Add the label to the recognized faces
                            if (recognizedNames != "")
                            {
                                recognizedNames += ", ";
                            }

                            recognizedNames += labels[res.Label];
                            
                            status = "erkannt";                       
                        }
                        else
                        {
                            //Draw the label "Unkown" as the criteria for same face was not met
                            currentFrame.Draw("Unbekannt", new Point(d.Region.X - 5, d.Region.Y - 5), FontFace.HersheyTriplex, 2.0d, new Bgr(Color.LightGreen), thickness: 3);
                        }
                    }
                    else
                    {
                        //Draw the label "Unkown" as there are no faces in the database
                        currentFrame.Draw("Unbekannt", new Point(d.Region.X - 5, d.Region.Y - 5), FontFace.HersheyTriplex, 2.0d, new Bgr(Color.LightGreen), thickness: 3);                      
                    }
                }

                if (status == "erkannt")
                {
                    Label1.Background = Brushes.Green;
                }
                else
                {
                    Label1.Background = Brushes.Red;
                }

                //Show the image with the drawn face
                imgBoxKamera.Image = currentFrame;
                //Show weither a face got recognized
                Label1.Content = status;
                //Show the labels of the faces that were recognized
                Label2.Content = recognizedNames;
                //Empty the recognized faces
                recognizedNames = "";
                //Empty the lists for face-dedection
                fullFaceRegions = new List<DetectedObject>();

                //Release the lock on the synchronizing Object
                Monitor.Exit(syncObj);
            }
        }

        private void i_Kamera_Loaded(object sender, RoutedEventArgs e)
        {
            // Create the interop host control.
            System.Windows.Forms.Integration.WindowsFormsHost host = new System.Windows.Forms.Integration.WindowsFormsHost();

            // Create the ImageBox control.
            imgBoxKamera = new ImageBox();
            imgBoxKamera.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            imgBoxKamera.Enabled = false;

            Grid.SetColumn(host, 3);
            Grid.SetRow(host, 1);
            Grid.SetColumnSpan(host, 2);
            Grid.SetRowSpan(host, 6);

            // Assign the ImageBox control as the host control's child.
            host.Child = imgBoxKamera;

            // Add the interop host control to the Grid
            // control's collection of child controls.
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
        /// Add the function FrameGrabber to the Event ComponentDispatcher.ThreadIdle
        /// This function can be called in a thread outside of the Main-Thread.
        /// </summary>
        private void AddFrameGrabberEvent()
        {
            if (this.btn_Zurueck3.Dispatcher.CheckAccess())
            {
                //We are on the thread that owns the control
                ComponentDispatcher.ThreadIdle += FrameGrabber;
            }
            else
            {
                //We are on a different thread, that's why we need to call Invoke to execute the method on the thread onwing the control
                this.Dispatcher.Invoke(new SetGrabberValuesDelegate(this.AddFrameGrabberEvent));
            }
        }
    }
}