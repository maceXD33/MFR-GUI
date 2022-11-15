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

namespace MFR_GUI.Pages
{
    public delegate void SetGrabberValuesDelegate();
    public delegate String GetTextFromTextBoxDelegate();

    static class Globals
    {
        //Declaration of synchronizing object for lock-statements
        public static readonly Object syncObj = new Object();

        public static String workingDirectory;
        public static String projectDirectory;
        
        //Declararation of all variables
        public static int cameraIndex;
        public static bool dataLoaded;
        public static VideoCapture grabber;
        public static FaceDetector faceDetector = new FaceDetector();
        public static Image<Bgr, Byte>? currentFrame;
        public static Image<Gray, byte>? gray;
        public static Image<Gray, byte>? TrainingFace;
        public static Image<Gray, byte>? result;
        public static int trainingFacesCount = 0;
        public static LBPHFaceRecognizer recognizer = new LBPHFaceRecognizer();
        public static List<Mat> trainingImagesMat = new List<Mat>();
        public static List<string> labels = new List<string>();
        public static List<int> labelNr = new List<int>();

        public static List<DetectedObject> fullFaceRegions = new List<DetectedObject>();
        public static List<DetectedObject> partialFaceRegions = new List<DetectedObject>();

        public static bool password_checked { get; set; }
    }
}