using Emgu.CV.Face;
using Emgu.CV;
using System;
using Emgu.CV.Models;
using System.IO;

namespace MFR_GUI.Pages
{
    static class Globals
    {
        // Definition of synchronizing objects
        public static readonly object syncObj1 = new object();
        public static readonly object syncObj2 = new object();

        // Definition of different directories
        public static readonly string workingDirectory = Environment.CurrentDirectory;
        public static readonly string projectDirectory = Directory.GetParent(Globals.workingDirectory).Parent.Parent.FullName;

        // Declararation/Definition of EmguCV objects
        public static VideoCapture videoCapture;
        public static FaceDetector faceDetector1 = new FaceDetector();
        public static FaceDetector faceDetector2 = new FaceDetector();
        public static FacemarkDetector facemarkDetector = new FacemarkDetector();
        public static LBPHFaceRecognizer recognizer = new LBPHFaceRecognizer(gridX: 3, gridY: 3);

        // Declararation/Definition of control variables
        public static int cameraIndex;
        public static bool dataLoaded = false;
        public static bool loadRecognizerFromFile = false;
        public static bool passwordChecked = false;
        public static bool detectorsLoaded = false;
    }
}