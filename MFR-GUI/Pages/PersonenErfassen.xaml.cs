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

namespace MFR_GUI.Pages
{
    /// <summary>
    /// Interaktionslogik für PersonenErfassen.xaml
    /// </summary>
    public partial class PersonenErfassen : Page
    {
        ImageBox imgBoxKamera;
        long longestTime = 0;

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
            this.NavigationService.Navigate(new Menu());
        }

        void FrameGrabber(object sender, EventArgs e)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            //Get the current frame from capture device
            lock (syncObj)
            {
                currentFrame = grabber.QueryFrame().ToImage<Bgr, Byte>().Resize(1920, 1080, Emgu.CV.CvEnum.Inter.Cubic);
            }

            //Convert it to Grayscale
            gray = currentFrame.Convert<Gray, Byte>();

            //Detect rectangular regions which contain a face
            Rectangle[] detectedFrontalFaces = face.DetectMultiScale(gray);

            //Action for each region detected
            foreach (Rectangle r in detectedFrontalFaces)
            {
                //Get the rectangular region out of the whole image
                result = currentFrame.Copy(r).Convert<Gray, Byte>().Resize(1080, 1080, Emgu.CV.CvEnum.Inter.Cubic);

                //Draw a rectangle around the region
                currentFrame.Draw(r, new Bgr(Color.Red), 3);

                //Try entering the critical region on the synchronizing object for the recognizer
                if (Monitor.TryEnter(syncObj))
                {
                    //Check if there are any trained faces
                    if (trainingFacesCount != 0)
                    {
                        //Get the result of the prediction from the recognizer
                        FaceRecognizer.PredictionResult res = recognizer.Predict(result);

                        //res.Distance < n determs how familiar the faces must look
                        if (res.Distance < 12000)
                        {
                            //Draw the label for the detected face
                            currentFrame.Draw(labels[res.Label] + ", " + res.Distance, new Point(r.X - 5, r.Y - 5), FontFace.HersheyTriplex, 1.0d, new Bgr(Color.LightGreen));

                            //Add the label to the recognized faces
                            recognizedNames += labels[res.Label] + ", ";
                        }
                        else
                        {
                            //Draw the label "Unkown" as the criteria for same face was not met
                            currentFrame.Draw("Unbekannt" + ", " + res.Distance, new Point(r.X - 5, r.Y - 5), FontFace.HersheyTriplex, 0.5d, new Bgr(Color.LightGreen));

                            //Add the label "Unkown" to the recognized faces
                            recognizedNames += "Unbekannt, ";
                        }
                    }
                    else
                    {
                        //Add the Add the label "Unkown" to the recognized faces, because there is no face that can be recognized
                        recognizedNames += "Unbekannt, ";
                    }

                    //Release the lock on the synchronizing Object
                    Monitor.Exit(syncObj);
                }

                //Set the number of faces detected on the scene
                Label2.Content = detectedFrontalFaces.Length.ToString();
            }

            sw.Stop();
            if (longestTime < sw.ElapsedMilliseconds)
            {
                Task.Delay((int)(sw.ElapsedMilliseconds - longestTime));
                longestTime = sw.ElapsedMilliseconds;
            }

            //Show the image with the drawn face
            imgBoxKamera.Image = currentFrame;
            //Show the labels of the faces that were recognized
            Label1.Content = recognizedNames;
            //Empty the recognized faces
            recognizedNames = "";
        }

        private void i_Kamera_Loaded(object sender, RoutedEventArgs e)
        {
            // Create the interop host control.
            System.Windows.Forms.Integration.WindowsFormsHost host = new System.Windows.Forms.Integration.WindowsFormsHost();

            // Create the ImageBox control.
            imgBoxKamera = new ImageBox();
            imgBoxKamera.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            imgBoxKamera.Enabled = false;

            //imgBoxKamera.Location = new Point(400, 200);

            Grid.SetColumn(host, 3);
            Grid.SetRow(host, 1);
            Grid.SetColumnSpan(host, 2);
            Grid.SetRowSpan(host, 6);

            // Assign the ImageBox control as the host control's child.
            host.Child = imgBoxKamera;

            // Add the interop host control to the Grid
            // control's collection of child controls.
            this.grid2.Children.Add(host);
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