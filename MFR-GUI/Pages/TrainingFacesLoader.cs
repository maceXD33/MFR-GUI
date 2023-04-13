using Emgu.CV.Structure;
using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using static MFR_GUI.Pages.Globals;
using Emgu.CV.Models;
using Emgu.CV.Util;
using System.Drawing;

namespace MFR_GUI.Pages
{
    internal class TrainingFacesLoader
    {
        public static Tuple<List<Mat>, List<string>, List<int>, int> LoadTrainingFacesFourReturns(Logger logger)
        {
            List<Mat> trainingImagesMat = new List<Mat>();
            List<string> labels = new List<string>();
            List<int> labelNr = new List<int>();
            int savedNamesCount = 0;

            try
            {
                //Load the file with the labels from previous trained faces
                string allLabels = File.ReadAllText(projectDirectory + "/Data/TrainedLabels.txt");
                string[] Labels = allLabels.Split('%');
                
                List<string> distinctLabels = Labels.Distinct().ToList();
                distinctLabels.RemoveAt(0);
                
                //Load the images from previous trained faces and add the images, labels and labelnumbers to Lists
                foreach (string name in distinctLabels)
                {
                    for (int i = 0; File.Exists(projectDirectory + "/TrainingFaces/" + name + "/" + name + i + ".bmp"); i++)
                    {
                        Image<Gray, byte> image = new Image<Gray, byte>(projectDirectory + "/TrainingFaces/" + name + "/" + name + i + ".bmp");
                        trainingImagesMat.Add(image.Mat);
                        labelNr.Add(savedNamesCount);
                    }

                    labels.Add(name);
                    savedNamesCount++;
                }

                return new Tuple<List<Mat>, List<string>, List<int>, int>(trainingImagesMat, labels, labelNr, savedNamesCount);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, ex.TargetSite.ToString(), ex.Source);
                return new Tuple<List<Mat>, List<string>, List<int>, int>(null, null, null, -1);
            }
        }

        public static Tuple<List<string>, int> LoadTrainingFacesTwoReturns(Logger logger)
        {
            List<string> labels = new List<string>();
            int savedNamesCount = 0;

            try
            {
                //Load the file with the labels from previous trained faces
                string allLabels = File.ReadAllText(projectDirectory + "/Data/TrainedLabels.txt");
                string[] Labels = allLabels.Split('%');

                List<string> distinctLabels = Labels.Distinct().ToList();
                distinctLabels.RemoveAt(0);

                //Load the images from previous trained faces and add the images, labels and labelnumbers to Lists
                foreach (string name in distinctLabels)
                {
                    labels.Add(name);
                    savedNamesCount++;
                }

                return new Tuple<List<string>, int>(labels, savedNamesCount);
            }
            catch (FileNotFoundException ex)
            {
                return new Tuple<List<string>, int>(new List<string>(), 0);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, ex.TargetSite.ToString(), ex.Source);
                return new Tuple<List<string>, int>(null, -1);
            }
        }

        public static Tuple<List<Mat>, List<int>> LoadTrainingFacesForMenu(Logger logger)
        {
            List<Mat> trainingImagesMat = new List<Mat>();
            List<string> labels = new List<string>();
            List<int> labelNr = new List<int>();
            int savedNamesCount = 0;

            try
            {
                //Load the file with the labels from previous trained faces
                string allLabels = File.ReadAllText(projectDirectory + "/Data/TrainedLabels.txt");
                string[] Labels = allLabels.Split('%');

                List<string> distinctLabels = Labels.Distinct().ToList();
                distinctLabels.RemoveAt(0);

                //Load the images from previous trained faces and add the images, labels and labelnumbers to Lists
                foreach (string name in distinctLabels)
                {
                    for (int i = 0; File.Exists(projectDirectory + "/TrainingFaces/" + name + "/" + name + i + ".bmp"); i++)
                    {
                        Image<Gray, byte> image = new Image<Gray, byte>(projectDirectory + "/TrainingFaces/" + name + "/" + name + i + ".bmp");
                        trainingImagesMat.Add(image.Mat);
                        labelNr.Add(savedNamesCount);
                    }

                    labels.Add(name);
                    savedNamesCount++;
                }

                return new Tuple<List<Mat>, List<int>>(trainingImagesMat, labelNr);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, ex.TargetSite.ToString(), ex.Source);
                return new Tuple<List<Mat>, List<int>>(null, null);
            }
        }

        public static void LoadFaceRecognizer()
        {
            recognizer.Read(projectDirectory + "/Data/recognizer.txt");
        }

        public static Tuple<List<Mat>, List<Image<Bgr, byte>>, List<string>, List<int>> GetTestingData(Logger logger)
        {
            List<Mat> trainingImagesMat = new List<Mat>();
            List<string> labels = new List<string>();
            List<int> labelNr = new List<int>();
            int savedNamesCount = 0;

            try
            {
                //Load the file with the labels from previous trained faces
                string allLabels = File.ReadAllText(projectDirectory + "/TrainingFaces/TestDataset/TrainedLabels.txt");
                string[] Labels = allLabels.Split('%');

                List<string> distinctLabels = Labels.Distinct().ToList();
                distinctLabels.RemoveAt(0);

                //Load the images from previous trained faces and add the images, labels and labelnumbers to Lists
                foreach (string name in distinctLabels)
                {
                    for (int i = 0; File.Exists(projectDirectory + "/TrainingFaces/TestDataset/" + name + "/image_" + i + ".jpg"); i++)
                    {
                        Image<Bgr, byte> image = new Image<Bgr, byte>(projectDirectory + "/TrainingFaces/TestDataset/" + name + "/image_" + i + ".jpg");
                        Mat m = ProcessTestingImage(image, logger);
                        if (m != null)
                        {
                            m.Save(projectDirectory + "/TrainingFaces/TestDataset/CroppedTrainingImages/" + name + i + ".png");
                            trainingImagesMat.Add(m);
                            labelNr.Add(savedNamesCount);
                        }
                    }

                    labels.Add(name);
                    savedNamesCount++;
                }

                List<Image<Bgr, Byte>> testImages = new List<Image<Bgr, Byte>>();
                for (int i = 0; File.Exists(projectDirectory + "/TrainingFaces/TestDataset/Recognize/image" + i + ".jpg"); i++)
                {
                    Image<Bgr, byte> image = new Image<Bgr, byte>(projectDirectory + "/TrainingFaces/TestDataset/Recognize/image" + i + ".jpg");
                    testImages.Add(image);
                }

                return new Tuple<List<Mat>, List<Image<Bgr, byte>>, List<string>, List<int>>(trainingImagesMat, testImages, labels, labelNr);
            }
            catch (Exception ex)
            {
                return new Tuple<List<Mat>, List<Image<Bgr, byte>>, List<string>, List<int>>(null, null, null, null);
            }
        }

        private static Mat ProcessTestingImage(Image<Bgr, Byte> image, Logger logger)
        {
            List<DetectedObject> fullFaceRegions = new List<DetectedObject>();
            List<DetectedObject> partialFaceRegions = new List<DetectedObject>();

            try
            {
                //Enter critical region
                lock (syncObj1)
                {
                    //Detect rectangular regions which contain a face
                    faceDetector1.Detect(image, fullFaceRegions, partialFaceRegions, confidenceThreshold: (float)0.6);
                }

                if (fullFaceRegions.Count != 0)
                {
                    List<Rectangle> recs = new List<Rectangle>();
                    foreach (DetectedObject o in fullFaceRegions)
                    {
                        Rectangle r = o.Region;

                        if (r.Right < image.Cols && r.Bottom < image.Rows)
                        {
                            recs.Add(r);
                        }
                    }

                    VectorOfVectorOfPointF vovop = facemarkDetector.Detect(image, recs.ToArray());

                    if (!ImageEditor.IsAngelOver30Degree(fullFaceRegions[0].Region, logger))
                    {
                        Image<Bgr, Byte> tempTrainingFace = ImageEditor.RotateAndAlignPicture(image, vovop[0], fullFaceRegions[0], logger);

                        fullFaceRegions = new List<DetectedObject>();
                        partialFaceRegions = new List<DetectedObject>();

                        //Enter critical region
                        lock (syncObj1)
                        {
                            //Detect rectangular regions which contain a face
                            faceDetector1.Detect(tempTrainingFace, fullFaceRegions, partialFaceRegions, confidenceThreshold: (float)0.99);
                        }

                        tempTrainingFace = ImageEditor.CropImage(fullFaceRegions, tempTrainingFace, logger);

                        Image<Gray, Byte> trainingFace = tempTrainingFace.Convert<Gray, Byte>();

                        trainingFace = trainingFace.Resize(100, 100, Emgu.CV.CvEnum.Inter.Cubic);

                        return trainingFace.Mat;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, "ProcessTestingImage", ex.Source);
            }

            return null;
        }
    }
}