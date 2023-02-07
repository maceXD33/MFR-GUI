using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Face;
using Emgu.CV.Structure;
using System.Threading;
using System;
using System.Windows;
using System.Windows.Controls;
using static MFR_GUI.Pages.Globals;
using Point = System.Drawing.Point;
using Color = System.Drawing.Color;
using Emgu.CV.UI;
using Emgu.CV.Models;
using System.Collections.Generic;
using Brushes = System.Windows.Media.Brushes;
using Timer = System.Threading.Timer;
using Emgu.CV.Util;
using System.Drawing;
using System.Windows.Forms.Integration;
using System.Windows.Forms;
using static MFR_GUI.Pages.TrainingFacesLoader;
using System.Diagnostics;

namespace MFR_GUI.Pages
{
    /// <summary>
    /// Interaktionslogik für PersonenErfassen.xaml
    /// </summary>
    public partial class PersonenErfassen : Page
    {
        private ImageBox _imgBoxKamera;
        private Timer _timer;
        private Logger _logger;

        private List<string> _labels;
        private int _savedNamesCount;

        public PersonenErfassen()
        {
            InitializeComponent();
            Label1.Content = "Status";
            Label2.Content = "Name";
            _logger = new Logger();

            Tuple<List<string>, int> tuple = LoadTrainingFacesTwoReturns(_logger);

            _labels = tuple.Item1;
            _savedNamesCount = tuple.Item2;

            generateImageBox();
        }

        private void btn_Zurueck3_Click(object sender, RoutedEventArgs e)
        {
            if (_timer != null)
            {
                _timer.Dispose();
            }
            if (_imgBoxKamera != null)
            {
                _imgBoxKamera.Dispose();
            }
            
            this.NavigationService.Navigate(new Menu());
        }

        private void FrameGrabber(object o)
        {
            Stopwatch t = new Stopwatch();
            //t.Start();
            List<DetectedObject> fullFaceRegions = new List<DetectedObject>();
            List<DetectedObject> partialFaceRegions = new List<DetectedObject>();
            Image<Bgr, Byte>? currentFrame;
            Image<Bgr, byte>? result;
            string status = "nicht erkannt";
            string recognizedNames = "";
            
            //Get the current frame from capture device
            currentFrame = grabber.QueryFrame().ToImage<Bgr, Byte>().Resize(320, 240, Emgu.CV.CvEnum.Inter.Cubic);
            //currentFrame = new Image<Bgr, byte>(@"C:\Users\HP\Dokumente\Visual Studio 2022\Projects\MFR-GUI\MFR-GUI\TrainingFaces\TestDataset\Recognize\image_80.jpg");

            if (Monitor.TryEnter(syncObj))
            {
                //Detect rectangular regions which contain a face
                faceDetector.Detect(currentFrame, fullFaceRegions, partialFaceRegions, confidenceThreshold: (float)0.9);

                Monitor.Exit(syncObj);

                //_logger.LogInfo("Anzahl kompletter Gesichter: " + fullFaceRegions.Count);

                List<Rectangle> recs = new List<Rectangle>();
                result = currentFrame.Copy();

                //Action for each region detected
                foreach (DetectedObject d in fullFaceRegions)
                {
                    Rectangle r = d.Region;

                    if(r.Right < 320 && r.Bottom < 240)
                    {
                        recs.Add(r);
                    }

                    //Draw a rectangle around the region
                    currentFrame.Draw(r, new Bgr(Color.Red), 1);
                }

                if (fullFaceRegions != null && fullFaceRegions.Count > 0 && result != null)
                {
                    if(Monitor.TryEnter(syncObj))
                    {
                        VectorOfVectorOfPointF vovop = fd.Detect(currentFrame, recs.ToArray());

                        Monitor.Exit(syncObj);

                        //t.Stop();
                        PrepareFaces(vovop, fullFaceRegions, partialFaceRegions, currentFrame, result, recs, ref status, ref recognizedNames);
                        //t.Start();
                    }
                }

                SetGUIElements(currentFrame, status, recognizedNames);

                //t.Stop();
                //_logger.LogInfo(DateTime.Now.ToString("hh:mm:ss.fff"));
                //_logger.LogInfo("FrameGrabber: " + t.ElapsedMilliseconds.ToString());
            }
        }

        private void PrepareFaces(VectorOfVectorOfPointF vovop, List<DetectedObject> fullFaceRegions, List<DetectedObject> partialFaceRegions, Image<Bgr, Byte>? currentFrame, Image<Bgr, byte>? result, List<Rectangle> recs, ref string status, ref string recognizedNames)
        {
            Stopwatch sw = new Stopwatch();
            Stopwatch sw1 = new Stopwatch();

            for (int i = 0; i < vovop.Size; i++)
            {
                //Check if there are any trained faces
                if (_savedNamesCount != 0)
                {
                    sw.Start();
                    if (!ImageEditor.IsAngelOver15Degree(fullFaceRegions[i].Region))
                    {
                        //_logger.LogInfo(fullFaceRegions[i].Region.ToString());
                        //_logger.LogInfo(fullFaceRegions[i].Confident.ToString());

                        result = ImageEditor.RotateAndAlignPicture(result, vovop[i], fullFaceRegions[i], _logger);

                        fullFaceRegions = new List<DetectedObject>();
                        partialFaceRegions = new List<DetectedObject>();

                        if (result != null)
                        {
                            //Enter critical region
                            lock (syncObj)
                            {
                                //Detect rectangular regions which contain a face
                                faceDetector.Detect(result, fullFaceRegions, partialFaceRegions, confidenceThreshold: (float)0.99);
                            }

                            if (fullFaceRegions.Count > 0)
                            {
                                result = CropImage(fullFaceRegions, result);

                                RecognizeFaces(currentFrame, result, recs, i, ref status, ref recognizedNames);   
                            }
                        }

                        sw.Stop();
                        _logger.LogInfo("If-Abfrage: " + sw.ElapsedMilliseconds.ToString());
                    }
                    else
                    {
                        currentFrame.Draw("Zu schief!", new Point(recs[i].X - 5, recs[i].Y - 5), FontFace.HersheyTriplex, 1.0d, new Bgr(Color.LightGreen), thickness: 1);
                    }
                }
                else
                {
                    //Draw the label "Unkown" as there are no faces in the database
                    currentFrame.Draw("Unbekannt", new Point(recs[i].X - 5, recs[i].Y - 5), FontFace.HersheyTriplex, 1.0d, new Bgr(Color.LightGreen), thickness: 1);
                }
            }
        }

        private Image<Bgr, byte> CropImage(List<DetectedObject> fullFaceRegions, Image<Bgr, byte> result)
        {
            if (fullFaceRegions.Count > 1)
            {
                Rectangle r = fullFaceRegions[1].Region;

                if (r.X < result.Width / 3 && r.Y < result.Height / 3)
                {
                    return result.Copy(fullFaceRegions[1].Region);
                }
                else
                {
                    return result.Copy(fullFaceRegions[0].Region);
                }
            }
            else
            {
                return result.Copy(fullFaceRegions[0].Region);
            }
        }

        private void RecognizeFaces(Image<Bgr, Byte>? currentFrame, Image<Bgr, byte>? result, List<Rectangle> recs, int i, ref string status, ref string recognizedNames)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            if (result != null)
            {
                result = result.Resize(100, 100, Emgu.CV.CvEnum.Inter.Cubic);

                if (Monitor.TryEnter(syncObj))
                {
                    //Get the result of the prediction from the recognizer
                    FaceRecognizer.PredictionResult res = recognizer.Predict(result.Convert<Gray, Byte>());

                    Monitor.Exit(syncObj);

                    //res.Distance < n determs how familiar the faces must look
                    if (res.Distance <= 80)
                    {
                        //Draw the label for the detected face
                        currentFrame.Draw(_labels[res.Label] + ", " + res.Distance, new Point(recs[i].X - 5, recs[i].Y - 5), FontFace.HersheyTriplex, 1.0d, new Bgr(Color.LightGreen), thickness: 1);

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
                        currentFrame.Draw("Unbekannt, " + res.Distance, new Point(recs[i].X - 5, recs[i].Y - 5), FontFace.HersheyTriplex, 1.0d, new Bgr(Color.LightGreen), thickness: 1);
                    }
                }
            }
            else
            {

            }

            sw.Stop();
            _logger.LogInfo("RecognizeFaces: " + sw.ElapsedMilliseconds.ToString());
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
            //Testing();
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
        /// Sets the Background of Label1 to Green when status is "erkannt". Sets the Background of Label1 to Red when status is "unbekannt".
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
                _imgBoxKamera.Image = image;
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

        private void Testing()
        {
            Tuple<List<Mat>, List<Image<Bgr, byte>>, List<string>, List<int>> t = TrainingFacesLoader.GetTestingData(_logger);

            _labels = t.Item3;
            _savedNamesCount = t.Item4.Count;

            recognizer.Train(t.Item1.ToArray(), t.Item4.ToArray());

            List<DetectedObject> fullFaceRegions = new List<DetectedObject>();
            List<DetectedObject> partialFaceRegions = new List<DetectedObject>();
            Image<Bgr, Byte>? result;
            int a = 0;

            foreach(Image<Bgr, Byte> image in t.Item2)
            {
                if (Monitor.TryEnter(syncObj))
                {
                    //Detect rectangular regions which contain a face
                    faceDetector.Detect(image, fullFaceRegions, partialFaceRegions);

                    Monitor.Exit(syncObj);

                    //_logger.LogInfo("Anzahl kompletter Gesichter: " + fullFaceRegions.Count);

                    List<Rectangle> recs = new List<Rectangle>();
                    result = image.Copy();

                    //Action for each region detected
                    foreach (DetectedObject d in fullFaceRegions)
                    {
                        recs.Add(d.Region);

                        //Draw a rectangle around the region
                        image.Draw(d.Region, new Bgr(Color.Red), 1);
                    }

                    if (Monitor.TryEnter(syncObj))
                    {
                        VectorOfVectorOfPointF vovop = fd.Detect(result, recs.ToArray());

                        Monitor.Exit(syncObj);

                        for (int i = 0; i < vovop.Size; i++)
                        {
                            //Check if there are any trained faces
                            if (_savedNamesCount != 0)
                            {
                                if (!ImageEditor.IsAngelOver15Degree(fullFaceRegions[i].Region))
                                {
                                    //_logger.LogInfo(fullFaceRegions[i].Region.ToString());
                                    //_logger.LogInfo(fullFaceRegions[i].Confident.ToString());

                                    result = ImageEditor.RotateAndAlignPicture(result, vovop[i], fullFaceRegions[i], _logger);

                                    fullFaceRegions = new List<DetectedObject>();
                                    partialFaceRegions = new List<DetectedObject>();

                                    //Enter critical region
                                    lock (syncObj)
                                    {
                                        //Detect rectangular regions which contain a face
                                        faceDetector.Detect(result, fullFaceRegions, partialFaceRegions, confidenceThreshold: (float)0.99);
                                    }

                                    string trainingFacesDirectory = projectDirectory + "/TrainingFaces/";

                                    if (fullFaceRegions.Count > 1)
                                    {
                                        Rectangle r = fullFaceRegions[1].Region;
                                        if (r.X < result.Width / 3 && r.Y < result.Height / 3)
                                        {
                                            result = result.Copy(fullFaceRegions[1].Region);
                                        }
                                        else
                                        {
                                            result = result.Copy(fullFaceRegions[0].Region);
                                        }
                                    }
                                    else
                                    {
                                        result = result.Copy(fullFaceRegions[0].Region);
                                    }

                                    if (result != null)
                                    {
                                        result = result.Resize(100, 100, Emgu.CV.CvEnum.Inter.Cubic);

                                        if (Monitor.TryEnter(syncObj))
                                        {
                                            //Get the result of the prediction from the recognizer
                                            FaceRecognizer.PredictionResult res = recognizer.Predict(result.Convert<Gray, Byte>());

                                            Monitor.Exit(syncObj);

                                            //res.Distance < n determs how familiar the faces must look
                                            if (res.Distance <= 160)
                                            {
                                                //Draw the label for the detected face
                                                image.Draw(_labels[res.Label] + ", " + res.Distance, new Point(recs[i].X - 5, recs[i].Y - 5), FontFace.HersheyTriplex, 1.0d, new Bgr(Color.LightGreen), thickness: 1);
                                            }
                                            else
                                            {
                                                //Draw the label "Unkown" as the criteria for same face was not met
                                                image.Draw("Unbekannt, " + res.Distance, new Point(recs[i].X - 5, recs[i].Y - 5), FontFace.HersheyTriplex, 1.0d, new Bgr(Color.LightGreen), thickness: 1);
                                            }

                                            result.Save(projectDirectory + "/TrainingFaces/TestDataset/CroppedRecognizeImages/" + _labels[res.Label] + i + a + ".bmp");
                                        }
                                    }
                                    else
                                    {

                                    }
                                }
                                else
                                {
                                    image.Draw("Zu schief!", new Point(recs[i].X - 5, recs[i].Y - 5), FontFace.HersheyTriplex, 1.0d, new Bgr(Color.LightGreen), thickness: 1);
                                }
                            }
                            else
                            {
                                //Draw the label "Unkown" as there are no faces in the database
                                image.Draw("Unbekannt", new Point(recs[i].X - 5, recs[i].Y - 5), FontFace.HersheyTriplex, 1.0d, new Bgr(Color.LightGreen), thickness: 1);
                            }
                        }
                    }

                    image.Save(projectDirectory + "/TrainingFaces/TestDataset/Output/image_" + a + ".bmp");
                    a++;
                    fullFaceRegions = new List<DetectedObject>();
                    partialFaceRegions = new List<DetectedObject>();
                }
            }
        }
    }
}