using Emgu.CV.Structure;
using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using static MFR_GUI.Pages.Globals;

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
                string allLabels = File.ReadAllText(projectDirectory + "/TrainingFaces/TrainedLabels.txt");
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
                return new Tuple<List<Mat>, List<string>, List<int>, int>(null, null, null, 0);
            }
        }

        public static Tuple<List<string>, int> LoadTrainingFacesTwoReturns(Logger logger)
        {
            List<Mat> trainingImagesMat = new List<Mat>();
            List<string> labels = new List<string>();
            List<int> labelNr = new List<int>();
            int savedNamesCount = 0;

            try
            {
                //Load the file with the labels from previous trained faces
                string allLabels = File.ReadAllText(projectDirectory + "/TrainingFaces/TrainedLabels.txt");
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

                return new Tuple<List<string>, int>(labels, savedNamesCount);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, ex.TargetSite.ToString(), ex.Source);
                return new Tuple<List<string>, int>(null, 0);
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
                string allLabels = File.ReadAllText(projectDirectory + "/TrainingFaces/TrainedLabels.txt");
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
    }
}