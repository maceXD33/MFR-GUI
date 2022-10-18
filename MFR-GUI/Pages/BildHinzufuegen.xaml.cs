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

namespace MFR_GUI.Pages
{
    /// <summary>
    /// Interaktionslogik für BildHinzufuegen.xaml
    /// </summary>
    public partial class BildHinzufuegen : Page
    {
        ImageBox imgBoxKamera;

        public BildHinzufuegen()
        {
            InitializeComponent();

            //Create and start a Task
            Task t = Task.Factory.StartNew(() =>
            {
                //Initialize the capture device
                grabber = new VideoCapture();
                this.AddFrameGrabberEvent();
            });
        }

        private void btn_speichern_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btn_Zurueck1_Click(object sender, RoutedEventArgs e)
        {
            Menu m = new Menu();
            this.NavigationService.Navigate(m);
        }

        void FrameGrabber(object sender, EventArgs e)
        {
            //Get the current frame from capture device
            currentFrame = grabber.QueryFrame().ToImage<Bgr, Byte>().Resize(1024, 1024, Emgu.CV.CvEnum.Inter.Cubic);
            //Convert it to Grayscale
            gray = currentFrame.Convert<Gray, Byte>();

            //Detect rectangular regions which contain a face
            Rectangle[] detectedFrontalFaces = face.DetectMultiScale(gray);

            //Action for each region detected
            foreach (Rectangle r in detectedFrontalFaces)
            {
                //Get the rectangular region out of the whole image
                result = currentFrame.Copy(r).Convert<Gray, Byte>().Resize(128, 128, Emgu.CV.CvEnum.Inter.Cubic);

                //Draw a rectangle around the region
                currentFrame.Draw(r, new Bgr(Color.Red), 3);
            }

            //Show the image with the drawn face
            imgBoxKamera.Image = currentFrame;
        }

        private void i_Kamera_Loaded(object sender, RoutedEventArgs e)
        {
            //Create the interop host control.
            System.Windows.Forms.Integration.WindowsFormsHost host = new System.Windows.Forms.Integration.WindowsFormsHost();

            //Create the ImageBox control.
            imgBoxKamera = new ImageBox();
            imgBoxKamera.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;

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
    }
}