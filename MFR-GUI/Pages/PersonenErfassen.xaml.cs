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
using static MFR_GUI.Pages.ImageEditor;
using System.Timers;
using System.Windows.Media;

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

            lock (syncObj1)
            {
                // Generate a ImageBox and start a Timer with FrameGrabber used for the Elapsed Event
                GenerateImageBox(FrameGrabber);
            }
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
        /// Afterwards the faces run through the FaceRecognizer and if they belong to a name
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
            Image<Bgr, byte> currentFrameCopy;
            string status = "nicht erkannt";
            string recognizedNames = "";

            // Get the next frame from the VideoCapture and resize it to 320x240
            currentFrame = videoCapture.QueryFrame().ToImage<Bgr, Byte>().Resize(320, 240, Emgu.CV.CvEnum.Inter.LinearExact);

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

                // Copy the currentFrame to currentFrameCopy to have the frame without anything drawn on it
                currentFrameCopy = currentFrame.Copy();

                // Run through the fullFaceRegions list, which contains faces
                foreach (DetectedObject d in fullFaceRegions)
                {
                    // Define a rectangle with the Region of the DetectedObject
                    Rectangle r = d.Region;

                    if (r.Right < currentFrame.Cols && r.Bottom < currentFrame.Rows)
                    {
                        recs.Add(r);

                        // Draw a red rectangle on the image around the detected face
                        currentFrame.Draw(r, new Bgr(Color.Red), 1);
                    }

                    // Check, if there are any faces saved in the FaceRecognizer
                    if (_savedNamesCount <= 0)
                    {
                        // There are no faces to recognize, so "Unbekannt" can be drawn above all red rectangles
                        currentFrame.Draw("Unbekannt", new Point(r.X - 5, r.Y - 5), FontFace.HersheyComplexSmall, 1.0d, new Bgr(Color.LightGreen), thickness: 1);
                    }
                }

                // Detect facial landmarks, run through the faces, crop and align them, try to recognize them and evaluate the result
                PrepareFaces(fullFaceRegions, partialFaceRegions, currentFrame, currentFrameCopy, recs, ref status, ref recognizedNames);

                // Acquire a lock on the synchronizing object
                lock (syncObj1)
                {
                    // Set the Image of the ImageBox, the Content of Label1 and the Content of Label2 (Threadsafe method)
                    SetGUIElements(currentFrame, status, recognizedNames);
                }
            }
        }

        /// <summary>
        /// Detects all facial landmarks inside the rectangles of recs on the currentFrame.
        /// Then runs through all of them and crops and aligns the faces. Afterwards they
        /// are run through the FaceRecognizer and the currentFrameCopy gets evaluated.
        /// </summary>
        /// <param name="fullFaceRegions"></param>
        /// <param name="partialFaceRegions"></param>
        /// <param name="currentFrame"></param>
        /// <param name="currentFrameCopy"></param>
        /// <param name="recs"></param>
        /// <param name="status"></param>
        /// <param name="recognizedNames"></param>
        private void PrepareFaces(List<DetectedObject> fullFaceRegions, List<DetectedObject> partialFaceRegions, Image<Bgr, Byte> currentFrame, Image<Bgr, byte> currentFrameCopy, List<Rectangle> recs, ref string status, ref string recognizedNames)
        {
            // Check, if the FaceDetecter detected any faces
            if (fullFaceRegions.Count > 0)
            {
                // Check, if there are any faces saved in the FaceRecognizer
                if (_savedNamesCount > 0)
                {
                    // Detect facial landmarks inside the rectangles the FaceDetector detected before
                    VectorOfVectorOfPointF vovop = facemarkDetector.Detect(currentFrameCopy, recs.ToArray());

                    // Run through all facial landmarks
                    for (int i = 0; i < vovop.Size; i++)
                    {
                        //if (i <= fullFaceRegions.Count - 1)
                        // Crop and align the face, then try to recognize it and evaluate the currentFrameCopy
                        ProcessFace(vovop[i], fullFaceRegions, partialFaceRegions, currentFrame, currentFrameCopy, recs, ref status, ref recognizedNames, i);
                    }
                }
            }
        }

        /// <summary>
        /// Cut out part of the image containing the face and uses the facial landmarks inside vop to rotate the frame 
        /// so that the eyes are aligned. Then crop the image so that only the face is on it and
        /// use it for face recognition. Evaluate the currentFrameCopy afterwards.
        /// </summary>
        /// <param name="vop"></param>
        /// <param name="fullFaceRegions"></param>
        /// <param name="partialFaceRegions"></param>
        /// <param name="currentFrame"></param>
        /// <param name="currentFrameCopy"></param>
        /// <param name="recs"></param>
        /// <param name="status"></param>
        /// <param name="recognizedNames"></param>
        /// <param name="i"></param>
        private void ProcessFace(VectorOfPointF vop, List<DetectedObject> fullFaceRegions, List<DetectedObject> partialFaceRegions, Image<Bgr, Byte> currentFrame, Image<Bgr, byte> currentFrameCopy, List<Rectangle> recs, ref string status, ref string recognizedNames, int i)
        {
            // Cut out part of the image and rotate it so that the eyes are aligned
            Image<Bgr, byte> alignedFace = RotateAndAlignPicture(currentFrameCopy, vop, fullFaceRegions[i], _logger);
            
            // Empty the two Lists to use them again
            fullFaceRegions = new List<DetectedObject>();
            partialFaceRegions = new List<DetectedObject>();

            // Check, if there was any error with aligning the face
            if (alignedFace != null)
            {
                // Acquire a lock on the synchronizing object
                lock (syncObj1)
                {
                    //Detect rectangular regions which contain a face
                    faceDetector1.Detect(alignedFace, fullFaceRegions, partialFaceRegions, confidenceThreshold: (float)0.99);
                }

                // Check, if the FaceDetecter detected any faces
                if (fullFaceRegions.Count > 0)
                {
                    // Crop the aligned Face so that it only contains the face
                    Image<Bgr, byte> alignedCroppedFace = CropImage(fullFaceRegions, alignedFace, _logger);
                    
                    // Try to recognize the face and evaluate the currentFrameCopy
                    RecognizeFace(currentFrame, alignedCroppedFace, recs, i, ref status, ref recognizedNames);
                }
                else
                {
                    currentFrame.Draw("Unbekannt", new Point(recs[i].X - 5, recs[i].Y - 5), FontFace.HersheyComplexSmall, 1.0d, new Bgr(Color.LightGreen), thickness: 1);
                }
            }
            else
            {
                _logger.LogInfo("alignedFace is null!");
            }
        }

        /// <summary>
        /// Try to recognize the alignedCroppedFace and evaluate the result afterwards.
        /// If there was an error with croping the aligned face "Unbekannt" gets drawn
        /// above the red rectanlge of this face
        /// </summary>
        /// <param name="currentFrame"></param>
        /// <param name="alignedCroppedFace"></param>
        /// <param name="recs"></param>
        /// <param name="i"></param>
        /// <param name="status"></param>
        /// <param name="recognizedNames"></param>
        private void RecognizeFace(Image<Bgr, Byte> currentFrame, Image<Bgr, byte> alignedCroppedFace, List<Rectangle> recs, int i, ref string status, ref string recognizedNames)
        {
            // Check, if there was an error cropping the aligend face
            if (alignedCroppedFace != null)
            {
                // Resize the aligned and cropped face for the face recognition
                alignedCroppedFace = alignedCroppedFace.Resize(100, 100, Emgu.CV.CvEnum.Inter.LinearExact);

                // Declare a method variable
                PredictionResult res;

                // Acquire a lock on the synchronizing object
                lock (syncObj1)
                {
                    //Get the currentFrameCopy of the prediction from the recognizer
                    res = recognizer.Predict(alignedCroppedFace.Convert<Gray, Byte>());
                }

                // Evaluate the result from the face recognition and change the variables determing
                // the output on the GUI depending on it.
                EvaluateResult(currentFrame, recs, res, ref recognizedNames, ref status, i);
            }
            else
            {
                // Draw "Unbekannt" above the red rectanlge as there was an error cropping the aligned face
                currentFrame.Draw("Unbekannt", new Point(recs[i].X - 5, recs[i].Y - 5), FontFace.HersheyComplexSmall, 1.0d, new Bgr(Color.LightGreen), thickness: 1);
                _logger.LogInfo("Falsch!");
            }
        }

        /// <summary>
        /// Checks, if the result from the FaceRecognizer is approved. When it is, draws the label above
        /// the red rectangle, adds the name to the recognized names and sets the status to "erkannt".
        /// Else "Unbekannt is drawn above the red rectangle.
        /// </summary>
        /// <param name="currentFrame"></param>
        /// <param name="recs"></param>
        /// <param name="res"></param>
        /// <param name="recognizedNames"></param>
        /// <param name="status"></param>
        /// <param name="i"></param>
        private void EvaluateResult(Image<Bgr, Byte> currentFrame, List<Rectangle> recs, PredictionResult res, ref string recognizedNames, ref string status, int i)
        {
            // res.Distance <= n determs how familiar the detected face and the face from the recognizer
            // must look to be considered the same
            if (res.Distance <= 65)
            {
                //Draw the label for the detected face
                currentFrame.Draw(_labels[res.Label], new Point(recs[i].X - 5, recs[i].Y - 5), FontFace.HersheyComplex, 1.0d, new Bgr(Color.LightGreen), thickness: 1);

                // Check, if there is alreade a name in the recognized faces
                if (recognizedNames != "")
                {
                    // Add a seperation between the two names
                    recognizedNames += ", ";
                }

                // Add the label to the recognized faces
                recognizedNames += _labels[res.Label];

                // Change the status to "erkannt"
                status = "erkannt";
            }
            else
            {
                //Draw the label "Unkown" as the criteria for same face was not met
                currentFrame.Draw("Unbekannt", new Point(recs[i].X - 5, recs[i].Y - 5), FontFace.HersheyComplexSmall, 1.0d, new Bgr(Color.LightGreen), thickness: 1);
            }
        }

        private void GenerateImageBox(ElapsedEventHandler elapsedEventHandler)
        {
            // Definition of a WindowsFormsHost method variable
            WindowsFormsHost host = new WindowsFormsHost();

            //Create the ImageBox
            _imgBoxKamera = new ImageBox();

            // Set the Border for _imageBoxKamera to a simple black line
            _imgBoxKamera.BorderStyle = BorderStyle.FixedSingle;
            // Set the Sizemode of _imageBoxKamera to Strectch Ímage so it uses the full space it gets provided with
            _imgBoxKamera.SizeMode = PictureBoxSizeMode.StretchImage;
            // Set the Enabled property of _imgBoxKamera to false so that the user can't zoom in and out on the ImageBox
            _imgBoxKamera.Enabled = false;
            // Subscribe the HideScrollbars-method to the SizeChanged event of the Page,
            // because when the user changes the size of the Window the ImageBox would display Scrollbars
            this.SizeChanged += HideScrollbars;

            // Set the space in which the WindowsFormsHost with the ImageBox is located within the WPF-Application
            Grid.SetColumn(host, 3);
            Grid.SetRow(host, 1);
            Grid.SetColumnSpan(host, 2);
            Grid.SetRowSpan(host, 6);

            // Set the Background of the WindowsFormsHost to white, because when host gets assigned children
            // it will display the Background of the WPF-Apllication
            host.Background = Brushes.White;

            // Assign the ImageBox as the host control's child.
            host.Child = _imgBoxKamera;
            //Add the interop host control to the Grid collection of child controls.
            this.grid2.Children.Add(host);

            // Create a new Timer and assign it to _timer
            _timer = new Timer();
            // Subscribe the ElapsedEventHandler to the Elasped Event of the Timer
            _timer.Elapsed += elapsedEventHandler;
            // Set the Interval of the Timer to 20ms
            _timer.Interval = 20;
            // Start the Timer
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
            // Check, if we are on the thread owning the control
            if (this.grid2.Dispatcher.CheckAccess())
            {
                //We are on the thread that owns the control
                //Set the Background of Label1 depending on, weither a face got recognized or not
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
                        PrepareFaces(fullFaceRegions, partialFaceRegions, image, result, recs, ref status, ref recognizedNames);
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