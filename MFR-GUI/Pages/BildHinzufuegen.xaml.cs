using Emgu.CV;
using Emgu.CV.Structure;
using System.Threading;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static MFR_GUI.Pages.Globals;
using Color = System.Drawing.Color;
using Emgu.CV.UI;
using System.IO;
using System.Collections.Generic;
using Emgu.CV.Models;
using Timer = System.Threading.Timer;
using Emgu.CV.Util;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using System.Windows.Forms.Integration;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.Windows.Input;
using System.Windows.Forms;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using static MFR_GUI.Pages.TrainingFacesLoader;

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

        public BildHinzufuegen()
        {
            InitializeComponent();

            _logger = new Logger();

            Tuple<List<string>, int> tuple = LoadTrainingFacesTwoReturns(_logger);

            _labels = tuple.Item1;
            _savedNamesCount = tuple.Item2;

            txt_Name.Focus();

            generateImageBox();
        }

        public void FrameGrabber(object state)
        {
            List<DetectedObject> fullFaceRegions = new List<DetectedObject>();
            List<DetectedObject> partialFaceRegions = new List<DetectedObject>();
            Image<Bgr, Byte> currentFrame;

            //Get the current frame from capture device
            currentFrame = grabber.QueryFrame().ToImage<Bgr, Byte>().Resize(320, 240, Emgu.CV.CvEnum.Inter.Cubic);
            //currentFrame = new Image<Bgr, byte>("C:\\Users\\HP\\Dokumente\\Visual Studio 2022\\Projects\\MFR-GUI\\MFR-GUI\\TrainingFaces\\test\\wholeFrame.bmp");


            if (Monitor.TryEnter(syncObj))
            {
                try
                {
                    //Detect rectangular regions which contain a face
                    faceDetector.Detect(currentFrame, fullFaceRegions, partialFaceRegions);
                    
                    _logger.LogInfo("fullFaceRegions: " + fullFaceRegions.Count);

                    foreach (DetectedObject d in fullFaceRegions)
                    {
                        currentFrame.Draw(d.Region, new Bgr(Color.Red), 1);
                        _logger.LogInfo("Rectangle: " + d.Region);
                        _logger.LogInfo("confidence: " + d.Confident);
                        _logger.LogInfo("confidence: " + d.Label);
                        _logger.LogInfo("confidence: " + d.ClassId);
                    }

                    _imgBoxKamera.Image = currentFrame;
                }
                finally
                {
                    // Ensure that the lock is released.
                    Monitor.Exit(syncObj);
                }
            }
            else
            {
                
            }
        }

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                btn_speichern_Click(sender, e);
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
                List<Mat> training = new List<Mat>();
                List<int> labelNr = new List<int>();
                string name = get_txt_Name();

                try
                {
                    //Get the current frame from capture device
                    currentFrame = grabber.QueryFrame().ToImage<Bgr, Byte>().Resize(320, 240, Emgu.CV.CvEnum.Inter.Cubic);

                    //Enter critical region
                    lock (syncObj)
                    {
                        //Detect rectangular regions which contain a face
                        faceDetector.Detect(currentFrame, fullFaceRegions, partialFaceRegions);
                    }

                    if (fullFaceRegions.Count != 0)
                    {
                        List<Rectangle> recs = new List<Rectangle>();
                        foreach (DetectedObject o in fullFaceRegions)
                        {
                            recs.Add(o.Region);
                        }

                        VectorOfVectorOfPointF vovop = fd.Detect(currentFrame, recs.ToArray());
                        
                        if(!ImageEditor.IsAngelOver15Degree(fullFaceRegions[0].Region))
                        {
                            _logger.LogInfo("Winkel is unter 15°");

                            Image<Bgr, Byte> tempTrainingFace = ImageEditor.RotateAndAlignPicture(currentFrame, vovop[0], fullFaceRegions[0], _logger);

                            fullFaceRegions = new List<DetectedObject>();
                            partialFaceRegions = new List<DetectedObject>();

                            //Enter critical region
                            lock (syncObj)
                            {
                                //Detect rectangular regions which contain a face
                                faceDetector.Detect(tempTrainingFace, fullFaceRegions, partialFaceRegions, confidenceThreshold: (float)0.99);
                            }

                            _logger.LogInfo("DedectedFaces in tempTrainingFace: " + fullFaceRegions.Count);

                            string trainingFacesDirectory = projectDirectory + "/TrainingFaces/";

                            if (fullFaceRegions.Count > 1)
                            {
                                Rectangle r = fullFaceRegions[1].Region;
                                if(r.X < tempTrainingFace.Width / 3 && r.Y < tempTrainingFace.Height / 3)
                                {
                                    tempTrainingFace = tempTrainingFace.Copy(fullFaceRegions[1].Region);
                                }
                                else
                                {
                                    tempTrainingFace = tempTrainingFace.Copy(fullFaceRegions[0].Region);
                                }
                            }
                            else
                            {
                                tempTrainingFace = tempTrainingFace.Copy(fullFaceRegions[0].Region);
                            }

                            Image<Gray, Byte> trainingFace = tempTrainingFace.Convert<Gray, Byte>();

                            //Resize the image of the detected face and add the image and label to the lists for training
                            trainingFace = trainingFace.Resize(240, 240, Emgu.CV.CvEnum.Inter.Cubic);

                            if (!_labels.Contains(name))
                            {
                                labelNr.Add(_savedNamesCount++);
                                File.AppendAllText(trainingFacesDirectory + "TrainedLabels.txt", "%" + name);
                            }
                            else
                            {
                                labelNr.Add(_labels.IndexOf(name));
                            }
                            
                            training.Add(trainingFace.Mat);

                            //Update the recognizer with the new Image and Label
                            recognizer.Update(training.ToArray(), labelNr.ToArray());

                            //save the image as bitmap-file
                            if (!Directory.Exists(trainingFacesDirectory + name + "/"))
                            {
                                Directory.CreateDirectory(trainingFacesDirectory + name + "/");
                            }

                            int i;
                            for (i = 0; File.Exists(trainingFacesDirectory + name + "/" + name + i + ".bmp"); i++);

                            trainingFace.Save(trainingFacesDirectory + name + "/" + name + i + ".bmp");

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
                    setTrainingStatus("Nicht gespeichert!", Brushes.Red);
                    _logger.LogError(ex.Message, "BildHinzufuegen.xaml.cs","btnSpeichern");
                }
            });
        }

        private void btn_Zurueck1_Click(object sender, RoutedEventArgs e)
        {
            if (_timer != null)
            {
                _timer.Dispose();
            }
            if(_imgBoxKamera != null)
            {
                _imgBoxKamera.Dispose();
            }
            if (_labels != null)
            {
                _labels.Clear();
            }
            if (_timer != null)
            {
                _timer.Dispose();
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
            
            _timer = new Timer(FrameGrabber, null, 200, 20);
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
                //We are on a different thread, that's why we need to call Invoke to execute the method on the thread onwing the control
                return (string)this.Dispatcher.Invoke(new GetTextFromTextBoxDelegate(this.get_txt_Name));
            }
        }

        private void allFaceFeatures(Image<Bgr, byte> image, VectorOfPointF vop, DetectedObject detectedObject)
        {
            for (int j = 0; j < vop.Size; j++)
            {
                Point p = Point.Round(vop[j]);
                _logger.LogInfo((j + 1).ToString() + "X: " + vop[j].X + " Y:" + vop[j].Y);

                //image[p.Y, p.X] = new Bgr(0, 0, 255);
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
                //We are on a different thread, that's why we need to call Invoke to execute the method on the thread onwing the control
                this.Dispatcher.Invoke(new SetTrainingStatusDelegate(setTrainingStatus), status, color);
            }
        }
    }
}