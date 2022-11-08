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
using System.Xml.Linq;
using System.IO;
using MessageBox = System.Windows.Forms.MessageBox;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Windows.Documents;
using System.Linq;
using Emgu.CV.Models;

namespace MFR_GUI.Pages
{
    /// <summary>
    /// Interaktionslogik für BildHinzufuegen.xaml
    /// </summary>
    public partial class BildHinzufuegen : Page
    {
        ImageBox imgBoxKamera;
        long longestTime = 0;

        public BildHinzufuegen()
        {
            InitializeComponent();

            //Create and start a Task
            Task t = Task.Factory.StartNew(() =>
            {
                this.AddFrameGrabberEvent();
            });
        }

        private void btn_speichern_Click(object sender, RoutedEventArgs e)
        {
            //Create and start a Task
            Task t = Task.Factory.StartNew(() =>
            {
                string name = get_txt_Name();
                
                try
                {
                    //increase the counter for trainingfaces
                    trainingFacesCount++;

                    //Get the current frame from capture device
                    currentFrame = grabber.QueryFrame().ToImage<Bgr, Byte>().Resize(1920, 1080, Emgu.CV.CvEnum.Inter.Cubic);

                    gray = currentFrame.Convert<Gray, Byte>();

                    List<Rectangle> dedectedFaces = new List<Rectangle>();

                    //Detect rectangular regions which contain a face
                    //Enter critical region
                    lock (syncObj)
                    {
                        //Detect rectangular regions which contain a face
                        faceDetector.Detect(currentFrame, fullFaceRegions, partialFaceRegions);
                    }

                    foreach (DetectedObject d in fullFaceRegions)
                    {
                        dedectedFaces.Add(d.Region);
                    }

                    //Take the first region as the training face
                    TrainingFace = gray.Copy(dedectedFaces[0]);

                    //Resize the image of the detected face and add the image and label to the lists for training
                    TrainingFace = TrainingFace.Resize(1080, 1080, Emgu.CV.CvEnum.Inter.Cubic);
                    labels.Add(name);
                    trainingImagesMat.Add(TrainingFace.Mat);
                    labelNr.Add(labelNr.Count);

                    //Train the recognizer with all Images and Labels
                    recognizer.Train(trainingImagesMat.ToArray(), labelNr.ToArray());

                    string trainingFacesDirectory = projectDirectory + "/TrainingFaces/";

                    Console.WriteLine();
                    //Write the number of trained faces in a file text for further load
                    File.WriteAllText(trainingFacesDirectory + "TrainedLabels.txt", trainingImagesMat.Count.ToString());

                    //Write the labels of trained faces in a file text for further load and save the images as bitmap-file
                    if (!Directory.Exists(trainingFacesDirectory + name + "/"))
                    {
                        Directory.CreateDirectory(trainingFacesDirectory + name + "/");
                    }

                    int i = 0;
                    while(File.Exists(trainingFacesDirectory + name + "/" + name + i + ".bmp"))
                    {
                        i++;
                    }

                    trainingImagesMat[trainingImagesMat.Count - 1].Save(trainingFacesDirectory + name + "/" + name + i + ".bmp");
            
                    for (int x = 0; x < labels.Count; x++)
                    {
                        File.AppendAllText(trainingFacesDirectory + "TrainedLabels.txt", "%" + labels[x]);
                    }

                    fullFaceRegions = new List<DetectedObject>();
                    partialFaceRegions = new List<DetectedObject>();

                    //Show a MessageBox for confirmation of successful training
                    MessageBox.Show(name + "´s face detected and added :)", "Training OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    //Show a MessageBox if there was an exception
                    MessageBox.Show("Enable the face detection first", "Training Fail", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            });
        }

        private void btn_Zurueck1_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Menu());
        }

        private void FrameGrabber(object sender, EventArgs e)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            lock (syncObj)
            {
                //Get the current frame from capture device
                currentFrame = grabber.QueryFrame().ToImage<Bgr, Byte>().Resize(1920, 1080, Emgu.CV.CvEnum.Inter.Cubic);
            }

            //Convert it to Grayscale
            //gray = currentFrame.Convert<Gray, Byte>();

            List<Rectangle> dedectedFaces = new List<Rectangle>();

            //Detect rectangular regions which contain a face
            //Enter critical region
            lock (syncObj)
            {
                //Detect rectangular regions which contain a face
                try
                {
                    faceDetector.Detect(currentFrame, fullFaceRegions, partialFaceRegions);
                }
                catch(NullReferenceException ex)
                {
                    Console.WriteLine();
                }
            }

            foreach(DetectedObject d in fullFaceRegions)
            {
                dedectedFaces.Add(d.Region);
            }

            //Action for each region detected
            foreach (Rectangle r in dedectedFaces)
            {
                //Draw a rectangle around the region
                currentFrame.Draw(r, new Bgr(Color.Red), 2);
            }

            sw.Stop();
            if (longestTime < sw.ElapsedMilliseconds)
            {
                Task.Delay((int)(sw.ElapsedMilliseconds - longestTime));
                longestTime = sw.ElapsedMilliseconds;
            }

            //Show the image with the drawn face
            imgBoxKamera.Image = currentFrame;
            fullFaceRegions = new List<DetectedObject>();
            partialFaceRegions = new List<DetectedObject>();
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
        }

        //Threadsafe method
        /// <summary>
        /// Add the function FrameGrabber to the Event ComponentDispatcher.ThreadIdle
        /// This function can be called in a thread outside of the Main-Thread.
        /// </summary>
        private void AddFrameGrabberEvent()
        {
            if (this.btn_speichern.Dispatcher.CheckAccess())
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

        //Threadsafe method
        /// <summary>
        /// Gets the Name Property of the TextView txt_Name
        /// This function can be called in a thread outside of the Main-Thread.
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
                return (String) this.Dispatcher.Invoke(new GetTextFromTextBoxDelegate(this.get_txt_Name));
            }
        }
    }
}