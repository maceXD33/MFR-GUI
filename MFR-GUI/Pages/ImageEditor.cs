﻿using Emgu.CV;
using Emgu.CV.Structure;
using System;
using static MFR_GUI.Pages.Globals;
using Emgu.CV.Models;
using Emgu.CV.Util;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Windows;

namespace MFR_GUI.Pages
{
    internal class ImageEditor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="image">The original image where the faces where detected</param>
        /// <param name="vop">The facial landmarks detected inside the detectedObject</param>
        /// <param name="detectedObject">The DetectedObject containing the rectanle in which a face got detected</param>
        /// <param name="logger"></param>
        /// <returns>Returns the </returns>
        public static Image<Bgr, byte>? RotateAndAlignPicture(Image<Bgr, byte> image, VectorOfPointF vop, DetectedObject detectedObject, Logger logger)
        {
            try
            {
                Rectangle rec = detectedObject.Region;
                Vector2 sumVec = new Vector2(0, 0);

                for (int j = 36; j < 42; j++)
                {
                    sumVec = Vector2.Add(sumVec, vop[j].ToVector2());
                }

                Vector2 rightEyeCenter = Vector2.Divide(sumVec, 6);

                sumVec = new Vector2(0, 0);

                for (int j = 42; j < 48; j++)
                {
                    sumVec = Vector2.Add(sumVec, vop[j].ToVector2());
                }

                Vector2 leftEyeCenter = Vector2.Divide(sumVec, 6);

                double value = (leftEyeCenter.Y - rightEyeCenter.Y) / (leftEyeCenter.X - rightEyeCenter.X);
                double angle = Math.Atan(value) * 180 / Math.PI;

                double diagonal = Math.Round(Math.Sqrt(rec.Width * rec.Width + rec.Height * rec.Height), MidpointRounding.ToPositiveInfinity);

                int side = Convert.ToInt32(diagonal);
                int x = Convert.ToInt32(rec.X - (diagonal - rec.Width) / 2);
                int y = Convert.ToInt32(rec.Y - (diagonal - rec.Height) / 2);

                if (x < 0)
                {
                    x = 0;
                }

                if (y < 0)
                {
                    y = 0;
                }

                Rectangle frameRec;
                Point rightUnderCorner = new Point(x + side, y + side);

                if (rightUnderCorner.X > image.Width || rightUnderCorner.Y > image.Height)
                {
                    if (rightUnderCorner.X > image.Width && rightUnderCorner.Y > image.Height)
                    {
                        frameRec = new Rectangle(x, y, image.Width - x, image.Height - y);
                    }
                    else if (rightUnderCorner.X > image.Width && rightUnderCorner.Y < image.Height)
                    {
                        frameRec = new Rectangle(x, y, image.Width - x, side);
                    }
                    else
                    {
                        frameRec = new Rectangle(x, y, side, image.Height - y);
                    }
                }
                else
                {
                    frameRec = new Rectangle(x, y, side, side);
                }

                Image<Bgr, byte> frame = image.Copy(frameRec);

                return frame.Rotate(-angle, new Bgr(255, 255, 255));
            }
            catch(Exception e)
            {
                logger.LogError(e.Message, "RotateAndAlignPicture.xaml.cs", e.Source);
                return null;
            }
        }

        //Normales Kopfverhätnis: 2:3 (Breite : Länge)
        public static bool IsAngelOver30Degree(Rectangle r, Logger logger)
        {
            double ratio = (double) r.Width / r.Height;

            if (ratio > 0.8983)
            {
                return true;
            }

            return false;
        }

        public static Image<Bgr, byte> CropImage(List<DetectedObject> fullFaceRegions, Image<Bgr, byte> image, Logger logger)
        {
            foreach (DetectedObject d in fullFaceRegions)
            {
                Rectangle r = d.Region;

                if (r.Right < image.Cols && r.Bottom < image.Rows)
                {
                    return image.Copy(r);
                }
            }

            return null;
        }
    }
}