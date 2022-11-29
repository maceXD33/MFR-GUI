using Emgu.CV.Face;
using Emgu.CV.Structure;
using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.RightsManagement;
using Emgu.CV.Models;
using System.IO;
using System.Windows.Forms;

namespace MFR_GUI.Pages
{
    public delegate void SetGrabberValuesDelegate();
    public delegate string GetTextFromTextBoxDelegate();
    public delegate void SetGUIElementsDelegate(Image<Bgr, byte> image, string status, string recognizedNames);

    static class Globals
    {
        //Declaration of synchronizing object for lock-statements
        public static readonly object syncObj = new object();

        public static string workingDirectory = Environment.CurrentDirectory;
        public static string projectDirectory = Directory.GetParent(Globals.workingDirectory).Parent.Parent.FullName;
        
        public static FaceAndLandmarkDetector fald= new FaceAndLandmarkDetector();
        public static FacemarkDetector fd = new FacemarkDetector();

        //Declararation of all variables
        public static int cameraIndex;
        public static bool dataLoaded;
        public static VideoCapture grabber;
        public static FaceDetector faceDetector = new FaceDetector();
        public static Image<Gray, byte>? gray;
        public static Image<Gray, byte>? TrainingFace;
        public static Image<Gray, byte>? result;
        public static int savedNamesCount = 0;
        public static LBPHFaceRecognizer recognizer = new LBPHFaceRecognizer();
        public static List<Mat> trainingImagesMat = new List<Mat>();
        public static List<string> labels = new List<string>();
        public static List<int> labelNr = new List<int>();

        public static bool password_checked;
    }
}