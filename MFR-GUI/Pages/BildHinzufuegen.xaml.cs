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
using Emgu.CV.Util;
using System.Security.Cryptography.Xml;
using Point = System.Drawing.Point;
using Emgu.CV.Reg;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Shapes;
using Rectangle = System.Drawing.Rectangle;
using System.Windows.Threading;
using System.Windows.Media;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;

namespace MFR_GUI.Pages
{
    /// <summary>
    /// Interaktionslogik für BildHinzufuegen.xaml
    /// </summary>
    public partial class BildHinzufuegen : Page
    {
        ImageBox imgBoxKamera;
        Timer timer;

        public BildHinzufuegen()
        {
            InitializeComponent();
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
                //Logger.LogInfo("BildHinzufuegen - FrameGrabber", "Lock on syncObj aquired");

                //Detect rectangular regions which contain a face
                faceDetector.Detect(currentFrame, fullFaceRegions, partialFaceRegions);

                Monitor.Exit(syncObj);

                List<Rectangle> recs = new List<Rectangle>();
                foreach (DetectedObject d in fullFaceRegions)
                {
                    recs.Add(d.Region);

                    currentFrame.Draw(d.Region, new Bgr(Color.Red), 1);
                }

                /*
                Bitmap bitmap = currentFrame.ToBitmap();
                
                if (Monitor.TryEnter(syncObj))
                {
                    //Detect rectangular regions which contain a face
                    VectorOfVectorOfPointF vovp = fd.Detect(currentFrame, recs.ToArray());

                    Monitor.Exit(syncObj);

                    int i;
                    for (i = 0; vovp.Size > i; i++)
                    {
                        Point Center = new Point(0, 0);
                        for (int j = 36; j < 42; j++)
                        {
                            Point p = Point.Round(vovp[i][j]);
                            //Logger.LogInfo((j + 1).ToString(), "X: " + vovp[i][j].X + " Y:" + vovp[i][j].Y);
                            bitmap.SetPixel(p.X, p.Y, Color.Red);
                            Center.Offset(p);
                        }

                        Point rightEyeCenter = new Point(Center.X / 6, Center.Y / 6);
                        Center = new Point(0, 0);

                        for (int j = 42; j < 48; j++)
                        {
                            Point p = Point.Round(vovp[i][j]);
                            //Logger.LogInfo((j + 1).ToString(), "X: " + vovp[i][j].X + " Y:" + vovp[i][j].Y);
                            bitmap.SetPixel(p.X, p.Y, Color.Red);
                            Center.Offset(p);
                        }

                        Point leftEyeCenter = new Point(Center.X / 6, Center.Y / 6);

                        bitmap.SetPixel(rightEyeCenter.X, rightEyeCenter.Y, Color.Red);
                        bitmap.SetPixel(leftEyeCenter.X, leftEyeCenter.Y, Color.Red);
                    }

                    //Logger.LogInfo("BildHinzufuegen - FrameGrabber", "Setting new image");
                    //Show the image with the drawn face
                    imgBoxKamera.Image = bitmap.ToImage<Bgr, Byte>();
                }
                */

                imgBoxKamera.Image = currentFrame;
            }
            else
            {
                //Logger.LogInfo("BildHinzufuegen - FrameGrabber", "Lock on syncObj couldn't be aquired");
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
                        List<Rectangle> recs = new List<Rectangle>();
                        foreach (DetectedObject o in fullFaceRegions)
                        {
                            recs.Add(o.Region);
                        }

                        //Detect rectangular regions which contain a face
                        VectorOfVectorOfPointF vovop = fd.Detect(currentFrame, recs.ToArray());

                        TrainingFace = rotateAndAlignPicture(gray, vovop[0], fullFaceRegions[0]);

                        //Resize the image of the detected face and add the image and label to the lists for training
                        TrainingFace = TrainingFace.Resize(240, 240, Emgu.CV.CvEnum.Inter.Cubic);
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
                        set_training_status("Gesicht gespeichert", Brushes.Green);                      
                    }
                    else
                    {
                        set_training_status("Kein Gesicht!", Brushes.Red);
                    }
                }
                catch (Exception ex)
                {
                    set_training_status("Nicht gespeichert!", Brushes.Red);
                    //Show a MessageBox if there was an exception
                    //MessageBox.Show("Enable the face detection first", "Training Fail", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            });
        }

        private void btn_Zurueck1_Click(object sender, RoutedEventArgs e)
        {
            timer.Dispose();
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

            timer = new Timer(FrameGrabber, null, 200, 50);
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
                return (string) this.Dispatcher.Invoke(new GetTextFromTextBoxDelegate(this.get_txt_Name));
            }
        }

        /*
        private Point getIntersectionPoint(VectorOfPointF vop, int p1, int p2, int p3, int p4)
        {
            double k1 = (vop[p1 - 1].Y - vop[p2 - 1].Y) / (vop[p1 - 1].X - vop[p2 - 1].X);
            double d1 = vop[p1 - 1].Y - k1 * vop[p1 - 1].X;

            double k2 = (vop[p3 - 1].Y - vop[p4 - 1].Y) / (vop[p3 - 1].X - vop[p4 - 1].X);
            double d2 = vop[p3 - 1].Y - k2 * vop[p3 - 1].X;

            Logger.LogInfo("f1(x)", k1 + "*x + " + d1);
            Logger.LogInfo("f2(x)", k2 + "*x + " + d2);

            int intersectionX = (int)Math.Round((d2 - d1) / (k1 - k2));
            int intersectionY = (int)Math.Round(k1 * intersectionX + d1);

            return new Point(intersectionX, intersectionY);
        }
        */

        private Image<Gray,byte> rotateAndAlignPicture(Image<Gray, byte> image, VectorOfPointF vop, DetectedObject detectedObject)
        {
            Point Center = new Point(0, 0);

            for (int j = 36; j < 42; j++)
            {
                Point p = Point.Round(vop[j]);
                //Logger.LogInfo((j + 1).ToString(), "X: " + vop[j].X + " Y:" + vop[j].Y);
                Center.Offset(p);
            }

            Point rightEyeCenter = new Point(Center.X / 6, Center.Y / 6);
            Center = new Point(0, 0);

            for (int j = 42; j < 48; j++)
            {
                Point p = Point.Round(vop[j]);
                //Logger.LogInfo((j + 1).ToString(), "X: " + vop[j].X + " Y:" + vop[j].Y);
                Center.Offset(p);
            }

            Point leftEyeCenter = new Point(Center.X / 6, Center.Y / 6);

            double value = (double)(leftEyeCenter.Y - rightEyeCenter.Y) / (leftEyeCenter.X - rightEyeCenter.X);
            double angle = (Math.Atan(value) * 180) / Math.PI;

            return image.Copy(detectedObject.Region).Rotate(-angle, new Gray(127));
        }

        private void set_training_status(string status, Brush color)
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
                this.Dispatcher.Invoke(new SetTrainingStatusDelegate(set_training_status), status, color);
            }
        }
    }
}