using Emgu.CV;
using Emgu.CV.Structure;
using System.Threading;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Emgu.CV.UI;
using System.IO;
using System.Collections.Generic;
using Emgu.CV.Models;
using Emgu.CV.Util;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Forms;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Timer = System.Timers.Timer;
using static MFR_GUI.Pages.Globals;
using static MFR_GUI.Pages.TrainingFacesLoader;
using MFR_GUI.Pages;

namespace MFR_GUI.Pages
{
    /// <summary>
    /// Interaktionslogik für BildHinzufuegen.xaml
    /// </summary>
    public partial class BildHinzufuegen : Page
    {
        private ImageBox _imgBoxKamera;
        private Timer _timer;
        private Logger _logger;

        private List<string> _labels;
        private int _savedNamesCount;

        private Timer _timer1;
        private Timer _timer2;

        public BildHinzufuegen()
        {
            InitializeComponent();

            _logger = new Logger();

            // Load the labels and the count of the saved names 
            Tuple<List<string>, int> tuple = LoadTrainingFacesTwoReturns(_logger);

            // Assign the returned values to the members
            _labels = tuple.Item1;
            _savedNamesCount = tuple.Item2;

            // Set the cursor into the TextBox so that the user can start writing his name
            // without needing to click into it
            txt_Name.Focus();

            // Generate a ImageBox and start a Timer 
            generateImageBox();
        }

        public void FrameGrabber(object sender, EventArgs e)
        {
            List<DetectedObject> fullFaceRegions = new List<DetectedObject>();
            List<DetectedObject> partialFaceRegions = new List<DetectedObject>();
            Image<Bgr, Byte> currentFrame;

            // Get the next frame from the VideoCapture and resize it to 320x240
            currentFrame = videoCapture.QueryFrame().ToImage<Bgr, Byte>().Resize(320, 240, Emgu.CV.CvEnum.Inter.Cubic);

            // Try to acquire a lock on the synchronizing object to use the FaceDetector, 
            // because the .Detect() method can't handle multiple access and returns around
            // 50 or more wrong rectangles
            if (Monitor.TryEnter(syncObj))
            {
                //_logger.LogInfo("syncObj Lock aquired!");

                // Detect rectangular regions which contain a face
                faceDetector.Detect(currentFrame, fullFaceRegions, partialFaceRegions, confidenceThreshold: (float)0.9);

                _logger.LogInfo(fullFaceRegions.Count.ToString());

                // Release the lock
                Monitor.Exit(syncObj);

                _logger.LogInfo(fullFaceRegions.Count.ToString());

                // Run through the fullFaceRegions list, which contains faces
                foreach (DetectedObject d in fullFaceRegions)
                {
                    // Draw a red rectangle on the image around the detected face
                    currentFrame.Draw(d.Region, new Bgr(Color.Red), 1);
                }

                //_logger.LogInfo("FrameGrabber: Set Image");

                // Acquire a lock on the synchronizing object
                lock (syncObjImage)
                {
                    // Set the Image of the ImageBox (Threadsafe method)
                    setImageOfImageBox(currentFrame);
                }
            }
            else
            {
                // The lock couldn't be acquired so we do nothing
                _logger.LogInfo("syncObj Lock not aquired!");
            }
        }

        public void FrameGrabber1(object sender, EventArgs e)
        {
            List<DetectedObject> fullFaceRegions = new List<DetectedObject>();
            List<DetectedObject> partialFaceRegions = new List<DetectedObject>();
            Image<Bgr, Byte> currentFrame;

            //Get the current frame from capture device
            currentFrame = videoCapture.QueryFrame().ToImage<Bgr, Byte>().Resize(320, 240, Emgu.CV.CvEnum.Inter.Cubic);
            //currentFrame = new Image<Bgr, byte>("C:\\Users\\HP\\Dokumente\\Visual Studio 2022\\Projects\\MFR-GUI\\MFR-GUI\\TrainingFaces\\test\\wholeFrame.bmp

            if (Monitor.TryEnter(syncObj1))
            {
                _logger.LogInfo("syncObj1 Lock aquired!");

                //Detect rectangular regions which contain a face
                faceDetector1.Detect(currentFrame, fullFaceRegions, partialFaceRegions, confidenceThreshold: (float)0.9);

                Monitor.Exit(syncObj1);

                _logger.LogInfo(fullFaceRegions.Count.ToString());

                foreach (DetectedObject d in fullFaceRegions)
                {
                    currentFrame.Draw(d.Region, new Bgr(Color.Red), 1);
                }

                _logger.LogInfo("FrameGrabber1: Set Image");
                lock (syncObjImage)
                {
                    setImageOfImageBox(currentFrame);
                }
            }
            else
            {
                _logger.LogInfo("syncObj1 Lock not aquired!");
            }
        }

        public void FrameGrabber2(object sender, EventArgs e)
        {
            List<DetectedObject> fullFaceRegions = new List<DetectedObject>();
            List<DetectedObject> partialFaceRegions = new List<DetectedObject>();
            Image<Bgr, Byte> currentFrame;

            //Get the current frame from capture device
            currentFrame = videoCapture.QueryFrame().ToImage<Bgr, Byte>().Resize(320, 240, Emgu.CV.CvEnum.Inter.Cubic);
            //currentFrame = new Image<Bgr, byte>("C:\\Users\\HP\\Dokumente\\Visual Studio 2022\\Projects\\MFR-GUI\\MFR-GUI\\TrainingFaces\\test\\wholeFrame.bmp

            if (Monitor.TryEnter(syncObj2))
            {
                _logger.LogInfo("syncObj2 Lock aquired!");

                //Detect rectangular regions which contain a face
                faceDetector2.Detect(currentFrame, fullFaceRegions, partialFaceRegions, confidenceThreshold: (float)0.9);

                Monitor.Exit(syncObj2);

                _logger.LogInfo(fullFaceRegions.Count.ToString());

                foreach (DetectedObject d in fullFaceRegions)
                {
                    currentFrame.Draw(d.Region, new Bgr(Color.Red), 1);
                }

                _logger.LogInfo("FrameGrabber2: Set Image");
                lock (syncObjImage)
                {
                    setImageOfImageBox(currentFrame);
                }
            }
            else
            {
                _logger.LogInfo("syncObj2 Lock not aquired!");
            }
        }

        /// <summary>
        /// Gets called when the user presses any key
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            // Look if the pressed key is the return key
            if (e.Key == Key.Return)
            {
                // Call the method, which is normally called when the button for saving is pressed
                btn_speichern_Click(sender, e);
            }
        }

        /// <summary>
        /// Gets called when the button for saving is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_speichern_Click(object sender, RoutedEventArgs e)
        {
            //Create and start a Task so the UI-Thread isn't blocked
            Task t = Task.Factory.StartNew(() =>
            {
                List<DetectedObject> fullFaceRegions = new List<DetectedObject>();
                List<DetectedObject> partialFaceRegions = new List<DetectedObject>();
                Image<Bgr, Byte> currentFrame;
                List<Mat> training = new List<Mat>();
                List<int> labelNr = new List<int>();
                string name = get_txt_Name();

                try
                {
                    // Get the next frame from the VideoCapture and resize it to 320x240
                    currentFrame = videoCapture.QueryFrame().ToImage<Bgr, Byte>().Resize(320, 240, Emgu.CV.CvEnum.Inter.Cubic);

                    // Acquire a lock on the synchronizing object
                    lock (syncObj)
                    {
                        //Detect rectangular regions which contain a face
                        faceDetector.Detect(currentFrame, fullFaceRegions, partialFaceRegions, confidenceThreshold: (float)0.9);
                    }

                    _logger.LogInfo(fullFaceRegions.Count.ToString());

                    // Check if the FaceDetector found a face 
                    if (fullFaceRegions.Count != 0)
                    {
                        // Make a list of rectangles for the FacemarkDetector, which contains only
                        // the first rectanlge as we only save one face
                        List<Rectangle> recs = new List<Rectangle>
                        {
                            fullFaceRegions[0].Region
                        };

                        // Check if the face is rotated more than 15°, because the FacemarkDetector
                        // has problems correctly detecting Facemarks if the face is tilted
                        if(!ImageEditor.IsAngelOver15Degree(fullFaceRegions[0].Region))
                        {
                            // Detect the facial landmarks inside the rectangles
                            VectorOfVectorOfPointF vovop = facemarkDetector.Detect(currentFrame, recs.ToArray());

                            _logger.LogInfo("Winkel is unter 15°");

                            // Rotate the image depending on the postition of the eyes and return a cropped image containing the face and background
                            Image<Bgr, Byte> tempTrainingFace = ImageEditor.RotateAndAlignPicture(currentFrame, vovop[0], fullFaceRegions[0], _logger);

                            // Empty the Lists to use them again for the FaceDetector
                            fullFaceRegions = new List<DetectedObject>();
                            partialFaceRegions = new List<DetectedObject>();

                            // Acquire a lock on the synchronizing object
                            lock (syncObj)
                            {
                                // Detect faces in the cropped image again to get only the face alone
                                faceDetector.Detect(tempTrainingFace, fullFaceRegions, partialFaceRegions, confidenceThreshold: (float)0.99);
                            }

                            _logger.LogInfo("DedectedFaces in tempTrainingFace: " + fullFaceRegions.Count);

                            // Declare the directory for saving the images of the training faces
                            string trainingFacesDirectory = projectDirectory + "/TrainingFaces/";

                            // Crop the image of the face out of image also containing background
                            tempTrainingFace = ImageEditor.CropImage(fullFaceRegions, tempTrainingFace);

                            //Resize the image of the detected face
                            tempTrainingFace = tempTrainingFace.Resize(100, 100, Emgu.CV.CvEnum.Inter.Cubic);

                            // Check if the name of the face to save already exists
                            if (!_labels.Contains(name))
                            {
                                // Add the count of saved names to labelNr and increase the count of saved names
                                labelNr.Add(_savedNamesCount++);
                                // Add the name to labels
                                _labels.Add(name);
                                // Save the name to a text file (Is used in the TrainingFacesLoader)
                                File.AppendAllText(projectDirectory + "/Data/TrainedLabels.txt", "%" + name);
                            }
                            else
                            {
                                // Add the index of the name in labels to labelNr
                                labelNr.Add(_labels.IndexOf(name));
                            }
                            
                            // Add the training image to a list of Mats
                            training.Add(tempTrainingFace.Convert<Gray, Byte>().Mat);

                            //Update the recognizer with the new Image and Label
                            recognizer.Update(training.ToArray(), labelNr.ToArray());

                            // Check if the directory of the name to save the training image exists
                            if (!Directory.Exists(trainingFacesDirectory + name + "/"))
                            {
                                // Create the directory to save the training image exists
                                Directory.CreateDirectory(trainingFacesDirectory + name + "/");
                            }

                            // Run through the training images and get the count of training images for that name
                            int i;
                            for (i = 0; File.Exists(trainingFacesDirectory + name + "/" + name + i + ".bmp"); i++);

                            // Save the training image with the count
                            tempTrainingFace.Save(trainingFacesDirectory + name + "/" + name + i + ".bmp");

                            //Show a MessageBox for confirmation of successful training
                            setTrainingStatus("Gesicht gespeichert", Brushes.Green);
                        }
                        else
                        {
                            setTrainingStatus("Zu schräg!", Brushes.Red);
                        }
                    }
                    else
                    {
                        setTrainingStatus("Kein Gesicht!", Brushes.Red);
                    }
                }
                catch (Exception ex)
                {
                    // Show the user that the saving didn't work
                    setTrainingStatus("Nicht gespeichert!", Brushes.Red);
                    // Log an error message
                    _logger.LogError(ex.Message, "BildHinzufuegen.xaml.cs", "btnSpeichern");
                }
            });
        }

        private void btn_Zurueck1_Click(object sender, RoutedEventArgs e)
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
            }

            recognizer.Write(projectDirectory + "/Data/recognizer.txt");

            if (_labels != null)
            {
                _labels.Clear();
            }

            if(_imgBoxKamera!= null)
            {
                _imgBoxKamera.Dispose();
            }

            this.NavigationService.Navigate(new Menu());
        }

        private void generateImageBox()
        {
            //Create the interop host control.
            WindowsFormsHost host = new WindowsFormsHost();

            //Create the ImageBox control.
            _imgBoxKamera = new ImageBox();

            _imgBoxKamera.BorderStyle = BorderStyle.FixedSingle;
            _imgBoxKamera.SizeMode = PictureBoxSizeMode.StretchImage;
            _imgBoxKamera.Enabled = false;

            this.SizeChanged += hideScrollbars;

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

            /*
            _timer1 = new Timer();
            _timer1.Elapsed += FrameGrabber1;
            _timer1.Interval = 20;
            Thread.Sleep(34);
            _timer1.Start();

            _timer2 = new Timer();
            _timer2.Elapsed += FrameGrabber2;
            _timer2.Interval = 20;
            Thread.Sleep(34);
            _timer2.Start();
            */
        }

        /// <summary>
        /// Hides the vertical and horizontal scrollbar of imgBoxKamera
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void hideScrollbars(object sender, RoutedEventArgs e)
        {
            _imgBoxKamera.HorizontalScrollBar.Hide();
            _imgBoxKamera.VerticalScrollBar.Hide();
        }

        //Threadsafe method
        /// <summary>
        /// Gets the Text Property of the TextView txt_Name.
        /// This function can be called in a thread outside of the GUI-Thread.
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
                //We are on a different thread, that's why we need to call Invoke to execute the method on the thread owning the control
                return (string)this.Dispatcher.Invoke(this.get_txt_Name);
            }
        }

        private void allFaceFeatures(Image<Bgr, byte> image, VectorOfPointF vop, DetectedObject detectedObject)
        {
            for (int j = 0; j < vop.Size; j++)
            {
                Point p = Point.Round(vop[j]);
                _logger.LogInfo((j + 1).ToString() + "X: " + vop[j].X + " Y:" + vop[j].Y);
            }

            image.Save(projectDirectory + "/TrainingFaces/test/allfacefeatures.bmp");
        }

        private void setTrainingStatus(string status, Brush color)
        {
            if (this.l_Fehler.Dispatcher.CheckAccess())
            {
                //We are on the thread that owns the control
                l_Fehler.Content = status;
                l_Fehler.Foreground = color;
            }
            else
            {
                //We are on a different thread, that's why we need to call Invoke to execute the method on the thread owning the control
                this.Dispatcher.Invoke(setTrainingStatus, status, color);
            }
        }

        private void setImageOfImageBox(Image<Bgr, byte> image)
        {
            /*
            if (this.grid2.Dispatcher.CheckAccess())
            {
                //We are on the thread that owns the control
                _imgBoxKamera.Image = image;
            }
            else
            {
                //We are on a different thread, that's why we need to call Invoke to execute the method on the thread owning the control
                this.Dispatcher.Invoke(setImageOfImageBox, image);
            }
            */
            
            if (_imgBoxKamera.InvokeRequired)
            {
                //We are on the thread that owns the control
                _imgBoxKamera.Image = image;
            }
            else
            {
                //We are on a different thread, that's why we need to call Invoke to execute the method on the thread owning the control
                _imgBoxKamera.Invoke(setImageOfImageBox, image);

            }
        }
    }
}