using Emgu.CV;
using Emgu.CV.Structure;
using System;
using static MFR_GUI.Pages.Globals;
using Emgu.CV.Models;
using Emgu.CV.Util;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;

namespace MFR_GUI.Pages
{
    internal class ImageEditor
    {
        public static Image<Bgr, byte>? RotateAndAlignPicture(Image<Bgr, byte> image, VectorOfPointF vop, DetectedObject detectedObject, Logger logger)
        {
            Rectangle rec = detectedObject.Region;
            Point Center = new Point(0, 0);

            if (rec.X >= 0 && rec.Y >= 10)
            {
                for (int j = 36; j < 42; j++)
                {
                    Point p = Point.Round(vop[j]);
                    Center.Offset(p);
                }

                Point rightEyeCenter = new Point(Center.X / 6, Center.Y / 6);
                
                Center = new Point(0, 0);

                for (int j = 42; j < 48; j++)
                {
                    Point p = Point.Round(vop[j]);
                    Center.Offset(p);
                }

                Point leftEyeCenter = new Point(Center.X / 6, Center.Y / 6);

                double value = (double)(leftEyeCenter.Y - rightEyeCenter.Y) / (leftEyeCenter.X - rightEyeCenter.X);
                double angle = Math.Atan(value) * 180 / Math.PI;

                double diagonal = Math.Round(Math.Sqrt(rec.Width * rec.Width + rec.Height * rec.Height), 1, MidpointRounding.ToPositiveInfinity);

                int width = Convert.ToInt32(diagonal);
                int height = Convert.ToInt32(diagonal);
                int x = Convert.ToInt32(rec.X - (diagonal - rec.Width) / 2);
                int y = Convert.ToInt32(rec.Y - (diagonal - rec.Height) / 2);

                if(x < 0)
                {
                    x = 0;
                }

                if(y < 0)
                {
                    y = 0;
                }

                Rectangle frameRec;
                Point rightUnderCorner = new Point((x + width), y + height);

                if (rightUnderCorner.X > image.Width || rightUnderCorner.Y > image.Height)
                {
                    if (rightUnderCorner.X > image.Width && rightUnderCorner.Y > image.Height)
                    {
                        logger.LogInfo("if");
                        frameRec = new Rectangle(x, y, image.Width - x, image.Height - y);
                    }
                    else if (rightUnderCorner.X > image.Width && rightUnderCorner.Y < image.Height)
                    {
                        logger.LogInfo("else if");
                        frameRec = new Rectangle(x, y, image.Width - x, height);
                    }
                    else
                    {
                        logger.LogInfo("else");
                        frameRec = new Rectangle(x, y, width, image.Height - y);
                    }
                }
                else
                {
                    frameRec = new Rectangle(x, y, width, height);
                }

                logger.LogInfo("frameRec: " + frameRec.ToString());

                Image<Bgr, byte> frame = image.Copy(frameRec);

                return frame.Rotate(-angle, new Bgr(255, 255, 255)).Resize(320, 240, Emgu.CV.CvEnum.Inter.Cubic);
            }

            return null;
        }

        //Normales Kopfverhätnis: 2:3 (Breite : Länge)
        public static bool IsAngelOver15Degree(Rectangle r)
        {
            if((double) r.Width/r.Height > 0.793)
            {
                return true;
            }

            return false;
        }

        public static bool IsAngelOver15Degree(Rectangle r, Logger logger)
        {
            double d = (double) r.Width / r.Height;

            logger.LogInfo("Width = " + r.Width);
            logger.LogInfo("Height = " + r.Height);
            logger.LogInfo("aspectRatio = " + d);

            if (d > 0.793)
            {
                return true;
            }

            return false;
        }
    }
}