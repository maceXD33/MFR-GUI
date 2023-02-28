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
using System.Windows.Media;

namespace MFR_GUI.Pages
{
    static class Globals
    {
        // Declaration of synchronizing objects
        public static readonly object syncObj = new object();
        public static readonly object syncObj1 = new object();
        public static readonly object syncObj2 = new object();
        public static readonly object syncObjImage = new object();

        // Declarations of different directories
        public static string workingDirectory = Environment.CurrentDirectory;
        public static string projectDirectory = Directory.GetParent(Globals.workingDirectory).Parent.Parent.FullName;

        // Declararation of EmguCV objects
        public static VideoCapture videoCapture;
        public static FaceDetector faceDetector = new FaceDetector();
        public static FaceDetector faceDetector1 = new FaceDetector();
        public static FaceDetector faceDetector2 = new FaceDetector();
        public static FacemarkDetector facemarkDetector = new FacemarkDetector();
        public static LBPHFaceRecognizer recognizer = new LBPHFaceRecognizer(radius: 1, neighbors: 8, gridX: 8, gridY: 8);

        // Declaration of 
        public static int cameraIndex;
        public static bool dataLoaded = false;
        public static bool loadRecognizerFromFile = false;
        public static bool passwordChecked = false;
    }
}