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
using Rectangle = System.Drawing.Rectangle;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Timer = System.Timers.Timer;
using static MFR_GUI.Pages.Globals;
using static MFR_GUI.Pages.TrainingFacesLoader;
using System.Timers;

namespace MFR_GUI.Pages
{
    /// <summary>
    /// Interaktionslogik für BildHinzufuegen.xaml
    /// </summary>
    public partial class BildHinzufuegen : Page
    {
        // Declaration of fields
        private ImageBox _imgBoxKamera;
        private Timer _timer;
        private Logger _logger;

        private List<string> _labels;
        private int _savedNamesCount;

        public BildHinzufuegen()
        {
            InitializeComponent();

            // Create a new Logger-Object and assing it to the _logger-field
            _logger = new Logger();

            // Load the labels and the count of the saved names 
            Tuple<List<string>, int> tuple = LoadTrainingFacesTwoReturns(_logger);

            // Assign the returned values to the fields
            _labels = tuple.Item1;
            _savedNamesCount = tuple.Item2;

            // Set the cursor into the TextBox so that the user can start writing his name
            // without needing to click into it
            txt_Name.Focus();

            // Generate a ImageBox and start a Timer with FrameGrabber used for the Elapsed Event
            generateImageBox(FrameGrabber);
        }

        /// <summary>
        /// Grabs frames from the camera and looks for faces on the frame.
        /// When it finds faces, a red rectangle gets drawn over the frame.
        /// The frame gets set as the Image property of _imgBoxKamera
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void FrameGrabber(object sender, EventArgs e)
        {
            // Declaration/Definition of method variables
            List<DetectedObject> fullFaceRegions = new List<DetectedObject>();
            List<DetectedObject> partialFaceRegions = new List<DetectedObject>();
            Image<Bgr, Byte> currentFrame;

            // Get the next frame from the VideoCapture and resize it to 320x240
            currentFrame = videoCapture.QueryFrame().ToImage<Bgr, Byte>().Resize(320, 240, Emgu.CV.CvEnum.Inter.LinearExact);

            // Try to acquire a lock on the synchronizing object to use the FaceDetector, 
            // because the .Detect() method can't handle multiple access and returns around
            // 50 or more wrong rectangles
            if (Monitor.TryEnter(syncObj2))
            {
                // Detect rectangular regions which contain a face
                faceDetector2.Detect(currentFrame, fullFaceRegions, partialFaceRegions, confidenceThreshold: (float)0.9);

                // Release the lock
                Monitor.Exit(syncObj2);

                // Run through the fullFaceRegions list, which contains faces
                foreach (DetectedObject d in fullFaceRegions)
                {
                    // Draw a red rectangle on the image around the detected face
                    currentFrame.Draw(d.Region, new Bgr(Color.Red), 1);
                }

                // Acquire a lock on the synchronizing object
                lock (syncObj2)
                {
                    // Set the Image of the ImageBox (Threadsafe method)
                    setImageOfImageBox(currentFrame);
                }
            }
            else
            {
                // The lock couldn't be acquired so we do nothing
                _logger.LogInfo("Lock couldn't be acquired!");
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
                // Declaration/Definition of method variables
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
                    lock (syncObj1)
                    {
                        //Detect rectangular regions which contain a face
                        faceDetector1.Detect(currentFrame, fullFaceRegions, partialFaceRegions, confidenceThreshold: (float)0.9);
                    }

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
                        if (!ImageEditor.IsAngelOver30Degree(fullFaceRegions[0].Region))
                        {
                            // Detect the facial landmarks inside the rectangles
                            VectorOfVectorOfPointF vovop = facemarkDetector.Detect(currentFrame, recs.ToArray());

                            // Rotate the image depending on the postition of the eyes and return a cropped image containing the face and background
                            Image<Bgr, Byte> tempTrainingFace = ImageEditor.RotateAndAlignPicture(currentFrame, vovop[0], fullFaceRegions[0], _logger);

                            // Empty the Lists to use them again for the FaceDetector
                            fullFaceRegions = new List<DetectedObject>();
                            partialFaceRegions = new List<DetectedObject>();

                            // Acquire a lock on the synchronizing object
                            lock (syncObj1)
                            {
                                // Detect faces in the cropped image again to get only the face alone
                                faceDetector1.Detect(tempTrainingFace, fullFaceRegions, partialFaceRegions, confidenceThreshold: (float)0.99);
                            }

                            _logger.LogInfo("DedectedFaces in tempTrainingFace: " + fullFaceRegions.Count);

                            // Declare the directory for saving the images of the training faces
                            string trainingFacesDirectory = projectDirectory + "/TrainingFaces/";

                            // Crop the image of the face out of image also containing background
                            tempTrainingFace = ImageEditor.CropImage(fullFaceRegions, tempTrainingFace);

                            if (tempTrainingFace != null)
                            {
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
                                for (i = 0; File.Exists(trainingFacesDirectory + name + "/" + name + i + ".bmp"); i++) ;

                                // Save the training image with the count
                                tempTrainingFace.Save(trainingFacesDirectory + name + "/" + name + i + ".bmp");

                                //Show a MessageBox for confirmation of successful training
                                setTrainingStatus("Gesicht gespeichert", Brushes.Green);
                            }
                            else
                            {
                                setTrainingStatus("Falsch!", Brushes.Red);
                            }
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


        /// <summary>
        /// Gets called when the button for going back to the menu is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Zurueck1_Click(object sender, RoutedEventArgs e)
        {
            if (_timer != null)
            {
                // _timer is not null, so it gets stopped and disposed afterwards
                _timer.Stop();
                _timer.Dispose();
            }

            // Save the LBHP-recognizer to a text-file
            recognizer.Write(projectDirectory + "/Data/recognizer.txt");

            if (_labels != null)
            {
                // _labels is not null, so the list gets cleared
                _labels.Clear();
            }

            if(_imgBoxKamera!= null)
            {
                // _imgBoxKamera is not null, so it gets disposed
                _imgBoxKamera.Dispose();
            }

            // Create a new Menu-object and navigate to it
            this.NavigationService.Navigate(new Menu());
        }

        /// <summary>
        /// Generates a WindowsForms ImageBox and displays it inside a WPF-Application.
        /// Initializes and starts a new Timer with elapsedEventHandler subscribed for the Elapsed Event.
        /// </summary>
        /// <param name="elapsedEventHandler">The ElapsedEventHandler that gets subscribed to the Elapsed Event of the Timer</param>
        private void generateImageBox(ElapsedEventHandler elapsedEventHandler)
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
            // Subscribe the hideScrollbars-method to the SizeChanged event of the Page,
            // because when the user changes the size of the Window the ImageBox would display Scrollbars
            this.SizeChanged += hideScrollbars;

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
            // Subscribe the elapsedEventHandler to the Elasped Event of the Timer
            _timer.Elapsed += elapsedEventHandler;
            // Set the Interval of the Timer to 20ms
            _timer.Interval = 20;
            // Start the Timer
            _timer.Start();
        }

        /// <summary>
        /// Hides the vertical and horizontal scrollbar of _imgBoxKamera
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void hideScrollbars(object sender, RoutedEventArgs e)
        {
            _imgBoxKamera.HorizontalScrollBar.Hide();
            _imgBoxKamera.VerticalScrollBar.Hide();
        }

        /// <summary>
        /// Gets the Text Property of the TextView txt_Name.
        /// This method can be called in a thread outside of the GUI-Thread.
        /// </summary>
        private string get_txt_Name()
        {
            if (this.txt_Name.Dispatcher.CheckAccess())
            {
                // We are on the thread that owns the control so we return the Text inside of the TextBox
                return txt_Name.Text;
            }
            else
            {
                // We are on a different thread, that's why we need to call Invoke to execute the method on the thread owning the control
                return (string)this.Dispatcher.Invoke(this.get_txt_Name);
            }
        }

        /// <summary>
        /// Sets the Content property of l_Fehler to status and the Foreground property to color.
        /// This method can be called in a thread outside of the GUI-Thread.
        /// </summary>
        /// <param name="status">The string that will be set as Content for the Label l_Fehler</param>
        /// <param name="color">The Brush that will be set as Foreground for the Label l_Fehler</param>
        private void setTrainingStatus(string status, Brush color)
        {
            if (this.l_Fehler.Dispatcher.CheckAccess())
            {
                // We are on the thread that owns the control
                // Set the Text of the Label to status
                l_Fehler.Content = status;
                // Set the color of the Text to color
                l_Fehler.Foreground = color;
            }
            else
            {
                // We are on a different thread, that's why we need to call Invoke to execute the method on the thread owning the control
                this.Dispatcher.Invoke(setTrainingStatus, status, color);
            }
        }

        /// <summary>
        /// Sets the Image property of _imgBoxKamera to image.
        /// This method can be called in a thread outside of the GUI-Thread.
        /// </summary>
        /// <param name="image">The Image that will be as Image for _imgBoxKamera</param>
        private void setImageOfImageBox(Image<Bgr, byte> image)
        {
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
        }
    }
}