using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.Threading;
using System;
using System.Windows;
using System.Windows.Controls;
using Emgu.CV.UI;
using Emgu.CV.Models;
using System.Collections.Generic;
using Emgu.CV.Util;
using System.Drawing;
using System.Windows.Forms.Integration;
using System.Windows.Forms;
using Point = System.Drawing.Point;
using Color = System.Drawing.Color;
using Brushes = System.Windows.Media.Brushes;
using Timer = System.Timers.Timer;
using static MFR_GUI.Pages.Globals;
using static MFR_GUI.Pages.TrainingFacesLoader;
using static Emgu.CV.Face.FaceRecognizer;

namespace MFR_GUI.Pages
{
    /// <summary>
    /// Interaktionslogik für PersonenErfassen.xaml
    /// </summary>
    public partial class PersonenErfassen : Page
    {
        // Declaration of fields
        private ImageBox _imgBoxKamera;
        private Timer _timer;
        private Logger _logger;

        private List<string> _labels;
        private int _savedNamesCount;

        private int a;

        public PersonenErfassen()
        {
            InitializeComponent();

            // Create a new Logger-Object and assing it to the _logger-field
            _logger = new Logger();

            // Load the labels and the count of the saved names 
            Tuple<List<string>, int> tuple = LoadTrainingFacesTwoReturns(_logger);

            // Assign the returned values to the fields
            _labels = tuple.Item1;
            _savedNamesCount = tuple.Item2;

            // Generate a ImageBox and start a Timer with FrameGrabber used for the Elapsed Event
            GenerateImageBox();
        }

        /// <summary>
        /// Gets called when the button for going back to the menu is clicked.
        /// Disposes of resources and then navigates to the menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_Zurueck3_Click(object sender, RoutedEventArgs e)
        {
            if(_timer != null)
            {
                // _timer is not null, so it gets stopped and disposed afterwards
                _timer.Stop();
                _timer.Dispose();
            }

            if (_labels != null)
            {
                // _labels is not null, so the list gets cleared
                _labels.Clear();
            }

            if (_imgBoxKamera != null)
            {
                // _imgBoxKamera is not null, so it gets disposed
                _imgBoxKamera.Dispose();
            }

            // Create a new Menu-object and navigate to it
            this.NavigationService.Navigate(new Menu());
        }

        /// <summary>
        /// Grabs frames from the camera and looks for faces on the frame.
        /// When it finds faces, a red rectangle gets drawn over the frame.
        /// Afterwards the faces run through the AI and if they belong to a name
        /// the name gets drawn above the red rectangle, else the word "Unbekannt" gets draw above.
        /// The frame gets set as the Image property of _imgBoxKamera and depending on weither a
        /// face was recognized or not the Content and Background of Label1 get set and the Content
        /// of Label2 gets set.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrameGrabber(object sender, EventArgs e)
        {
            // Declaration/Definition of method variables
            List<DetectedObject> fullFaceRegions = new List<DetectedObject>();
            List<DetectedObject> partialFaceRegions = new List<DetectedObject>();
            Image<Bgr, Byte> currentFrame;
            Image<Bgr, byte> result;
            string status = "nicht erkannt";
            string recognizedNames = "";

            // Get the next frame from the VideoCapture and resize it to 320x240
            currentFrame = videoCapture.QueryFrame().ToImage<Bgr, Byte>().Resize(320, 240, Emgu.CV.CvEnum.Inter.Cubic);

            // Try to acquire a lock on the synchronizing object to use the FaceDetector, 
            // because the .Detect() method can't handle multiple access and returns around
            // 50 or more wrong rectangles
            if (Monitor.TryEnter(syncObj1))
            {
                //Detect rectangular regions which contain a face
                faceDetector1.Detect(currentFrame, fullFaceRegions, partialFaceRegions, confidenceThreshold: (float)0.9);

                // Release the lock
                Monitor.Exit(syncObj1);

                // Define a List of rectangles where the detected faces will be safed
                List<Rectangle> recs = new List<Rectangle>();

                // Copy the currentFrame to result to have the frame without anything drawn on it
                result = currentFrame.Copy();

                // Run through the fullFaceRegions list, which contains faces
                foreach (DetectedObject d in fullFaceRegions)
                {
                    // Define a rectangle with the Region of the DetectedObject
                    Rectangle r = d.Region;

                    /*
                    if(r.Right < 320 && r.Bottom < 240)
                    {
                        recs.Add(r);
                    }
                    */

                    // Add the Rectangle to the list of Rectangles
                    recs.Add(r);

                    // Draw a red rectangle on the image around the detected face
                    currentFrame.Draw(r, new Bgr(Color.Red), 1);

                    if (_savedNamesCount > 0)
                    {
                        currentFrame.Draw("Unbekannt", new Point(r.X - 5, r.Y - 5), FontFace.HersheyComplexSmall, 1.0d, new Bgr(Color.LightGreen), thickness: 1);
                    }
                }

                // Check, if 
                if (fullFaceRegions.Count > 0)
                {
                    if (_savedNamesCount > 0)
                    {
                        VectorOfVectorOfPointF vovop = facemarkDetector.Detect(currentFrame, recs.ToArray());

                        PrepareFaces(vovop, fullFaceRegions, partialFaceRegions, currentFrame, result, recs, ref status, ref recognizedNames);
                    }
                }

                lock (syncObj1)
                {
                    SetGUIElements(currentFrame, status, recognizedNames);
                }

                _logger.LogInfo("Set Frame");
            }
        }

        private void PrepareFaces(VectorOfVectorOfPointF vovop, List<DetectedObject> fullFaceRegions, List<DetectedObject> partialFaceRegions, Image<Bgr, Byte> currentFrame, Image<Bgr, byte> result, List<Rectangle> recs, ref string status, ref string recognizedNames)
        {
            for (int i = 0; i < vovop.Size; i++)
            {
                //Check if there are any trained faces
                //if(_savedNamesCount != 0)
                {
                    //if (i <= fullFaceRegions.Count - 1)
                    {
                        ProcessFace(vovop[i], fullFaceRegions, partialFaceRegions, currentFrame, result, recs, ref status, ref recognizedNames, i);
                    }
                }
                /*
                else
                {
                    //Draw the label "Unkown" as there are no faces in the database
                    currentFrame.Draw("Unbekannt", new Point(recs[i].X - 5, recs[i].Y - 5), FontFace.HersheyComplexSmall, 1.0d, new Bgr(Color.LightGreen), thickness: 1);
                }
                */
            }
        }

        private void ProcessFace(VectorOfPointF vop, List<DetectedObject> fullFaceRegions, List<DetectedObject> partialFaceRegions, Image<Bgr, Byte> currentFrame, Image<Bgr, byte> result, List<Rectangle> recs, ref string status, ref string recognizedNames, int i)
        {
            result = ImageEditor.RotateAndAlignPicture(result, vop, fullFaceRegions[i], _logger);

            fullFaceRegions = new List<DetectedObject>();
            partialFaceRegions = new List<DetectedObject>();

            if (result != null)
            {
                //Enter critical region
                lock (syncObj1)
                {
                    //Detect rectangular regions which contain a face
                    faceDetector1.Detect(result, fullFaceRegions, partialFaceRegions, confidenceThreshold: (float)0.99);
                }

                if (fullFaceRegions.Count > 0)
                {
                    result = ImageEditor.CropImage(fullFaceRegions, result);
                    
                    RecognizeFace(currentFrame, result, recs, i, ref status, ref recognizedNames);
                }
            }
        }

        private void RecognizeFace(Image<Bgr, Byte> currentFrame, Image<Bgr, byte> result, List<Rectangle> recs, int i, ref string status, ref string recognizedNames)
        {
            if (result != null)
            {
                result = result.Resize(100, 100, Emgu.CV.CvEnum.Inter.Cubic);

                PredictionResult res;

                lock (syncObj1)
                {
                    //Get the result of the prediction from the recognizer
                    res = recognizer.Predict(result.Convert<Gray, Byte>());
                }

                EvaluateResult(currentFrame, recs, res, ref recognizedNames, ref status, i);
            }
            else
            {
                currentFrame.Draw("Unbekannt", new Point(recs[i].X - 5, recs[i].Y - 5), FontFace.HersheyComplexSmall, 1.0d, new Bgr(Color.LightGreen), thickness: 1);
                _logger.LogInfo("Falsch!");
            }
        }

        private void EvaluateResult(Image<Bgr, Byte> currentFrame, List<Rectangle> recs, PredictionResult res, ref string recognizedNames, ref string status, int i)
        {
            //res.Distance <= n determs how familiar the faces must look
            if (res.Distance <= 65)
            {
                //Draw the label for the detected face
                currentFrame.Draw(_labels[res.Label], new Point(recs[i].X - 5, recs[i].Y - 5), FontFace.HersheyComplex, 1.0d, new Bgr(Color.LightGreen), thickness: 1);

                //Add the label to the recognized faces
                if (recognizedNames != "")
                {
                    recognizedNames += ", ";
                }

                recognizedNames += _labels[res.Label];

                status = "erkannt";
            }
            else
            {
                //Draw the label "Unkown" as the criteria for same face was not met
                currentFrame.Draw("Unbekannt", new Point(recs[i].X - 5, recs[i].Y - 5), FontFace.HersheyComplexSmall, 1.0d, new Bgr(Color.LightGreen), thickness: 1);
            }
        }

        private void GenerateImageBox()
        {
            //Create the interop host control.
            WindowsFormsHost host = new WindowsFormsHost();

            //Create the ImageBox control.
            _imgBoxKamera = new ImageBox();

            _imgBoxKamera.BorderStyle = BorderStyle.FixedSingle;
            _imgBoxKamera.SizeMode = PictureBoxSizeMode.StretchImage;
            _imgBoxKamera.Enabled = false;

            this.SizeChanged += HideScrollbars;

            Grid.SetColumn(host, 3);
            Grid.SetRow(host, 1);
            Grid.SetColumnSpan(host, 2);
            Grid.SetRowSpan(host, 6);

            host.Background = Brushes.White;

            // Assign the ImageBox control as the host control's child.
            host.Child = _imgBoxKamera;
            //Add the interop host control to the Grid control's collection of child controls.
            this.grid2.Children.Add(host);

            _timer = new Timer();
            _timer.Elapsed += FrameGrabber;
            _timer.Interval = 20;
            _timer.Start();
        }

        /// <summary>
        /// Hides the vertical and horizontal scrollbar of imgBoxKamera
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HideScrollbars(object sender, RoutedEventArgs e)
        {
            _imgBoxKamera.HorizontalScrollBar.Hide();
            _imgBoxKamera.VerticalScrollBar.Hide();
        }

        //Threadsafe method
        /// <summary>
        /// Sets the Background of Label1 to Green when status is "erkannt". Sets the Background of Label1 to Red when status is "unbekannt".
        /// Sets the Content-Property of Label1 to status.
        /// Sets the Image-Property of _imgBoxKamera to image.
        /// Sets the Content-Property of Label2 to recognizedNames.
        /// This method can be called in a thread outside of the GUI-Thread.
        /// </summary>
        /// <param name="image">The Image that will be set for the ImageBox imgBoxKamera</param>
        /// <param name="status">The string that will determ the colour of the Background and it will be written as the Content for the Label Label1</param>
        /// <param name="recognizedNames">The string that will be written as the content for the Label Label2</param>
        private void SetGUIElements(Image<Bgr, byte> image, string status, string recognizedNames)
        {
            if (this.grid2.Dispatcher.CheckAccess())
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
                _imgBoxKamera.Image = image;
                //Show weither a face got recognized
                Label1.Content = status;
                //Show the labels of the faces that were recognized
                Label2.Content = recognizedNames;
            }
            else
            {
                //We are on a different thread, that's why we need to call Invoke to execute the method on the thread onwing the control
                this.Dispatcher.Invoke(this.SetGUIElements, image, status, recognizedNames);
            }
        }

        private void Testing()
        {
            Tuple<List<Mat>, List<Image<Bgr, byte>>, List<string>, List<int>> t = TrainingFacesLoader.GetTestingData(_logger);

            _labels = t.Item3;
            _savedNamesCount = t.Item4.Count;

            recognizer.Train(t.Item1.ToArray(), t.Item4.ToArray());

            List<DetectedObject> fullFaceRegions = new List<DetectedObject>();
            List<DetectedObject> partialFaceRegions = new List<DetectedObject>();
            Image<Bgr, Byte>? result;
            string status = "nicht erkannt";
            string recognizedNames = "";

            foreach (Image<Bgr, Byte> image in t.Item2)
            {
                if (Monitor.TryEnter(syncObj1))
                {
                    //Detect rectangular regions which contain a face
                    faceDetector1.Detect(image, fullFaceRegions, partialFaceRegions);

                    Monitor.Exit(syncObj1);

                    List<Rectangle> recs = new List<Rectangle>();
                    result = image.Copy();

                    //Action for each region detected
                    foreach (DetectedObject d in fullFaceRegions)
                    {
                        Rectangle r = d.Region;

                        recs.Add(r);

                        //Draw a rectangle around the region
                        image.Draw(r, new Bgr(Color.Red), 1);
                    }

                    if (fullFaceRegions != null && fullFaceRegions.Count > 0 && result != null)
                    {
                        VectorOfVectorOfPointF vovop = facemarkDetector.Detect(image, recs.ToArray());

                        PrepareFaces(vovop, fullFaceRegions, partialFaceRegions, image, result, recs, ref status, ref recognizedNames);
                    }

                    image.Save(projectDirectory + "/TrainingFaces/TestDataset/OutputNeu/image_" + a + ".bmp");
                    a++;
                    fullFaceRegions = new List<DetectedObject>();
                    partialFaceRegions = new List<DetectedObject>();
                }
            }
        }
    }
}