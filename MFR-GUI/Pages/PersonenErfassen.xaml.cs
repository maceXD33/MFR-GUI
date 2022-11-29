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
using Point = System.Drawing.Point;
using Color = System.Drawing.Color;
using Emgu.CV.UI;
using System.Windows.Interop;
using Emgu.CV.Models;
using System.Collections.Generic;
using Brushes = System.Windows.Media.Brushes;
using Timer = System.Threading.Timer;
using System.Diagnostics;
using Emgu.CV.Util;
using System.Drawing;
using System.Xml.Linq;

namespace MFR_GUI.Pages
{
    /// <summary>
    /// Interaktionslogik für PersonenErfassen.xaml
    /// </summary>
    public partial class PersonenErfassen : Page
    {
        ImageBox imgBoxKamera;
        Timer timer;
        int count = 0;

        public PersonenErfassen()
        {
            InitializeComponent();
            Label1.Content = "Status";
            Label2.Content = "Name";
        }

        private void btn_Zurueck3_Click(object sender, RoutedEventArgs e)
        {
            timer.Dispose();
            this.NavigationService.Navigate(new Menu());
        }

        void FrameGrabber(object o)
        {
            List<DetectedObject> fullFaceRegions = new List<DetectedObject>();
            List<DetectedObject> partialFaceRegions = new List<DetectedObject>();
            Image<Bgr, Byte>? currentFrame;
            string status = "nicht erkannt";
            string recognizedNames = "";

            Logger.LogInfo("PersonenErfassen - FrameGrabber", "FrameGrabber started");

            //Get the current frame from capture device
            currentFrame = grabber.QueryFrame().ToImage<Bgr, Byte>().Resize(320, 240, Emgu.CV.CvEnum.Inter.Cubic);

            if (Monitor.TryEnter(syncObj))
            {

                //Detect rectangular regions which contain a face
                faceDetector.Detect(currentFrame, fullFaceRegions, partialFaceRegions);

                Monitor.Exit(syncObj);

                List<Rectangle> recs = new List<Rectangle>();

                //Action for each region detected
                foreach (DetectedObject d in fullFaceRegions)
                {
                    recs.Add(d.Region);
                }

                VectorOfVectorOfPointF vovop = fd.Detect(currentFrame, recs.ToArray());

                for (int i = 0; i < vovop.Size; i++)
                {
                    //Check if there are any trained faces
                    if (savedNamesCount != 0)
                    {
                        result = rotateAndAlignPicture(currentFrame, vovop[i], fullFaceRegions[i]);

                        result = result.Resize(240, 240, Emgu.CV.CvEnum.Inter.Cubic);

                        //result.Save(projectDirectory + "/" +  count++ + ".bmp");

                        //Draw a rectangle around the region
                        currentFrame.Draw(recs[i], new Bgr(Color.Red), 1);

                        if (Monitor.TryEnter(syncObj))
                        {
                            //Get the result of the prediction from the recognizer
                            FaceRecognizer.PredictionResult res = recognizer.Predict(result);

                            Monitor.Exit(syncObj);

                            //res.Distance < n determs how familiar the faces must look
                            if (res.Distance <= 35)
                            {
                                //Draw the label for the detected face
                                currentFrame.Draw(labels[res.Label] + "," + res.Distance, new Point(recs[i].X - 5, recs[i].Y - 5), FontFace.HersheyTriplex, 1.0d, new Bgr(Color.LightGreen), thickness: 1);

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
                                currentFrame.Draw("Unbekannt" + "," + res.Distance, new Point(recs[i].X - 5, recs[i].Y - 5), FontFace.HersheyTriplex, 1.0d, new Bgr(Color.LightGreen), thickness: 1);
                            }
                        }
                    }
                    else
                    {
                        //Draw the label "Unkown" as there are no faces in the database
                        currentFrame.Draw("Unbekannt", new Point(recs[i].X - 5, recs[i].Y - 5), FontFace.HersheyTriplex, 1.0d, new Bgr(Color.LightGreen), thickness: 1);
                    }
                }

                SetGUIElements(currentFrame, status, recognizedNames);
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

            timer = new Timer(FrameGrabber, null, 100, 50);
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
        /// Sets the Background of Label1 to Green, when status is "erkannt". Sets the Background of Label1 to Red, when status is "unbekannt".
        /// 
        /// </summary>
        /// <param name="image">The Image that will be set for the ImageBox imgBoxKamera</param>
        /// <param name="status">The string that will determ the colour of the Background and it will be written as the Content for the Label Label1</param>
        /// <param name="recognizedNames">The string that will be written as the content for the Label Label2</param>
        private void SetGUIElements(Image<Bgr, byte> image, string status, string recognizedNames)
        {
            if (this.Label1.Dispatcher.CheckAccess())
            {
                //We are on the thread that owns the control
                if (status == "erkannt")
                {
                    Label1.Background = Brushes.Green;
                }
                else
                {
                    Label1.Background = Brushes.OrangeRed;
                }

                //Show the image with the drawn face
                imgBoxKamera.Image = image;
                //Show weither a face got recognized
                Label1.Content = status;
                //Show the labels of the faces that were recognized
                Label2.Content = recognizedNames;
            }
            else
            {
                //We are on a different thread, that's why we need to call Invoke to execute the method on the thread onwing the control
                this.Dispatcher.Invoke(new SetGUIElementsDelegate(this.SetGUIElements), image, status, recognizedNames);
            }
        }

        private Image<Gray, byte> rotateAndAlignPicture(Image<Bgr, byte> image, VectorOfPointF vop, DetectedObject detectedObject)
        {
            Point Center = new Point(0, 0);

            for (int j = 36; j < 42; j++)
            {
                Point p = Point.Round(vop[j]);
                Logger.LogInfo((j + 1).ToString(), "X: " + vop[j].X + " Y:" + vop[j].Y);
                Center.Offset(p);
            }

            Point rightEyeCenter = new Point(Center.X / 6, Center.Y / 6);
            Center = new Point(0, 0);

            for (int j = 42; j < 48; j++)
            {
                Point p = Point.Round(vop[j]);
                Logger.LogInfo((j + 1).ToString(), "X: " + vop[j].X + " Y:" + vop[j].Y);
                Center.Offset(p);
            }

            Point leftEyeCenter = new Point(Center.X / 6, Center.Y / 6);

            double value = (double)(leftEyeCenter.Y - rightEyeCenter.Y) / (leftEyeCenter.X - rightEyeCenter.X);
            double angle = (Math.Atan(value) * 180) / Math.PI;
            
            return image.Convert<Gray, byte>().Copy(detectedObject.Region).Rotate(-angle, new Gray(127));
        }
    }
}